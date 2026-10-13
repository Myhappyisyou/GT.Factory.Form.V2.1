using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using GT.Factory.Station.Helper.LanModelSync;
using GT.Factory.Station.Model;
using GT.Factory.Common.Helpers;

namespace GT.Factory.Station.OP120
{
    public partial class MainForm : BaseMainForm
    {
        protected ControlEngine controlEngine;

        private readonly AppRuntime _runtime;

        public MainForm(AppRuntime runtime)
            : base(runtime)
        {
            this.Text = "MES_OP120";

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
        //    await controlEngine.Init();
        //}


        protected override ControlEngineBase CreateControlEngine()
        {
            return new ControlEngine();
        }

    }
}