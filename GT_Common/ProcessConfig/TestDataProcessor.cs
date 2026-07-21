using GT_Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using GT_Common;
using GT_Common.Helper.ClientTask;
using Consumables = GT_Common.Model.Consumables;
using GT_Common.Helper;
using System.IO;
using GT_Common.Helper.Logging;
using GT_Common.MyEnum;
using GT_Common.DriverForm.Batch;
using AYNETTEK.HFModbus;
using System.Text;
using GT_Common.Util.LableStatus;
using System.Runtime.Remoting.Contexts;
using TaskContracts.Models;
using System.Threading;

namespace GT_Common.ProcessConfig
{
    public class TestDataProcessor : ITestDataProcessor
    {
        private readonly ComponentManager _componentManager;
        private readonly UploadManager _uploadManager;
        private readonly AccessMdbHelper _db;
        private readonly List<Consumables> _consumables;
        private ClientTaskSender _sender;
        private readonly ProductionMonitor _productionMonitor;
        public TestDataProcessor(ComponentManager componentManager, UploadManager uploadManager, AccessMdbHelper db, List<Consumables> Consumables, ClientTaskSender clientTask, ProductionMonitor productionMonitor)
        {
            _componentManager = componentManager;
            _uploadManager = uploadManager;
            _db = db;
            _consumables= Consumables;
            _sender = clientTask;
            _productionMonitor = productionMonitor;

        }

        public async Task HandleData(ProcessContext context)
        {
            try
            {
                DisplayLog.LogProcessStartBuildTestData(context.Process.Name, context.Sn);

                // ================= 第一阶段：前置准备(判断是否需要正式MES流程) =================

                if (GetHistoryKindBySn(context.Sn) != HistoryKind.MyCsvData)
                {
                    var localList = BuildSaveItems(context);

                    ShowCurrentDataOnUI(context, context.Sn, context.Data);

                    SaveLocally(context, context.Sn, localList);

                    context.WritePlcResult = 1;
                    return;
                }
                // ================= 第二阶段：条码解析(确定 MainBar、PartBar) =================

                bool mainBarOk = ResolveBarInfo(context);

                // ================= 第三阶段：构建上传数据(统一测试数据格式) =================

                var newList = BuildSaveItems(context);

                DisplayLog.LogProcessFinishedBuildTestData(context.Process.Name, context.MainBar);

                // ================= 第四阶段：MES相关逻辑 =================
                bool needRemove = context.Process.PlcReadConfig.CombinedUpload?.IsFinalStep == true 
                                   || context.Result == "NG";

                if (needRemove)
                {
                    Shared.currentBarNo = context.MainBar;

                    Shared.currentProductStatus = (ProductStatus)(context.Result == "NG" ? 2 : 1);
                }

                // ================= 4. 其他异步 =================
                bool mesOk = true;
                bool shopOrderResult = true;

                if (context.Process.PlcReadConfig.CombinedUpload?.IsEnabled == true)
                {
                    var dataItem = UploadCombinedData(context, newList);

                    if (dataItem != null)
                    {
                        if (!Shared.isOffline)
                        {
                            shopOrderResult = BydMesCom.工单信息查询("", Config.Instance.Resource, context.MainBar, out ShopOrderInfo shopOrder, out string para, out string xml);
                            if (!shopOrderResult)
                            {
                                context.WritePlcResult = 2;  // 无效订单
                                DisplayLog.Warn($"流程 【{context.Process.Name}】【{context.MainBar}】获取工单失败", true);
                            }
                            Shared.shopOrder = shopOrder;
                            context.MesOrder = shopOrder.Order;
                            mesOk = await Task.Run(() => UpLoadMes(context, dataItem));

                            Shared.actionTipStatus = mesOk && shopOrderResult ? ProductStatus.OK : ProductStatus.NG;

                            Shared.actionTip = $"{context.MainBar}\n条码上传MES--{StatusStyle.GetDisplayText(Shared.actionTipStatus)}";
                        }

                        _productionMonitor.OnProductDetected(context.MainBar, context.Result == "OK");

                        await SaveLocally(context, context.MainBar, newList);

                        context.MesResult = mesOk ? "OK" : "NG";
                        // 后台附属任务

                        RunBackgroundTasks(context, dataItem);
                    }
                    else
                    {
                        Shared.actionTipStatus = (ProductStatus)(context.Result == "NG" ? 2 : 1);

                        Shared.actionTip = $"{context.MainBar}\n流程【{context.Process.Name}】--{StatusStyle.GetDisplayText(Shared.actionTipStatus)}";

                        DisplayLog.Error($"流程【{context.Process.Name}】【{context.Sn}】读取数据为null", null);
                    }
                }

                ShowCurrentDataOnUI(context, context.MainBar, context.Data);

                // ================= 5. 唯一PLC裁决点 =================

                //if (needRemove && mesOk)
                //{
                //    RemoveBindingIfNeed(context);
                //}
                context.WritePlcResult = (mainBarOk && mesOk) ? 1 : 2;

                DisplayLog.LogProcessUploadData(context.Process.Name);
            }
            catch (Exception ex)
            {
                context.WritePlcResult = 2;

                DisplayLog.Error($"流程【{context.Process.Name}】【{context.Sn}】全流程异常", ex, true);

                throw;
            }
        }

