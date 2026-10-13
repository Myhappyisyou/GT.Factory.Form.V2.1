using GT.Factory.PLC.Enums;
using GT.Factory.PLC.PlcComm;
using GT.Factory.Station.MyEnum;
using GT.Factory.Station.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading.Tasks;


namespace GT.Factory.Station.ProcessConfig
{

    // ==================== 接口定义 ====================

    #region 接口定义

    /// <summary>
    /// 结果读取能力
    /// </summary>
    public interface IResultReadable
    {
        ResultConfig ResultConfig { get; }
    }

    /// <summary>
    /// PLC 测点读取能力
    /// </summary>
    public interface IPlcReadable
    {
        PlcReadConfig PlcReadConfig { get; }
    }

    /// <summary>
    /// 文件处理能力
    /// </summary>
    public interface IFileSupport
    {
        FileConfig FileConfig { get; }
    }

    /// <summary>
    /// 条码查询类型
    /// </summary>
    public interface IBarSearchable
    {
        SearchBarType SearchBarType { get; }
    }

    /// <summary>
    /// 状态标志位读取能力
    /// </summary>
    public interface IStatusFlagReadable
    {
        string StatusFlagAddress { get; }
    }

    /// <summary>
    /// 节拍读取能力
    /// </summary>
    public interface ITaktTimeReadable
    {
        string TaktTimeAddress { get; }
    }

   
    #endregion

    // ==================== PlcConfig类 ====================


    [JsonObject(MemberSerialization.OptIn)]
    public class PlcConfig : BaseParaFileConfiguration<PlcConfig>, IValidatableConfig
    {

        public PlcConfig()
        {
            FixedInformation = new FixedInformation();
            AlarmConfig = new AlarmConfig();
            EquipmentStata = new EquipmentStata();
            StandardProcess = new List<StandardProcess>();
            CheckResultProcess = new List<CheckResultProcess>();
            ReworkProcess = new List<ReworkProcess>();
            CalibrationProcesses = new List<CalibrationProcess>();
            BarBindProcess = new List<BarBindProcess>();
            PeriodProcesses = new List<PeriodProcess>();
            InstrumentProcesses = new List<InstrumentProcess>();
        }

        // ==================== 基本配置 ====================

        [DisplayName("基本配置"), PropertyOrder(1)]
        [Description("基本配置")]
        [JsonProperty]
        public PlcConnectionConfig PlcConnection { get; set; }


        // ==================== 固定信息 ====================

        #region 固定信息

        //固定信息类
        [DisplayName("固定信息")]
        [Description("固定信息（工单、产量。。。）")]
        [JsonProperty]
        public FixedInformation FixedInformation { get; set; }

        // ==================== 报警信息 ====================

        //报警类
        [DisplayName("报警配置")]
        [Description("报警配置")]
        [JsonProperty]
        public AlarmConfig AlarmConfig { get; set; }

        // ==================== 状态信息 ====================

        //状态类
        [DisplayName("设备状态配置")]
        [Description("设备状态配置")]
        [JsonProperty]
        public EquipmentStata EquipmentStata { get; set; }


        #endregion

        //正常交握流程
        [DisplayName("正常交握流程配置")]
        [Description("正常交握流程配置")]
        [JsonProperty]
        public List<StandardProcess> StandardProcess { get; set; }

        //过站检交握流程
        [DisplayName("过站检交握流程配置")]
        [Description("过站检交握流程配置")]
        [JsonProperty]
        public List<CheckResultProcess> CheckResultProcess { get; set; }

        //返工交握流程
        [DisplayName("返工交握流程配置")]
        [Description("返工交握流程配置")]
        [JsonProperty]
        public List<ReworkProcess> ReworkProcess { get; set; }

        //仪器交握流程
        [DisplayName("仪器交握流程配置")]
        [Description("仪器交握流程配置")]
        [JsonProperty]
        public List<InstrumentProcess> InstrumentProcesses { get; set; }

        //点检交握流程
        [DisplayName("点检交握流程配置")]
        [Description("点检交握流程配置")]
        [JsonProperty]
        public List<CalibrationProcess> CalibrationProcesses { get; set; }

        //绑定交握流程
        [DisplayName("绑定交握流程配置")]
        [Description("绑定交握流程配置")]
        [JsonProperty]
        public List<BarBindProcess> BarBindProcess { get; set; }

        //抽检交握流程
        [DisplayName("抽检交握流程配置")]
        [Description("抽检交握流程配置")]
        [JsonProperty]
        public List<PeriodProcess> PeriodProcesses { get; set; }

