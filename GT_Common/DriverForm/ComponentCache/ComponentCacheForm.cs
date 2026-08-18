using GT.Factory.UI.Apply;
using GT.Factory.UI.Enum;
using GT.Factory.UI.PageTemplate;
using GT.Factory.UI.Style;
using GT_Common;
using GT_Common.DriverForm.CacheViewer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.DriverForm.ComponentCache
{


    public partial class ComponentCacheForm : Form
    {
        private UIToolPage page;

        private ComponentManager componentManager;

        public ComponentCacheForm()
        {
            this.componentManager = new ComponentManager();
            componentManager.Load();
            Text = "缓存内容查看器";
            Width = 1000;
            Height = 600;
            StartPosition = FormStartPosition.CenterScreen;

            InitUI();

        }

        private void InitUI()
        {
            page = new UIToolPage();

            var btnRefresh = UIButton.Primary("刷新");
            btnRefresh.Click += (s, e) => RefreshGrid();
            page.AddButton(btnRefresh, true);

            Controls.Add(page);

            Load += (s, e) => RefreshGrid();
        }

        private void RefreshGrid()
        {
            try
            {
                page.SetState(UIPageState.Loading);

                componentManager.Load();

                var data = componentManager._buffer?.Items
                    ?.Select(x => new
                    {
                        x.AssemblyCode,
                        x.ComponentCode,
                        x.Timestamp
                    })
                    .ToList();

                if (data == null || data.Count == 0)
                {
                    page.SetState(UIPageState.Empty, "暂无缓存数据");
                    return;
                }

                page.Grid.DataSource = data;

                page.SetState(UIPageState.Data);
            }
            catch (Exception ex)
            {
                page.SetState(UIPageState.Error, ex.Message);
            }
        }

    }

}
