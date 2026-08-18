using GT.Factory.LIN.ProjectDebug;
using GT.Factory.UI.Style;
using GT.Factory.UI.Theme;
using System;
using System.Windows.Forms;


namespace GT.Factory.LIN.UI.Controls
{


    /// <summary>
    /// LIN调试状态栏
    ///
    /// 用于显示:
    ///
    /// Frame数量
    ///
    /// Signal数量
    ///
    /// Trace数量
    ///
    ///
    /// 数据来源:
    ///
    /// LinDebugSession
    ///
    ///
    /// 注意:
    ///
    /// 本控件:
    ///
    /// 只显示
    ///
    ///
    /// 不负责:
    ///
    /// LIN启动
    ///
    /// LIN停止
    ///
    /// Frame发送
    ///
    /// Signal修改
    ///
    ///
    /// UI与LIN Core隔离
    ///
    /// </summary>
    public class LinStatusBar :
        UserControl
    {



        #region 控件字段



        /// <summary>
        /// Frame数量显示
        /// </summary>
        private Label _frameLabel;



        /// <summary>
        /// Signal数量显示
        /// </summary>
        private Label _signalLabel;



        /// <summary>
        /// Trace数量显示
        /// </summary>
        private Label _traceLabel;



        #endregion






        #region 构造



        /// <summary>
        /// 创建状态栏
        ///
        /// 全代码创建
        ///
        /// 无Designer
        ///
        /// </summary>
        public LinStatusBar()
        {

            Initialize();

        }



        #endregion








        #region 初始化



        /// <summary>
        /// 初始化UI
        ///
        /// </summary>
        private void Initialize()
        {


            //
            // 基础属性
            //
            Dock =
                DockStyle.Bottom;


            Height =
                32;


            BackColor =
                UITheme.Card;



            //
            // 横向布局
            //
            var panel =
                new FlowLayoutPanel();



            panel.Dock =
                DockStyle.Fill;



            panel.FlowDirection =
                FlowDirection.LeftToRight;



            panel.WrapContents =
                false;



            panel.Padding =
                new Padding(
                    10,
                    7,
                    10,
                    5);




            //
            // 创建Label
            //
            _frameLabel =
                CreateLabel();



            _signalLabel =
                CreateLabel();



            _traceLabel =
                CreateLabel();





            panel.Controls.Add(
                _frameLabel);



            panel.Controls.Add(
                _signalLabel);



            panel.Controls.Add(
                _traceLabel);





            Controls.Add(
                panel);



        }








        /// <summary>
        /// 创建状态文字
        ///
        /// 使用公共UI字体
        ///
        /// </summary>
        private Label CreateLabel()
        {


            return new Label
            {


                AutoSize =
                    true,


                Font =
                    UITheme.BodyFont,


                ForeColor =
                    UITheme.Text,


                Margin =
                    new Padding(
                        15,
                        3,
                        15,
                        3)


            };


        }



        #endregion







        #region 更新状态



        /// <summary>
        /// 更新状态显示
        ///
        /// 调试周期调用
        ///
        /// </summary>
        public void UpdateStatus(
            LinDebugSession session)
        {


            if (session == null)
                return;



            //
            // Frame数量
            //
            _frameLabel.Text =
                "Frames: " +
                session.Frames.Count;




            //
            // Signal数量
            //
            _signalLabel.Text =
                "Signals: " +
                session.Signals.Count;




            //
            // Trace数量
            //
            _traceLabel.Text =
                "Trace: " +
                session.Trace.Items.Count;



        }



        #endregion



    }


}