        public void Validate()
        {
            
            // ========== 校验 StandardProcess ==========
            if (StandardProcess == null || !StandardProcess.Any())
                throw new InvalidOperationException("至少需要一个标准交握流程配置");

            foreach (var process in StandardProcess)
            {
                if (string.IsNullOrWhiteSpace(process.Name))
                    throw new ArgumentException("标准流程名称不能为空", nameof(process.Name));

                if (process.PlcReadConfig != null)
                {
                    process.PlcReadConfig.Validate();
                }
            }

            // ========== 校验 CheckResultProcess ==========
            if (CheckResultProcess != null)
            {
                foreach (var process in CheckResultProcess)
                {
                    if (string.IsNullOrWhiteSpace(process.Name))
                        throw new ArgumentException("过站检流程名称不能为空");

                    //if (process.TriggerAddress <= 0)
                    //    throw new ArgumentOutOfRangeException("过站检流程触发地址必须大于0");
                }
            }

            // ========== 校验 BarBindProcess ==========
            if (BarBindProcess != null)
            {
                foreach (var process in BarBindProcess)
                {
                    if (string.IsNullOrWhiteSpace(process.Name))
                        throw new ArgumentException("绑定流程名称不能为空");

                    //if (process.TriggerAddress <= 0)
                    //    throw new ArgumentOutOfRangeException("绑定流程触发地址必须大于0");
                }
            }

            // ========== 校验 CalibrationProcesses ==========
            if (CalibrationProcesses != null)
            {
                foreach (var process in CalibrationProcesses)
                {
                    if (string.IsNullOrWhiteSpace(process.Name))
                        throw new ArgumentException("点检流程名称不能为空");

                    //if (process.TriggerAddress <= 0)
                    //    throw new ArgumentOutOfRangeException("点检流程触发地址必须大于0");

                    if (process.PlcReadConfig != null)
                    {
                        process.PlcReadConfig.Validate();
                    }
                }
            }

            // ========== 校验 PeriodProcesses ==========
            if (PeriodProcesses != null)
            {
                foreach (var process in PeriodProcesses)
                {
                    if (string.IsNullOrWhiteSpace(process.Name))
                        throw new ArgumentException("抽检流程名称不能为空");

                    //if (process.TriggerAddress <= 0)
                    //    throw new ArgumentOutOfRangeException("抽检流程触发地址必须大于0");

                    if (process.PlcReadConfig != null)
                    {
                        process.PlcReadConfig.Validate();
                    }
                }
            }

            // ========== 校验 ReworkProcesses（新增） ==========
            if (ReworkProcess != null)
            {
                foreach (var process in ReworkProcess)
                {
                    if (string.IsNullOrWhiteSpace(process.Name))
                        throw new ArgumentException("返工流程名称不能为空");

                    //if (process.TriggerAddress <= 0)
                    //    throw new ArgumentOutOfRangeException("返工流程触发地址必须大于0");
                }
            }

            // ========== 新增：校验 InstrumentProcesses ==========
            if (InstrumentProcesses != null)
            {
                foreach (var process in InstrumentProcesses)
                {
                    if (string.IsNullOrWhiteSpace(process.Name))
                        throw new ArgumentException("仪器通讯流程名称不能为空");

                    if (string.IsNullOrWhiteSpace(process.InstrumentName))
                        throw new ArgumentException($"仪器通讯流程【{process.Name}】的仪器标识不能为空");

                }
            }
        }
    }

    // ==================== 固定信息类 ====================

    #region 固定配置信息

