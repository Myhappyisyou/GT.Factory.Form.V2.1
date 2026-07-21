using GT_Common.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.DriverForm.DataDetail
{
    public partial class DataDetailForm : Form
    {
        private TableLayoutPanel mainLayout;
        private Label lblHeader;
        private DataGridView dgv;
        private Font boldFont;

        public DataDetailForm(List<SaveItem> items, string okFlag, string barNo, string partBar)
        {
            boldFont = new Font("Segoe UI", 9, FontStyle.Bold);

            InitUI();
            LoadData(items, okFlag, barNo, partBar);
        }

        private void InitUI()
        {
            this.Text = "测试详情";
            this.Width = 950;
            this.Height = 700;
            this.StartPosition = FormStartPosition.CenterParent;

            // ===== 主布局 =====
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 130)); // 上面固定
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // 下面填充

            this.Controls.Add(mainLayout);


            // ===== Header Label（单列，垂直信息）=====
            lblHeader = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                AutoSize = false,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(10)
            };

            mainLayout.Controls.Add(lblHeader, 0, 0);

            // ===== 表格 =====
            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White
            };

            mainLayout.Controls.Add(dgv, 0, 1);

            // ===== 列 =====
            dgv.Columns.Add("Index", "序号");
            dgv.Columns.Add("Name", "测试项");
            dgv.Columns.Add("Up", "上限");
            dgv.Columns.Add("Value", "实际值");
            dgv.Columns.Add("Down", "下限");
            dgv.Columns.Add("Unit", "单位");
            dgv.Columns.Add("Result", "结果");

            // 👉 初始化列宽（关键）
            InitGridColumnWidth();

            // 👉 对齐优化
            InitGridStyle();
        }

        private void InitGridColumnWidth()
        {
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

            dgv.Columns["Index"].Width = 60;

            dgv.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgv.Columns["Name"].MinimumWidth = 180;

            dgv.Columns["Up"].Width = 120;

            dgv.Columns["Value"].Width = 120;
            dgv.Columns["Value"].MinimumWidth = 80;

            dgv.Columns["Down"].Width = 120;

            dgv.Columns["Unit"].Width = 60;
            dgv.Columns["Result"].Width = 80;
        }

        private void InitGridStyle()
        {
            // 表头
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;

            // 居中列
            dgv.Columns["Index"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns["Up"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns["Value"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns["Down"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns["Unit"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.Columns["Result"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void LoadData(List<SaveItem> items, string okFlag, string barNo, string partBar)
        {
            if (items == null || items.Count == 0) return;

            var first = items.First();

            // ===== 基本信息 =====
          
            lblHeader.Text =
                $"【产品条码】{barNo}\n" +
                $"【部件条码】{partBar}\n" +
                $"【结果】{okFlag}\n" +
                $"【操作员】{first.Operator_no}\n" +
                $"【工位】{first.Process_no}\n" +
                $"【工单】{first.Order_no}\n" +
                $"【测试时间】{first.Do_time}\n" +
                $"【测试节拍】{first.Test_beat}";

            // 根据整体结果改变 Header 背景色
            //bool anyNg = items.Any(it =>
            //{
            //    if (string.IsNullOrWhiteSpace(it.Test_item_name)) return false;

            //    var vals = it.Test_item_value?.Split(',');
            //    var ups = it.Test_item_up?.Split(',');
            //    var downs = it.Test_item_down?.Split(',');
            //    //for (int i = 0; i < vals?.Length; i++)
            //    //{
            //    //    if (double.TryParse(vals[i], out double actual) &&
            //    //        double.TryParse(SafeGet(ups, i), out double max) &&
            //    //        double.TryParse(SafeGet(downs, i), out double min))
            //    //    {
            //    //        if (actual > max || actual < min)
            //    //            return true;
            //    //    }
            //    //}
            //    for (int i = 0; i < vals?.Length; i++)
            //    {
            //        if (it.Ok_flag == "OK")
            //        {
            //            return true;
            //        }
            //    }
            //    return false;
            //});

            // "全都是OK才是OK" → 只要有一个不是 "OK" 就是 NG
            bool anyNg = items.Any(it => it.Ok_flag != "OK");

            lblHeader.BackColor = anyNg ? Color.FromArgb(255, 230, 230) : Color.FromArgb(230, 255, 230);

            // ===== DataGridView 数据 =====
            dgv.SuspendLayout();

            int idx = 1;
            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.Test_item_name)) continue;

                var names = item.Test_item_name.Split(',');
                var ups = item.Test_item_up?.Split(',');
                var downs = item.Test_item_down?.Split(',');
                var vals = item.Test_item_value?.Split(',');
                var units = item.Test_item_unit?.Split(',');
                var results = item.Ok_flag?.Split(',');

                for (int i = 0; i < names.Length; i++)
                {
                    string name = names[i];
                    string up = SafeGet(ups, i);
                    string down = SafeGet(downs, i);
                    string val = SafeGet(vals, i);
                    string unit = string.IsNullOrWhiteSpace(SafeGet(units, i)) ? "-" : SafeGet(units, i);
                    string result = results[i];

                    //bool isNg = false;
                    //if (double.TryParse(val, out double actual) &&
                    //    double.TryParse(up, out double max) &&
                    //    double.TryParse(down, out double min))
                    //{
                    //    if (actual > max || actual < min)
                    //        isNg = true;
                    //}

                    int rowIndex = dgv.Rows.Add(idx, name,  up, val, down, unit, result);

                    if (result == "NG")
                    {
                        var row = dgv.Rows[rowIndex];
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230);
                        row.DefaultCellStyle.ForeColor = Color.Red;
                        row.DefaultCellStyle.Font = boldFont;
                    }

                    idx++;
                }
            }

            dgv.ResumeLayout();
        }

        private static string SafeGet(string[] arr, int index)
        {
            return (arr != null && index >= 0 && index < arr.Length) ? arr[index] : string.Empty;
        }
    }
}