        //  条码解析
        private bool ResolveBarInfo(ProcessContext context)
        {
            bool mainBarOk = true;

            switch (context.Process.SearchBarType)
            {
                case SearchBarType.NonFunctional:
                    context.MainBar = context.Sn;
                    context.PartBar = null;

                    break;
                case SearchBarType.MainBar:

                    context.PartBar = context.Sn;

                    if (_componentManager.TryGetAssemblyCode(context.Sn, out var assembly))
                    {
                        context.MainBar = assembly;
                        DisplayLog.Info($"流程【{context.Process.Name}】【{context.Sn}】查询到主码 【{assembly}】", true);
                    }
                    else
                    {
                        mainBarOk = false;
                        DisplayLog.Warn($"流程【{context.Process.Name}】【{context.Sn}】需要主码但未绑定，降级使用SN", true);
                        context.MainBar = context.Sn; // 降级
                    }
                    break;
                case SearchBarType.PartBar:

                    context.MainBar = context.Sn;

                    if (_componentManager.TryGetComponentCode(context.Sn, out var component))
                    {
                        context.PartBar = component;
                        DisplayLog.Info($"流程【{context.Process.Name}】【{context.Sn}】查询到部件码 【{component}】", true);
                    }
                    else
                    {
                        mainBarOk = false;
                        DisplayLog.Warn($"流程【{context.Process.Name}】【{context.Sn}】未查询到部件码，降级使用SN", true);
                        context.PartBar = "";
                    }
                    break;
                default:
                    context.MainBar = context.Sn;

                    break;
            }
            return mainBarOk;
        }

        //  构建上传数据
        private List<SaveItem> BuildSaveItems(ProcessContext context) 
        {
            var newList = context.Data.Select(x => new SaveItem
            {
                Test_item_name = string.IsNullOrEmpty(x.Unit) ? x.Name : $"{x.Name}({x.Unit})",
                Test_item_up = x.Upper.Value.ToString(),
                Test_item_down = x.Lower.Value.ToString(),
                Test_item_value = x.Value.Value.ToString(),
                Test_item_unit = x.Unit,
                Ok_flag = Convert.ToInt32(x.Result.Value) == 1 ? "OK" : "NG"

            }).ToList();

            return newList;
        }

