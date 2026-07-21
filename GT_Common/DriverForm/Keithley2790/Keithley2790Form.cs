using GT_Common.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.DriverForm.Keithley2790
{
    public partial class Keithley2790Form : Form
    {
        ComboBox comboMachine;
        ComboBox comboCmd;
        Button btnSend;
        Button btnClear;
        TextBox txtReceive;

        Keithley2790Helper client = new Keithley2790Helper();
        // 你自己的封装类

        public string ST40_1_IP = "192.168.6.97"; //    OP070

        //public string UIDIP = "192.168.4.121";//192.168.2.29

        public int ST40_1_Port { get; set; } = 1030;

        // UID
        public string ST40_2_IP = "192.168.6.97"; //    OP070

        //public string UIDIP = "192.168.4.121";//192.168.2.29

        public int ST40_2_Port { get; set; } = 1031;

        //复位
        public static string CommandRST = "*RST\r\n FORM:ELEM READ\r\n";

        //外壳电阻
        public static string CommandPERC = "CALC1:FORM PERC\r\n CALC1:STAT OFF\r\n SENS:FUNC \"RES\"\r\n SENS:RES:RANG 100\r\n SENS:RES:NPLC 0.1\r\n ROUT:MULT:CLOS (@108,115,118)\r\n READ?\r\n ROUT:OPEN:ALL\r\n";

        //弹片电阻
        public static string CommandCLOSE = "ROUT:MULT:OPEN (@122)\r\n SOUR:CURR 0.01, (@127)\r\n CALC1:FORM S1I\r\n SENS:VOLT:NPLC 0.1\r\n CALC1:STAT ON\r\n ROUT:MULT:CLOS (@101,102,117,118,121,124)\r\n READ?\r\n ROUT:OPEN:ALL\r\n";

        //桥式电阻
        public static string CommandOPEN = "ROUT:MULT:OPEN (@122)\r\n SOUR:CURR 0.01, (@127)\r\n CALC1:FORM S1I\r\n SENS:VOLT:NPLC 0.1\r\n CALC1:STAT ON\r\n ROUT:MULT:CLOS (@101,102,117,118,121)\r\n READ?\r\n ROUT:OPEN:ALL\r\n";

        //绝缘电阻
        public static string CommandVOLT = "SOUR:VOLT 500, (@128)\r\n CALC1:FORM S1V\r\n CALC1:STAT ON\r\n TRIG:DEL 2\r\n ROUT:MULT:CLOS (@101,103,108,116,118,121,122,123)\r\n READ?\r\n TRIG:DEL 0\r\n ROUT:OPEN:ALL\r\n";

        public Keithley2790Form()
        {
            this.Text = "Keithley 2790 测试页面";
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
            comboCmd = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Left = 180,
                Top = 15,
                Width = 180
            };
            this.Controls.Add(comboCmd);

            // ========== 发送按钮 ==========
            btnSend = new Button
            {
                Text = "发送",
                Left = 380,
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
            comboMachine.Items.Add("Keithley2790-1");
            comboMachine.Items.Add("Keithley2790-2");
            comboMachine.SelectedIndex = 0;

            // 指令
            comboCmd.Items.Add("复位");
            comboCmd.Items.Add("通路测试&外壳电阻");
            comboCmd.Items.Add("短路测试&弹片电阻");
            comboCmd.Items.Add("桥丝测试&桥式电阻");
            comboCmd.Items.Add("绝缘测试&绝缘电阻");
            comboCmd.SelectedIndex = 0;

            // 绑定事件
            btnSend.Click += BtnSend_Click;
            btnClear.Click += (s, e) => txtReceive.Clear();
        }

        private async void BtnSend_Click(object sender, EventArgs e)
        {
            try
            {
                string cmd = comboCmd.Text;
                string rsp = "";

                string ip = comboMachine.SelectedIndex == 0 ? ST40_1_IP : ST40_2_IP;
                int port = comboMachine.SelectedIndex == 0 ? ST40_1_Port : ST40_2_Port;

                // ===== 根据下拉框的指令执行不同处理 =====
                switch (cmd)
                {
                    case "复位":
                        await client.ResetDeviceAsync(ip, port);   // 你自己的函数
                        break;

                    case "通路测试&外壳电阻":
                        rsp = await client.GetMeasurementsAsync(ip, port, 3000, Keithley2790Helper.CommandPERC);   // 你自己的函数
                        break;

                    case "短路测试&弹片电阻":
                        rsp = await client.GetMeasurementsAsync(ip, port, 3000, Keithley2790Helper.CommandCLOSE);   // 你自己的函数
                        break;

                    case "桥丝测试&桥式电阻":
                        rsp = await client.GetMeasurementsAsync(ip, port, 3000, Keithley2790Helper.CommandOPEN);   // 你自己的函数
                        break;

                    case "绝缘测试&绝缘电阻":
                        rsp = await client.GetMeasurementsAsync(ip, port, 3000, Keithley2790Helper.CommandVOLT);   // 你自己的函数
                        break;

                    default:
                        // 默认当普通命令发送
                        break;
                }
                CommMethod.TryExtractScientificFloat(rsp, out float value);

                // UI 显示
                txtReceive.AppendText($"[{comboMachine.SelectedIndex}--发送] {cmd}\r\n");
                txtReceive.AppendText($"[{comboMachine.SelectedIndex}--接收] {rsp}----{value}\r\n\r\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show("发送失败：" + ex.Message);
            }
        }
    }
}
