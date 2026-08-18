using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RecipeParameter.RecipeParameter
{
    public class SingleParameterConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(ParameterBase).IsAssignableFrom(objectType);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jo = new JObject();

            if (value is LimitParameter limit)
            {
                jo["Type"] = "Limit";
                jo["Name"] = limit.Name;
                jo["Description"] = limit.Description;
                jo["LowerLimit"] = JToken.FromObject(limit.LowerLimit, serializer);
                jo["UpperLimit"] = JToken.FromObject(limit.UpperLimit, serializer);
                jo["Unit"] = limit.Unit;
            }
            else if (value is BooleanParameter b)
            {
                jo["Type"] = "Boolean";
                jo["Name"] = b.Name;
                jo["Description"] = b.Description;
                jo["Value"] = b.Value;
            }
            else if (value is EnumParameter e)
            {
                jo["Type"] = "Enum";
                jo["Name"] = e.Name;
                jo["Description"] = e.Description;
                jo["Value"] = e.Value;
                jo["Options"] = JArray.FromObject(e.Options);
            }
            else if (value is TextParameter t)
            {
                jo["Type"] = "Text";
                jo["Name"] = t.Name;
                jo["Description"] = t.Description;
                jo["Value"] = t.Value;
            }
            else if (value is IntParameter s)
            {
                jo["Type"] = "Int";
                jo["Name"] = s.Name;
                jo["Description"] = s.Description;
                jo["Value"] = JToken.FromObject(s.Value, serializer);
            }

            jo.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            string type = jo["Type"]?.ToString();

            if (type == "Limit")
            {
                return new LimitParameter
                {
                    Name = jo["Name"]?.ToString(),
                    Description = jo["Description"]?.ToString(),
                    LowerLimit = jo["LowerLimit"]?.ToObject<LimitPlcInfo>(serializer),
                    UpperLimit = jo["UpperLimit"]?.ToObject<LimitPlcInfo>(serializer),
                    Unit = jo["Unit"]?.ToString()
                };
            }
            else if (type == "Boolean")
            {
                return new BooleanParameter
                {
                    Name = jo["Name"]?.ToString(),
                    Description = jo["Description"]?.ToString(),
                    Value = jo["Value"]?.ToObject<bool>() ?? false
                };
            }
            else if (type == "Enum")
            {
                return new EnumParameter
                {
                    Name = jo["Name"]?.ToString(),
                    Description = jo["Description"]?.ToString(),
                    Value = jo["Value"]?.ToString(),
                    Options = jo["Options"]?.ToObject<List<string>>() ?? new List<string>()
                };
            }
            else if (type == "Text")
            {
                return new TextParameter
                {
                    Name = jo["Name"]?.ToString(),
                    Description = jo["Description"]?.ToString(),
                    Value = jo["Value"]?.ToString()
                };
            }
            else if (type == "Int")
            {
                return new IntParameter
                {
                    Name = jo["Name"]?.ToString(),
                    Description = jo["Description"]?.ToString(),
                    Value = jo["Value"]?.ToObject<LimitPlcInfo>() ?? new LimitPlcInfo()
                };
            }
            else
            {
                throw new NotSupportedException("Unsupported parameter type: " + type);
            }
        }

    }

}
