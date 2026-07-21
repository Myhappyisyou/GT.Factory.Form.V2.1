using GT_Common.MyEnum;
using GT_Common.DriverForm.Recipe.RecipeParameter;
using GT_Common;
using RecipeParameter.RecipeParameter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace GT_Common.DriverForm.Recipe
{
    public partial class SearchRecipeForm : Form
    {
        private LimitConfig config = new LimitConfig();
        private DataGridView dgv;
        public SearchRecipeForm()
        {
            InitializeComponent();
            InitializeUI();
            this.Load += (s, e) =>
            {
                ApplyUiTheme();
                LoadConfig();
                LoadOperatorView();
            };
        }

        private void InitializeUI()
        {
            Text = "上下限参数配置展示";
            Width = 600;
            Height = 800;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250); // 浅灰蓝色系，柔和眼睛

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            Controls.Add(dgv);
        }

        private void ApplyUiTheme()
        {
            // 窗体背景
            this.BackColor = Color.FromArgb(245, 247, 250);
        }

        private void LoadConfig()
        {
            config = LimitConfigManager.Load();
        }

        private void LoadOperatorView()
        {
            var list = new List<OperatorRow>();

            foreach (var group in config.Groups)
            {
                foreach (var param in group.Parameters)
                {
                    if (param is LimitParameter l)
                    {
                        list.Add(new OperatorRow
                        {
                            Name = param.Name,
                            Down = l.LowerLimit.Value?.ToString(),
                            Up = l.UpperLimit.Value?.ToString(),
                            Unit = l.Unit,
                        });
                    }
                }
            }

            dgv.DataSource = list;

            dgv.Columns["Name"].HeaderText = "参数";
            dgv.Columns["Down"].HeaderText = "下限";
            dgv.Columns["Up"].HeaderText = "上限";
            dgv.Columns["Unit"].HeaderText = "单位";
        }
    }

    class OperatorRow
    {
        public string Name { get; set; }
        public string Down { get; set; }
        public string Up { get; set; }
        public string Unit { get; set; }
    }
}

