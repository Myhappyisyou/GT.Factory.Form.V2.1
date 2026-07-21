using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EditJson
{
    public class JsonService
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        };

        public T LoadFromFile<T>(string filePath) where T : class, new()
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return new T();
                }

                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<T>(json, _settings);
            }
            catch (JsonException ex)
            {
                throw new ApplicationException($"JSON解析错误: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"加载文件失败: {ex.Message}", ex);
            }
        }

        public void SaveToFile<T>(string filePath, T data)
        {
            try
            {
                string json = JsonConvert.SerializeObject(data, _settings);
                File.WriteAllText(filePath, json);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new ApplicationException($"没有写入权限: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"保存文件失败: {ex.Message}", ex);
            }
        }

        public string FormatJson(string json)
        {
            try
            {
                var obj = JToken.Parse(json);
                return obj.ToString(Formatting.Indented);
            }
            catch
            {
                return json;
            }
        }

        public bool IsValidJson(string json)
        {
            try
            {
                JToken.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public T Clone<T>(T source)
        {
            var json = JsonConvert.SerializeObject(source, _settings);
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }
    }
}
