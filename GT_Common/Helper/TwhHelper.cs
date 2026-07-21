using GT_Common.Helper;
using GT_Common.ProcessConfig;
using GT_Common.Helper.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GT_Common.Helper
{
    public static class TwhHelper
    {
        private const byte Tail = 0x0D; // CR

        public static string CommandFormat = "#01R00M100\r\n";

        public static string CommandFormatKA  = "#01R00M101\r\n";

        private const int BufferSize = 500;
        private const int DefaultDelay = 500;

        public static bool IsConnected { get; private set; }

        /// <summary>
        /// 获取测量数据（异步）
        /// </summary>
        public static async Task<string> GetMeasurementsAsync(string ip, int port, int waitTimeout,string command)
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
        public static async Task<string> SendCommandAsync(string ip, int port, string command, int waitTimeout)
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
                System.Diagnostics.Debug.WriteLine($"TwhHelper发送指令失败: {ex.Message}");
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
        public static async Task<bool> TestConnectionAsync(string ip, int port, int timeout = 3000)
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
        private static async Task<string> SendAndReceiveAsync(NetworkStream stream, string command, int waitTimeout)
        {
            byte[] sendData = Encoding.ASCII.GetBytes(command);
            await stream.WriteAsync(sendData, 0, sendData.Length);

            // 等待设备处理
            await Task.Delay(DefaultDelay);

            byte[] buffer = new byte[BufferSize];
            var readTask = stream.ReadAsync(buffer, 0, buffer.Length);
            var timeoutTask = Task.Delay(waitTimeout);
            var completed = await Task.WhenAny(readTask, timeoutTask);

            if (completed != readTask)
                return string.Empty;

            int bytesRead = readTask.Result;
            return Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
        }


        //测量值处理逻辑
        public static List<IMeasurement> GetAndBuildMeasurements(string ip, int port)
        {
            string data = TwhHelper.GetMeasurementsAsync(ip, port, 1000, TwhHelper.CommandFormat)
            .GetAwaiter().GetResult();
            DisplayLog.Info($"电阻焊数据 {data}");
            List<IMeasurement> measurements = CommMethod.ParseMeasurements(data);
            List<IMeasurement> lsMeasurements = new List<IMeasurement>();

            if (measurements.Count == 21)
            {
                //for (int i = 0; i < 5; i++)
                //{
                //    lsMeasurements.Add(measurements[i]);

                //    lsMeasurements.Add(new Measurement<float>
                //    {
                //        Name = measurements[i + 10].Name,
                //        Value = measurements[i].Value * measurements[i + 10].Value,
                //        Unit = measurements[i + 10].Unit,
                //        Status = measurements[i + 10].Status
                //    });

                //    lsMeasurements.Add(measurements[20]);
                //}
                lsMeasurements.Add(measurements[0]);

                lsMeasurements.Add(measurements[20]);

                lsMeasurements.Add(new Measurement<float>
                {
                    Name = measurements[10].Name,
                    Value = (float)measurements[10].Value * (float)measurements[10].Value,
                    Unit = measurements[10].Unit,
                    Status = measurements[10].Status
                });

            }

            return lsMeasurements;
        }

        //KA 数据 + CSV 构造
        public static StringBuilder GetKaCsv(string ip, int port)
        {
            string dataKa = TwhHelper.GetMeasurementsAsync(ip, port, 3000, TwhHelper.CommandFormatKA)
                                     .GetAwaiter().GetResult();

            var powerData = TwhPowerDataParser.Parse(dataKa);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"解析结果: {powerData}");
            sb.AppendLine($"统计信息: {TwhPowerDataParser.GetStatistics(powerData)}");

            foreach (var m in powerData.Measurements)
            {
                sb.AppendLine($"X{m.Key}: {m.Value:F2}KA");
            }

            return sb;
        }

    }

}
