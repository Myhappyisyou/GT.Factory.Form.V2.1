using GT.Factory.LIN.Hardware;
using GT.Factory.LIN.Model;
using GT.Factory.LIN.Project;
using GT.Factory.LIN.ProjectDebug;
using System;
using System.Net.NetworkInformation;
using System.Windows.Forms;


namespace GT.Factory.LIN.UI
{


    /// <summary>
    /// LIN调试主机
    ///
    /// 是LIN调试界面的启动入口。
    ///
    ///
    /// 类似:
    ///
    /// CANoe Application Host
    ///
    ///
    /// 负责:
    ///
    /// 1. 创建LIN工程
    ///
    /// 2. 创建Debug Session
    ///
    /// 3. 启动Controller
    ///
    /// 4. 显示Debug Form
    ///
    ///
    /// 不负责:
    ///
    /// 1. LIN通信
    ///
    /// 2. Frame处理
    ///
    /// 3. Signal解析
    ///
    ///
    /// 所有LIN逻辑仍然属于:
    ///
    /// GT.Factory.LIN.Core
    ///
    /// </summary>
    public class LinDebugHost
    {

        #region 私有字段

        /// <summary>
        /// LIN工程
        ///
        /// 一个LDF对应一个工程
        ///
        /// </summary>
        private LinProject _project;

        /// <summary>
        /// 调试Session
        ///
        /// UI数据入口
        ///
        /// </summary>
        private LinDebugSession _session;

        /// <summary>
        /// 调试窗体
        ///
        /// </summary>
        private LinDebugForm _form;

        #endregion

        #region 启动

        /// <summary>
        /// 启动LIN调试软件
        ///
        ///
        /// 调用流程:
        ///
        /// Load LDF
        ///
        ///     |
        ///
        /// LinProject
        ///
        ///     |
        ///
        /// LinDebugSession
        ///
        ///     |
        ///
        /// LinDebugForm
        ///
        ///
        /// </summary>
        public void Start(
            string projectFile,
            ILinAdapter adapter)
        {

            if (string.IsNullOrWhiteSpace(projectFile))
            {
                throw new ArgumentNullException(
                    nameof(projectFile));
            }


            if (adapter == null)
            {
                throw new ArgumentNullException(
                    nameof(adapter));
            }



            //
            // 创建工程
            //
            //
            // 1.加载LIN工程
            //
            // .linproj
            //
            //      |
            //      |
            // LinProject
            //
            _project =
                LinProjectLoader.Load(
                    projectFile,
                    adapter);

            //
            // 创建调试Session
            //
            //
            // 注意:
            //
            // UI只认识Session
            //
            //
            _session =
                new LinDebugSession(
                    _project);

            //
            // 启动LIN
            //
            //
            // Controller属于Core
            //
            // UI不关心内部
            //
            string schedule = null;


            if (_project.Mode == LinRunMode.Simulation)
            {
                if (_project.SimulationConfig != null)
                {
                    schedule =
                        _project.SimulationConfig.Schedule;
                }

            }

            _project.Controller.Start(schedule);

            //
            // 创建界面
            //
            _form =
                new LinDebugForm(
                    _session);

            //
            // 显示
            //
            Application.Run(
                _form);

        }



        #endregion

        #region 停止

        /// <summary>
        /// 停止LIN调试
        ///
        /// 释放资源
        ///
        /// </summary>
        public void Stop()
        {

            //
            // 停止窗体
            //
            if (_form != null)
            {

                if (!_form.IsDisposed)
                {
                    _form.Close();
                }

            }

            //
            // 停止LIN
            //
            if (_project != null)
            {

                _project.Controller.Stop();

            }

            _session =
                null;

            _project =
                null;

            _form =
                null;

        }

        #endregion

        #region 属性

        /// <summary>
        /// 当前LIN工程
        ///
        /// 调试工具可以访问
        ///
        /// </summary>
        public LinProject Project
        {
            get
            {
                return _project;
            }
        }

        /// <summary>
        /// 当前调试Session
        ///
        /// </summary>
        public LinDebugSession Session
        {
            get
            {
                return _session;
            }
        }

        /// <summary>
        /// 当前窗口
        ///
        /// </summary>
        public LinDebugForm Form
        {
            get
            {
                return _form;
            }
        }

        #endregion

    }

}