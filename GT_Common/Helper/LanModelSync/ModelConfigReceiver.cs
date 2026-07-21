using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.LanModelSync
{
    public class ModelConfigReceiver
    {
        private readonly int _port;

        public event Action<ProductModel> OnModelReceived;

        public ModelConfigReceiver(int port)
        {
            _port = port;
        }

        public void Start()
        {
            Task.Run(() =>
            {
                TcpListener listener = new TcpListener(IPAddress.Any, _port);
                listener.Start();
                while (true)
                {
                    TcpClient client = null;
                    try
                    {
                        // 接受客户端连接
                        client = listener.AcceptTcpClient();

                        // 使用网络流
                        using (NetworkStream stream = client.GetStream())
                        {
                            // 使用流读取器
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                // 读取JSON数据
                                string json = reader.ReadToEnd();

                                // 反序列化JSON
                                var model = JsonConvert.DeserializeObject<ProductModel>(json);

                                // 安全调用事件（防止多线程问题）
                                var handler = OnModelReceived;
                                if (handler != null)
                                {
                                    // 触发事件
                                    handler(model);
                                }
                            }
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        // JSON解析错误处理
                        Console.WriteLine($"JSON解析错误: {jsonEx.Message}");
                    }
                    catch (IOException ioEx)
                    {
                        // 网络I/O错误处理
                        Console.WriteLine($"网络通信错误: {ioEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        // 其他未知错误处理
                        Console.WriteLine($"处理客户端连接时出错: {ex.Message}");
                    }
                    finally
                    {
                        // 确保客户端总是关闭
                        if (client != null)
                        {
                            try
                            {
                                client.Close();
                            }
                            catch (Exception closeEx)
                            {
                                Console.WriteLine($"关闭客户端时出错: {closeEx.Message}");
                            }
                            finally
                            {
                                client = null; // 帮助GC回收
                            }
                        }
                    }
                }
            });
        }
    }
}
