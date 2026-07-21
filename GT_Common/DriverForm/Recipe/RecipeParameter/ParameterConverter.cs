using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeParameter.RecipeParameter
{
    public class ParameterConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IEnumerable<ParameterBase>).IsAssignableFrom(objectType); // ✅ 支持更多情况
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var array = JArray.Load(reader);
            var result = new List<ParameterBase>();

            foreach (var token in array)
            {
                var jo = (JObject)token;
                var type = jo["Type"]?.ToString();

                ParameterBase obj;
                switch (type)
                {
                    case "Limit":
                        obj = jo.ToObject<LimitParameter>(serializer);
                        break;
                    case "Boolean":
                        obj = jo.ToObject<BooleanParameter>(serializer);
                        break;
                    case "Enum":
                        obj = jo.ToObject<EnumParameter>(serializer);
                        break;
                    case "Text":
                        obj = jo.ToObject<TextParameter>(serializer);
                        break;
                    case "Int":
                        obj = jo.ToObject<IntParameter>(serializer);
                        break;
                    default:
                        obj = jo.ToObject<ParameterBase>(serializer);
                        break;
                }

                result.Add(obj);
            }

            return result;
        }

        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var list = value as IEnumerable<ParameterBase>;
            if (list == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartArray();

            foreach (var item in list)
            {
                var tokenWriter = new JTokenWriter();
                new ParameterConverter().WriteJson(tokenWriter, item, serializer);
                tokenWriter.Token.WriteTo(writer); // ✅ 正确写入
            }

            writer.WriteEndArray();
        }
    }
}