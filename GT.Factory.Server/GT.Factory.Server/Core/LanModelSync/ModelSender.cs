using GT.Factory.Logging.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.Server.Core.LanModelSync
{
    public class ModelSender
    {
        public static void SendModelToClients(ProductModel model, string[] clientIPs, int port)
        {
            string json = JsonConvert.SerializeObject(model);
            byte[] data = Encoding.UTF8.GetBytes(json);
            foreach (var ip in clientIPs)
            {
                try
                {
                    using (TcpClient client = new TcpClient())
                    {
                        client.Connect(ip, port);
                        client.GetStream().Write(data, 0, data.Length);
                    }
                }
                catch(Exception ex)
                {
                    DisplayLog.Error($"[ChangeOrder] 型号切换失败 IP={ip}", ex, true);
                }
            }
        }

        //  异步并行发送
        public static async Task SendModelToClientsAsync(ProductModel model, string[] clientIPs, int port)
        {
            string json = JsonConvert.SerializeObject(model);
            byte[] data = Encoding.UTF8.GetBytes(json);

            var tasks = clientIPs.Select(async ip =>
            {
                try
                {
                    using (TcpClient client = new TcpClient())
                    {
                        await client.ConnectAsync(ip, port);
                        using (var stream = client.GetStream())
                        {
                            await stream.WriteAsync(data, 0, data.Length);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DisplayLog.Error($"[ChangeOrder] 型号切换失败 IP={ip}",ex,true);
                }
            });

            await Task.WhenAll(tasks); // 并行等待所有发送完成
        }
    }
}
