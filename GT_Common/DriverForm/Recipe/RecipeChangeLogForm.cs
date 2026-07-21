using GT_Common.DriverForm.Recipe.RecipeParameter;
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

namespace GT_Common.DriverForm.Recipe
{
    public partial class RecipeChangeLogForm : Form
    {
        private Panel panelTop;
        private DateTimePicker dtpDate;
        private Button btnLoad;
        private Button btnExport;
        private TextBox txtSearch;
        private Label lblCount;
        private DataGridView dgvLogs;

        private List<RecipeChangeLog> allLogs = new List<RecipeChangeLog>();

        public RecipeChangeLogForm()
        {
            this.Text = "配方修改日志";
            this.Width = 960;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterScreen;

            InitializeControls();

            InitGrid();
        }

        private void InitializeControls()
        {
            panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40
            };

            dtpDate = new DateTimePicker
            {
                Location = new System.Drawing.Point(10, 10),
                Width = 150
            };

            btnLoad = new Button
            {
                Text = "加载",
                Location = new System.Drawing.Point(170, 8)
            };
            btnLoad.Click += BtnLoad_Click;

            btnExport = new Button
            {
                Text = "导出CSV",
                Location = new System.Drawing.Point(250, 8)
            };
            btnExport.Click += BtnExport_Click;

            txtSearch = new TextBox
            {
                Location = new System.Drawing.Point(380, 9),
                Width = 200
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            lblCount = new Label
            {
                Location = new System.Drawing.Point(600, 12),
                AutoSize = true,
                Text = "总数：0"
            };

            var lblSearch = new Label
            {
                Text = "搜索:",
                Location = new System.Drawing.Point(340, 12),
                AutoSize = true
            };

            panelTop.Controls.Add(dtpDate);
            panelTop.Controls.Add(btnLoad);
            panelTop.Controls.Add(btnExport);
            panelTop.Controls.Add(txtSearch);
            panelTop.Controls.Add(lblSearch);
            panelTop.Controls.Add(lblCount);

            dgvLogs = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true
            };

            this.Controls.Add(dgvLogs);
            this.Controls.Add(panelTop);
        }

        private void InitGrid()
        {
            dgvLogs.AutoGenerateColumns = false;

            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "时间",
                DataPropertyName = "ChangeTime",
                Width = 120
            });

            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "用户",
                DataPropertyName = "UserName",
                Width = 60
            });

            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "配方",
                DataPropertyName = "RecipeName",
                Width = 100
            });

            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "参数路径",
                DataPropertyName = "ParameterPath",
                Width = 250
            });

            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "旧值",
                DataPropertyName = "OldValue",
                Width = 180
            });

            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "新值",
                DataPropertyName = "NewValue",
                Width = 180
            });

            dgvLogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "来源",
                DataPropertyName = "Source",
                Width = 60
            });
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            allLogs = RecipeChangeLogReader.ReadLogs(dtpDate.Value);
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
            dgvLogs.DataSource = null;
            dgvLogs.DataSource = logs;
            lblCount.Text = $"总数：{logs.Count}";
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (dgvLogs.Rows.Count == 0)
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

                        foreach (RecipeChangeLog log in dgvLogs.DataSource as List<RecipeChangeLog>)
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
