using GT.Factory.LIN.Core;
using GT.Factory.LIN.Model;
using GT.Factory.LIN.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.LIN.ProjectDebug
{

    /// <summary>
    /// LIN调试运行环境
    ///
    /// 面向调试软件。
    ///
    /// 管理:
    ///
    /// Controller
    /// DebugSession
    ///
    /// 不负责:
    ///
    /// Trace
    /// Frame解析
    /// Signal数据
    ///
    /// Trace由LinDebugSession管理
    ///
    /// </summary>
    public class LinDebugRuntime
    {


        #region 私有字段


        /// <summary>
        /// LIN控制器
        /// </summary>
        private readonly LinController _controller;


        #endregion



        #region 属性


        /// <summary>
        /// 调试会话
        ///
        /// 包含:
        ///
        /// Frame
        /// Signal
        /// Trace
        ///
        /// </summary>
        public LinDebugSession Session
        {
            get;
            private set;
        }



        /// <summary>
        /// 当前运行状态
        /// </summary>
        public bool IsRunning
        {
            get;
            private set;
        }


        #endregion




        #region 构造


        /// <summary>
        /// 创建LIN调试运行环境
        /// </summary>
        public LinDebugRuntime(
            LinProject project)
        {

            if (project == null)
                throw new ArgumentNullException(
                    nameof(project));

            _controller =
                project.Controller;


            Session =
                new LinDebugSession(
                    project);


        }


        #endregion




        #region 运行控制



        /// <summary>
        /// 启动LIN调试
        /// </summary>
        public void Start(
            string schedule)
        {


            _controller.Start(
                schedule);



            IsRunning = true;


        }





        /// <summary>
        /// 停止LIN调试
        /// </summary>
        public void Stop()
        {


            _controller.Stop();



            IsRunning = false;


        }


        #endregion


    }

}