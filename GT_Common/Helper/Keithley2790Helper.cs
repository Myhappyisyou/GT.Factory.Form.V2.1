using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GT_Common.Helper
{
    public  class Keithley2790Helper
    {
        private const byte Tail = 0x0D; // CR

        ////复位
        //public static string CommandRST = "*RST\r\n FORM:ELEM READ\r\n";
        ////外壳电阻
        //public static string CommandPERC = "CALC1:FORM PERC\r\n CALC1:STAT OFF\r\n SENS:FUNC \"RES\"\r\n SENS:RES:RANG 100\r\n SENS:RES:NPLC 0.1\r\n ROUT:MULT:CLOS (@108,115,118)\r\n READ?\r\n ROUT:OPEN:ALL\r\n";

        ////桥式电阻
        //public static string CommandOPEN = "ROUT:MULT:OPEN (@122)\r\n SOUR:CURR 0.01, (@127)\r\n CALC1:FORM S1I\r\n SENS:VOLT:NPLC 0.1\r\n CALC1:STAT ON\r\n ROUT:MULT:CLOS (@101,102,117,118,121)\r\n READ?\r\n ROUT:OPEN:ALL\r\n";

        ////绝缘电阻
        //public static string CommandVOLT = "SOUR:VOLT 500, (@128)\r\n CALC1:FORM S1V\r\n CALC1:STAT ON\r\n TRIG:DEL 2\r\n ROUT:MULT:CLOS (@101,103,108,116,118,121,122,123)\r\n READ?\r\n TRIG:DEL 0\r\n ROUT:OPEN:ALL\r\n";

        //复位
        public static string CommandRST = "*RST\r\n FORM:ELEM READ\r\n";

        //外壳电阻
        public static string CommandPERC = "CALC1:FORM PERC\r\n CALC1:STAT OFF\r\n SENS:FUNC \"RES\"\r\n SENS:RES:RANG 100\r\n SENS:RES:NPLC 0.1\r\n ROUT:MULT:CLOS (@108,115,118)\r\n READ?\r\n ROUT:OPEN:ALL\r\n";

        //弹片电阻
        //public static string CommandCLOSE = "ROUT:MULT:OPEN (@122)\r\n SOUR:CURR 0.01, (@127)\r\n CALC1:FORM S1I\r\n SENS:VOLT:NPLC 0.1\r\n CALC1:STAT ON\r\n ROUT:MULT:CLOS (@101,102,117,118,121,124)\r\n READ?\r\n ROUT:OPEN:ALL\r\n";

        public static string CommandCLOSE = "ROUT:MULT:OPEN (@122)\r\n SOUR:CURR 0.01, (@127)\r\n CALC1:FORM S1I\r\n SENS:VOLT:NPLC 0.1\r\n CALC1:STAT ON\r\n ROUT:MULT:CLOS (@101,102,117,118,121)\r\n READ?\r\n ROUT:OPEN:ALL\r\n";

        //桥式电阻
        public static string CommandOPEN = "ROUT:MULT:OPEN (@122)\r\n SOUR:CURR 0.01, (@127)\r\n CALC1:FORM S1I\r\n SENS:VOLT:NPLC 0.1\r\n CALC1:STAT ON\r\n ROUT:MULT:CLOS (@101,102,117,118,121)\r\n READ?\r\n ROUT:OPEN:ALL\r\n";

        //绝缘电阻
        public static string CommandVOLT = "SOUR:VOLT 500, (@128)\r\n CALC1:FORM S1V\r\n CALC1:STAT ON\r\n TRIG:DEL 2\r\n ROUT:MULT:CLOS (@101,103,108,116,118,121,122,123)\r\n READ?\r\n TRIG:DEL 0\r\n ROUT:OPEN:ALL\r\n";


        private const int BufferSize = 500;
        private const int DefaultDelay = 500;

        public static bool IsConnected { get; private set; }


        /// <summary>
        /// 执行无返回值的命令（如复位、配置等）
        /// </summary>
        /// <param name="ip">设备IP</param>
        /// <param name="port">端口</param>
        /// <param name="command">命令字符串</param>
        /// <param name="waitTimeout">超时时间(ms)</param>
        /// <returns>执行是否成功</returns>
        public async Task<bool> ExecuteCommandAsync(string ip, int port, string command, int waitTimeout = 3000)
        {
            if (string.IsNullOrWhiteSpace(command))
                throw new ArgumentException("指令不能为空");

            string formattedCommand = command.EndsWith("\r\n") ? command : command + "\r\n";

            TcpClient client = null;
            NetworkStream stream = null;

            try
            {
                client = new TcpClient();

                // 连接超时控制
                var connectTask = client.ConnectAsync(ip, port);
                var timeoutTask = Task.Delay(waitTimeout);
                var completed = await Task.WhenAny(connectTask, timeoutTask);

                if (completed != connectTask || !client.Connected)
                    return false;

                IsConnected = true;
                stream = client.GetStream();

                // 发送命令
                byte[] sendData = Encoding.ASCII.GetBytes(formattedCommand);
                await stream.WriteAsync(sendData, 0, sendData.Length);

                // 等待命令执行完成
                await Task.Delay(DefaultDelay);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Keithley2790Helper执行命令失败: {ex.Message}");
                return false;
            }
            finally
            {
                stream?.Dispose();
                client?.Close();
                IsConnected = false;
            }
        }

        /// <summary>
        /// 快速执行复位命令
        /// </summary>
        public async Task<bool> ResetDeviceAsync(string ip, int port, int waitTimeout = 3000)
        {
            return await ExecuteCommandAsync(ip, port, CommandRST, waitTimeout);
        }

        /// <summary>
        /// 获取测量数据（异步）
        /// </summary>
        public  async Task<string> GetMeasurementsAsync(string ip, int port, int waitTimeout,string command)
        {
            TcpClient client = null;
            NetworkStream stream = null;
            try
            {
                client = new TcpClient();

                // 使用 Task.WhenAny 实现超时
                var connectTask = client.ConnectAsync(ip, port);
                var timeoutTask = Task.Delay(waitTimeout);
                var completed = await Task.WhenAny(connectTask, timeoutTask);

                if (completed != connectTask || !client.Connected)
                    return string.Empty;

                IsConnected = true;
                stream = client.GetStream();

                return await SendAndReceiveAsync(stream, command, waitTimeout);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TwhHelper通信错误: {ex.Message}");
                return string.Empty;
            }
            finally
            {
                stream?.Dispose();
                client?.Close();
                IsConnected = false;
            }
        }

        /// <summary>
        /// 发送自定义指令（异步）
        /// </summary>
        public  async Task<string> SendCommandAsync(string ip, int port, string command, int waitTimeout)
        {
            if (string.IsNullOrWhiteSpace(command)) throw new ArgumentException("指令不能为空");

            string formattedCommand = command.EndsWith("\r\n") ? command : command + "\r\n";

            TcpClient client = null;
            NetworkStream stream = null;

            try
            {
                client = new TcpClient();
                var connectTask = client.ConnectAsync(ip, port);
                var timeoutTask = Task.Delay(waitTimeout);
                var completed = await Task.WhenAny(connectTask, timeoutTask);

                if (completed != connectTask || !client.Connected)
                    return string.Empty;

                stream = client.GetStream();
                return await SendAndReceiveAsync(stream, formattedCommand, waitTimeout);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Keithley2790Helper发送指令失败: {ex.Message}");
                return string.Empty;
            }
            finally
            {
                stream?.Dispose();
                client?.Close();
            }
        }

        /// <summary>
        /// 测试连接
        /// </summary>
        public  async Task<bool> TestConnectionAsync(string ip, int port, int timeout = 3000)
        {
            try
            {
                var client = new TcpClient();
                var connectTask = client.ConnectAsync(ip, port);
                var timeoutTask = Task.Delay(timeout);
                var completed = await Task.WhenAny(connectTask, timeoutTask);

                bool result = completed == connectTask && client.Connected;
                client.Close();
                return result;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 核心发送/接收逻辑
        /// </summary>
        //private  async Task<string> SendAndReceiveAsync(NetworkStream stream, string command, int waitTimeout)
        //{
        //    byte[] sendData = Encoding.ASCII.GetBytes(command);
        //    await stream.WriteAsync(sendData, 0, sendData.Length);

        //    // 等待设备处理
        //    await Task.Delay(DefaultDelay);

        //    byte[] buffer = new byte[BufferSize];
        //    var readTask = stream.ReadAsync(buffer, 0, buffer.Length);
        //    var timeoutTask = Task.Delay(waitTimeout);
        //    var completed = await Task.WhenAny(readTask, timeoutTask);

        //    if (completed != readTask)
        //        return string.Empty;

        //    int bytesRead = readTask.Result;
        //    return Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
        //}

        private async Task<string> SendAndReceiveAsync(NetworkStream stream, string command, int waitTimeout)
        {
            // 发送命令
            byte[] sendData = Encoding.ASCII.GetBytes(command);
            await stream.WriteAsync(sendData, 0, sendData.Length);

            // 等待设备处理
            await Task.Delay(DefaultDelay);

            List<byte> receivedBytes = new List<byte>();
            byte[] buffer = new byte[BufferSize];
            stream.ReadTimeout = waitTimeout;

            try
            {
                while (true)
                {
                    var readTask = stream.ReadAsync(buffer, 0, buffer.Length);
                    var timeoutTask = Task.Delay(waitTimeout);

                    var completed = await Task.WhenAny(readTask, timeoutTask);
                    if (completed != readTask)
                        break; // 超时

                    int bytesRead = readTask.Result;
                    if (bytesRead <= 0)
                        break; // 断开

                    for (int i = 0; i < bytesRead; i++)
                    {
                        byte b = buffer[i];
                        receivedBytes.Add(b);

                        // ✅ 检查是否收到了结束符 0x0D
                        if (b == 0x0D)
                        {
                            // 找到起始符 0x13
                            int startIndex = receivedBytes.IndexOf(0x13);
                            if (startIndex >= 0 && startIndex < receivedBytes.Count - 1)
                            {
                                int len = receivedBytes.Count - startIndex - 1;
                                byte[] frame = receivedBytes.Skip(startIndex + 1).Take(len - 1).ToArray();

                                return Encoding.ASCII.GetString(frame).Trim();
                            }
                        }
                    }
                }
            }
            catch (IOException)
            {
                // 网络中断也算结束
            }

            return string.Empty; // 没有完整数据
        }


    }

}
