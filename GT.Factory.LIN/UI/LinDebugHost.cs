using GT.Factory.LIN.Hardware;
using GT.Factory.LIN.Model;
using GT.Factory.LIN.Project;
using GT.Factory.LIN.ProjectDebug;
using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows.Forms;


namespace GT.Factory.LIN.UI
{
    /// <summary>
    /// LIN Debug 主机。
    ///
    /// 负责：
    ///
    /// Project 生命周期：
    ///     OpenProject
    ///     CloseProject
    ///
    /// Runtime 生命周期：
    ///     StartRuntime
    ///     StopRuntime
    ///
    /// 不负责：
    ///
    ///     LIN协议实现
    ///     Signal解析
    ///     Frame Encode / Decode
    ///     Runtime内部逻辑
    ///     DebugSession内部逻辑
    /// </summary>
    public class LinDebugHost
    {
        #region 私有字段

        /// <summary>
        /// 当前LIN工程。
        /// </summary>
        private LinProject _project;

        /// <summary>
        /// 当前Debug Session。
        /// </summary>
        private LinDebugSession _session;

        /// <summary>
        /// 当前Debug窗口。
        /// </summary>
        private LinDebugForm _form;

        /// <summary>
        /// 当前LIN适配器。
        /// </summary>
        private ILinAdapter _adapter;

        #endregion


        #region 启动

        /// <summary>
        /// 启动LIN Debug软件。
        ///
        /// 注意：
        ///
        /// 软件启动时不打开任何工程。
        ///
        /// 只创建Debug窗口。
        /// </summary>
        public void Start(
            ILinAdapter adapter)
        {
            if (adapter == null)
            {
                throw new ArgumentNullException(
                    nameof(adapter));
            }

            _adapter =
                adapter;

            //
            // 创建空的Debug窗口。
            //
            _form =
                new LinDebugForm();

            //
            // 工程打开请求。
            //
            _form.ProjectOpenRequested +=
                Form_ProjectOpenRequested;

            //
            // 工程关闭请求。
            //
            _form.ProjectCloseRequested +=
               Form_ProjectCloseRequested;

            //
            // 工程开始请求。
            //
            _form.RuntimeStartRequested +=
                Form_RuntimeStartRequested;

            //
            // 工程停止请求。
            //
            _form.RuntimeStopRequested +=
                Form_RuntimeStopRequested;

            //
            // 显示软件。
            //
            Application.Run(
                _form);

            //
            // 主窗口关闭后，
            // 确保Runtime停止。
            //
            StopRuntime();

            //
            // Application.Run结束后，
            // 确保当前LIN工程停止。
            //
            StopCurrentProject();

            //
            // 清理引用。
            //
            _session = null;
            _project = null;
            _form = null;
            _adapter = null;
        }




        /// <summary>
        /// 根据指定工程启动Debug。
        ///
        /// 该重载保留。
        ///
        /// 主要用于：
        ///
        /// 1. 测试
        /// 2. 外部宿主
        /// 3. 后续自动打开工程
        ///
        /// 正常软件启动入口不使用该方法。
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

            _adapter =
                adapter;

            //
            // 创建Debug窗口。
            //
            _form =
                new LinDebugForm();

            //
            // 工程打开请求。
            //
            _form.ProjectOpenRequested +=
                Form_ProjectOpenRequested;

            //
            // 工程关闭请求。
            //
            _form.ProjectCloseRequested +=
                Form_ProjectCloseRequested;

            //
            // Runtime启动请求。
            //
            _form.RuntimeStartRequested +=
                Form_RuntimeStartRequested;

            //
            // Runtime停止请求。
            //
            _form.RuntimeStopRequested +=
                Form_RuntimeStopRequested;

            //
            // 先加载指定工程。
            //
            OpenProject(
                projectFile);

            //
            // 如果工程加载失败，
            // 仍然允许进入空的Debug界面。
            //
            Application.Run(
                _form);

            StopCurrentProject();

