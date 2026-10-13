using GT.Factory.Common.Helpers;
using GT.Factory.Common.Interfaces;
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


namespace GT.Factory.Management.UI.ConfigCenter
{
    /// <summary>
    /// 配置中心独立窗口宿主。
    ///
    /// 真正的配置中心页面由 ConfigCenterControl 提供。
    /// </summary>
    public partial class ConfigCenterForm : Form
    {
        public ConfigCenterForm()
        {
            Text = "系统配置中心";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterScreen;

            ConfigCenterControl control =
                new ConfigCenterControl();

            control.Dock = DockStyle.Fill;

            Controls.Add(control);
        }
    }
}

//namespace GT.Factory.Management.UI.ConfigCenter
//{
//    public partial class ConfigCenterForm : Form
//    {
//        private TabControl tabControl;
//        private TextBox txtSearch;
//        private Button btnSave;
//        private Button btnReset;

//        public ConfigCenterForm()
//        {
//            InitUI();
//            //LoadTabs();
//        }

//        private void InitUI()
//        {
//            this.Text = "系统配置中心";
//            this.Width = 900;
//            this.Height = 600;
//            this.StartPosition = FormStartPosition.CenterScreen;

//            txtSearch = new TextBox
//            {
//                Dock = DockStyle.Top,
//                //PlaceholderText = "🔍 搜索参数..."
//            };
//            txtSearch.TextChanged += TxtSearch_TextChanged;

//            tabControl = new TabControl
//            {
//                Dock = DockStyle.Fill
//            };

//            var bottomPanel = new FlowLayoutPanel
//            {
//                Dock = DockStyle.Bottom,
//                Height = 45,
//                FlowDirection = FlowDirection.RightToLeft
//            };

//            btnSave = new Button { Text = "保存", Width = 100 };

//            btnSave.Click += BtnSave_Click;

//            btnReset = new Button();

//            btnReset.Text = "恢复默认";

//            btnReset.Width = 100;

//            btnReset.Click +=
//                BtnReset_Click;

//            bottomPanel.Controls.Add(btnSave);
//            bottomPanel.Controls.Add(btnReset);

//            this.Controls.Add(tabControl);
//            this.Controls.Add(txtSearch);
//            this.Controls.Add(bottomPanel);
//        }

//        //private void LoadTabs()
//        //{
//        //    tabControl.TabPages.Clear();

//        //    var cfg = Config.Instance;

//        //    var props = typeof(Config).GetProperties()
//        //        .Where(p =>
//        //        {
//        //            var browsable = p.GetCustomAttributes(typeof(BrowsableAttribute), true)
//        //                             .FirstOrDefault() as BrowsableAttribute;

//        //            return browsable == null || browsable.Browsable;
//        //        });

//        //    var groups = props
//        //        .GroupBy(p => GetCategory(p))
//        //        .Where(g => g.Any()); // 防止空Tab

//        //    foreach (var group in groups)
//        //    {
//        //        var page = new TabPage(group.Key);

//        //        var grid = new PropertyGrid
//        //        {
//        //            Dock = DockStyle.Fill,
//        //            SelectedObject = new CategoryWrapper(cfg, group.Key)
//        //        };

//        //        page.Controls.Add(grid);
//        //        tabControl.TabPages.Add(page);
//        //    }
//        //}

//        //private void LoadTabs()
//        //{
//        //    tabControl.TabPages.Clear();


//        //    foreach (IConfigProvider provider
//        //        in ConfigManager.Providers)
//        //    {
//        //        TabPage page =
//        //            new TabPage(provider.Name);


//        //        PropertyGrid grid =
//        //            new PropertyGrid();

//        //        grid.Dock =
//        //            DockStyle.Fill;

//        //        grid.PropertySort = 
//        //            PropertySort.Categorized;

//        //        grid.SelectedObject =
//        //            provider.Config;


//        //        page.Controls.Add(grid);

//        //        tabControl.TabPages.Add(page);
//        //    }
//        //}


//        //private string GetCategory(PropertyInfo prop)
//        //{
//        //    var attr = prop.GetCustomAttribute<CategoryAttribute>();
//        //    return attr?.Category ?? "其他";
//        //}

//        //private void TxtSearch_TextChanged(object sender, EventArgs e)
//        //{
//        //    string keyword = txtSearch.Text.ToLower();

//        //    foreach (TabPage tab in tabControl.TabPages)
//        //    {
//        //        var grid = tab.Controls[0] as PropertyGrid;
//        //        if (grid == null) continue;

//        //        var wrapper = grid.SelectedObject as CategoryWrapper;
//        //        wrapper.SetFilter(keyword);

//        //        grid.SelectedObject = null;
//        //        grid.SelectedObject = wrapper;
//        //    }
//        //}

//        ////private void BtnSave_Click(object sender, EventArgs e)
//        ////{
//        ////    Config.Save();
//        ////    MessageBox.Show("保存成功");
//        ////}

//        //private void BtnSave_Click(
//        //   object sender,
//        //   EventArgs e)
//        //{
//        //    try
//        //    {
//        //        foreach (IConfigProvider provider
//        //            in ConfigManager.Providers)
//        //        {
//        //            provider.Save();
//        //        }


//        //        MessageBox.Show(
//        //            "所有配置保存成功。",
//        //            "配置中心",
//        //            MessageBoxButtons.OK,
//        //            MessageBoxIcon.Information);
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        MessageBox.Show(
//        //            "配置保存失败：" + ex.Message,
//        //            "配置中心",
//        //            MessageBoxButtons.OK,
//        //            MessageBoxIcon.Error);
//        //    }
//        //}

//        //private void BtnReset_Click(
//        //    object sender,
//        //    EventArgs e)
//        //{
//        //    DialogResult result =
//        //        MessageBox.Show(
//        //            "确定恢复所有配置的默认值吗？",
//        //            "恢复默认",
//        //            MessageBoxButtons.YesNo,
//        //            MessageBoxIcon.Question);


//        //    if (result != DialogResult.Yes)
//        //    {
//        //        return;
//        //    }


//        //    foreach (IConfigProvider provider
//        //        in ConfigManager.Providers)
//        //    {
//        //        provider.Reset();
//        //    }


//        //    LoadTabs();
//        //}
//    }
//}
