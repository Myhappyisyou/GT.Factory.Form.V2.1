using GT_Common.Helper;
using GT_Common.ProcessConfig;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.DriverForm.Twh
{
    public partial class TwhForm : Form
    {
        ComboBox comboMachine;
        Button btnSend;
        Button btnClear;
        TextBox txtReceive;

        Keithley2790Helper client = new Keithley2790Helper();
        // 你自己的封装类

        public string Thw_ST70_IP { get; set; } = "192.168.4.183";
        public int Thw_ST70_Port { get; set; } = 4002;

        public string Thw_ST80_IP { get; set; } = "192.168.4.183";
        public int Thw_ST80_Port { get; set; } = 4003;

        public string Thw_ST90_IP { get; set; } = "192.168.4.183";
        public int Thw_ST90_Port { get; set; } = 4004;


        public TwhForm()
        {
            this.Text = "Twh 测试页面";
            this.Width = 520;
            this.Height = 420;
            this.StartPosition = FormStartPosition.CenterScreen;

            BuildUI();
            InitLogic();
        }

        private void BuildUI()
        {
            // ========== 机台选择 ==========
            comboMachine = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Left = 15,
                Top = 15,
                Width = 150
            };
            this.Controls.Add(comboMachine);

            // ========== 指令选择 ==========
           

            // ========== 发送按钮 ==========
            btnSend = new Button
            {
                Text = "测试",
                Left = 180,
                Top = 15,
                Width = 80
            };
            this.Controls.Add(btnSend);

            // ========== 接收区域 ==========
            txtReceive = new TextBox
            {
                Left = 15,
                Top = 55,
                Width = 445,
                Height = 280,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(txtReceive);

            // ========== 清空按钮 ==========
            btnClear = new Button
            {
                Text = "清空接收区",
                Left = 15,
                Top = 345,
                Width = 120
            };
            this.Controls.Add(btnClear);
        }

        private void InitLogic()
        {
            // 机台（你按需替换）
            comboMachine.Items.Add("STA70");
            comboMachine.Items.Add("STA80");
            comboMachine.Items.Add("STA90");
            comboMachine.SelectedIndex = 0;

            // 绑定事件
            btnSend.Click += BtnSend_Click;
            btnClear.Click += (s, e) => txtReceive.Clear();
        }

        private async void BtnSend_Click(object sender, EventArgs e)
        {
            try
            {
                int cmdMachine = comboMachine.SelectedIndex;

                string ip = Thw_ST70_IP;
                int port = Thw_ST70_Port;

                switch (cmdMachine)
                {
                    case 0:
                        ip = Thw_ST70_IP;
                        port = Thw_ST70_Port;
                        break;
                    case 1:
                        ip = Thw_ST80_IP;
                        port = Thw_ST80_Port;
                        break;
                    case 2:
                        ip = Thw_ST90_IP;
                        port = Thw_ST90_Port;
                        break;
                    default:
                        break;

                }

              
                string data = await Task.Run(() => TwhHelper.GetMeasurementsAsync(ip, port, 1000, TwhHelper.CommandFormat));

                txtReceive.AppendText($"[RX] {data}\r\n\r\n");

                List<IMeasurement> measurements = CommMethod.ParseMeasurements(data);
                List<IMeasurement> lsMeasurements = new List<IMeasurement>();

                if (measurements.Count == 21)
                {
                    // 前5组数据（索引0-4分别对应10-14）
                    for (int i = 0; i < 5; i++)
                    {
                        // 第一个测量值
                        lsMeasurements.Add(new Measurement<float>
                        {
                            Name = measurements[i].Name,
                            Value = (float)measurements[i].Value,
                            Unit = measurements[i].Unit,
                            Status = measurements[i].Status
                        });

                        txtReceive.AppendText($"[RX] {measurements[i].Name}>>>{measurements[i].Value}>>>{measurements[i].Unit}>>>{measurements[i].Status}\r\n\r\n");

                        // 第二个测量值（乘积）
                        lsMeasurements.Add(new Measurement<float>
                        {
                            Name = measurements[i + 10].Name,
                            Value = (float)measurements[i].Value * (float)measurements[i + 10].Value,
                            Unit = measurements[i + 10].Unit,
                            Status = measurements[i + 10].Status
                        });
                        txtReceive.AppendText($"[RX] {measurements[i + 10].Name}>>>{(float)measurements[i + 10].Value * (float)measurements[i + 10].Value}>>>{measurements[i + 10].Unit}>>>{measurements[i + 10].Status}\r\n\r\n");

                        // 第三个测量值（固定索引20）
                        lsMeasurements.Add(new Measurement<float>
                        {
                            Name = measurements[20].Name,
                            Value = (float)measurements[20].Value,
                            Unit = measurements[20].Unit,
                            Status = measurements[20].Status
                        });

                        txtReceive.AppendText($"[RX] {measurements[20].Name}>>>{measurements[20].Value}>>>{measurements[20].Unit}>>>{measurements[20].Status}\r\n\r\n");

                    }
                }

                string dataKa = await Task.Run(() => TwhHelper.GetMeasurementsAsync(ip, port, 3000, TwhHelper.CommandFormatKA));

                // 解析单个数据包
                var powerData = TwhPowerDataParser.Parse(dataKa);

                Console.WriteLine($"解析结果: {powerData}");
                Console.WriteLine($"统计信息: {TwhPowerDataParser.GetStatistics(powerData)}");
                Console.WriteLine($"是否有异常值: {TwhPowerDataParser.HasAbnormalValues(powerData)}");
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"解析结果: {powerData}");
                sb.AppendLine($"统计信息: {TwhPowerDataParser.GetStatistics(powerData)}");

                // 获取前10个测量点的值
                var first10 = powerData.Measurements;

                foreach (var measurement in first10)
                {
                    //Console.WriteLine($"  X{measurement.Key}: {measurement.Value:F2}KA");
                    sb.AppendLine($"  X{measurement.Key}: {measurement.Value:F2}KA");

                }
                txtReceive.AppendText($"[RX] {sb}\r\n\r\n");
              
            }
            catch (Exception ex)
            {
                MessageBox.Show("发送失败：" + ex.Message);
            }
        }
    }
}
