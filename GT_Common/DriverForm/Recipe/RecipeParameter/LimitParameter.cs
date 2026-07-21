using GT_Common.MyEnum;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RecipeParameter.RecipeParameter
{
    [JsonObject(MemberSerialization.OptIn)]
    //[JsonConverter(typeof(ParameterConverter))]
    public abstract class ParameterBase
    {
        // 参数名
        [DisplayName("参数名")]
        [Description("参数名")]
        [JsonProperty] 
        public string Name { get; set; }

        // 描述
        [DisplayName("描述")]
        [Description("描述")]
        [JsonProperty] 
        public string Description { get; set; }

        [DisplayName("业务主键")]
        [Description("测点唯一标识")]
        [JsonProperty(Required = Required.Always)]
        public string FieldName { get; set; }

        // 单位
        [DisplayName("单位")]
        [Description("单位")]
        [JsonProperty]
        public string Unit { get; set; }

        [JsonProperty]
        [DisplayName("修改权限")]
        [Description("需要的最小角色才能修改")]
        [Browsable(false)]  //  不显示

        public UserLevel EditableRole { get; set; } = UserLevel.ADM;

    }

    public class LimitParameter : ParameterBase
    {
        // 下限
        [DisplayName("下限")]
        [Description("下限")]
        [JsonProperty] 
        public LimitPlcInfo LowerLimit { get; set; }

        // 上限
        [DisplayName("上限")]
        [Description("上限")]
        [JsonProperty] 
        public LimitPlcInfo UpperLimit { get; set; }


        //// 单位
        //[DisplayName("单位")]
        //[Description("单位")]
        //[JsonProperty]
        //public string Unit { get; set; }

        // 上下限校验
        [DisplayName("上下限校验")]
        [Description("上下限校验")]
        [JsonIgnore]
        public bool IsValid
        {
            get
            {
                var low = LowerLimit?.GetNumericValue();
                var high = UpperLimit?.GetNumericValue();
                if (low == null || high == null) return false;
                return low <= high;
            }
        }
    }

    public class BooleanParameter : ParameterBase
    {
        // 值
        [DisplayName("值")]
        [Description("值")]
        [JsonProperty] 
        public bool Value { get; set; }
    }

    public class EnumParameter : ParameterBase
    {
        // 值
        [DisplayName("值")]
        [Description("值")]
        [JsonProperty] 
        public string Value { get; set; }

        // 选项
        [DisplayName("选项")]
        [Description("选项")]
        [JsonProperty] 
        public List<string> Options { get; set; } = new List<string>();
    }

    public class TextParameter : ParameterBase
    {
        // 值
        [DisplayName("值")]
        [Description("值")]
        [JsonProperty] 
        public string Value { get; set; }
    }

    public class IntParameter : ParameterBase
    {
        // 值
        [DisplayName("值")]
        [Description("值")]
        [JsonProperty] 
        public LimitPlcInfo Value { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    [TypeConverter(typeof(LimitPlcInfoTypeConverter))]

    public class LimitPlcInfo
    {
        // 地址
        [DisplayName("地址")]
        [Description("PLC地址")]
        [JsonProperty] 
        public string Address { get; set; }

        // 数据类型
        [DisplayName("数据类型")]
        [Description("数据类型")]
        [JsonProperty]
        [TypeConverter(typeof(ValueTypeConverter))]

        public string ValueType { get; set; }

        // 值
        [DisplayName("值")]
        [Description("值")]
        [JsonProperty]
        [JsonConverter(typeof(PlcValueConverter))]
        [TypeConverter(typeof(PlcValueTypeConverter))]  // 关键

        public object Value { get; set; }

        /// <summary>
        /// 尝试转换为 double，方便做数值比较
        /// </summary>
        public double? GetNumericValue()
        {
            if (Value == null) return null;

            try
            {
                return Convert.ToDouble(Value);
            }
            catch
            {
                return null;
            }
        }

        [JsonIgnore]
        [Browsable(false)]  //  不显示

        public bool IsReadOnly { get; set; } = false;

        public override string ToString() => $"{Address} ({ValueType}) = {Value}";
    }


    public class PlcValueTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string str)
            {
                var info = context?.Instance as LimitPlcInfo;
                switch (info?.ValueType?.ToUpper())
                {
                    case "int":
                        if (int.TryParse(str, out var i)) return i;
                        break;
                    case "DOUBLE":
                        if (double.TryParse(str, out var d)) return d;
                        break;
                    case "float":
                        if (float.TryParse(str, out var f)) return f;
                        break;
                    case "BOOL":
                    case "BOOLEAN":
                        if (bool.TryParse(str, out var b)) return b;
                        break;
                }
                return str; // 默认返回字符串
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return value?.ToString() ?? "";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }


    /// <summary>
    /// 自定义 PLC 值转换器：根据 ValueType 自动处理 JSON 的序列化/反序列化
    /// </summary>
    public class PlcValueConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(object);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // 直接读到 JToken
            var token = JToken.Load(reader);

            if (token.Type == JTokenType.Integer)
            {
                return token.ToObject<long>();
            }
            else if (token.Type == JTokenType.Float)
            {
                return token.ToObject<double>();
            }
            else if (token.Type == JTokenType.Boolean)
            {
                return token.ToObject<bool>();
            }
            else if (token.Type == JTokenType.String)
            {
                return token.ToObject<string>();
            }
            else if (token.Type == JTokenType.Null)
            {
                return null;
            }
            else
            {
                return token.ToString();
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // 直接写入原始值
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                JToken.FromObject(value).WriteTo(writer);
            }
        }
    }

    public class ValueTypeConverter : StringConverter
    {
        // 告诉 PropertyGrid，这个属性支持标准值（下拉）
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;

        // 告诉 PropertyGrid，这个属性只能从候选值里选（true=只读下拉，false=可以手动输入）
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;

        // 提供候选列表
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new[] { "int", "float", "bool", "string" });
        }
    }

    //  动态控制子属性只读
    public class LimitPlcInfoTypeConverter : ExpandableObjectConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var props = base.GetProperties(context, value, attributes);
            var newProps = new List<PropertyDescriptor>();

            if (value is LimitPlcInfo info)
            {
                foreach (PropertyDescriptor pd in props)
                {
                    // 只控制 Address 和 ValueType
                    if (pd.Name == nameof(info.Address) || pd.Name == nameof(info.ValueType))
                    {
                        var newPd = TypeDescriptor.CreateProperty(
                            info.GetType(),
                            pd,
                            new ReadOnlyAttribute(info.IsReadOnly) // 根据父参数权限设置
                        );
                        newProps.Add(newPd);
                    }
                    else
                    {
                        newProps.Add(pd); // 其他属性不变
                    }
                }
            }
            else
            {
                return props;
            }

            return new PropertyDescriptorCollection(newProps.ToArray());
        }
    }


}

