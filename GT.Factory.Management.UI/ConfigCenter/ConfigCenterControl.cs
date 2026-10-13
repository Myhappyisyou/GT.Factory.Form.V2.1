using GT.Factory.Common.Helpers;
using GT.Factory.Common.Interfaces;
using GT.Factory.UI.Style;
using GT.Factory.UI.Theme;
using System;
using System.Windows.Forms;

namespace GT.Factory.Management.UI.ConfigCenter
{
    /// <summary>
    /// 系统配置中心业务页面。
    ///
    /// 该控件本身不依赖 Form，可以被：
    /// 1. ConfigCenterForm 独立窗口承载；
    /// 2. GT.Factory.Server 主界面直接嵌入；
    ///
    /// 配置的实际管理由 ConfigManager.Providers 负责。
    /// 本页面只负责配置展示、编辑、保存和恢复默认值。
    /// </summary>
    public class ConfigCenterControl : UserControl
    {
        private TabControl _tabControl;
        private TextBox _txtSearch;
        private Button _btnSave;
        private Button _btnReset;

        /// <summary>
        /// 创建配置中心页面。
        /// </summary>
        public ConfigCenterControl()
        {
            InitializeUI();
            LoadTabs();
        }

        /// <summary>
        /// 初始化配置中心页面布局。
        /// </summary>
        private void InitializeUI()
        {
            BackColor = UITheme.BgBase;

            _txtSearch = new TextBox();
            _txtSearch.Dock = DockStyle.Top;
            _txtSearch.Height = 32;
            _txtSearch.Margin = new Padding(8);
            _txtSearch.TextChanged += TxtSearch_TextChanged;

            _tabControl = new TabControl();
            _tabControl.Dock = DockStyle.Fill;

            Panel bottomPanel = new Panel();
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Height = 48;

            _btnSave = UIButton.Primary("保存");
            _btnSave.Width = 100;
            _btnSave.Height = 32;
            _btnSave.Click += BtnSave_Click;

            _btnReset = UIButton.Default("恢复默认");
            _btnReset.Width = 100;
            _btnReset.Height = 32;
            _btnReset.Click += BtnReset_Click;

            bottomPanel.Controls.Add(_btnSave);
            bottomPanel.Controls.Add(_btnReset);

            _btnSave.Left = bottomPanel.Width - _btnSave.Width - 16;
            _btnReset.Left =
                _btnSave.Left - _btnReset.Width - 8;

            _btnSave.Anchor =
                AnchorStyles.Top | AnchorStyles.Right;

            _btnReset.Anchor =
                AnchorStyles.Top | AnchorStyles.Right;

            _btnSave.Top = 8;
            _btnReset.Top = 8;

            bottomPanel.Resize += delegate
            {
                _btnSave.Left =
                    bottomPanel.Width - _btnSave.Width - 16;

                _btnReset.Left =
                    _btnSave.Left - _btnReset.Width - 8;
            };

            Controls.Add(_tabControl);
            Controls.Add(_txtSearch);
            Controls.Add(bottomPanel);
        }

        /// <summary>
        /// 根据当前注册的配置提供者创建配置页签。
        /// </summary>
        private void LoadTabs()
        {
            _tabControl.TabPages.Clear();

            foreach (IConfigProvider provider
                in ConfigManager.Providers)
            {
                if (provider == null)
                    continue;

                TabPage page = new TabPage(provider.Name);

                PropertyGrid grid = new PropertyGrid();
                grid.Dock = DockStyle.Fill;
                grid.PropertySort = PropertySort.Categorized;
                grid.ToolbarVisible = false;
                grid.HelpVisible = true;
                grid.SelectedObject = provider.Config;

                page.Controls.Add(grid);

                _tabControl.TabPages.Add(page);
            }
        }

        /// <summary>
        /// 搜索配置。
        ///
        /// 当前 PropertyGrid 直接绑定配置对象，
        /// 不能继续使用旧版 CategoryWrapper。
        ///
        /// 因此这里暂时不改变 PropertyGrid 的 SelectedObject，
        /// 保留搜索入口，后续如果确实需要配置过滤，
        /// 再针对 PropertyGrid 做独立的过滤方案。
        /// </summary>
        private void TxtSearch_TextChanged(
            object sender,
            EventArgs e)
        {
            string keyword = _txtSearch.Text.Trim();

            if (keyword.Length == 0)
                return;

            // 当前 PropertyGrid 使用真实配置对象，
            // 不能直接通过 CategoryWrapper 进行过滤。
            //
            // 后续如果需要搜索：
            // 可以根据 PropertyDescriptor 创建过滤后的
            // PropertyGrid 对象，而不是修改配置对象本身。
        }

        /// <summary>
        /// 保存所有配置。
        /// </summary>
        private void BtnSave_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                foreach (IConfigProvider provider
                    in ConfigManager.Providers)
                {
                    if (provider == null)
                        continue;

                    provider.Save();
                }

                MessageBox.Show(
                    "所有配置保存成功。",
                    "配置中心",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "配置保存失败：" + ex.Message,
                    "配置中心",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 恢复所有配置默认值。
        /// </summary>
        private void BtnReset_Click(
            object sender,
            EventArgs e)
        {
            DialogResult result =
                MessageBox.Show(
                    "确定恢复所有配置的默认值吗？",
                    "恢复默认",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            foreach (IConfigProvider provider
                in ConfigManager.Providers)
            {
                if (provider == null)
                    continue;

                provider.Reset();
            }

            LoadTabs();
        }
    }
}
