using GT_Common.Helper.PlcComm;
using RecipeParameter.RecipeParameter;
using System.Collections.Generic;
using static GT_Common.UploadSql;

namespace GT_Common.ProcessConfig
{
    public class ProcessContext
    {
        public StandardProcess Process { get; set; }
        public PatchReader Reader { get; set; }
        public LimitConfig LimitConfig { get; set; }
        public List<ResultCodeParse> LsResultCodeParses { get; set; }
        public string Sn { get; set; }

        public string MainBar { get; set; }

        public string PartBar { get; set; }

        public string Result { get; set; }
        public string ResultCode { get; set; }
        public string MesResult { get; set; }
        public string TaktTime { get; set; }
        public string MesOrder { get; set; }

        public int WritePlcResult { get; set; }
        public List<PlcMeasureGroup> Data { get; set; } = new List<PlcMeasureGroup>();
        public List<PartInfo> PartInfos { get; set; } = new List<PartInfo>();
        public CalibrationOrPeriodInspection CalibrationOrPeriodInspection { get; set; }

        public List<TypeStationId> calibrationType { get; set; } = new List<TypeStationId>();

        public int CalibrationFlag { get; set; }
        public Dictionary<string, object> CustomProperties { get; } = new Dictionary<string, object>();

        public T GetProperty<T>(string key, T defaultValue = default) =>
            CustomProperties.TryGetValue(key, out var value) ? (T)value : defaultValue;
    }
}
