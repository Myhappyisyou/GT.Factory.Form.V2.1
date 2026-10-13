using GT.Factory.LIN.Model;
using GT.Factory.LIN.ProjectDebug;
using GT.Factory.UI.Theme;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.LIN.UI.Controls
{
    /// <summary>
    /// LIN 调试监控面板（原 LinDebugForm.Initialize 中内联的主布局）。
    ///
    /// 布局：
    ///
    ///   +------------------+----------------------------+
    ///   |   FrameTree      |         TraceGrid           |   40%
    ///   +------------------+----------------------------+
    ///   |               SignalPanel                      |   30%
    ///   +-------------------------------------------------+
    ///   |              FrameDataPanel                     |   30%
    ///   +-------------------------------------------------+
    ///
    /// 职责：
    /// 只负责“实时调试”这一组视图的组合和 Session 绑定，
    /// 不关心自己被放在 Form 根节点还是 TabPage 里。
    ///
    /// 这样 LinDebugForm 只需要：
    ///   tabPage_Debug.Controls.Add(new LinDebugMonitorPanel());
    /// </summary>
    public class LinDebugMonitorPanel : UserControl, ILinDebugSessionView
    {
        #region 控件

        private LinFrameTreeView _frameTree;
        private LinTraceGrid _traceGrid;
        private LinSignalPanel _signalPanel;
        private LinFrameDataPanel _frameDataPanel;
        private SplitContainer _topSplit;

        #endregion

        #region 数据

        private LinDebugSession _session;

        #endregion

        public LinDebugMonitorPanel()
        {
            Dock = DockStyle.Fill;
            BackColor = UITheme.BgBase;

            var main = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 40)); // Frame / Trace
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 30)); // Signal
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 30)); // FrameData

            _topSplit = new SplitContainer { Dock = DockStyle.Fill };

            // SplitterDistance 必须在控件有真实尺寸之后设置
            HandleCreated += (s, e) =>
            {
                if (_topSplit.Width > _topSplit.Panel1MinSize + _topSplit.Panel2MinSize)
                {
                    _topSplit.SplitterDistance = 280;
                }
            };

            _frameTree = new LinFrameTreeView();
            _topSplit.Panel1.Controls.Add(_frameTree);

            _traceGrid = new LinTraceGrid();
            _topSplit.Panel2.Controls.Add(_traceGrid);

            main.Controls.Add(_topSplit, 0, 0);

            _signalPanel = new LinSignalPanel();
            main.Controls.Add(_signalPanel, 0, 1);

            _frameDataPanel = new LinFrameDataPanel();
            main.Controls.Add(_frameDataPanel, 0, 2);

            Controls.Add(main);

            _frameTree.FrameSelected += FrameTree_FrameSelected;
            _signalPanel.SignalValueChanged += SignalPanel_SignalValueChanged;
        }

        #region ILinDebugSessionView

        public void BindSession(LinDebugSession session)
        {
            _session = session;

            if (session == null)
            {
                ClearSession();
                return;
            }

            _frameTree.LoadFrames(session.Frames);
            _signalPanel.LoadSignals(session.Signals);
            _frameDataPanel.LoadFrames(session.Frames);
            _traceGrid.UpdateTrace(session.Trace.Items);

            session.TraceAdded -= Session_TraceAdded;
            session.TraceAdded += Session_TraceAdded;

            session.FrameChanged -= Session_FrameChanged;
            session.FrameChanged += Session_FrameChanged;
        }

        public void UnbindSession(LinDebugSession session)
        {
            if (session == null) return;

            session.TraceAdded -= Session_TraceAdded;
            session.FrameChanged -= Session_FrameChanged;
        }

        public void ClearSession()
        {
            _frameTree.LoadFrames(null);
            _signalPanel.LoadSignals(null);
            _frameDataPanel.LoadFrames(null);
            _traceGrid.UpdateTrace(null);
        }

        #endregion

        #region 内部事件（不再需要 Form 转发）

        private void FrameTree_FrameSelected(LinFrameInfo frame)
        {
            if (frame == null) return;
            _signalPanel.ShowFrame(frame);
        }

        private void SignalPanel_SignalValueChanged(string name, object value)
        {
            _session?.SetSignalValue(name, value);
        }

        private void Session_TraceAdded(LinTraceItem item)
        {
            _traceGrid.AddTrace(item);
        }

        private void Session_FrameChanged(LinFrameInfo frame)
        {
            _frameDataPanel.UpdateFrame(frame);
        }

        #endregion
    }
}
