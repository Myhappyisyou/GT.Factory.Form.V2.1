using GT_Common.Helper.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace GT_Common.Helper
{
    public static class BydMesComAsync
    {
        // 使用单个静态 HttpClient 实例，避免资源浪费
        private static readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// 用户验证（异步）
        /// </summary>
        public static async Task<(bool 验证结果, string MES反馈, string XMLOUT)> 用户验证Async(string userName,string passWord)
        {
            string param = "&message=<PRODUCTION_REQUEST><USER><SITE>" + Config.Instance.Site + "</SITE><NAME>" + userName + "</NAME><PWD>" + passWord + "</PWD></USER></PRODUCTION_REQUEST>";
            string url = $"http://{Config.Instance.IP}:{Config.Instance.PORT}{Config.Instance.URL}";

            string result = await GetHtmlByPostAsync(url, param, Config.Instance.TimeOut);
            return (CutResult(result), result, param);
        }

        /// <summary>
        /// 条码验证（异步）
        /// </summary>
        public static async Task<(bool 验证结果, string MES反馈, string XMLOUT)> 条码验证Async(string 产品条码)
        {
            string param = $"&message=<PRODUCTION_REQUEST><START><SFC_LIST><SFC><SITE>{Config.Instance.Site}</SITE><ACTIVITY>XML</ACTIVITY><ID>{产品条码}</ID><RESOURCE>{Config.Instance.Resource}</RESOURCE><OPERATION>{Config.Instance.Operation}</OPERATION><USER>{Shared.user.JobNub}</USER><QTY></QTY><DATE_TIME></DATE_TIME><COMPLEX>N</COMPLEX></SFC></SFC_LIST></START></PRODUCTION_REQUEST>!erpautogy03!1234567@byd";
            string url = $"http://{Config.Instance.IP}:{Config.Instance.PORT}{Config.Instance.URL}";

            string result = await GetHtmlByPostAsync(url, param, Config.Instance.TimeOut);
            return (CutResult(result), result, param);
        }

        /// <summary>
        /// 条码上传（异步）
        /// </summary>
        public static async Task<(bool 验证结果, string MES反馈, string XMLOUT)> 条码上传Async(bool 测试结果, string 产品条码, string 测试项)
        {
            string param;
            if (测试结果)
            {
                param = BuildPassParam(产品条码, 测试项);
            }
            else
            {
                param = BuildErrorParam(产品条码, 测试项);
            }

            string url = $"http://{Config.Instance.IP}:{Config.Instance.PORT}{Config.Instance.URL}";
            string result = await GetHtmlByPostAsync(url, param, Config.Instance.TimeOut);
            return (CutResult(result), result, param);
        }

        // 构建通过验证报文
        private static string BuildPassParam(string 产品条码, string 测试项)
        {
            return $"&message=PASS<PRODUCTION_REQUEST><COMPLETE><SFC_LIST><SFC><SITE>{Config.Instance.Site}</SITE><ACTIVITY>XML</ACTIVITY><ID>{产品条码}</ID><RESOURCE>{Config.Instance.Resource}</RESOURCE><OPERATION>{Config.Instance.Operation}</OPERATION><USER>{Shared.user.JobNub}</USER><QTY>1</QTY><DATE_TIME></DATE_TIME><DATE_STARTED></DATE_STARTED></SFC></SFC_LIST></COMPLETE></PRODUCTION_REQUEST>!erpautogy03!1234567@byd!PASS,测试文件{Config.Instance.MesVison},测试软件{Config.Instance.MesVison}{测试项}";
        }

        // 构建错误验证报文
        private static string BuildErrorParam(string 产品条码, string 测试项)
        {
            return $"&message=ERROR<PRODUCTION_REQUEST><NC_LOG_COMPLETE><SITE>{Config.Instance.Site}</SITE><OWNER TYPE=\"USER\">{Shared.user.JobNub}</OWNER><NC_CONTEXT>{产品条码}</NC_CONTEXT><QTY></QTY><IDENTIFIER></IDENTIFIER><FAILURE_ID></FAILURE_ID><DEFECT_COUNT>1</DEFECT_COUNT><COMMENTS></COMMENTS><DATE_TIME></DATE_TIME><RESOURCE>{Config.Instance.Resource}</RESOURCE><OPERATION>{Config.Instance.Operation}</OPERATION><ROOT_CAUSE_OPER></ROOT_CAUSE_OPER><NC_CODE>{Config.Instance.NcCode}</NC_CODE><ACTIVITY>XML</ACTIVITY></NC_LOG_COMPLETE></PRODUCTION_REQUEST>!erpautogy03!1234567@byd!ERROR,测试文件{Config.Instance.MesVison},{Config.Instance.MesVison}{测试项}";
        }

        /// <summary>
        /// 根据返回 HTML 判断是否成功
        /// </summary>
        private static bool CutResult(string html)
        {
            return html.Contains("</b>Y</td>");
        }


        //  工单信息查询
        public static async Task<(ShopOrderInfo 工单信息, bool 验证结果, string MES反馈, string XMLOUT)> 工单信息查询(string 工单, string 资源, string SFC条码)
        {
            string param = $"&message=<PRODUCTION_REQUEST><SHOP_ORDER><SITE>{Config.Instance.Site}</SITE><USER></USER><ORDER>{工单}</ORDER><RESOURCE>{资源}</RESOURCE><SFC >{SFC条码}</SFC></SHOP_ORDER></PRODUCTION_REQUEST> ";
            string url = $"http://{Config.Instance.IP}:{Config.Instance.PORT}{Config.Instance.URL}";

            string result = await GetHtmlByPostAsync(url, param, Config.Instance.TimeOut);
            bool 验证结果 = CutResult(result);
            ShopOrderInfo shopOrderInfo = null;
            if (验证结果)
            {
                 shopOrderInfo  = ParseFromHtml(result);
            }
            return (shopOrderInfo, 验证结果, param, param);
        }

        public static ShopOrderInfo ParseFromHtml(string html)
        {
            // 1️⃣ 提取 XML
            int start = html.IndexOf("<?xml");
            int end = html.LastIndexOf("</PRODUCTION_RESPONSE>");

            if (start < 0 || end < 0)
                throw new Exception("未找到XML内容");

            end += "</PRODUCTION_RESPONSE>".Length;

            string xml = html.Substring(start, end - start);

            // 2️⃣ HTML 解码
            xml = WebUtility.HtmlDecode(xml);

            // 3️⃣ 解析 XML
            return ParseShopOrder(xml);
        }

        public static string ExtractXml(string html)
        {
            int start = html.IndexOf("<?xml");
            int end = html.LastIndexOf("</PRODUCTION_RESPONSE>");

            if (start < 0 || end < 0)
                throw new Exception("未找到XML内容");

            end += "</PRODUCTION_RESPONSE>".Length;

            return html.Substring(start, end - start);
        }

        public static bool TryParseShopOrder(string html, out ShopOrderInfo result)
        {
            result = null;

            try
            {
                var xml = ExtractXml(html);
                xml = WebUtility.HtmlDecode(xml);
                result = ParseShopOrder(xml);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private static ShopOrderInfo ParseShopOrder(string xml)
        {
            var doc = XDocument.Parse(xml);
            var shopOrder = doc.Root.Element("SHOP_ORDER");

            if (shopOrder == null)
                throw new Exception("未找到 SHOP_ORDER 节点");

            return new ShopOrderInfo
            {
                Order = shopOrder.Element("ORDER")?.Value,
                Item = shopOrder.Element("ITEM")?.Value,
                ItemDesc = shopOrder.Element("ITEM_DESC")?.Value,
                Router = shopOrder.Element("ROUTER")?.Value,
                OrderNum = shopOrder.Element("ORDER_NUM")?.Value,
                CompNum = shopOrder.Element("COMP_NUM")?.Value,
                CompRate = shopOrder.Element("COMP_RATE")?.Value
            };
        }

        //  解析组件信息
        private static AssembleSfcResponse ParseFromHtmlResponse(string html)
        {
            CutResult(html);

            // 2. 提取 Return info 后面的 XML 内容
            // 匹配 <b>Return info： </b> 后面到 </td> 之前的内容
            var xmlPattern = @"<b>Return info：\s*</b>(.*?)</td>";
            var xmlMatch = Regex.Match(html, xmlPattern, RegexOptions.Singleline);

            if (!xmlMatch.Success)
                throw new Exception("未找到 Return info 中的 XML 内容");

            string xmlContent = xmlMatch.Groups[1].Value.Trim();

            // 3. 预处理 XML：转义特殊字符 &（但排除已经是 &amp; 的情况）
            xmlContent = Regex.Replace(xmlContent, @"&(?!amp;|lt;|gt;|quot;|apos;)", "&amp;");

            // 4. 解析 XML
            return ParseAssembleSfc(xmlContent);
        }

        private static AssembleSfcResponse ParseAssembleSfc(string xml)
        {
            var doc = XDocument.Parse(xml);

            var listNode = doc.Root?.Element("ASSEMBLE_SFC")?.Element("LIST");
            if (listNode == null)
                throw new Exception("未找到 LIST 节点");

            var result = new AssembleSfcResponse();

            foreach (var component in listNode.Elements("COMPONENT"))
            {
                result.Components.Add(new ComponentInfo
                {
                    Resource = component.Element("RESRCE")?.Value,
                    Operation = component.Element("OPERATION")?.Value,
                    Sfc = component.Element("SFC")?.Value,
                    Item = component.Element("ITEM")?.Value,
                    DataField = component.Element("DATA_FIELD")?.Value
                });
            }

            return result;
        }
        /// <summary>
        /// 发送异步 POST 请求
        /// </summary>
        private static async Task<string> GetHtmlByPostAsync(string url, string param, int timeout)
        {
            StringContent content = null;
            CancellationTokenSource cts = null;

            try
            {
                // ✅ 记录发送
                MesLog.SendLogger.Information("记录发送\r\n\r\nURL: {Url}\r\nRequest: {Param}", url, $"{param +"\r\n\r\n"}");

                // 创建请求内容（使用 GB2312 编码）
                content = new StringContent(param, Encoding.GetEncoding("GB2312"), "application/x-www-form-urlencoded");

                // 设置请求超时时间
                cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeout));

                // 发起 POST 请求
                HttpResponseMessage response = await _httpClient.PostAsync(url, content, cts.Token);
                response.EnsureSuccessStatusCode();

                // 读取返回内容
                // ✅ 手动指定接收编码为 GB2312
                byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();
                string result = Encoding.GetEncoding("GB2312").GetString(responseBytes);

                MesLog.ReceiveLogger.Information("记录接收\r\nURL: {Url}\r\nResponse: {Response}", url, $"{result + "\r\n\r\n"}");
           
                // 替换 HTML 特殊字符
                return result.Replace("&lt;", "<").Replace("&gt;", ">");
            }
            catch (Exception ex)
            {
                // ✅ 错误同时记录两边
                MesLog.SendLogger.Error(ex + "\r\n\r\n", "记录发送\r\nMES请求异常");
                MesLog.ReceiveLogger.Error(ex + "\r\n\r\n", "记录接收\r\n收MES响应异常");
                return ex.Message;
            }
            finally
            {
                // 手动释放资源（因为不使用 using var）
                if (content != null)
                    content.Dispose();

                if (cts != null)
                    cts.Dispose();
            }
        }

    }
}
