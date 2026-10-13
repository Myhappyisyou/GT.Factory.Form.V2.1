using GT.Factory.Common.Models;
using GT.Factory.Logging.Logging;
using GT.Factory.PLC.PlcComm;
using GT.Factory.Station.Helper;
using GT.Factory.Station.MyEnum;
using HslCommunication;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GT.Factory.Station.ProcessConfig
{
    public class ProcessMethods
    {
        //打标
        private static object markObj = new object();

        /// <summary>
        /// 读取plc数据
        /// </summary>
        /// <param name="plcConfigs"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<PlcMeasureGroup> ReadPlcMeasureGroups(List<PlcMeasureGroup> groups, PatchReader reader)
        {
            foreach (var g in groups)
            {
                g.Upper.Value = ReadChannel(g.Upper, reader);
                g.Lower.Value = ReadChannel(g.Lower, reader);
                g.Value.Value = ReadChannel(g.Value, reader);
                g.Result.Value = ReadChannel(g.Result, reader);
            }
            return groups;
        }

        private static object ReadChannel(PlcChannel channel, PatchReader reader)
        {
            string address = channel.Address;

            switch (channel.DataType)
            {
                case PlcDataType.Float:
                    return reader.Data.F(address);
                case PlcDataType.Int:
                    return reader.Data.I(address);
                case PlcDataType.Short:
                    return reader.Data.S(address);
                case PlcDataType.UShort:
                    return reader.Data.US(address);
                case PlcDataType.Bool:
                    return reader.Data.B(address, channel.Length);
                case PlcDataType.Double:
                    return reader.Data.D(address) / channel.ScalingFactor;
                case PlcDataType.String:
                    return reader.Data.Str2(address + 2, channel.Length);
                default:
                    throw new NotSupportedException("Unsupported DataType: " + channel.DataType);
            }
        }

        public static void WritePlcSwitch(
            List<WriteItemConfig> configs,
            List<IMeasurement> measurements,
            PatchReader reader)
        {
            var items = configs.Zip(measurements, (c, m) => new { c, m });
            foreach (var item in items)
                WritePlc(item.c, item.m, reader);
        }

        private static void WritePlc(
            WriteItemConfig plcConfig,
            IMeasurement measurement,
            PatchReader reader)
        {
            try
            {
                object val = measurement.Value;  // 直接从接口获取

                switch (plcConfig.DataType)
                {
                    case PlcDataType.Float:
                        reader.Plc.Write(plcConfig.Address, Convert.ToSingle(val));
                        break;
                    case PlcDataType.Short:
                        reader.Plc.Write(plcConfig.Address, Convert.ToInt16(val));
                        break;
                    case PlcDataType.Int:
                        reader.Plc.Write(plcConfig.Address, Convert.ToInt32(val));
                        break;
                    case PlcDataType.DInt:
                        reader.Plc.Write(plcConfig.Address, Convert.ToInt64(val));
                        break;
                    case PlcDataType.Bool:
                        reader.Plc.Write(plcConfig.Address, Convert.ToBoolean(val));
                        break;
                    case PlcDataType.String:
                        reader.Plc.Write(plcConfig.Address, Convert.ToString(val));
                        break;
                    default:
                        Console.WriteLine($"未知类型：{plcConfig.DataType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                DisplayLog.Info($"写入失败：{measurement.Name} - {ex.Message}");
                Console.WriteLine($"写入失败：{measurement.Name} - {ex.Message}");
            }
        }

        /// <summary>
        /// 读取SN
        /// </summary>
        /// <param name="e"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static string ReadSnCode(ProcessBase e, PatchReader reader)
        {
            string address = e.SnConfig.Address;
            string sn = StrHelper.ReplaceIllegalChar(reader.Data.Str2(address, e.SnConfig.Length));
            DisplayLog.LogProcessReadSn(e.Name, sn);
            sn = sn.Trim('?');
            //DisplayLog.LogProcessReadSn(e.Name, sn);
            return sn;
        }

        /// <summary>
        /// 读取绑定SN
        /// </summary>
        /// <param name="e"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static List<PartInfo> ReadBinSnCode(ProcessBase e, PatchReader reader, List<PartInfo> partInfos)
        {
            DisplayLog.LogProcessReadBindBar(e.Name);

            foreach (var g in partInfos)
            {
                string sn = StrHelper.ReplaceIllegalChar(ReadChannel(g.PlcChannel, reader).ToString());
                DisplayLog.LogProcessReadBindBar(e.Name, sn);
                sn = sn.Trim('?');
                g.PlcChannel.Value = sn;
            }
            return partInfos;
        }

        /// <summary>
        /// 读取产品NG代码
        /// </summary>
        /// <param name="e"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static int GetNgCode(ProcessBase e, string sn)
        {
            string NgCode = AppRuntime.Current.DataRuntime.Repositories.Processes.SelectNgCodeAsync(sn).GetAwaiter().GetResult();
            DisplayLog.LogProcessGetNgCode(e.Name, sn, NgCode);
            Regex regex = new Regex(@"^(\d+)_.*$");
            Match match = regex.Match(NgCode);
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// 读取结果标志
        /// </summary>
        /// <param name="e"></param>
        /// <param name="reader"></param>
        /// <param name="resultCode"></param>
        /// <returns></returns>
        public static string ReadResult(IResultReadable e, PatchReader reader, out string resultCode)
        {
            string address = e.ResultConfig.Address;

            ushort resultValue = reader.Data.US(address);
            resultCode = resultValue.ToString();
            // 获取名称（如果是 ProcessBase，取 Name）
            string processName = (e as ProcessBase)?.Name ?? "Unknown";
            DisplayLog.LogProcessReadResult(processName, resultCode);
            return resultValue != 0 && resultValue % 10 == 0 ? "OK" : "NG";
        }

        /// <summary>
        /// 读取节拍
        /// </summary>
        /// <param name="e"></param>
        /// <param name="reader"></param>
        /// <param name="resultCode"></param>
        /// <returns></returns>
        public static string ReadTaktTime(ITaktTimeReadable e, PatchReader reader)  // ← 改为 ITaktTimeReadable
        {
            string address = e.TaktTimeAddress;

            // 地址为 0，认为未配置
            if (string.IsNullOrEmpty(address))
            {
                string processName = (e as ProcessBase)?.Name ?? "Unknown";
                DisplayLog.Warn($"工序【{processName}】未配置 TaktTimeAddress");
                return "0";
            }

            try
            {
                float taktTimeValue = reader.Data.F(address);

                if (taktTimeValue <= 0 || float.IsNaN(taktTimeValue))
                {
                    string processName = (e as ProcessBase)?.Name ?? "Unknown";
                    DisplayLog.Warn($"工序【{processName}】读取到无效 TaktTime: {taktTimeValue}");
                    return "0";
                }

                string name = (e as ProcessBase)?.Name ?? "Unknown";
                DisplayLog.LogProcessReadTaktTime(name, taktTimeValue.ToString("F2"));
                return taktTimeValue.ToString("F2");
            }
            catch (Exception ex)
            {
                string processName = (e as ProcessBase)?.Name ?? "Unknown";
                DisplayLog.Error($"工序【{processName}】读取 TaktTime 失败，地址={e.TaktTimeAddress}", ex, true);
                return "0";
            }
        }

        /// <summary>
        /// 读取点检标志
        /// </summary>
        /// <param name="e"></param>
        /// <param name="reader"></param>
        /// <param name="resultCode"></param>
        /// <returns></returns>
        public static short ReadCalibrationFlag(IStatusFlagReadable e, PatchReader reader)  // ← 改为 IStatusFlagReadable
        {
            string address = e.StatusFlagAddress;

            short calibrationFlag = reader.Data.S(address);

            string processName = (e as ProcessBase)?.Name ?? "Unknown";
            DisplayLog.LogProcessReadCalibrationFlag(processName, calibrationFlag.ToString());

            return calibrationFlag;
        }

        /// <summary>
        /// 读取PLC反馈数据
        /// </summary>
        /// <param name="e"></param>
        /// <param name="reader"></param>
        /// <param name="sn"></param>
        /// <returns></returns>
        public static async Task<List<PlcMeasureGroup>> ReadPlcData(IPlcReadable e, PatchReader reader, string sn)  // ← 改为 IPlcReadable
        {
            var groups = ReadPlcMeasureGroups(e.PlcReadConfig.MeasureGroups, reader);

            string processName = (e as ProcessBase)?.Name ?? "Unknown";
            DisplayLog.LogProcessReadPlcData(processName, sn, Flatten(groups));

            return groups;
        }

        private static string Flatten(List<PlcMeasureGroup> groups)
        {
            var list = new List<string>();

            foreach (var g in groups)
            {
                list.Add(g.Upper.Value?.ToString() ?? "");
                list.Add(g.Lower.Value?.ToString() ?? "");
                list.Add(g.Value.Value?.ToString() ?? "");
                list.Add(g.Result.Value?.ToString() ?? "");
            }

            return string.Join(",", list);
        }

        /// <summary>
        /// 数据缓存
        /// </summary>
        /// <param name="sn"></param>
        /// <param name="result"></param>
        /// <param name="resultCode"></param>
        /// <param name="data"></param>
        /// <param name="e"></param>
        /// <param name="uploadManager"></param>
        public static DataItem UploadCombinedData(
            string sn,
            string result,
            string resultCode,
            string[] data_name,
            string[] data_result,
            string[] data,
            string[] data_up,
            string[] data_down,
            CombinedUpload combinedUpload,
            UploadManager uploadManager)
        {
            var item = new DataItem()
            {
                Sn = sn,
                Do_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Data_name = data_name,
                Data_result = data_result,
                Data = data,
                Data_up = data_up,
                Data_down = data_down,
                NgMsg = resultCode,
                Result = result,
                Step = combinedUpload == null ? 0 : combinedUpload.CurrentStep,
                IsLastStep = combinedUpload != null && combinedUpload.IsFinalStep,
            };
            return uploadManager.Update(item);
        }

        /// <summary>
        /// 文件解析与上传
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sn"></param>
        /// <param name="reader"></param>
        public static List<string> HandleFileUploads(IFileSupport e, string sn, PatchReader reader)  // ← 改为 IFileSupport
        {
            List<string> lsFilePath = new List<string>();
            string fileSn = null;

            // 获取 ProcessNo（需要从 ProcessBase 获取）
            string processNo = (e as ProcessBase)?.ProcessNo ?? "";
            string processName = (e as ProcessBase)?.Name ?? "Unknown";

            foreach (var item in e.FileConfig.FileItems)
            {
                try
                {
                    string address = item.Address;

                    fileSn = StrHelper.ReplaceIllegalChar(reader.Data.Str2(address, item.Length));
                    fileSn = fileSn.Trim('?');
                    string ftpPath = CommMethod.FtpFilePath(processName, sn, fileSn, item);
                    lsFilePath.Add(ftpPath);

                    AppRuntime.Current.DataRuntime.ProcessService.UploadFileAsync(
                        new ProcessFilePayload
                        {
                            BarNo = sn,
                            ProcessNo = processNo,
                            FileName = item.FileName,
                            FileType = item.FileType,
                            FilePath = ftpPath
                        }).GetAwaiter().GetResult();

                    DisplayLog.LogProcessFileUpload(processName, sn, fileSn);
                }
                catch (Exception ex)
                {
                    DisplayLog.LogProcessFileUpload(processName, sn, fileSn, ex);
                }
            }
            return lsFilePath;
        }

        /// <summary>
        /// 反馈结果
        /// </summary>
        /// <param name="e"></param>
        /// <param name="reader"></param>
        /// <param name="sn"></param>
        public static void WritePlcResult(ProcessBase e, PatchReader reader, int value = 2)
        {
            OperateResult write = reader.Plc.Write(e.ResultWriteConfig.Address, (short)value);
            DisplayLog.WriteIsSucess(write.IsSuccess, e.Name, value);
        }

        /// <summary>
        /// 反馈完成信号
        /// </summary>
        /// <param name="e"></param>
        /// <param name="reader"></param>
        public static void WriteTriggerFinish(ProcessBase e, PatchReader reader)
        {
            OperateResult writeResult = reader.Plc.Write(e.FinishFeedbackAddress, (short)2);
            DisplayLog.IsSucess(writeResult.IsSuccess, e.Name);
        }

        /// <summary>
        /// 上传点检
        /// </summary>
        /// <param name="e"></param>
        /// <param name="reader"></param>
        public static CalibrationOrPeriodInspection LoadCalibrationOrPeriodInspection(CalibrationProcess e, PatchReader reader)
        {
            string address = e.StatusFlagAddress;

            int flag = reader.Data.US(address);

            if (e is CalibrationProcess calibration)
            {
                string CalibrationStationId = calibration.TypeStationIds
                    .Where(t => t.CalibrationType == flag)
                    .Select(t => t.CalibrationStationId)
                    .FirstOrDefault();
                return SelectCalibrationItem(e.ProcessNo, CalibrationStationId);
            }

            return SelectCalibrationItem(e.ProcessNo, "");
        }

        /// <summary>
        /// 获取数据库生成产品码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="LineNo"></param>
        /// <param name="ProductModel"></param>
        /// <param name="OP90_x"></param>
        /// <param name="ProductNo"></param>
        /// <returns></returns>
        public static string GenerateProductCode(ProcessContext context, string LineNo, int ProductModel, int OP90_x, string ProductNo)
        {
            try
            {
                lock (markObj)
                {
                    //数据上传
                    string sn = "";
                    //string sn = UploadSql.GenerateProductCode(context.Process.ProcessNo, LineNo, ProductModel, OP90_x, ProductNo);
                    DisplayLog.LogGenerateProductCode(context.Process.Name, sn);
                    return sn;
                }
            }
            catch (Exception ex)
            {
                DisplayLog.LogGenerateProductCode(context.Process.Name, "", ex);
                return "";
            }
        }

        /// <summary>
        /// 查询点检配置项
        /// </summary>
        /// <param name="inspection"></param>
        /// <param name="station_id"></param>
        /// <returns></returns>
        public static CalibrationOrPeriodInspection SelectCalibrationItem(string inspection, string station_id)
        {
            try
            {
                return AppRuntime.Current.DataRuntime.Repositories.Processes.SelectCalibrationItem(inspection, station_id).GetAwaiter().GetResult();
                //return UploadSql.SelectCalibrationItem(inspection, station_id);
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"查询点检配置项:失败", ex, true);

                return null;
            }
        }

        public static async Task UploadPartNumbersAsync(string barNo, string processNo, params PartInfo[] parts)
        {
            if (string.IsNullOrWhiteSpace(barNo))
                throw new ArgumentNullException(nameof(barNo));
            if (string.IsNullOrWhiteSpace(processNo))
                throw new ArgumentNullException(nameof(processNo));
            if (parts == null || parts.Length == 0)
                throw new ArgumentException("至少需要提供一个零件信息", nameof(parts));

            try
            {
                //var tasks = parts.Select(part => UploadSql.UploadSinglePartAsync(barNo, processNo, part));

                var tasks = parts.Select(part => AppRuntime.Current.DataRuntime.Repositories.Parts.BindPartAsync(barNo, processNo, part.Name, part.BarCode));

                await Task.WhenAll(tasks);
                DisplayLog.Info($"产品码:{barNo}所有零件数据上传成功");
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"产品码:{barNo}上传失败", ex, true);
            }
        }
    }
}
