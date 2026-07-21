using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Model
{
    public class GetAssemblySnTestInfoRequest
    {
        public string accessToken { get; set; }

        public string appId { get; set; }

        public string requestId { get; set; }

        public RequestData data { get; set; }
    }

    public class RequestData
    { 
        public string assemblyNumber { get; set; }

        public string stationCode { get; set; }

        public string testItem { get; set; }

    }


    public class TestInfoResponse
    {
        public string msg { get; set; }
        public int code { get; set; }

        public ResponseData data { get; set; }
    }

    public class ResponseData
    {
        public string snNumber { get; set; }

        public string stationCode { get; set; }

        public string testItem { get; set; }

        public string testValue { get; set; }

        public string testTime { get; set; }

    }

    public static class ApiResponseHelper
    {
        public static bool IsSuccess<T>(this T response, out string message) where T : TestInfoResponse
        {
            if (response == null)
            {
                message = "响应为空";
                return false;
            }

            switch (response.code)
            {
                case 200:
                    message = "成功";
                    break;
                case 401:
                    message = "未授权";
                    break;
                case 601:
                    message = $"参数校验失败：{response.msg}";
                    break;
                case 6011:
                    message = "必填字段不能为空";
                    break;
                case 6012:
                    message = "日期格式错误";
                    break;
                case 701:
                    message = $"查询失败：{response.msg}";
                    break;
                case 7011:
                    message = "未查询到相关数据";
                    break;
                case 7012:
                    message = "无相应查询权限";
                    break;
                case 7013:
                    message = "超出查询上限";
                    break;
                default:
                    message = $"未知错误，Code:{response.code}，Msg:{response.msg}";
                    break;
            }

            return response.code == 200 && response.data != null;
        }

        public static string GetDisplayText(this TestInfoResponse response)
        {
            if (!response.IsSuccess(out string errorMsg))
            {
                return $"错误：{errorMsg}";
            }
            var sb = new StringBuilder();
            sb.AppendLine($"Msg-【{response.msg}】");
            sb.AppendLine($"Code-【{response.code}】");
            sb.AppendLine($"SnNumber-【{response.data.snNumber}】");
            sb.AppendLine($"TestTime-【{response.data.testTime}】");
            sb.Append($"TestValue-【{response.data.testValue}】");
            return sb.ToString();
        }
    }

}
