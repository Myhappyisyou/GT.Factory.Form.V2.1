using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.QueryClient
{
    public class QueryClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public QueryClient(string baseUrl)
        {
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            _baseUrl = baseUrl;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        }

        /// <summary>
        /// GET 请求
        /// </summary>
        public async Task<T> GetAsync<T>(string route)
        {
            var response = await _httpClient.GetAsync(_baseUrl + route);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// POST 请求，带请求体
        /// </summary>
        public async Task<T> PostAsync<T, P>(string route, P payload)
        {
            string jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_baseUrl + route, content);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// 简单封装，POST 请求返回对象
        /// </summary>
        public Task<T> CallAsync<T, P>(string route, P payload) => PostAsync<T, P>(route, payload);

        /// <summary>
        /// 简单封装，GET 请求返回对象
        /// </summary>
        public Task<T> CallAsync<T>(string route) => GetAsync<T>(route);
    }
}
