using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using GT.Factory.Station.DriverForm.Batch;
using GT.Factory.Station.DriverForm.Keithley2790;
using GT.Factory.Station.Helper.LanModelSync;
using GT.Factory.Common.Enums;
using GT.Factory.Station.Model;
using GT.Factory.Common.Helpers;
using GT.Factory.Login;

namespace GT.Factory.Station.OP110
{
    public partial class MainForm : BaseMainForm
    {
        private Keithley2790Form keithley2790Form;

        protected ControlEngine controlEngine;

        private readonly AppRuntime _runtime;

        public MainForm(AppRuntime runtime)
            : base(runtime)
        {
            this.Text = "MES_OP110";

            if (runtime == null)
                throw new ArgumentNullException("runtime");

            _runtime = runtime;

        }

        // 重写配置数据表格列
        protected override void ConfigureDataUIColumns()
        {
            dataUI.SetColumns(new List<(string, int, Func<TestDispItem, string>)>
            {
                ("序号", 60, item => item.OrderNub),
                ("工单", 120, item => item.OrderNub),
                ("管壳码", 200, item => item.MainBar),
                ("扩散器码", 200, item => item.PartBar),
                ("测试人", 100, item => item.UserName),
                ("完成时间", 180, item => item.DoTime),
                ("节拍", 60, item => item.TaktTime),
                ("测试结果", 100, item => item.Ok_flag),
                ("MES上传结果", 140, item => item.MesResult),
            });
        }

        // 重写菜单，添加 OP110 特有菜单
        protected override void IniteMenu()
        {
            base.IniteMenu();

            this.MainMenuStrip.Items.Add(CreateBatchMenu());      // 新增批次码菜单
        }

        // 重写初始化
        //protected override async Task InitControlEngine()
        //{
        //    if (controlEngine != null)
        //    {
        //        controlEngine.Shutdown();
        //        controlEngine = null;
        //    }

        //    Shared.productModel = _jsonService.LoadFromFile<ProductModel>(
        //        PathCenter.ConfigFile(Path.Combine("model", $"{Shared.productName}.json")));

        //    controlEngine = new ControlEngine();
        //    controlEngine.Init();
        //}

        protected override ControlEngineBase CreateControlEngine()
        {
            return new ControlEngine();
        }

        // 重写仪器仪表菜单，添加更多子项
        protected override ToolStripMenuItem CreateDriverMenu()
        {
            var menu = base.CreateDriverMenu();
            AddMenuItem(menu, "电阻测试仪", tsm_KeithleyFormClicked);
            return menu;
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

        #region OP110 特有菜单事件

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
            AuthHelper.RequireLogin(UserLevel.PE,  user =>
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

        /// <summary>
        /// 电阻测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_KeithleyFormClicked(object sender, EventArgs e)
        {
            if (keithley2790Form?.IsDisposed != false)
                keithley2790Form = new Keithley2790Form();
            keithley2790Form.Show();
        }
        #endregion
    }
}