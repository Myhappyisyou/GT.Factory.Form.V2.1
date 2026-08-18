using GT.Factory.UI.Component;
using GT.Factory.UI.PageTemplate;
using GT.Factory.UI.Style;
using GT_Common;
using GT_Common.DriverForm.Recipe.RecipeParameter;
using GT_Common.MyEnum;
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
        private UIToolPage page;

        private LimitConfig config = new LimitConfig();

        private DataGridView dgv;
        public SearchRecipeForm()
        {
            InitializeComponent();
            InitializeUI();
            this.Load += (s, e) =>
            {
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

            InitUI();
        }

        private void InitUI()
        {
            page = new UIToolPage();

            var btnLoad = UIButton.Primary("加载");

            btnLoad.Click += (s, e) => LoadOperatorView();

            page.AddButton(btnLoad, true);

            Controls.Add(page);
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

            page.Grid.DataSource = list;

            page.Grid.Columns["Name"].HeaderText = "参数";
            page.Grid.Columns["Down"].HeaderText = "下限";
            page.Grid.Columns["Up"].HeaderText = "上限";
            page.Grid.Columns["Unit"].HeaderText = "单位";
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