            _session = null;
            _project = null;
            _form = null;
            _adapter = null;
        }

        #endregion


        #region Project生命周期

        /// <summary>
        /// Form请求打开工程。
        /// </summary>
        private void Form_ProjectOpenRequested(
            object sender,
            EventArgs e)
        {
            string projectFile =
                SelectProjectFile();

            if (string.IsNullOrWhiteSpace(
                projectFile))
            {
                return;
            }

            OpenProject(
                projectFile);
        }

        /// <summary>
        /// 加载并切换LIN工程。
        ///
        /// 这是当前Project Debug唯一的工程切换入口。
        /// </summary>
        private void OpenProject(
            string projectFile)
        {
            if (string.IsNullOrWhiteSpace(
                projectFile))
            {
                return;
            }

            string fullPath;

            try
            {
                fullPath =
                    Path.GetFullPath(
                        projectFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "工程路径无效：\r\n\r\n" +
                    ex.Message,
                    "打开LIN工程",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            if (!File.Exists(fullPath))
            {
                MessageBox.Show(
                    "工程文件不存在：\r\n\r\n" +
                    fullPath,
                    "打开LIN工程",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (_adapter == null)
            {
                MessageBox.Show(
                    "LIN适配器尚未初始化。",
                    "打开LIN工程",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }


            LinProject newProject;

            //
            // 第一步：
            //
            // 先加载新工程。
            //
            // 只有新工程加载成功，
            // 才停止旧工程。
            //
            try
            {
                newProject =
                    LinProjectLoader.Load(
                        fullPath,
                        _adapter);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "加载LIN工程失败：\r\n\r\n" +
                    ex.Message,
                    "打开LIN工程",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }


            LinDebugSession newSession;

            //
            // 第二步：
            //
            // 创建新的Debug Session。
            //
            try
            {
                newSession =
                    new LinDebugSession(
                        newProject);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "创建LIN Debug Session失败：\r\n\r\n" +
                    ex.Message,
                    "打开LIN工程",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                //
                // 新Project还没有成为当前Project，
                // 因此这里不影响旧Project。
                //
                return;
            }


            //
            // 第三步：
            //
            // 新工程和Session都准备完成后，
            // 再停止旧工程。
            //
            StopCurrentProject();


            //
            // 第四步：
            //
            // 切换当前Project和Session。
            //
            _project =
                newProject;

            _session =
                newSession;


            //
            // 第五步：
            //
            // 启动新Project。
            //
            //StartController(
            //    _project);


            // 第五步：
            //
            // 通知Form切换Session。
            //
            // 注意：
            //
            // 打开工程只负责加载Project和Session，
            // 不负责启动LIN Runtime。
            //
            // Runtime必须由：
            //
            //     通信 → 启动
            //
            // 单独触发。
            //
            if (_form != null &&
                !_form.IsDisposed)
            {
                _form.ReplaceSession(
                    _session);
            }
        }


        /// <summary>
        /// Form请求关闭工程。
        /// </summary>
        private void Form_ProjectCloseRequested(
            object sender,
            EventArgs e)
        {
            CloseProject();
        }


        /// <summary>
        /// 用户主动关闭当前工程。
        /// </summary>
        private void CloseProject()
        {
            if (_project == null &&
                _session == null)
            {
                return;
            }


            //
            // 如果正在运行，
            // 先停止Runtime。
            //
            StopRuntime();


            //
            // 清除Project。
            //
            CloseProjectInternal();


            //
            // 清除UI。
            //
            if (_form != null &&
                !_form.IsDisposed)
            {
                _form.ClearSession();
            }
        }


        /// <summary>
        /// 内部关闭Project。
        ///
        /// 不负责UI。
        /// </summary>
        private void CloseProjectInternal()
        {
            _session =
                null;

            _project =
                null;
        }

        /// <summary>
        /// 启动当前Project的Controller。
        /// </summary>
        private void StartController(
            LinProject project)
        {
            if (project == null)
            {
                return;
            }

            string schedule =
                null;

            if (project.Mode ==
                LinRunMode.Simulation)
            {
                if (project.SimulationConfig != null)
                {
                    schedule =
                        project.SimulationConfig.Schedule;
                }
            }

            project.Controller.Start(
                schedule);
        }


        /// <summary>
        /// 停止当前Project。
        /// </summary>
        private void StopCurrentProject()
        {
            if (_project == null)
            {
                return;
            }

            if (_project.Controller == null)
            {
                return;
            }

            try
            {
                _project.Controller.Stop();
            }
            catch
            {
                //
                // 当前阶段不在Host中处理Controller异常。
                //
                // Runtime生命周期后续单独治理。
                //
            }
        }

        #endregion

        #region Runtime生命周期

        /// <summary>
        /// Form请求启动LIN。
        /// </summary>
        private void Form_RuntimeStartRequested(
            object sender,
            EventArgs e)
        {
            StartRuntime();
        }


        /// <summary>
        /// Form请求停止LIN。
        /// </summary>
        private void Form_RuntimeStopRequested(
            object sender,
            EventArgs e)
        {
            StopRuntime();
        }


        /// <summary>
        /// 启动当前Project Runtime。
        /// </summary>
        private void StartRuntime()
        {
            if (_project == null)
            {
                MessageBox.Show(
                    "当前没有打开LIN工程。",
                    "启动LIN",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }


            if (_project.Controller == null)
            {
                MessageBox.Show(
                    "当前工程没有可用的LIN Controller。",
                    "启动LIN",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }


            try
            {
                string schedule =
                    null;

                if (_project.SimulationConfig != null)
                {
                    schedule =
                        _project.SimulationConfig.Schedule;
                }


                _project.Controller.Start(
                    schedule);


                if (_form != null &&
                    !_form.IsDisposed)
                {
                    _form.SetRuntimeRunning(
                        true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "启动LIN失败：\r\n\r\n" +
                    ex.Message,
                    "启动LIN",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// 停止当前Project Runtime。
        /// </summary>
        private void StopRuntime()
        {
            if (_project == null)
            {
                return;
            }

            if (_project.Controller == null)
            {
                return;
            }


            try
            {
                _project.Controller.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "停止LIN失败：\r\n\r\n" +
                    ex.Message,
                    "停止LIN",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }


            if (_form != null &&
                !_form.IsDisposed)
            {
                _form.SetRuntimeRunning(
                    false);
            }
        }


        #endregion


        #region 工程选择

        /// <summary>
        /// 显示LIN工程选择对话框。
        /// </summary>
        private string SelectProjectFile()
        {
            using (var dialog =
                new OpenFileDialog())
            {
                dialog.Title =
                    "打开LIN工程";

                dialog.Filter =
                    "LIN工程文件 (*.linproj)|*.linproj|所有文件 (*.*)|*.*";

                dialog.FilterIndex =
                    1;

                dialog.CheckFileExists =
                    true;

                dialog.Multiselect =
                    false;

                dialog.InitialDirectory =
                    Application.StartupPath;

                if (dialog.ShowDialog(
                    _form) != DialogResult.OK)
                {
                    return null;
                }

                return dialog.FileName;
            }
        }



        #endregion


        #region 停止

        /// <summary>
        /// 停止LIN Debug。
        /// </summary>
        public void Stop()
        {
            StopCurrentProject();

            if (_form != null &&
                !_form.IsDisposed)
            {
                _form.Close();
            }

            _session =
                null;

            _project =
                null;

            _form =
                null;

            _adapter =
                null;
        }

        #endregion


        #region 属性

        /// <summary>
        /// 当前LIN工程。
        /// </summary>
        public LinProject Project
        {
            get
            {
                return _project;
            }
        }


        /// <summary>
        /// 当前Debug Session。
        /// </summary>
        public LinDebugSession Session
        {
            get
            {
                return _session;
            }
        }


        /// <summary>
        /// 当前Debug窗口。
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
