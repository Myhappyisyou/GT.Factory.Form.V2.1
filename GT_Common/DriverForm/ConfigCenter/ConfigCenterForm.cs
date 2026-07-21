using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.DriverForm.ConfigCenter
{
    public partial class ConfigCenterForm : Form
    {
        private TabControl tabControl;
        private TextBox txtSearch;
        private Button btnSave;
        private Button btnReset;

        public ConfigCenterForm()
        {
            InitUI();
            LoadTabs();
        }

        private void InitUI()
        {
            this.Text = "系统配置中心";
            this.Width = 900;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterScreen;

            txtSearch = new TextBox
            {
                Dock = DockStyle.Top,
                //PlaceholderText = "🔍 搜索参数..."
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            var bottomPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                FlowDirection = FlowDirection.RightToLeft
            };

            btnSave = new Button { Text = "保存", Width = 100 };

            btnSave.Click += BtnSave_Click;

            bottomPanel.Controls.Add(btnSave);

            this.Controls.Add(tabControl);
            this.Controls.Add(txtSearch);
            this.Controls.Add(bottomPanel);
        }

        private void LoadTabs()
        {
            tabControl.TabPages.Clear();

            var cfg = Config.Instance;

            var props = typeof(Config).GetProperties()
                .Where(p =>
                {
                    var browsable = p.GetCustomAttributes(typeof(BrowsableAttribute), true)
                                     .FirstOrDefault() as BrowsableAttribute;

                    return browsable == null || browsable.Browsable;
                });

            var groups = props
                .GroupBy(p => GetCategory(p))
                .Where(g => g.Any()); // 防止空Tab

            foreach (var group in groups)
            {
                var page = new TabPage(group.Key);

                var grid = new PropertyGrid
                {
                    Dock = DockStyle.Fill,
                    SelectedObject = new CategoryWrapper(cfg, group.Key)
                };

                page.Controls.Add(grid);
                tabControl.TabPages.Add(page);
            }
        }

        private string GetCategory(PropertyInfo prop)
        {
            var attr = prop.GetCustomAttribute<CategoryAttribute>();
            return attr?.Category ?? "其他";
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.ToLower();

            foreach (TabPage tab in tabControl.TabPages)
            {
                var grid = tab.Controls[0] as PropertyGrid;
                if (grid == null) continue;

                var wrapper = grid.SelectedObject as CategoryWrapper;
                wrapper.SetFilter(keyword);

                grid.SelectedObject = null;
                grid.SelectedObject = wrapper;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            Config.Save();
            MessageBox.Show("保存成功");
        }

    }
}
