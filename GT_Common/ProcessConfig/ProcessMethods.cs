using GT_Common.Helper;
using HslCommunication;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using GT_Common.Helper.PlcComm;
using System.Text.RegularExpressions;
using GT_Common.Helper.Logging;
using GT_Common.MyEnum;

namespace GT_Common.ProcessConfig
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
            switch (channel.DataType)
            {
                case PlcDataType.Float:
                    return reader.Data.F(channel.Address);
                case PlcDataType.Int:
                    return reader.Data.I(channel.Address);
                case PlcDataType.Short:
                    return reader.Data.S(channel.Address);
                case PlcDataType.UShort:
                    return reader.Data.US(channel.Address);
                case PlcDataType.Bool:
                    return reader.Data.B(channel.Address, channel.Length);
                case PlcDataType.Double:
                    return reader.Data.D(channel.Address) / channel.ScalingFactor;
                case PlcDataType.String:
                    return reader.Data.Str2(channel.Address + 2, channel.Length);
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
        public static string ReadSnCode(StandardProcess e, PatchReader reader)
        {
            string sn = StrHelper.ReplaceIllegalChar(reader.Data.Str2(e.SnConfig.Address + 2, e.SnConfig.Length));
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
        public static List<PartInfo> ReadBinSnCode(StandardProcess e, PatchReader reader, List<PartInfo> partInfos)
        {
            DisplayLog.LogProcessReadBindBar(e.Name);

            foreach (var g in partInfos)
            {
                string sn = StrHelper.ReplaceIllegalChar(ReadChannel(g.PlcChannel, reader).ToString());
                DisplayLog.LogProcessReadBindBar(e.Name, sn);
                sn = sn.Trim('?');
                //DisplayLog.LogProcessReadBindBar(e.Name, sn);
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
        public static int GetNgCode(StandardProcess e, string sn)
        {
            string NgCode = UploadSql.SelectNgCode(sn);
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
        public static string ReadResult(StandardProcess e, PatchReader reader, out string resultCode)
        {
            ushort resultValue = reader.Data.US(e.ResultConfig.Address);
            resultCode = resultValue.ToString();
            DisplayLog.LogProcessReadResult(e.Name, resultCode);
            return resultValue != 0 && resultValue % 10 == 0 ? "OK" : "NG";
        }

        /// <summary>
        /// 读取节拍
        /// </summary>
        /// <param name="e"></param>
        /// <param name="reader"></param>
        /// <param name="resultCode"></param>
        /// <returns></returns>
        public static string ReadTaktTime(StandardProcess e, PatchReader reader)
        {
            // 地址为 0，认为未配置
            if (e.TaktTimeAddress <= 0)
            {
                DisplayLog.Warn($"工序【{e.Name}】未配置 TaktTimeAddress");
                return "0";
            }

            try
            {
                float taktTimeValue = reader.Data.F(e.TaktTimeAddress);

                if (taktTimeValue <= 0 || float.IsNaN(taktTimeValue))
                {
                    DisplayLog.Warn($"工序【{e.Name}】读取到无效 TaktTime: {taktTimeValue}");
                    return "0";
                }

                DisplayLog.LogProcessReadTaktTime(e.Name, taktTimeValue.ToString("F2"));
                return taktTimeValue.ToString("F2");
            }
            catch (Exception ex)
            {
                DisplayLog.Error($"工序【{e.Name}】读取 TaktTime 失败，地址={e.TaktTimeAddress}", ex, true);
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
        public static short ReadCalibrationFlag(StandardProcess e, PatchReader reader)
        {
            short calibrationFlag = reader.Data.S(e.StatusFlagAddress);
            DisplayLog.LogProcessReadCalibrationFlag(e.Name, calibrationFlag.ToString());
            return calibrationFlag;
        }

        /// <summary>
        /// 读取PLC反馈数据
        /// </summary>
        /// <param name="e"></param>
        /// <param name="reader"></param>
        /// <param name="sn"></param>
        /// <returns></returns>
        public static async Task<List<PlcMeasureGroup>> ReadPlcData(StandardProcess e, PatchReader reader, string sn)
        {
            var groups = ReadPlcMeasureGroups(e.PlcReadConfig.MeasureGroups, reader);

            DisplayLog.LogProcessReadPlcData(e.Name, sn, Flatten(groups));

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
        public static DataItem UploadCombinedData(string sn, string result, string resultCode, string[] data_name, string[] data_result, string[] data, string[] data_up, string[] data_down, StandardProcess e, UploadManager uploadManager)
        {
            var item = new DataItem()
            {
                Sn = sn,
                Do_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Data_name = data_name,
                Data_result= data_result,
                Data = data,
                Data_up= data_up,
                Data_down= data_down,
                NgMsg = resultCode,
                Result = result,
                Step = e.PlcReadConfig.CombinedUpload.CurrentStep,
                IsLastStep = e.PlcReadConfig.CombinedUpload.IsFinalStep,
            };
            return uploadManager.Update(item);
        }

        /// <summary>
        /// 文件解析与上传
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sn"></param>
        /// <param name="reader"></param>
        public static List<string> HandleFileUploads(StandardProcess e, string sn, PatchReader reader)
        {
            List<string> lsFilePath = new List<string>();
            string fileSn = null;
            foreach (var item in e.FileConfig.FileItems)
            {
                try
                {
                    fileSn = StrHelper.ReplaceIllegalChar(reader.Data.Str2(item.Address + 2, item.Length));
                    fileSn = fileSn.Trim('?');
                    string ftpPath = CommMethod.FtpFilePath(e.Name, sn, fileSn, item);
                    lsFilePath.Add(ftpPath);
                    UploadSql.UploadProcessFile(e.ProcessNo, sn, item.FileType, item.FileName, ftpPath);
                    DisplayLog.LogProcessFileUpload(e.Name, sn, fileSn);
                }
                catch (Exception ex)
                {
                    DisplayLog.LogProcessFileUpload(e.Name, sn, fileSn, ex);
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
        public static void WritePlcResult(StandardProcess e, PatchReader reader, string sn,int value = 2)
        {
            OperateResult write = reader.Plc.Write(e.ResultWriteConfig.Address, (short)value);
            DisplayLog.WriteIsSucess(write, e.Name, value);
        }

        /// <summary>
        /// 反馈完成信号
        /// </summary>
        /// <param name="e"></param>
        /// <param name="reader"></param>
        public static void WriteTriggerFinish(StandardProcess e, PatchReader reader)
        {
            OperateResult writeResult = reader.Plc.Write(e.FinishFeedbackAddress, (short)2);
            DisplayLog.IsSucess(writeResult, e.Name);
        }

        /// <summary>
        /// 上传点检
        /// </summary>
        /// <param name="e"></param>
        /// <param name="reader"></param>
        public static CalibrationOrPeriodInspection LoadCalibrationOrPeriodInspection(StandardProcess e, PatchReader reader)
        {
            int flag = reader.Data.US(e.StatusFlagAddress);
            
            if (e is CalibrationProcess calibration)
            {
                string CalibrationStationId = calibration.TypeStationIds.Where(t => t.CalibrationType == flag).Select(t => t.CalibrationStationId).FirstOrDefault();
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
                    String sn = UploadSql.GenerateProductCode(context.Process.ProcessNo, LineNo, ProductModel, OP90_x, ProductNo);
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
                return UploadSql.SelectCalibrationItem(inspection, station_id);
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
                var tasks = parts.Select(part => UploadSql.UploadSinglePartAsync(barNo, processNo, part));
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