        //  后台任务
        private void RunBackgroundTasks(ProcessContext context,DataItem dataItem)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    ShowOnUI(context);
                    await SendConsumablesTaskAsync(context);
                    StandardProcessSaveToAccess(context, dataItem);
                    UpdateConsumables(context);
                    await SendProcessPropertyAsync(context, dataItem);

                }
                catch (Exception ex)
                {
                    DisplayLog.Error($"流程【{context.Process.Name}】【{context.Sn}】后台附属任务异常", ex, true);
                }
            });
        }

        //  解绑
        private void RemoveBindingIfNeed(ProcessContext context)
        {
            switch (context.Process.SearchBarType)
            {
                case SearchBarType.MainBar:
                    _componentManager.Remove(context.MainBar, true);
                    break;

                case SearchBarType.PartBar:
                    _componentManager.Remove(context.MainBar);
                    break;
            }
        }

        //  发送数据到服务端ACCESS
        private async Task SendProcessPropertyAsync(ProcessContext context, DataItem dataItem)
        {
            if (dataItem == null)
                return;

            try
            {
                // ⭐ 结果归一化（显式、可读）
                //CommMethod.NormalizeResultToNgIfAnyNg(dataItem.Data_result);

                await _sender.SendTaskAsync("InsertProcessProperty", new
                {
                    DataItem = dataItem,
                    Process_no = context.Process.ProcessNo,
                    Order_no = Shared.workOrder,
                    Operator_no = Shared.user.JobNub,
                    Ok_flag = dataItem.Data_result,
                    Do_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Test_beat = context.TaktTime
                });

                DisplayLog.Info(
                    $"流程【{context.Process.Name}】【{context.Sn}】发送 DbTask 数据成功",
                    true);
            }
            catch (Exception ex)
            {
                DisplayLog.Error(
                    $"流程【{context.Process.Name}】【{context.Sn}】发送 DbTask 数据失败",
                    ex,
                    true);
            }
        }

        //  发送易损件数据到服务端ACCESS
        private async Task SendConsumablesTaskAsync(ProcessContext context)
        {
            try
            {
                foreach (var item in _consumables)
                {
                    await _sender.SendTaskAsync("UpdateConsumable", new
                    {
                        ProcessName = item.ProcessName,
                        StationName = item.StationName,
                        Location = item.Location,
                        Name = item.Name,
                        TheoreticalCount = item.TheoreticalCount,
                        UsedCount = item.UsedCount,
                        RemainderCount = item.RemainderCount
                    });
                }
                
                DisplayLog.Info($"流程 【{context.Process.Name}】 {context.MainBar}发送易损件信息DbTask数据成功", true);
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"流程 【{context.Process.Name}】 {context.MainBar}发送易损件信息DbTask数据失败", ex, true);
            }
        }

        //  点检
        public async Task CalibrationHandleData(ProcessContext context)
        {
            //var testData = BuildTestData(context);
            var calType = context.calibrationType

            .FirstOrDefault(t => t.CalibrationType == context.CalibrationFlag);

            var newList = context.Data.Select(x => new SaveItem
            {
                Test_item_name = string.IsNullOrEmpty(x.Unit)
                ? $"{calType?.Name}_{x.Name}"
                : $"{calType?.Name}_{x.Name}({x.Unit})",
                Test_item_up = x.Upper.Value.ToString(),
                Test_item_down = x.Lower.Value.ToString(),
                Test_item_value = x.Value.Value.ToString(),
                Test_item_unit = x.Unit,
                Ok_flag = Convert.ToInt32(x.Result.Value).ToString(),
            }).ToList();

            //  保存到ACCESS
            //SaveToAccess(context, newList);

            UpdateSHPeriodOrCalibrationValues(context, newList);
            //  本地
            await SaveLocally(context, context.Sn, newList);

            //  UI展示
            //ShowOnUI(context, testData);

            ShowCurrentDataOnUI(context, context.Sn, context.Data);

            context.WritePlcResult = 1;
        }

        //  保存本地csv
        private async Task SaveLocally(ProcessContext context, string sn, List<SaveItem> testData)
        {
            if (testData == null || testData.Count == 0)
            {
                DisplayLog.Warn(
                    $"流程【{context.Process.Name}】【{sn}】SaveLocally：testData 为空，跳过本地保存",
                    true);
                return;
            }

            //// 1️⃣ 取结果数组
            //var okFlags = testData.Select(t => t.Ok_flag).ToArray();

            //// 2️⃣ NG 归一化（关键规则）
            //CommMethod.NormalizeResultToNgIfAnyNg(okFlags);

            // 3️⃣ 构造保存对象（使用统一后的 Ok_flag）
            var saveItems = testData.Select((t, index) => new SaveItem
            {
                Process_no = context.Process.ProcessNo,
                Order_no = Shared.workOrder,
                Bar_no = sn,
                Operator_no = Shared.user.JobNub,
                Do_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Ok_flag = t.Ok_flag,
                Test_beat = context.TaktTime,
                Test_item_name = t.Test_item_name,
                Test_item_up = t.Test_item_up,
                Test_item_down = t.Test_item_down,
                Test_item_value = t.Test_item_value,
            }).ToList();

            // 4️⃣ 根据 sn 前5位决定 HistoryKind
            HistoryKind historyKind = GetHistoryKindBySn(sn);
            DisplayLog.Info(
                   $"流程【{context.Process.Name}】SaveLocally【{sn}】为 {historyKind.GetFolderName()}",
                   true);
            await CsvSaverAsync.SaveListToMonthlyCsvAsync(
                saveItems,
                Path.Combine(
                      PathCenter.History,   // D:\GT_System\MES_OP010\Data\History
                      historyKind.GetFolderName()
                  ),
                append: true,
                retainDays: Config.Instance.RretainDays
            );
        }

        // 根据 SN 前5位获取对应的 HistoryKind
        private HistoryKind GetHistoryKindBySn(string sn)
        {
            if (string.IsNullOrEmpty(sn) || sn.Length < 5)
            {
                return HistoryKind.MyCsvData; // 默认
            }

            string prefix = sn.Substring(0, 5);

            switch (prefix)
            {
                case var _ when prefix == Config.Instance.WaterBurstMark:
                    return HistoryKind.水爆件生产数据;
                case var _ when prefix == Config.Instance.CutPieceMark:
                    return HistoryKind.切割件生产数据;
                default:
                    return HistoryKind.MyCsvData;
            }
        }

        private static readonly object _lock = new object();

        //  保存数据到本地Access
        public void StandardProcessSaveToAccess(ProcessContext context, DataItem testData)
        {
            if (testData == null)
            { 
                return;
            }
            lock (_lock)
            {

                DatabaseSessionManager.EnsureDatabase();

                var db = DbContext.CurrentDb;

                bool result = UploadSql.Ac_InsertProcessPropertyWithResult(
                db,
                context.Process.ProcessNo,
                Shared.workOrder,
                context.MainBar,
                Shared.user.JobNub,
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                testData.Data_result,
                context.TaktTime,
                testData.Data_name,
                testData.Data_up,
                testData.Data_down,
                testData.Data
            );
            }
        }

        //  更新本地Access易损件
        private void UpdateConsumables(ProcessContext context)
        {
            try
            {
                DatabaseSessionManager.EnsureDatabase();

                var db = DbContext.CurrentDb;

                foreach (var item in _consumables)
                {
                    
                    UploadSql.Ac_UpdateConsumables(db, item);
                }

                DisplayLog.Info($"流程 【{context.Process.Name}】 {context.MainBar}更新易损件信息成功", true);
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"流程 【{context.Process.Name}】 {context.MainBar}更新易损件信息失败", ex, true);
            }
        }

        //  下料DataUI显示
        private void ShowOnUI(ProcessContext context)
        {
            Shared.Instance.dispItem.Enqueue(new TestDispItem
            {
                OrderNub = Shared.workOrder,
                BarNo = context.MainBar,
                MainBar = context.MainBar,
                PartBar = context.PartBar,
                DoTime = DateTime.Now.ToString(),
                UserName = Shared.user.JobNub,
                Ok_flag = context.Result,
                MesResult = context.MesResult,
                TaktTime = context.TaktTime,
                //Data = context.Data
            });
        }

        //  点检数据上传
        private void UpdateSHPeriodOrCalibrationValues(ProcessContext context, List<SaveItem> testData)
        {
            UploadSql.UpdateSHPeriodOrCalibrationValues(
                context.Sn,
                context.Process.ProcessNo.Substring(0,5),
                DateTime.Now.ToString(),
                testData
                );
        }

        //  缓存绑定
        private DataItem UploadCombinedData(ProcessContext context, List<SaveItem> testData)
        {
            return ProcessMethods.UploadCombinedData(
                context.MainBar,
                context.Result,
                context.ResultCode,
                testData.Select(t => t.Test_item_name).ToArray(),
                testData.Select(t => t.Ok_flag).ToArray(),
                testData.Select(t => t.Test_item_value).ToArray(),
                testData.Select(t => t.Test_item_up).ToArray(),
                testData.Select(t => t.Test_item_down).ToArray(),
                context.Process,
                _uploadManager
            );
        }

        //  展示CurrentProductDataUI
        private void ShowCurrentDataOnUI(ProcessContext context,string sn, List<PlcMeasureGroup> testData)
        {
            var currentTestItemMDs = new BindingList<TestItemMD>(
                   testData.Select(item => new TestItemMD
                   {
                       Col0 = item.Name,
                       Col1 = item.Upper.Value.ToString() + item.Unit,
                       Col2 = item.Value.Value.ToString() + item.Unit,
                       Col3 = item.Lower.Value.ToString() + item.Unit,
                       Col4 = Convert.ToInt32(item.Result.Value) == 1 ? "OK" : "NG"
                   }).ToList()
               );

            lock (Shared._dicCurrentTestItemMDsLock)
            {
                Shared.dicCurrentTestItemMDs[context.Process.Name] = new currentProductDataModel()
                {
                    Sn = sn,
                    Data = currentTestItemMDs
                };
            }
        }

        //  上传byd-mes
        private bool UpLoadMes(ProcessContext context, DataItem testData)
        {
            /* "
           总成条码,测试信息,AAAA!
           产线名称,测试信息,L发生器!
           工位名称,测试信息,OP010管壳!
           当前工单,测试信息,AAAA!
           当前配方名,测试信息,LC-260H!
           操作人工号,测试信息,AAAA!
           测试时间,测试信息,2026-04-14!
           测试总结果,测试信息,OK!
           实际节拍,测试信息,AAAA!
           离散装配结果,测试信息,SKIP!
           MES上传结果,测试信息,!
           零部件1条码,测试信息,!
           壳体批次号,,AAAA!
           "*/

            string testTitle = $"!总成条码,测试信息,{context.MainBar}!产线名称,测试信息,L型自动2线!工位名称,测试信息,{Shared.ProcessName}!当前工单,测试信息,{context.MesOrder}!当前配方名,测试信息,{Shared.productName}!操作人工号,测试信息,{Shared.user.JobNub}!测试时间,测试信息,{DateTime.Now}!测试总结果,测试信息,{context.Result}!实际节拍,测试信息,{context.TaktTime}!MES上传结果,测试信息,";

            string partBarInfo = string.IsNullOrEmpty(context.PartBar) ? "" : $"!零部件1条码,测试信息,{context.PartBar}!";

            string partList = BuildMesText(Shared.Instance.partRuntimes);

            string testItme = $"{testTitle}{partBarInfo}{partList}{GenerateResultStr(testData)}{GenerateFieldStr(testData)}";
            //  BYD-MES 
            BydMesCom.条码上传(context.Result == "OK", context.MainBar, testItme, out bool 验证成功, out string MES反馈, out string XMLOUT);

            return 验证成功;
        }

        //  组合批次码信息
        public static string BuildMesText(List<PartRuntime> list)
        {
            if (list == null || list.Count == 0)
                return string.Empty;

            return string.Join("",
                          list.Select(item =>
                              $"!{item.PartName ?? ""},测试信息,{item.BYDBatchNub ?? ""}"
                          ));
        }

        //  组合测试项数据
        //private static string GenerateFieldStr(DataItem data)
        //{
        //    if (data.Data_name == null || data.Data_name.Length == 0)
        //        return "";

        //    return string.Join("!",
        //        data.Data_name.Zip(data.Data_down, (name, down) => new { name, down })
        //            .Zip(data.Data_up, (prev, up) => new { prev.name, prev.down, up })
        //            .Zip(data.Data, (prev, value) => new { prev.name, prev.down, prev.up, value })
        //            .Zip(data.Data_result, (prev, result) =>
        //                $"{prev.name},{prev.down}~{prev.up},{prev.value}"));
        //}

        private static string GenerateResultStr(DataItem data)
        {
            if (data.Data_name == null || data.Data_name.Length == 0)
                return string.Empty;

            var sb = new StringBuilder();

            int count = data.Data_name.Length;

            for (int i = 0; i < count; i++)
            {
                string name = GetValue(data.Data_name, i);

                // 去除括号及其中的内容（如 "(mm)"），然后加上"结果"后缀
                string processedName = RemoveParenthesesContent(name) + "结果";

                string result = GetValue(data.Data_result, i);

                sb.Append($"!{processedName},,{result}");
            }

            return sb.ToString();
        }

        private static string GenerateFieldStr(DataItem data)
        {
            if (data.Data_name == null || data.Data_name.Length == 0)
                return string.Empty;

            var sb = new StringBuilder();

            int count = data.Data_name.Length;

            for (int i = 0; i < count; i++)
            {
                string name = GetValue(data.Data_name, i);
                string down = GetValue(data.Data_down, i);
                string up = GetValue(data.Data_up, i);
                string value = GetValue(data.Data, i);

                sb.Append($"!{name},{down}~{up},{value}");
            }

            return sb.ToString();
        }

        private static string GetValue(string[] arr, int index)
        {
            if (arr == null)
                return string.Empty;

            if (index < 0 || index >= arr.Length)
                return string.Empty;

            return arr[index] ?? string.Empty;
        }

        /// <summary>
        /// 去除字符串中的括号及其内部内容（支持中英文括号）
        /// </summary>
        /// <param name="input">原始字符串</param>
        /// <returns>去除括号内容后的字符串</returns>
        private static string RemoveParenthesesContent(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input ?? string.Empty;

            // 去除英文括号及其内容，如 (mm) -> 空
            string result = System.Text.RegularExpressions.Regex.Replace(input, @"\([^)]*\)", "");
            // 去除中文括号及其内容，如 （mm） -> 空
            result = System.Text.RegularExpressions.Regex.Replace(result, @"（[^）]*）", "");
            // 去除末尾可能多余的空格
            return result.TrimEnd();
        }
    }
}
