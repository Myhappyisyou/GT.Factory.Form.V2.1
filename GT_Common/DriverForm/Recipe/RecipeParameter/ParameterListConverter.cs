using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeParameter.RecipeParameter
{
    public class ParameterListConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IEnumerable<ParameterBase>).IsAssignableFrom(objectType);
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
            var singleConverter = new SingleParameterConverter();

            foreach (var item in list)
            {
                var tokenWriter = new JTokenWriter();
                singleConverter.WriteJson(tokenWriter, item, serializer);
                tokenWriter.Token.WriteTo(writer);
            }

            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var array = JArray.Load(reader);
            var result = new List<ParameterBase>();
            var singleConverter = new SingleParameterConverter();

            foreach (var token in array)
            {
                var reader2 = token.CreateReader();
                var param = (ParameterBase)singleConverter.ReadJson(reader2, typeof(ParameterBase), null, serializer);
                result.Add(param);
            }

            return result;
        }
    }

}
