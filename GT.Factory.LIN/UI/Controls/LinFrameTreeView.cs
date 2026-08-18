using GT.Factory.LIN.ProjectDebug;
using GT.Factory.UI.Style;
using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace GT.Factory.LIN.UI.Controls
{

    /// <summary>
    /// LIN Frame树控件
    ///
    /// 显示:
    ///
    /// Frame
    ///   |
    ///   + Signal
    ///
    ///
    /// 继承:
    ///
    /// TreeView
    ///
    ///
    /// 样式来源:
    ///
    /// GT.Factory.UI.Style.UITree
    ///
    ///
    /// 注意:
    ///
    /// 本控件只负责:
    ///
    /// 1. LIN数据展示
    ///
    /// 2. Frame/Signal节点管理
    ///
    ///
    /// 不负责:
    ///
    /// 1. LIN解析
    ///
    /// 2. LIN通信
    ///
    /// </summary>
    public class LinFrameTreeView :
        TreeView
    {

        #region Event


        /// <summary>
        /// Frame选择事件
        ///
        /// UI层通知外部:
        ///
        /// 用户选择了哪个Frame
        ///
        /// 不负责处理Signal
        ///
        /// </summary>
        public event Action<LinFrameInfo> FrameSelected;


        #endregion


        /// <summary>
        /// 创建LIN Frame树
        ///
        /// 完全代码创建
        ///
        /// 不使用Designer
        ///
        /// </summary>
        public LinFrameTreeView()
        {

            //
            // 创建标准TreeView
            //
            // 使用统一工业UI样式
            //
            var styleTree =
                UITree.Create();

            //
            // 应用基础属性
            //
            ApplyStyle(
                styleTree);


            //
            // Frame选择事件
            //
            AfterSelect +=
                LinFrameTreeView_AfterSelect;
        }



        /// <summary>
        /// 应用公共TreeView样式
        ///
        /// 说明:
        ///
        /// GT.Factory.UI负责视觉规范
        ///
        /// LIN UI负责业务扩展
        ///
        /// </summary>
        private void ApplyStyle(
            TreeView styleTree)
        {

            Dock =
                styleTree.Dock;

            BorderStyle =
                styleTree.BorderStyle;

            BackColor =
                styleTree.BackColor;

            ForeColor =
                styleTree.ForeColor;

            Font =
                styleTree.Font;

            FullRowSelect =
                styleTree.FullRowSelect;

            HideSelection =
                styleTree.HideSelection;

            HotTracking =
                styleTree.HotTracking;

            ShowLines =
                styleTree.ShowLines;

            ShowPlusMinus =
                styleTree.ShowPlusMinus;

            ShowRootLines =
                styleTree.ShowRootLines;

            ItemHeight =
                styleTree.ItemHeight;

        }

        /// <summary>
        /// 加载Frame数据
        ///
        /// 数据来源:
        ///
        /// LinDebugSession.Frames
        ///
        /// </summary>
        public void LoadFrames(
            List<LinFrameInfo> frames)
        {

            Nodes.Clear();

            if (frames == null)
                return;

            foreach (var frame in frames)
            {

                //
                // Frame节点
                //
                var frameNode =
                    new TreeNode(
                        frame.Name);

                //
                // 保存对象
                //
                // 后续点击可以直接获取
                //
                frameNode.Tag =
                    frame;

                foreach (var signal in frame.Signals)
                {

                    //
                    // Signal节点
                    //
                    var signalNode =
                        new TreeNode(
                            signal.Name);

                    signalNode.Tag =
                        signal;

                    frameNode.Nodes.Add(
                        signalNode);

                }

                Nodes.Add(
                    frameNode);

            }

        }


        /// <summary>
        /// Tree节点选择事件
        ///
        /// 支持:
        ///
        /// Frame节点
        ///
        /// Signal节点
        ///
        ///
        /// 最终统一输出:
        ///
        /// LinFrameInfo
        ///
        /// </summary>
        private void LinFrameTreeView_AfterSelect(
            object sender,
            TreeViewEventArgs e)
        {

            Console.WriteLine(
        $"Tree Select:{DateTime.Now:HH:mm:ss.fff}");
            if (e.Node == null)
                return;


            //
            // 情况1:
            // Frame节点
            //
            var frame =
                e.Node.Tag
                as LinFrameInfo;


            if (frame != null)
            {

                FrameSelected?.Invoke(
                    frame);


                return;

            }


            //
            // 情况2:
            // Signal节点
            //
            var signal =
                e.Node.Tag
                as LinSignalInfo;


            if (signal != null)
            {

                //
                // 通过Signal找到所属Frame
                //
                if (signal.Frame != null)
                {

                    FrameSelected?.Invoke(
                        signal.Frame);

                }

            }

        }
    }

}