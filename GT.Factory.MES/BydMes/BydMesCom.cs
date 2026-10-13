using GT.Factory.Common.Models.Mes;
using GT.Factory.Logging.Logging;
using GT.Factory.MES.BydMes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace GT.Factory.Station.Helper
{
    public class BydMesCom
    {
        private static string ParamOUT;

        // 用户验证（异步）
        public static void 用户验证(string userName, string passWord, out bool 验证结果, out string MES反馈, out string XMLOUT)
        {
            MES反馈 = GetHtmlByPost("http://" + MesConfigProvider.Instance.Settings.IP + ":" + MesConfigProvider.Instance.Settings.PORT + MesConfigProvider.Instance.Settings.URL, "&message=<PRODUCTION_REQUEST><USER><SITE>" + MesConfigProvider.Instance.Settings.Site + "</SITE><NAME>" + userName + "</NAME><PWD>" + passWord + "</PWD></USER></PRODUCTION_REQUEST>", MesConfigProvider.Instance.Settings.TimeOut);
            验证结果 = CutResult(MES反馈);

            XMLOUT = ParamOUT;
        }

        // 条码验证（异步）
        public static void 条码验证(string 产品条码,string 用户工号, out bool 验证结果, out string MES反馈, out string XMLOUT)
        {
            MES反馈 = GetHtmlByPost("http://" + MesConfigProvider.Instance.Settings.IP + ":" + MesConfigProvider.Instance.Settings.PORT + MesConfigProvider.Instance.Settings.URL, "&message=<PRODUCTION_REQUEST><START><SFC_LIST><SFC><SITE>" + MesConfigProvider.Instance.Settings.Site + "</SITE><ACTIVITY>XML</ACTIVITY><ID>" + 产品条码 + "</ID><RESOURCE>" + MesConfigProvider.Instance.Settings.Resource + "</RESOURCE><OPERATION>" + MesConfigProvider.Instance.Settings.Operation + "</OPERATION><USER>" + 用户工号 + "</USER><QTY></QTY><DATE_TIME></DATE_TIME><COMPLEX>N</COMPLEX></SFC></SFC_LIST></START></PRODUCTION_REQUEST>!erpautogy03!1234567@byd", MesConfigProvider.Instance.Settings.TimeOut);
            验证结果 = CutResult(MES反馈);
            //Shared.lsActionTips.Add(new ActionTipModel { Code = -1, Tips = $"{产品条码}校验MES-{验证结果}" });

            XMLOUT = ParamOUT;
        }

        // 条码上传
        public static void 条码上传(bool 测试结果, string 产品条码, string 用户工号, string 测试项, out bool 验证结果, out string MES反馈, out string XMLOUT)
        {
            string text = "";
            text = (MES反馈 = ((!测试结果) ? ErrorValidate(产品条码, 用户工号, 测试项) : PassValidate(产品条码, 用户工号, 测试项)));
            验证结果 = CutResult(MES反馈);
            //Shared.lsActionTips.Add(new ActionTipModel { Code = 1, Tips = $"{产品条码}上传MES-{验证结果}" });
            XMLOUT = ParamOUT;
        }

        // 构建通过验证报文
        private static string PassValidate(string 产品条码, string 用户工号, string 测试项)
        {
            return GetHtmlByPost("http://" + MesConfigProvider.Instance.Settings.IP + ":" + MesConfigProvider.Instance.Settings.PORT + MesConfigProvider.Instance.Settings.URL, "&message=PASS<PRODUCTION_REQUEST><COMPLETE><SFC_LIST><SFC><SITE>" + MesConfigProvider.Instance.Settings.Site + "</SITE><ACTIVITY>XML</ACTIVITY><ID>" + 产品条码 + "</ID><RESOURCE>" + MesConfigProvider.Instance.Settings.Resource + "</RESOURCE><OPERATION>" + MesConfigProvider.Instance.Settings.Operation + "</OPERATION><USER>" + 用户工号 + "</USER><QTY>1</QTY><DATE_TIME></DATE_TIME><DATE_STARTED></DATE_STARTED></SFC></SFC_LIST></COMPLETE></PRODUCTION_REQUEST>!erpautogy03!1234567@byd!PASS," + $"测试文件{MesConfigProvider.Instance.Settings.MesVison}" + "," + $"测试软件{MesConfigProvider.Instance.Settings.MesVison}" + 测试项, MesConfigProvider.Instance.Settings.TimeOut);
        }

        // 构建错误验证报文
        private static string ErrorValidate(string 产品条码, string 用户工号, string 测试项)
        {
            return GetHtmlByPost("http://" + MesConfigProvider.Instance.Settings.IP + ":" + MesConfigProvider.Instance.Settings.PORT + MesConfigProvider.Instance.Settings.URL, "&message=ERROR<PRODUCTION_REQUEST><NC_LOG_COMPLETE><SITE>" + MesConfigProvider.Instance.Settings.Site + "</SITE><OWNER TYPE=\"USER\">" + 用户工号 + "</OWNER><NC_CONTEXT>" + 产品条码 + "</NC_CONTEXT><QTY></QTY><IDENTIFIER></IDENTIFIER><FAILURE_ID></FAILURE_ID><DEFECT_COUNT>1</DEFECT_COUNT><COMMENTS></COMMENTS><DATE_TIME></DATE_TIME><RESOURCE>" + MesConfigProvider.Instance.Settings.Resource + "</RESOURCE><OPERATION>" + MesConfigProvider.Instance.Settings.Operation + "</OPERATION><ROOT_CAUSE_OPER></ROOT_CAUSE_OPER><NC_CODE>" + MesConfigProvider.Instance.Settings.NcCode + "</NC_CODE><ACTIVITY>XML</ACTIVITY></NC_LOG_COMPLETE></PRODUCTION_REQUEST>!erpautogy03!1234567@byd!ERROR," + $"测试文件{MesConfigProvider.Instance.Settings.MesVison}" + "," + $"测试软件{MesConfigProvider.Instance.Settings.MesVison}" + 测试项, MesConfigProvider.Instance.Settings.TimeOut);
        }

        //  离散装配
        public static void 离散装配(string 产品条码, string 部件码, string 用户工号, out bool 验证结果, out string MES反馈, out string XMLOUT)
        {
            string param = $"message=" +
                $"<PRODUCTION_REQUEST><ASSEMBLE_COMPONENTS>" +
                $"<USER>{用户工号}</USER >" +
                $"<SITE>{MesConfigProvider.Instance.Settings.Site}</SITE>" +
                $"<PARENT_SFC>{产品条码}</PARENT_SFC>" +
                $"<OPERATION>{MesConfigProvider.Instance.Settings.Operation}</OPERATION>" +
                $"<RESOURCE>{MesConfigProvider.Instance.Settings.Resource}</RESOURCE >" +
                $"<CHECK_OPER>True</CHECK_OPER>" +
                $"<EVENT>baseFinished:AssemblyPoint</EVENT>" +
                $"<IDENTIFIER_LIST>" +
                $"<IDENTIFIER_OBJECT>" +
                $"<IDENTIFIER>{部件码}</IDENTIFIER>" +
                $"<REVISION>{MesConfigProvider.Instance.Settings.MeterVison}</REVISION>" +
                $"<QTY>1</QTY>" +
                $"<ASSY_DATA_VALUES>" +
                $"<ASSY_DATA>" +
                $"<DATA_FIELD>EXTERNAL_LOT</DATA_FIELD>" +
                $"<DATA_ATTR>{部件码}</DATA_ATTR>" +
                $"</ASSY_DATA>" +
                $"</ASSY_DATA_VALUES>" +
                $"</IDENTIFIER_OBJECT>" +
                $"</IDENTIFIER_LIST>" +
                $"</ASSEMBLE_COMPONENTS>" +
                $"</PRODUCTION_REQUEST>!erpautogy03!1234567@byd";

            MES反馈 = GetHtmlByPost($"http://" + MesConfigProvider.Instance.Settings.IP + ":" + MesConfigProvider.Instance.Settings.PORT + MesConfigProvider.Instance.Settings.URL, param, MesConfigProvider.Instance.Settings.TimeOut);
            验证结果 = CutResult(MES反馈);
            //Shared.lsActionTips.Add(new ActionTipModel { Code = -1, Tips = $"{产品条码}离散装配MES-{验证结果}" });
            XMLOUT = ParamOUT;

        }

        //  查询离散装配
        public static void 查询离散装配(string 产品条码,out AssembleSfcResponse 验证结果, out string MES反馈, out string XMLOUT)
        {
            string param = $"message=" +
                $"<PRODUCTION_REQUEST>" +
                $"<ASSEMBLE_SFC>" +
                $"<SITE>{MesConfigProvider.Instance.Settings.Site}</SITE>" +
                $"<SFC>{产品条码}</SFC>" +
                $"</ASSEMBLE_SFC>" +
                $"</PRODUCTION_REQUEST>";

            MES反馈 = GetHtmlByPost($"http://" + MesConfigProvider.Instance.Settings.IP + ":" + MesConfigProvider.Instance.Settings.PORT + MesConfigProvider.Instance.Settings.URL, param, MesConfigProvider.Instance.Settings.TimeOut);
            验证结果 = ParseFromHtmlResponse(MES反馈);
            //Shared.lsActionTips.Add(new ActionTipModel { Code = -1, Tips = $"{产品条码}查询离散装配MESMES-{验证结果.Components.Count}" });
            XMLOUT = ParamOUT;

        }

        //  判断是否成功
        private static bool CutResult(string html)
        {
            return html.Contains("</b>Y</td>") ? true : false;
        }

        //  工单信息查询
        public static bool 工单信息查询(string 工单, string 资源, string SFC条码, out ShopOrderInfo 工单信息, out string MES反馈, out string XMLOUT)
        {
            MES反馈 = GetHtmlByPost("http://" + MesConfigProvider.Instance.Settings.IP + ":" + MesConfigProvider.Instance.Settings.PORT + MesConfigProvider.Instance.Settings.URL, "&message=<PRODUCTION_REQUEST><SHOP_ORDER>" +
                    "<SITE>" + MesConfigProvider.Instance.Settings.Site + "</SITE>" +
                    "<USER> " + "</USER>" +
                    "<ORDER>" + 工单 + "</ORDER>" +
                    "<RESOURCE>" + 资源 + "</RESOURCE>" +
                    "<SFC >" + SFC条码 + "</SFC>" +
                    "</SHOP_ORDER>" +
                    "</PRODUCTION_REQUEST> ", MesConfigProvider.Instance.Settings.TimeOut);
            bool result = CutResult(MES反馈);
            if (result)
            {
                工单信息 = ParseFromHtml(MES反馈);
            }
            else
            {
                工单信息 = null;
            }
            XMLOUT = ParamOUT;
           return result;
        }

        //  解析工单信息

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

        public static void 工单绑定(string 工单, string 用户工号, string 用户密码, out string MES反馈, out string XMLOUT)
        {
            MES反馈 = GetHtmlByPost("http://" + MesConfigProvider.Instance.Settings.IP + ":" + MesConfigProvider.Instance.Settings.PORT + MesConfigProvider.Instance.Settings.URL, "&message=<PRODUCTION_REQUEST>" +
                    "<RESOURCE_BANDING_SHOPORDER >" +
                    "<SITE> " + MesConfigProvider.Instance.Settings.Site + " </SITE>" +
                    "<NAME> " + 用户工号 + " </NAME>" +
                    "<PWD> " + 用户密码 + " </PWD>" +
                    "<RESOURCE> " + MesConfigProvider.Instance.Settings.Resource + " </RESOURCE>" +
                    "<SHOPORDER> " + 工单 + " </SHOPORDER>" +
                    "</RESOURCE_BANDING_SHOPORDER>" +
                    "</PRODUCTION_REQUEST>", MesConfigProvider.Instance.Settings.TimeOut);
            // 验证结果 = ParseShopOrder(MES反馈);
            XMLOUT = ParamOUT;
        }

        #region 获取设备集成信息

        /// <summary>
        /// 获取设备集成信息
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <param name="验证结果">验证结果</param>
        /// <param name="MES反馈">MES反馈原始内容</param>
        /// <param name="XMLOUT">输出XML</param>
        /// <returns>解析后的响应对象</returns>
        public static DeviceIntegrationResponse 获取设备集成信息(string SFC条码,string 用户工号, out bool 验证结果, out string MES反馈, out string XMLOUT)
        {
            // 默认返回失败状态，
            // 防止发生异常时 out 参数没有正确赋值。
            验证结果 = false;

            // 默认 MES 返回为空。
            MES反馈 = string.Empty;

            // 默认请求 XML 为空。
            XMLOUT = string.Empty;

            try
            {
                // 1. 构建请求XML
                StringBuilder sb = new StringBuilder();
                sb.Append("<PRODUCTION_REQUEST><SIDE>");
                sb.Append($"<SITE>{MesConfigProvider.Instance.Settings.Site}</SITE>");
                sb.Append($"<USER>{用户工号}</USER>");
                sb.Append($"<SFC>{SFC条码}</SFC>");
                sb.Append($"<OPERATION>{MesConfigProvider.Instance.Settings.FrontOperation}</OPERATION>");
                sb.Append($"<RESOURCE>{MesConfigProvider.Instance.Settings.FrontResource}</RESOURCE>");
                sb.Append($"<ISOK>PASS</ISOK>");
                sb.Append("<LIST>");
                if (!string.IsNullOrWhiteSpace(MesConfigProvider.Instance.Settings.FrontTestItem))
                {
                    sb.Append($"<ITEM>{MesConfigProvider.Instance.Settings.FrontTestItem}</ITEM>");
                }
                sb.Append("</LIST>");
                sb.Append("</SIDE></PRODUCTION_REQUEST>");

                string param = $"message={sb.ToString()}";
                XMLOUT = sb.ToString();

                // 2. 发送POST请求
                MES反馈 = GetHtmlByPost($"http://" + MesConfigProvider.Instance.Settings.IP + ":" + MesConfigProvider.Instance.Settings.PORT + MesConfigProvider.Instance.Settings.URL, param, MesConfigProvider.Instance.Settings.TimeOut);

                // 3. 验证结果（根据实际返回格式判断）
                验证结果 = CutResult(MES反馈);

                // 如果 MES 返回验证失败，
                // 不再继续解析业务数据。
                if (!验证结果)
                {
                    //Shared.lsActionTips.Add(
                    //    new ActionTipModel
                    //    {
                    //        Code = -1,

                    //        Tips =
                    //            $"获取设备集成信息失败："
                    //            + $"{SFC条码}"
                    //    });

                    return null;
                }
                var response = ParseDeviceIntegrationResponse(MES反馈);

                //Shared.lsActionTips.Add(
                //    new ActionTipModel
                //    {
                //        Code = -1,

                //        Tips =
                //            $"获取设备集成信息-"
                //            + $"{SFC条码}-"
                //            + $"{验证结果}"
                //    });

                return response;
            }
            catch (Exception ex)
            {
                // 发生任何异常时，
                // 统一返回失败状态。
                验证结果 = false;

                //Shared.lsActionTips.Add(
                //    new ActionTipModel
                //    {
                //        Code = -1,

                //        Tips =
                //            $"获取设备集成信息异常："
                //            + $"{SFC条码}-"
                //            + $"{ex.Message}"
                //    });

                // 记录完整异常，
                // 方便后续排查网络、MES 或解析问题。
                DisplayLog.Error(
                    $"获取设备集成信息异常【{SFC条码}】",
                    ex,
                    true);

                // 异常情况下返回 null，
                // 调用方通过验证结果和返回值判断失败。
                return null;
            }

        }


        /// <summary>
        /// 解析设备集成信息响应
        /// </summary>
        /// <param name="html">原始HTML响应</param>
        /// <returns>解析后的响应对象</returns>
        private static DeviceIntegrationResponse ParseDeviceIntegrationResponse(string html)
        {
            // 1. 先调用CutResult验证（如果需要）
            CutResult(html);

            // 2. 提取 Return info 后面的 XML 内容
            var xmlPattern = @"<b>Return info：\s*</b>(.*?)</td>";
            var xmlMatch = Regex.Match(html, xmlPattern, RegexOptions.Singleline);

            string xmlContent;
            if (xmlMatch.Success)
            {
                xmlContent = xmlMatch.Groups[1].Value.Trim();
            }
            else
            {
                // 如果没有Return info包装，尝试直接匹配PRODUCTION_RESPONSE
                var directMatch = Regex.Match(html, @"(<PRODUCTION_RESPONSE>.*?</PRODUCTION_RESPONSE>)", RegexOptions.Singleline);
                if (directMatch.Success)
                {
                    xmlContent = directMatch.Groups[1].Value;
                }
                else
                {
                    throw new Exception("未找到 Return info 中的 XML 内容或PRODUCTION_RESPONSE节点");
                }
            }

            // 3. 预处理 XML：转义特殊字符
            xmlContent = Regex.Replace(xmlContent, @"&(?!amp;|lt;|gt;|quot;|apos;)", "&amp;");

            // 4. 解析 XML
            return ParseProductionResponse(xmlContent);
        }

        /// <summary>
        /// 解析PRODUCTION_RESPONSE XML
        /// </summary>
        /// <param name="xml">XML内容</param>
        /// <returns>解析后的响应对象</returns>
        private static DeviceIntegrationResponse ParseProductionResponse(string xml)
        {
            var doc = XDocument.Parse(xml);

            var sideNode = doc.Root?.Element("SIDE");
            if (sideNode == null)
                throw new Exception("未找到 SIDE 节点");

            var response = new DeviceIntegrationResponse
            {
                SFC = sideNode.Element("SFC")?.Value,
                Operation = sideNode.Element("OPERATION")?.Value,
                Resource = sideNode.Element("RESOURCE")?.Value,
                IsOk = sideNode.Element("ISOK")?.Value,
                Time = sideNode.Element("TIME")?.Value
            };

            var listNode = sideNode.Element("LIST");
            if (listNode != null)
            {
                foreach (var item in listNode.Elements("ITEM"))
                {
                    string itemValue = item.Value;
                    if (!string.IsNullOrEmpty(itemValue))
                    {
                        // 格式: 测试项,测试参数,测试值
                        var parts = itemValue.Split(',');
                        var testItemInfo = new TestItemInfo();

                        if (parts.Length >= 1)
                            testItemInfo.TestItem = parts[0];
                        if (parts.Length >= 2)
                            testItemInfo.TestParameter = parts[1];
                        if (parts.Length >= 3)
                            testItemInfo.TestValue = parts[2];

                        response.TestItems.Add(testItemInfo);
                    }
                }
            }

            return response;
        }


        #endregion

        private static string GetHtmlByPost(string URL, string Param, int TimeOut)
        {
            ParamOUT = Param;
            string text;
            try
            {
                // ✅ 记录发送
                MesLog.SendLogger.Information("记录发送\r\nURL: {Url}\r\nRequest: {Param}", URL, Param + "\r\n\r\n");

                byte[] bytes = Encoding.GetEncoding("GB2312").GetBytes(Param);
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(URL);
                httpWebRequest.ContentType = "application/x-www-form-urlencoded";
                httpWebRequest.Accept = "*/*";
                httpWebRequest.UserAgent = "Mozilla/4.0(compatible;MSIE 6.0;Windows NT 5.1;SV1;Maxthon;.NET CLR 1.1.4322)";
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentLength = bytes.Length;
                httpWebRequest.Timeout = TimeOut;
                httpWebRequest.ServicePoint.Expect100Continue = false;
                Stream requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.GetEncoding("GB2312"));
                text = streamReader.ReadToEnd();
                streamReader.Close();
                httpWebRequest.Abort();
                httpWebResponse.Close();
                //记录接收
                MesLog.ReceiveLogger.Information("记录接收\r\nURL: {Url}\r\nResponse: {Response}", URL, text + "\r\n\r\n");
            }
            catch (Exception ex)
            {
                text = ex.Message;
                MesLog.SendLogger.Information("记录发送\r\nURL: {Url}\r\nRequest: {Param}", URL, Param + "\r\n\r\n");
                MesLog.ReceiveLogger.Information("记录接收\r\nURL: {Url}\r\nResponse: {Response}", URL, text + "\r\n\r\n");

                // ✅ 错误同时记录两边
                MesLog.SendLogger.Error(ex + "\r\n\r\n", "记录发送\r\nMES请求异常");
                MesLog.ReceiveLogger.Error(ex + "\r\n\r\n", "记录接收\r\nMES响应异常");
            }

            return text.Replace("&lt;", "<").Replace("&gt;", ">");
        }
    }

    //  工单信息
    


    [Serializable]
    public class AssembleSfcResponse
    {
        public List<ComponentInfo> Components { get; set; } = new List<ComponentInfo>();
    }

    [Serializable]
    public class ComponentInfo
    {
        /// <summary>
        /// 资源
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// 操作
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// 条码
        /// </summary>
        public string Sfc { get; set; }

        /// <summary>
        /// 组件物料
        /// </summary>
        public string Item { get; set; }

        /// <summary>
        /// 装配类型
        /// </summary>
        public string DataField { get; set; }
    }

    /// <summary>
    /// 获取设备集成信息响应模型
    /// </summary>
    public class DeviceIntegrationResponse
    {
        public string SFC { get; set; }
        public string Operation { get; set; }
        public string Resource { get; set; }
        public string IsOk { get; set; }  // PASS/ERROR
        public string Time { get; set; }
        public List<TestItemInfo> TestItems { get; set; } = new List<TestItemInfo>();
    }

    /// <summary>
    /// 测试项信息模型
    /// </summary>
    public class TestItemInfo
    {
        public string TestItem { get; set; }      // 测试项
        public string TestParameter { get; set; } // 测试参数
        public string TestValue { get; set; }     // 测试值
    }
}