    /// <summary>
    /// 固定配置信息
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(ExpandableObjectConverter))]

    public class FixedInformation
    {
        //订单数量
        [DisplayName("订单数量")]
        [Description("读取订单数量Plc地址")]
        [JsonProperty]
        public string OrderCount { get; set; }

        //总数量
        [DisplayName("总数量")]
        [Description("读取总数量Plc地址")]
        [JsonProperty]
        public string TotalCount { get; set; }

        //完成数量
        [DisplayName("完成数量")]
        [Description("读取完成数量Plc地址")]
        [JsonProperty]
        public string FinishCount { get; set; }

        //合格数量
        [DisplayName("合格数量")]
        [Description("读取合格数量Plc地址")]
        [JsonProperty]
        public string OkNub { get; set; }

        //合格率
        [DisplayName("合格率")]
        [Description("读取合格率Plc地址")]
        [JsonProperty]
        public string Rate { get; set; }

        //当前产品码
        [DisplayName("当前产品码")]
        [Description("读取当前产品码Plc地址")]
        [JsonProperty]
        public string CurrentBarNo { get; set; }

        //当前产品码长度
        [DisplayName("当前产品码长度")]
        [Description("当前产品码长度")]
        [JsonProperty]
        public int CurrentBarNoLength { get; set; }

        //当前产品状态
        [DisplayName("当前产品状态")]
        [Description("读取当前产品状态Plc地址")]
        [JsonProperty]
        public string CurrentProductStatus { get; set; }

        //操作提示
        [DisplayName("操作提示")]
        [Description("读取操作提示Plc地址")]
        [JsonProperty]
        public string OperateTips { get; set; }

        //产品名称
        [DisplayName("产品名称")]
        [Description("写入产品名称Plc地址")]
        [JsonProperty]
        public string ProductName { get; set; }

        //产品代码
        [DisplayName("产品代码")]
        [Description("写入产品代码Plc地址")]
        [JsonProperty]
        public string ProductCode { get; set; }

        //员工号
        [DisplayName("员工号")]
        [Description("写入员工号Plc地址")]
        [JsonProperty]
        public string JobNub { get; set; }

        //权限等级
        [DisplayName("权限等级")]
        [Description("写入权限等级Plc地址")]
        [JsonProperty]
        public string LevelEnum { get; set; }

        //设备时间
        [DisplayName("设备时间")]
        [Description("设备时间")]
        [JsonProperty]
        public string TimeToPlc { get; set; }

        //设备时间
        [DisplayName("用户名")]
        [Description("写入用户名Plc地址")]
        [JsonProperty]
        public string UserName { get; set; }

        // 可选：重写 ToString() 显示摘要
        public override string ToString() => $"固定配置信息";
    }

    /// <summary>
    /// 报警配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class AlarmConfig
    {
        //是否有次配置标志
        [DisplayName("是否启用")]
        [Description("是否启用读取设备报警")]
        [JsonProperty]
        public bool IsNeed { get; set; }

        //报警起始地址
        [DisplayName("报警起始地址")]
        [Description("报警起始地址")]
        [JsonProperty]
        public string AlarmStartAddress { get; set; }

        //报警地址长度
        [DisplayName("报警地址长度")]
        [Description("读取报警地址长度")]
        [JsonProperty]
        public ushort AlarmLength { get; set; }

        //报警工序号
        [DisplayName("报警工序号")]
        [Description("报警工序号")]
        [JsonProperty]
        public string AlarmProcess { get; set; }

        // 可选：重写 ToString() 显示摘要
        public override string ToString() => $"报警配置 (启用: {IsNeed})";
    }

    /// <summary>
    /// 设备状态
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class EquipmentStata
    {
        //是否有次配置标志
        [DisplayName("是否启用")]
        [Description("读取设备状态是否启用")]
        [JsonProperty]
        public bool IsNeed { get; set; }

        //设备状态地址
        [DisplayName("设备状态地址")]
        [Description("读取设备状态是否启用")]
        [JsonProperty]
        public string EquipmentStataAddress { get; set; }

        //设备ID
        [DisplayName("设备工序号")]
        [Description("设备工序号")]
        [JsonProperty]
        public string EquipmentId { get; set; }

        // 可选：重写 ToString() 显示摘要
        public override string ToString() => $"设备状态配置 (启用: {IsNeed})";
    }


    #endregion


    // ==================== 流程基类 ====================

    #region 流程基类
    /// <summary>
    /// 流程基类(抽象)
    /// </summary>
    public abstract class ProcessBase
    {
        //交握流程注释
        [DisplayName("交握流程名称")]
        [Description("交握流程名称")]
        [JsonProperty]
        public string Name { get; set; }

        //交握流程名
        [DisplayName("交握流程标识名")]
        [Description("交握流程标识名，不能重复")]
        [JsonProperty]
        public string SignalName { get; set; }

        //触发地址
        [DisplayName("交握流程触发地址")]
        [Description("交握流程触发地址")]
        [JsonProperty]
        public string TriggerAddress { get; set; }

        // SN码读取配置
        [DisplayName("SN码读取配置")]
        [Description("SN码读取配置")]
        [JsonProperty]
        public SnConfig SnConfig { get; set; }        // SN码读取配置

        // 结果回写配置
        [DisplayName("结果回写配置")]
        [Description("结果回写配置")]
        [JsonProperty]
        public ResultWriteConfig ResultWriteConfig { get; set; } // 结果回写配置

        //参数名称
        [DisplayName("匹配配方组名称")]
        [Description("匹配配方组名称")]
        [JsonProperty]
        public string ParameterGroupName { get; set; }

        //工序号
        [DisplayName("工序号")]
        [Description("工序号")]
        [JsonProperty]
        public string ProcessNo { get; set; }

        //完成反馈地址
        [DisplayName("完成反馈地址")]
        [Description("完成反馈地址")]
        [JsonProperty]
        public string FinishFeedbackAddress { get; set; }

        //流程类型
        [DisplayName("流程类型")]
        [Description("流程类型")]
        [JsonProperty]
        public ProcessType ProcessType { get; set; }

        /// <summary>
        /// 验证流程基础配置
        /// </summary>
        public virtual void Validate()
        {
            if (string.IsNullOrWhiteSpace(SignalName))
                throw new ArgumentException("流程名称不能为空");

            // ========== 修改：TriggerAddress 是 string，先验证格式再验证值 ==========
            if (string.IsNullOrWhiteSpace(TriggerAddress))
                throw new ArgumentException("触发地址不能为空");

            if (!PlcAddressHelper.TryToReadAddress(TriggerAddress, out int triggerValue))
                throw new ArgumentException($"触发地址格式不正确: {TriggerAddress}");

            if (triggerValue <= 0)
                throw new ArgumentOutOfRangeException(nameof(TriggerAddress), "触发地址必须大于0");
        }
    }
    #endregion


    // ==================== 7大流程配置 ====================

    #region 流程配置

    /// <summary>
    /// 标准流程配置
    /// </summary>
    public class StandardProcess : ProcessBase, IResultReadable, IPlcReadable, IFileSupport, IBarSearchable, IStatusFlagReadable, ITaktTimeReadable
    {
        // 状态标志位地址
        [DisplayName("状态标志位地址")]
        [Description("状态标志位地址")]
        [JsonProperty]
        public string StatusFlagAddress { get; set; }     // 状态标志位地址

        // 节拍
        [DisplayName("节拍")]
        [Description("节拍地址")]
        [JsonProperty]
        public string TaktTimeAddress { get; set; }     // 节拍

        // 查询码类型
        [DisplayName("查询码类型")]
        [Description("查询码类型")]
        [JsonProperty]
        public SearchBarType SearchBarType { get; set; }   // 是否需要主码


        // 治具配置
        [DisplayName("治具配置")]
        [Description("治具配置")]
        [JsonProperty]
        public FixtureConfig FixtureConfig { get; set; } // 治具配置

        // 结果配置
        [DisplayName("结果配置")]
        [Description("结果配置")]
        [JsonProperty]
        public ResultConfig ResultConfig { get; set; } // 结果配置

        // PLC读取配置
        [DisplayName("PLC读取配置")]
        [Description("PLC读取配置")]
        [JsonProperty]
        public PlcReadConfig PlcReadConfig { get; set; } // PLC读取配置

        // PLC写入配置
        [DisplayName("PLC写入配置")]
        [Description("PLC写入配置")]
        [JsonProperty]
        public PlcWriteConfig PlcWriteConfig { get; set; } // PLC写入配置


        // 文件配置
        [DisplayName("文件配置")]
        [Description("文件配置")]
        [JsonProperty]
        public FileConfig FileConfig { get; set; }    // 文件配置



        public override void Validate()
        {
            base.Validate();
            // 标准流程特有验证逻辑
            if (ResultWriteConfig?.IsEnabled == true && string.IsNullOrEmpty(ResultWriteConfig.Address))
                throw new ArgumentException("结果回写地址不能为空");
        }
    }

    /// <summary>
    /// 过站检流程配置
    /// </summary>
    public class CheckResultProcess : ProcessBase
    {
        // 过站检测信息
        [DisplayName("过站检信息")]
        [Description("过站检信息")]
        [JsonProperty]
        public CheckResultInfo CheckResultInfo { get; set; }

        // PLC写入配置
        [DisplayName("PLC写入配置")]
        [Description("PLC写入配置")]
        [JsonProperty]
        public PlcWriteConfig PlcWriteConfig { get; set; } // PLC写入配置
    }

    /// <summary>
    /// 返工流程配置
    /// </summary>
    public class ReworkProcess : ProcessBase
    {
        // PLC写入配置
        [DisplayName("PLC写入配置")]
        [Description("PLC写入配置")]
        [JsonProperty]
        public PlcWriteConfig PlcWriteConfig { get; set; } // PLC写入配置
    }

    /// <summary>
    /// 点检流程配置
    /// </summary>
    public class CalibrationProcess : ProcessBase, IResultReadable, IPlcReadable, IFileSupport, IStatusFlagReadable
    {
        // 状态标志位地址
        [DisplayName("状态标志位地址")]
        [Description("状态标志位地址")]
        [JsonProperty]
        public string StatusFlagAddress { get; set; }     // 状态标志位地址

        [DisplayName("点检类型")]
        [Description("点检类型")]
        [JsonProperty]
        public List<TypeStationId> TypeStationIds { get; set; }

        // 结果配置
        [DisplayName("结果配置")]
        [Description("结果配置")]
        [JsonProperty]
        public ResultConfig ResultConfig { get; set; } // 结果配置

        // PLC读取配置
        [DisplayName("PLC读取配置")]
        [Description("PLC读取配置")]
        [JsonProperty]
        public PlcReadConfig PlcReadConfig { get; set; } // PLC读取配置

        // 文件配置
        [DisplayName("文件配置")]
        [Description("文件配置")]
        [JsonProperty]
        public FileConfig FileConfig { get; set; }    // 文件配置
    }

    /// <summary>
    /// 抽检流程配置
    /// </summary>
    public class PeriodProcess : ProcessBase, IResultReadable, IPlcReadable, IFileSupport, IStatusFlagReadable
    {
        // 状态标志位地址
        [DisplayName("状态标志位地址")]
        [Description("状态标志位地址")]
        [JsonProperty]
        public string StatusFlagAddress { get; set; }     // 状态标志位地址

        [DisplayName("点检类型")]
        [Description("点检类型")]
        [JsonProperty]
        public List<TypeStationId> TypeStationIds { get; set; }

        // 结果配置
        [DisplayName("结果配置")]
        [Description("结果配置")]
        [JsonProperty]
        public ResultConfig ResultConfig { get; set; } // 结果配置

        // PLC读取配置
        [DisplayName("PLC读取配置")]
        [Description("PLC读取配置")]
        [JsonProperty]
        public PlcReadConfig PlcReadConfig { get; set; } // PLC读取配置

        // 文件配置
        [DisplayName("文件配置")]
        [Description("文件配置")]
        [JsonProperty]
        public FileConfig FileConfig { get; set; }    // 文件配置
    }


    /// <summary>
    /// 绑定流程配置
    /// </summary>
    public class BarBindProcess : ProcessBase
    {
        // 绑定部件码信息
        [DisplayName("绑定部件码信息")]
        [Description("绑定部件码信息")]
        [JsonProperty]
        public List<PartInfo> PartInfos { get; set; }
    }


    /// <summary>
    /// 仪器通讯流程配置
    /// 职责：PLC触发后与仪器通讯，将仪器数据/结果写回PLC
    /// 数据上传、结果判断由后续 StandardProcess 负责
    /// </summary>
    public class InstrumentProcess : ProcessBase
    {
        /// <summary>
        /// 仪器标识
        /// 系统根据此 Key 在注册表中匹配对应的 IInstrumentHandler
        /// 例：HeliumTester / TorqueTester / VisionCamera
        /// </summary>
        [DisplayName("仪器标识")]
        [Description("唯一标识，系统根据此标识匹配对应仪器处理器，需与代码注册的Name一致")]
        [JsonProperty]
        public string InstrumentName { get; set; }

        [DisplayName("缓存上传配置")]
        [Description("缓存上传配置")]
        [JsonProperty]
        public CombinedUpload CombinedUpload { get; set; } = new CombinedUpload();

        // 把仪器数据写回 PLC
        [DisplayName("PLC写入配置")]
        [Description("仪器数据写回PLC的地址配置")]
        [JsonProperty]
        public PlcWriteConfig PlcWriteConfig { get; set; }

        public override void Validate()
        {
            base.Validate();

            if (string.IsNullOrWhiteSpace(InstrumentName))
                throw new ArgumentException("仪器驱动标识(DriverKey)不能为空");

            if (PlcWriteConfig?.IsEnabled == true &&
                (PlcWriteConfig.WriteItems == null || PlcWriteConfig.WriteItems.Count == 0))
                throw new ArgumentException("仪器流程启用了PLC写入，但未配置写入项");
        }
    }



    #endregion

    // ==================== 流程内容配置 ====================

    #region 流程内容配置

    /// <summary>
    /// SN码读取配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class SnConfig
    {
        // 是否启用
        [DisplayName("是否启用")]
        [Description("产品码读取是否启用")]
        [JsonProperty]
        public bool IsEnabled { get; set; }         // 是否启用

        // SN码地址
        [DisplayName("产品码地址")]
        [Description("产品码PLC地址")]
        [JsonProperty]
        public string Address { get; set; }            // SN码地址

        // 数据长度
        [DisplayName("产品码长度")]
        [Description("产品码长度（长度为奇数的要加1补成偶数）")]
        [JsonProperty]
        public int Length { get; set; }             // 数据长度

        public override string ToString() => $"SN码读取配置 (启用: {IsEnabled})";// 可选：重写 ToString() 显示摘要
    }

    /// <summary>
    /// PLC读取配置（改造后，按测点组织）
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class PlcReadConfig
    {
        [DisplayName("是否启用")]
        [Description("PLC读取配置是否启用")]
        [JsonProperty]
        public bool IsEnabled { get; set; }

        [DisplayName("缓存上传配置")]
        [Description("缓存上传配置")]
        [JsonProperty]
        public CombinedUpload CombinedUpload { get; set; }   

        [DisplayName("业务测点配置列表")]
        [Description("每个测点包含上限、下限、实际值、结果")]
        [JsonProperty]
        public List<PlcMeasureGroup> MeasureGroups { get; set; } = new List<PlcMeasureGroup>();

        /// <summary>
        /// 验证PLC读取配置
        /// </summary>
        public void Validate()
        {
            if (!IsEnabled)
                return;

            if (MeasureGroups == null || MeasureGroups.Count == 0)
                throw new InvalidOperationException("启用了PLC读取配置但未配置任何测点");

            // 检查 FieldName 唯一性
            var duplicateFieldNames = MeasureGroups
                .GroupBy(g => g.FieldName)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateFieldNames.Any())
                throw new InvalidOperationException($"测点中存在重复的业务主键(FieldName): {string.Join(", ", duplicateFieldNames)}");

            for (int i = 0; i < MeasureGroups.Count; i++)
            {
                MeasureGroups[i].Validate(i);
            }
        }

        public override string ToString() => $"PLC读取配置 (启用: {IsEnabled}, 测点数量: {MeasureGroups.Count})";
    }

    /// <summary>
    /// 一个业务测点
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(OrderedExpandableObjectConverter))]
    public class PlcMeasureGroup : IDataErrorInfo
    {
        public PlcMeasureGroup()
        {
            Upper = new PlcChannel();
            Lower = new PlcChannel();
            Result = new PlcChannel();
            Value = new PlcChannel();
        }

        [PropertyOrder(1)]
        [DisplayName("业务主键")]
        [Description("测点唯一标识")]
        [JsonProperty]
        public string FieldName { get; set; }   // 比如 "BeforeWeight"

        [PropertyOrder(2)]
        [DisplayName("显示名称")]
        [Description("测点显示名称")]
        [JsonProperty]
        public string Name { get; set; }        // 比如 "氦检前产品重量"

        [PropertyOrder(3)]
        [DisplayName("单位")]
        [Description("单位")]
        [JsonProperty]
        public string Unit { get; set; } = "";

        [PropertyOrder(4)]
        [DisplayName("上限")]
        [Description("上限通道")]
        [JsonProperty]
        public PlcChannel Upper { get; set; }

        [PropertyOrder(5)]
        [DisplayName("下限")]
        [Description("下限通道")]
        [JsonProperty]
        public PlcChannel Lower { get; set; }

        [PropertyOrder(6)]
        [DisplayName("测试结果")]
        [Description("结果通道")]
        [JsonProperty]
        public PlcChannel Result { get; set; }

        [PropertyOrder(7)]
        [DisplayName("实际值")]
        [Description("实际值通道")]
        [JsonProperty]
        [Required(ErrorMessage = "实际值通道必须配置")]
        [ValidPlcChannel(ErrorMessage = "实际值通道配置无效")]
        public PlcChannel Value { get; set; }

        // IDataErrorInfo 实现
        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(FieldName) && string.IsNullOrWhiteSpace(FieldName))
                    return "业务主键不能为空";

                if (columnName == nameof(Name) && string.IsNullOrWhiteSpace(Name))
                    return "显示名称不能为空";

                if (columnName == nameof(Value) && (Value == null || !Value.IsConfigured()))
                    return "实际值通道必须配置";

                return null;
            }
        }

        /// <summary>
        /// 验证测点配置
        /// </summary>
        /// <param name="index">测点索引（用于错误信息定位）</param>
        public void Validate(int index = -1)
        {
            string prefix = index >= 0 ? $"第 {index + 1} 个测点" : "测点";

            if (string.IsNullOrWhiteSpace(FieldName))
                throw new ArgumentException($"{prefix}的业务主键(FieldName)不能为空");

            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentException($"{prefix}的显示名称(Name)不能为空");

            // 校验上限通道
            if (Upper != null && Upper.IsConfigured())
                Upper.Validate($"{prefix}的上限通道");

            // 校验下限通道
            if (Lower != null && Lower.IsConfigured())
                Lower.Validate($"{prefix}的下限通道");

            // 校验结果通道
            if (Result != null && Result.IsConfigured())
                Result.Validate($"{prefix}的结果通道");

            // 校验实际值通道（通常必须配置）
            if (Value == null || !Value.IsConfigured())
                throw new ArgumentException($"{prefix}的实际值通道(Value)必须配置");

            Value.Validate($"{prefix}的实际值通道");

            // 可选：验证上下限的合理性（如果有配置）
            if (Upper?.IsConfigured() == true && Lower?.IsConfigured() == true)
            {
                // 注意：由于地址可能是不同位置的PLC地址，这里只做配置完整性检查
                // 实际的数值大小比较需要在运行时根据读取的值进行
            }
        }
    }

    /// <summary>
    /// 单条PLC通道配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class PlcChannel : IDataErrorInfo
    {
        [DisplayName("PLC地址")]
        [Description("PLC数据地址")]
        [JsonProperty]
        public string Address { get; set; }

        [DisplayName("读取长度")]
        [Description("PLC读取长度")]
        [JsonProperty]
        public int Length { get; set; }

        [DisplayName("数据类型")]
        [Description("PLC数据类型")]
        [JsonProperty]
        public PlcDataType DataType { get; set; }

        [DisplayName("缩放系数")]
        [Description("缩放系数")]
        [JsonProperty]
        public double ScalingFactor { get; set; } = 1.0;

        // 新增：读取结果
        [JsonIgnore] // 不序列化到配置文件
        public object Value { get; set; }

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                if (!IsConfigured()) return null;

                if (columnName == nameof(Address)
                && string.IsNullOrWhiteSpace(Address))
                    return "PLC地址不能为空";

                if (columnName == nameof(Length) && Length <= 0)
                    return "读取长度必须大于0";

                if (columnName == nameof(DataType) && DataType < 0)
                    return "数据类型不能为空";

                if (columnName == nameof(ScalingFactor) && ScalingFactor <= 0)
                    return "缩放系数必须大于0";

                return null;
            }
        }

        /// <summary>
        /// 判断通道是否有有效配置
        /// </summary>
        public bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(Address) && Length > 0 && DataType > 0;
        }

        /// <summary>
        /// 验证通道配置
        /// </summary>
        /// <param name="channelName">通道名称（用于错误信息）</param>
        public void Validate(string channelName)
        {
            if (string.IsNullOrWhiteSpace(Address))
                throw new ArgumentOutOfRangeException($"{channelName}的PLC地址", Address, "PLC地址不能为负数");

            if (Length <= 0)
                throw new ArgumentException($"{channelName}的读取长度必须大于0，当前值: {Length}");

            if (DataType < 0)
                throw new ArgumentException($"{channelName}的数据类型不能为空");

            // 验证数据类型是否支持
            //var supportedTypes = new[] { "BOOL", "BYTE", "WORD", "DWORD", "SHORT", "INT", "UINT", "DINT", "UDINT", "FLOAT", "REAL", "LREAL", "STRING" };
            //if (!supportedTypes.Contains(DataType.ToUpperInvariant()))
            //    throw new NotSupportedException($"{channelName}的数据类型 '{DataType}' 不支持，支持的类型: {string.Join(", ", supportedTypes)}");

            if (ScalingFactor <= 0)
                throw new ArgumentOutOfRangeException($"{channelName}的缩放系数", ScalingFactor, "缩放系数必须大于0");
        }

        public override string ToString()
        {
            return $"地址:{Address}, 读取长度:{Length}, 数据类型:{DataType}";
        }
    }

    /// <summary>
    /// 批量上传配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(ExpandableObjectConverter))]

    public class CombinedUpload
    {
        // 是否启用
        [DisplayName("是否启用")]
        [Description("批量上传配置是否启用")]
        [JsonProperty]
        public bool IsEnabled { get; set; }         // 是否启用

        // 当前步数
        [DisplayName("当前步数")]
        [Description("当前上传数据是第几步数")]
        [JsonProperty]
        public int CurrentStep { get; set; }         // 总步数

        // 是否最后一步
        [DisplayName("是否最后一步")]
        [Description("当前上传数据是否是最后一步")]
        [JsonProperty]
        public bool IsFinalStep { get; set; }       // 是否最后一步
        public override string ToString() => $"批量上传配置 (启用: {IsEnabled})";   // 可选：重写 ToString() 显示摘要
    }

    /// <summary>
    /// 治具配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class FixtureConfig
    {
        // 是否启用
        [DisplayName("是否启用")]
        [Description("治具配置是否启用")]
        [JsonProperty]
        public bool IsEnabled { get; set; }         // 是否启用

        // 治具号地址
        [DisplayName("治具号PLC地址")]
        [Description("治具号PLC地址")]
        [JsonProperty]
        public string Address { get; set; }            // 治具号地址

        // 数据长度
        [DisplayName("读取长度")]
        [Description("读取PLC长度")]
        [JsonProperty]
        public int Length { get; set; }             // 数据长度

        public override string ToString() => $"治具配置 (启用: {IsEnabled})";   // 可选：重写 ToString() 显示摘要
    }

    
    /// <summary>
    /// 结果配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ResultConfig
    {
        // 是否启用
        [DisplayName("是否启用")]
        [Description("结果配置是否启用")]
        [JsonProperty]
        public bool IsEnabled { get; set; }         // 是否启用

        // 是否启用
        [DisplayName("结果地址")]
        [Description("结果PLC地址")]
        [JsonProperty]
        public string Address { get; set; }            // 结果地址

        // 是否启用
        [DisplayName("合格工艺代码")]
        [Description("合格工艺代码")]
        [JsonProperty]
        public int SuccessFlag { get; set; }        // 成功标志值
        public override string ToString() => $"结果配置 (启用: {IsEnabled})";// 可选：重写 ToString() 显示摘要
    }

    /// <summary>
    /// 文件配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class FileConfig
    {
        // 是否启用
        [DisplayName("是否启用")]
        [Description("文件配置是否启用")]
        [JsonProperty]
        public bool IsEnabled { get; set; }         // 是否启用

        // 文件项列表
        [DisplayName("文件项列表")]
        [Description("文件项列表")]
        [JsonProperty]
        public List<FileItemConfig> FileItems { get; set; } = new List<FileItemConfig>(); // 文件项列表

        public override string ToString() => $"文件配置 (启用: {IsEnabled})";// 可选：重写 ToString() 显示摘要
    }

    /// <summary>
    /// 文件项配置
    /// </summary>
    public class FileItemConfig
    {
        
        // 文件信息地址
        [DisplayName("文件码地址")]
        [Description("文件码PLC地址")]
        [JsonProperty]
        public string Address { get; set; }            // 文件信息地址

        // 数据长度
        [DisplayName("文件码长度")]
        [Description("文件码长度")]
        [JsonProperty]
        public int Length { get; set; }             // 数据长度

        // 文件类型
        [DisplayName("文件类型")]
        [Description("文件类型")]
        [JsonProperty]
        public string FileType { get; set; }        // 文件类型

        // 文件扩展名
        [DisplayName("文件扩展名")]
        [Description("文件扩展名")]
        [JsonProperty]
        public string FileExtension { get; set; }   // 文件扩展名

        // 文件名
        [DisplayName("文件名")]
        [Description("文件名")]
        [JsonProperty]
        public string FileName { get; set; }        // 文件名

        // 文件夹名
        [DisplayName("文件夹名")]
        [Description("文件夹名")]
        [JsonProperty]
        public string FolderName { get; set; }     // 文件夹名

        // 相机名称
        [DisplayName("相机名称")]
        [Description("相机名称")]
        [JsonProperty]
        public string CameraName { get; set; }      // 相机名称

        // 相机类型
        [DisplayName("相机类型")]
        [Description("相机类型")]
        [JsonProperty]
        public int CameraType { get; set; }         // 相机类型


        [DisplayName("测试项名称")]
        [JsonProperty]
        public string Name { get; set; }    // 相机类型

        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(Name) && string.IsNullOrWhiteSpace(Name))
                    return "显示名称不能为空";


                return null;
            }
        }
    }

    /// <summary>
    /// 结果回写配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ResultWriteConfig
    {
        // 是否启用
        [DisplayName("是否启用")]
        [Description("结果回写配置是否启用")]
        [JsonProperty]
        public bool IsEnabled { get; set; }         // 是否启用

        // 回写地址
        [DisplayName("回写地址")]
        [Description("回写PLC地址")]
        [JsonProperty]
        public string Address { get; set; }         // 回写地址
        public override string ToString() => $"结果回写配置 (启用: {IsEnabled})";// 可选：重写 ToString() 显示摘要
    }

    /// <summary>
    /// PLC写入配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class PlcWriteConfig
    {
        // 是否启用
        [DisplayName("是否启用")]
        [Description("PLC写入配置是否启用")]
        [JsonProperty]
        public bool IsEnabled { get; set; }        // 是否启用

        // 写入项列表
        [DisplayName("写入项列表")]
        [Description("写入项列表")]
        [JsonProperty]
        public List<WriteItemConfig> WriteItems { get; set; } // 写入项列表
        public override string ToString() => $"PLC写入配置 (启用: {IsEnabled})";// 可选：重写 ToString() 显示摘要
    }

    // <summary>
    /// 写入项配置
    /// </summary>
    public class WriteItemConfig
    {
        // 写入项名称
        [DisplayName("写入项名称")]
        [Description("写入项名称")]
        [JsonProperty]
        public string Name { get; set; }           // 写入项名称

        // 写入地址
        [DisplayName("写入地址")]
        [Description("写入PLC地址")]
        [JsonProperty]
        public string Address { get; set; }         // 写入地址

        // 数据长度
        [DisplayName("数据长度")]
        [Description("数据长度")]
        [JsonProperty]
        public int Length { get; set; }            // 数据长度

        // 数据类型
        [DisplayName("数据类型")]
        [Description("数据类型(INT/FLOAT等)")]
        [JsonProperty]
        public PlcDataType DataType { get; set; }       // 数据类型(INT/FLOAT等)
    }

    /// <summary>
    /// 过站检信息
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class CheckResultInfo
    {
        // 过站检类别
        [DisplayName("过站检类别")]
        [Description("过站检类别")]
        [JsonProperty]
        public CheckType CheckType { get; set; }

        // 过站检检查第几步
        [DisplayName("过站检步骤")]
        [Description("过站检检查第几步")]
        [JsonProperty]
        public int Step { get; set; }

        // 可选：重写 ToString() 显示摘要
        public override string ToString() => $"过站检信息";
    }

    /// <summary>
    /// 绑定码信息
    /// </summary>
    public class PartInfo
    {
        // 部件码名称
        [DisplayName("部件码名称")]
        [Description("部件码名称")]
        [JsonProperty]
        public string Name { get; set; }

        // 部件码名称
        [DisplayName("部件码")]
        [Description("部件码")]
        [JsonProperty]
        public string BarCode { get; set; }
        // 部件码名称
        [DisplayName("产品码地址")]
        [Description("产品码地址")]
        [JsonProperty]
        public PlcChannel PlcChannel { get; set; }
    }

    
    /// <summary>
    /// 抽检点检配置
    /// </summary>
    public class TypeStationId
    {
        [DisplayName("点检名")]
        [Description("点检名")]
        [JsonProperty]
        public string Name { get; set; }           // 数据项名称

        [DisplayName("点检类别")]
        [Description("点检类别")]
        [JsonProperty]
        public int CalibrationType { get; set; }

        [DisplayName("工位号")]
        [Description("工位号")]
        [JsonProperty]
        public string CalibrationStationId { get; set; }

        public override string ToString() => $"{Name}点检";
    }

    public class ProcessCollectionEditor : CollectionEditor
    {
        public ProcessCollectionEditor(Type type) : base(type) { }

        protected override object CreateInstance(Type itemType)
        {
            return new StandardProcess(); // 或其他派生类
        }
    }

    #endregion


    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyOrderAttribute : Attribute
    {
        public int Order { get; }
        public PropertyOrderAttribute(int order) => Order = order;
    }

    public class OrderedExpandableObjectConverter : ExpandableObjectConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var props = TypeDescriptor.GetProperties(value, attributes).Cast<PropertyDescriptor>();
            var sorted = props.OrderBy(p =>
            {
                var attr = (PropertyOrderAttribute)p.Attributes[typeof(PropertyOrderAttribute)];
                return attr?.Order ?? int.MaxValue;
            }).ToArray();
            return new PropertyDescriptorCollection(sorted);
        }
    }

    /// <summary>
    /// 自定义验证特性
    /// </summary>
    public class ValidPlcChannelAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var channel = value as PlcChannel;
            if (channel == null) return ValidationResult.Success;

            // 如果通道没有任何配置，跳过验证
            if (!channel.IsConfigured()) return ValidationResult.Success;


            if (channel.Length <= 0)
                return new ValidationResult($"读取长度必须大于0，当前值: {channel.Length}");

            //if (string.IsNullOrWhiteSpace(channel.DataType))
            //    return new ValidationResult("数据类型不能为空");

            //var supportedTypes = new[] { "BOOL", "BYTE", "WORD", "DWORD", "INT", "UINT", "DINT", "UDINT", "REAL", "LREAL", "STRING" };
            //if (!supportedTypes.Contains(channel.DataType.ToUpperInvariant()))
            //    return new ValidationResult($"不支持的数据类型: {channel.DataType}");

            if (channel.ScalingFactor <= 0)
                return new ValidationResult($"缩放系数必须大于0，当前值: {channel.ScalingFactor}");

            return ValidationResult.Success;
        }
    }
}

