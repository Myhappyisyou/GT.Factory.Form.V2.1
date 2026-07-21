using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.MyEnum
{
    //  PLC类型
    public enum PlcType
    {
        SiemensS1500,
        SiemensS1200,
        KeyenceNano
    }

    public enum ProcessType
    {
        /// <summary>
        /// 标准流程
        /// </summary>
        StandardProcess,
        /// <summary>
        /// 过站检流程
        /// </summary>
        CheckResultProcesses,
        /// <summary>
        /// 返工流程
        /// </summary>
        ReworkProcess,
        /// <summary>
        /// 点检流程
        /// </summary>
        CalibrationProcesses,
        /// <summary>
        /// 抽检流程
        /// </summary>
        PeriodProcess,
        /// <summary>
        /// 绑定流程
        /// </summary>
        BarBindProcess,
        /// <summary>
        /// 绑定流程
        /// </summary>
        SaveHMILogProcess
    }

    public enum CheckType
    {
        /// <summary>
        /// 本地查询
        /// </summary>
        local,
        /// <summary>
        /// MES查询
        /// </summary>
        Mes
    }

    public enum SearchBarType
    {
        /// <summary>
        /// 不需要此功能
        /// </summary>
        NonFunctional,
        /// <summary>
        /// 主码
        /// </summary>
        MainBar,
        /// <summary>
        /// 部件码
        /// </summary>
        PartBar,

    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PlcDataType
    {
        Bool,

        Byte,
        Word,
        DWord,

        Short,
        UShort,

        Int,
        UInt,

        DInt,
        UDInt,

        Float,
        Double,

        String
    }

    public enum CalibrationMode
    {
        Normal = 0,     // 正常生产
        Standard = 1,   // 标定
        UpperNg = 2,    // 上限 NG
        LowerNg = 3     // 下限 NG
    }

    public enum IndicatorStatus
    {
        Normal,
        Warning,
        Error,
        Disabled
    }

    public enum ProductStatus
    {
        Standby,    // 待机
        OK,         // 通过
        NG          // 失败
    }

    public enum UserLevel
    {
        None = 0,

        [Description("操作员")]
        OP = 1,

        [Description("设备工程师")]
        ME = 2,

        [Description("质量工程师")]
        QE = 3,

        [Description("工艺工程师")]
        PE = 4,

        [Description("管理员")]
        ADM = 5
    }

    public enum ViewMode
    {
        Engineer,
        Operator
    }

    //  来料类型
    public enum InMetType
    {
        External,   //外部批次
        Inventory   //库存批次
    }

    public static class EnumToObj
    {
       
        public static string GetDisplayText(InMetType inMetType)
        {
            switch (inMetType)
            {
                case InMetType.External:
                    return "外部批次";
                case InMetType.Inventory:
                    return "库存批次";
                default:
                    return "外部批次";
            }
        }
    }


    #region 用户

    public enum LoginMode
    {
        None = 0,
        AccountWithMes,           // 用户名密码 + MES校验
        AccountAndCardWithMes,    // 用户名密码 + 刷卡 + MES校验
        RoleSelect,               // 直接选择角色
        LocalOnly                 // 本地登录
    }

    /// <summary>
    /// 身份识别方式类型
    /// 可扩展（扫码、人脸等）
    /// </summary>
    public enum IdentityType
    {
        None = 0,
        /// <summary>
        /// 账号 + 密码
        /// </summary>
        [Description("账号密码")] 
        AccountPassword = 1,

        /// <summary>
        /// 刷卡登录
        /// </summary>
        [Description("刷卡")]
        Card = 2,

        /// <summary>
        /// 角色直接选择
        /// </summary>
        [Description("角色选择")]
        RoleSelect = 3
    }
    #endregion

    #region 数据库

    /// <summary>
    /// 数据库表名前缀
    /// </summary>
    public enum TablePrefixType
    {
        None = 0,

        /// <summary>
        /// GT 客户
        /// </summary>
        [Description("GT 客户")]
        GT = 1,

        /// <summary>
        /// SH 客户
        /// </summary>
        [Description("SH 客户")]
        SH = 2
    }

    /// <summary>
    /// 数据库类型枚举
    /// 用于标识当前数据库驱动类型
    /// 可扩展（如 PostgreSql）
    /// </summary>
    public enum DatabaseType
    {
        None = 0,

        /// <summary>
        /// Microsoft SQL Server
        /// </summary>
        SqlServer = 1,

        /// <summary>
        /// MySQL
        /// </summary>
        MySql = 2,

        /// <summary>
        /// SQLite（推荐本地缓存使用）
        /// </summary>
        SQLite = 3,

        /// <summary>
        /// Access（不推荐新项目使用）
        /// </summary>
        Access = 4
    }

    #endregion

    #region 数据库表

    /// <summary>
    /// 系统逻辑表名（不带前缀）
    /// </summary>
    public enum LogicalTable
    {
        [Description("ProcessProperty")]
        ProcessProperty,

        [Description("ProcessPropertyParse")]
        PropertyParse,

        [Description("ProcessPart")]
        Part,

        [Description("ProcessAlarm")]
        Alarm,

        [Description("ProcessAlarmParse")]
        AlarmParse,

        [Description("ProcessPeriodTest")]
        Period,

        [Description("ProcessCalibration")]
        Calibration
    }


    #endregion
}
