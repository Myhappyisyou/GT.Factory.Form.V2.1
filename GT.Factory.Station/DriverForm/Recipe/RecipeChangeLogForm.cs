using GT.Factory.UI.Component;
using GT.Factory.UI.PageTemplate;
using GT.Factory.UI.Style;
using GT.Factory.Station.DriverForm.Recipe.RecipeParameter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.Station.DriverForm.Recipe
{
    public partial class RecipeChangeLogForm : Form
    {
        private UIToolPage page;

        private UIFormRow<DateTimePicker> dtpDate;

        private TextBox txtSearch;

        private List<RecipeChangeLog> allLogs = new List<RecipeChangeLog>();

        public RecipeChangeLogForm()
        {
            InitializeComponent();

            this.Text = "配方修改日志";

            this.StartPosition = FormStartPosition.CenterScreen;

            InitUI();

            InitGrid();
        }

        private void InitUI()
        {
            page = new UIToolPage();

            dtpDate = new UIFormRow<DateTimePicker>("查询日期：", UIInput.DateTime());

            var btnLoad = UIButton.Primary("加载");

            btnLoad.Click += BtnLoad_Click;

            var btnExport = UIButton.Primary("导出");
            btnExport.Click += BtnExport_Click;

            page.AddControl(dtpDate);

            page.AddButton(btnLoad);
            page.AddButton(btnExport);

            Controls.Add(page);

        }

        private void InitGrid()
        {
            page.Grid.AutoGenerateColumns = false;

            page.Grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "时间",
                DataPropertyName = "ChangeTime",
                Width = 120
            });

            page.Grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "用户",
                DataPropertyName = "UserName",
                Width = 60
            });

            page.Grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "配方",
                DataPropertyName = "RecipeName",
                Width = 100
            });

            page.Grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "参数路径",
                DataPropertyName = "ParameterPath",
                Width = 250
            });

            page.Grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "旧值",
                DataPropertyName = "OldValue",
                Width = 180
            });

            page.Grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "新值",
                DataPropertyName = "NewValue",
                Width = 180
            });

            page.Grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "来源",
                DataPropertyName = "Source",
                Width = 60
            });
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            allLogs = RecipeChangeLogReader.ReadLogs(dtpDate.Input.Value);

            BindGrid(allLogs);
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (allLogs == null) return;

            string key = txtSearch.Text.Trim();

            var filtered = allLogs
                .Where(x =>
                    string.IsNullOrEmpty(key) ||
                    x.ParameterPath.Contains(key) ||
                    x.UserName.Contains(key) ||
                    x.RecipeName.Contains(key))
                .OrderByDescending(x => x.ChangeTime)
                .ToList();

            BindGrid(filtered);
        }

        private void BindGrid(List<RecipeChangeLog> logs)
        {
            page.Grid.DataSource = null;
            page.Grid.DataSource = logs;
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (page.Grid.Rows.Count == 0)
            {
                MessageBox.Show("当前没有数据可导出。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV 文件|*.csv";
                sfd.FileName = $"RecipeChangeLog-{DateTime.Now:yyyyMMddHHmmss}.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var lines = new List<string>();
                        // 添加表头
                        lines.Add("时间,用户,配方,参数路径,旧值,新值,来源");

                        foreach (RecipeChangeLog log in page.Grid.DataSource as List<RecipeChangeLog>)
                        {
                            string line = $"{log.ChangeTime},{log.UserName},{log.RecipeName},{log.ParameterPath},{log.OldValue},{log.NewValue},{log.Source}";
                            lines.Add(line);
                        }

                        File.WriteAllLines(sfd.FileName, lines, Encoding.UTF8);
                        MessageBox.Show("导出成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"导出失败：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
