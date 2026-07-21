using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using EditJson;
using System.IO;
using System.Collections.Generic;
using GT_Common;
using GT_Common.DriverForm.Batch;
using GT_Common.Helper;
using GT_Common.Helper.LanModelSync;
using GT_Common.Model;
using GT_Common.MyEnum;

namespace OP050
{
    public partial class MainForm : BaseMainForm
    {
        protected ControlEngine controlEngine;

        public MainForm() : base()
        {
            this.Text = "MES_OP050";
        }

        // 重写配置数据表格列
        protected override void ConfigureDataUIColumns()
        {
            dataUI.SetColumns(new List<(string, int, Func<TestDispItem, string>)>
            {
                ("序号", 60, item => null),
                ("工单", 120, item => item.OrderNub),
                ("管壳码", 200, item => item.MainBar),
                ("测试人", 100, item => item.UserName),
                ("完成时间", 180, item => item.DoTime),
                ("节拍", 60, item => item.TaktTime),
                ("测试结果", 100, item => item.Ok_flag),
                ("MES上传结果", 140, item => item.MesResult),
            });
        }

        // 重写菜单，添加 OP050 特有菜单
        protected override void IniteMenu()
        {
            base.IniteMenu();

            this.MainMenuStrip.Items.Add(CreateBatchMenu());      // 新增批次码菜单
        }

        // 重写初始化
        protected override void InitControlEngine()
        {
            if (controlEngine != null)
            {
                controlEngine.Shutdown();
                controlEngine = null;
            }

            Shared.productModel = _jsonService.LoadFromFile<ProductModel>(
                PathCenter.ConfigFile(Path.Combine("model", $"{Shared.productName}.json")));

            controlEngine = new ControlEngine();
            controlEngine.Init();
        }

        // 新增批次码菜单
        protected virtual ToolStripMenuItem CreateBatchMenu()
        {
            var menu = new ToolStripMenuItem("批次码");
            AddMenuItem(menu, "扫批次码", tsm_BatchScanFormClicked);
            AddMenuItem(menu, "配置批次码", tsm_BatchConfigFormClicked);
            AddMenuItem(menu, "批次运行管理", tsm_BatchRuntimeFormClicked);
            return menu;
        }

        #region OP010 特有菜单事件

        /// <summary>
        /// 扫批次码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_BatchScanFormClicked(object sender, EventArgs e)
        {
            if (BatchScanForm?.IsDisposed != false)
                BatchScanForm = new BatchScanForm(Shared.productName);
            BatchScanForm.Show();
        }

        /// <summary>
        /// 批次号配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_BatchConfigFormClicked(object sender, EventArgs e)
        {
            AuthHelper.RequireLogin(UserLevel.PE, LocalConfig.Instance.UIDIP, LocalConfig.Instance.UIDPort, user =>
            {
                if (BatchConfigForm?.IsDisposed != false)
                    BatchConfigForm = new BatchConfigForm();
                BatchConfigForm.Show();
            });
        }

        /// <summary>
        /// 批次码管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_BatchRuntimeFormClicked(object sender, EventArgs e)
        {
            if (BatchRuntimeForm?.IsDisposed != false)
                BatchRuntimeForm = new BatchRuntimeForm();
            BatchRuntimeForm.Show();
        }
        #endregion
    }
}