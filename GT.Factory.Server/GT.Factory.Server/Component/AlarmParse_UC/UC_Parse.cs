using GT.Factory.Logging.Logging;
using GT.Factory.Server.Core.Alarms;
using GT.Factory.Server.Help;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace GT.Factory.Server
{
    public partial class UC_Parse : UserControl
    {
        private List<AlaemParse> lsAlaemParse;

        private AccessMdbHelper db;
        string dbPath = @"D:\公共参数\GTDatabase.mdb"; // 目标电脑 IP 和共享名

        public UC_Parse()
        {
            InitializeComponent();
        }

        private void UC_Parse_Load(object sender, EventArgs e)
        {
            //  初始化dgvAlarmData
            InitializeDataGridView();

            //  工具栏相关设置
            toolStrip1.BackColor = Color.FromArgb(46, 51, 73);
            tsbInsert.Enabled = true;

            //  连接数据库
            db = new AccessMdbHelper(dbPath);
            //  加载报警数据表
            lsAlaemParse = UploadSql.SelectProcessAlarmParse(db);
        }

        //  初始化dgvAlarmData
        private void InitializeDataGridView()
        {
            dgvAlarmData.DataSource = null;
            dgvAlarmData.Columns.Clear();
            dgvAlarmData.ColumnCount = 6;
            dgvAlarmData.Columns[0].HeaderText = "工序号";
            dgvAlarmData.Columns[0].Name = "process_no";
            dgvAlarmData.Columns[0].Width = 60;
            dgvAlarmData.Columns[0].HeaderCell.Style.ForeColor = Color.White;
            dgvAlarmData.Columns[0].HeaderCell.Style.BackColor = Color.FromArgb(46, 51, 73);

            dgvAlarmData.Columns[1].HeaderText = "机台名称";
            dgvAlarmData.Columns[1].Name = "process_name";
            dgvAlarmData.Columns[1].Width = 120;
            dgvAlarmData.Columns[1].HeaderCell.Style.ForeColor = Color.White;
            dgvAlarmData.Columns[1].HeaderCell.Style.BackColor = Color.FromArgb(46, 51, 73);

            dgvAlarmData.Columns[2].HeaderText = "故障发生工位";
            dgvAlarmData.Columns[2].Name = "alarm_station";
            dgvAlarmData.Columns[2].Width = 60;
            dgvAlarmData.Columns[2].HeaderCell.Style.ForeColor = Color.White;
            dgvAlarmData.Columns[2].HeaderCell.Style.BackColor = Color.FromArgb(46, 51, 73);

            dgvAlarmData.Columns[3].HeaderText = "故障类型";
            dgvAlarmData.Columns[3].Name = "alarm_grade";
            dgvAlarmData.Columns[3].Width = 60;
            dgvAlarmData.EnableHeadersVisualStyles = false;
            dgvAlarmData.Columns[3].HeaderCell.Style.ForeColor = Color.White;
            dgvAlarmData.Columns[3].HeaderCell.Style.BackColor = Color.FromArgb(46, 51, 73);

            dgvAlarmData.Columns[4].HeaderText = "PLC地址";
            dgvAlarmData.Columns[4].Name = "plc_address";
            dgvAlarmData.Columns[4].Width = 60;
            dgvAlarmData.EnableHeadersVisualStyles = false;
            dgvAlarmData.Columns[4].HeaderCell.Style.ForeColor = Color.White;
            dgvAlarmData.Columns[4].HeaderCell.Style.BackColor = Color.FromArgb(46, 51, 73);

            dgvAlarmData.Columns[5].HeaderText = "报警信息";
            dgvAlarmData.Columns[5].Name = "alarm_parse";
            dgvAlarmData.Columns[5].Width = 300;
            dgvAlarmData.EnableHeadersVisualStyles = false;
            dgvAlarmData.Columns[5].HeaderCell.Style.ForeColor = Color.White;
            dgvAlarmData.Columns[5].HeaderCell.Style.BackColor = Color.FromArgb(46, 51, 73);

        }

        //  插入到数据库
        private void tsbInsert_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            dgvAlarmData.EndEdit();
            foreach (DataGridViewRow row in dgvAlarmData.Rows)
            {
                try
                {
                    UploadSql.UploadAlarmParse(row);
                }
                catch (Exception ex)
                {
                    DisplayLog.Error("报警信息更新失败", ex, true);
                }
            }
        }

        //  打开报警配置文件
        private void tsbOpenFile_Click(object sender, EventArgs e)
        {
            InitializeDataGridView();

            dgvAlarmData.DataSource = null;
            OpenFileDialog dialog = new OpenFileDialog();
            //是否支持多个文件的打开？
            dialog.Multiselect = false;
            //标题
            dialog.Title = "请选择txt文件";
            //文件类型
            dialog.Filter = "txt files (*.txt,*.*)|*.txt;*.*";//过滤选项设置，文本文件，所有文件。
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //获取文件路径
                String filePath = dialog.FileName;
                StreamReader sr = new StreamReader(filePath, Encoding.GetEncoding("gb2312"));
                string str = sr.ReadToEnd();
                string[] stt = str.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < stt.Length; i++)
                {
                    string[] arry = stt[i].Split('\t');
                    dgvAlarmData.Rows.Add(arry);
                    //StringBuilder sb = new StringBuilder();
                    //sb = SqlOperate.UpDate("SHProcessAlarmParse", $"alarm_description = '{arry[2]}'", $"process_no = {arry[0]} and plc_address = '{arry[1]}'");
                    //MSSqlHelper.ExecuteNonQuery(MSSqlHelper.Conn1, CommandType.Text, sb.ToString());
                }
            }
        }

        //  从数据库读取
        private void tsbReadFromData_Click(object sender, EventArgs e)
        {
            dgvAlarmData.Columns.Clear();
            dgvAlarmData.DataSource = lsAlaemParse;
        }
    }
}
