using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GT.Factory.LIN.ProjectDebug;


namespace GT.Factory.LIN.UI.ViewModel
{


    /// <summary>
    /// LIN调试界面ViewModel
    ///
    /// 作用:
    ///
    /// 连接:
    ///
    /// LinDebugSession
    ///
    /// 与
    ///
    /// WinForm UI
    ///
    ///
    /// 注意:
    ///
    /// 这里不创建控件
    ///
    /// 不引用:
    ///
    /// Button
    /// DataGridView
    /// TreeView
    ///
    /// </summary>
    public class LinDebugViewModel :
        INotifyPropertyChanged
    {

        #region 私有字段

        /// <summary>
        /// LIN调试会话
        ///
        /// 来自:
        ///
        /// GT.Factory.LIN.ProjectDebug
        ///
        /// </summary>
        private readonly LinDebugSession _session;

        #endregion

        #region 属性

        /// <summary>
        /// 当前选中的Frame
        /// </summary>
        private LinFrameInfo _selectedFrame;

        public LinFrameInfo SelectedFrame
        {

            get
            {
                return _selectedFrame;
            }

            set
            {
                _selectedFrame = value;

                OnPropertyChanged();
            }

        }

        /// <summary>
        /// 当前LIN会话
        /// </summary>
        public LinDebugSession Session
        {
            get
            {
                return _session;
            }
        }

        #endregion

        #region 构造

        /// <summary>
        /// 创建调试ViewModel
        ///
        /// UI只需要绑定这里
        ///
        /// </summary>
        public LinDebugViewModel(
            LinDebugSession session)
        {

            if (session == null)
            {
                throw new ArgumentNullException(
                    nameof(session));
            }

            _session = session;

        }


        #endregion

        #region 方法

        /// <summary>
        /// 设置Signal值
        ///
        /// UI调用入口
        ///
        /// </summary>
        public void SetSignal(
            string name,
            object value)
        {

            _session.SetSignalValue(
                name,
                value);

        }

        /// <summary>
        /// 刷新Signal
        ///
        /// </summary>
        public void Refresh()
        {

            _session.RefreshSignals();

        }

        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(
            [CallerMemberName] string name = null)
        {

            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(name));

        }

        #endregion

    }

}
