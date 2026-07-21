using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace GT_Common.Helper.LanModelSync
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ProductModel
    {
        public ProductModel()
        {
            BaseInfo = new BaseInfo();
            DetailInfo = new DetailInfo();
        }

        [JsonProperty]
        public BaseInfo BaseInfo { get; set; }

        [JsonProperty]
        public DetailInfo DetailInfo { get; set; }

        [JsonIgnore]
        public string FilePath { get; set; }

        public override string ToString() => BaseInfo?.ProductName ?? "新产品";
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class BaseInfo
    {
        [JsonProperty("ProductCode")]
        public string ProductCode { get; set; }

        [JsonProperty]
        public string ProductName { get; set; }

        [JsonProperty]
        public string ProductNumber { get; set; }

        [JsonProperty]
        public RecipeCode RecipeCode { get; set; }
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(ProductName))
                return ProductName;
            return "基础信息";
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class DetailInfo
    {
        [JsonProperty]
        public GunpowderType Info1 { get; set; }

        [JsonProperty]
        public PipeType Info2 { get; set; }

        [JsonProperty]
        public ProductLength Info3 { get; set; }

        // 客户端无需 UI，ToString 可简单处理
        public override string ToString() => "详细信息";
    }

    public enum RecipeCode
    {
        R1 = 1,
        R2 = 2,
        R3 = 3,
        R4 = 4,
        R5 = 5,
        R6 = 6,
        R7 = 7,
        R8 = 8,
        R9 = 9,
        R10 = 10
    }

    // 火药类型
    public enum GunpowderType
    {
        BK药 = 1,
        BQ药,
    }

    // 管径类型
    public enum PipeType
    {
        大 = 1,
        小,
    }

    public enum ProductLength
    {
        L1 = 1,
        L2 = 2,
        L3 = 3,
        L4 = 4,
        L5 = 5,
        L6 = 6,
        L7 = 7,
        L8 = 8,
        L9 = 9,
        L10 = 10
    }

    // 可选：PLC写入参数使用的字典形式
    public static class DetailInfoHelper
    {
        public static Dictionary<string, string> ToDictionary(DetailInfo detail)
        {
            var dict = new Dictionary<string, string>
            {
                { nameof(detail.Info1), detail.Info1.ToString() },
                { nameof(detail.Info2), detail.Info2.ToString() },
                { nameof(detail.Info3), detail.Info3.ToString() }
            };
            return dict;
        }
    }
}
