using GT.Factory.LIN.Model;
using GT.Factory.UI.Style;
using GT.Factory.UI.Theme;
using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace GT.Factory.LIN.UI.Controls
{

    /// <summary>
    /// LIN Trace显示控件
    ///
    /// 功能：
    ///
    /// 1. 显示LIN总线通信记录
    ///
    /// 2. 显示:
    /// 
    /// 时间
    /// 方向
    /// Frame
    /// ID
    /// 长度
    /// Data
    /// Checksum
    ///
    ///
    /// 数据来源:
    ///
    /// LinTraceLogger.Items
    ///
    ///
    /// 注意:
    ///
    /// 本控件不知道:
    ///
    /// LinController
    /// LinEngine
    /// LinRuntime
    ///
    ///
    /// 只负责:
    ///
    /// Model --> UI
    ///
    /// </summary>
    public class LinTraceGrid : UserControl
    {

        #region 控件

        /// <summary>
        /// Trace数据表格
        ///
        /// 使用DataGridView
        ///
        /// 由代码创建
        ///
        /// 不使用Designer
        ///
        /// </summary>
        private DataGridView _grid;

        #endregion

        #region 数据缓存

        /// <summary>
        /// 当前显示数据
        ///
        /// 保留一份UI数据
        ///
        /// 防止直接绑定外部List
        ///
        /// </summary>
        private readonly List<LinTraceItem> _items;

        #endregion

        private readonly Queue<LinTraceItem> _buffer =
    new Queue<LinTraceItem>();

        private readonly Timer _timer;

        private const int MaxRows = 2000;

        #region 构造

        /// <summary>
        /// 创建LIN Trace显示控件
        ///
        /// </summary>
        public LinTraceGrid()
        {

            //
            // 初始化缓存
            //
            _items =
                new List<LinTraceItem>();

            //
            // 创建界面
            //
            Initialize();


            _timer = new Timer();

            _timer.Interval = 100;

            _timer.Tick += (s, e) =>
            {
                Flush();
            };

            _timer.Start();

        }

        #endregion

        #region UI初始化

        /// <summary>
        /// 初始化控件
        ///
        /// 所有控件这里创建
        ///
        /// 无Designer
        ///
        /// </summary>
        private void Initialize()
        {

            //
            // UserControl基础属性
            //
            this.Dock =
                DockStyle.Fill;

            this.BackColor =
                UITheme.BgBase;

            //
            // 创建DataGridView
            //
            _grid =
                new DataGridView();

            //
            // 填充父容器
            //
            _grid.Dock =
                DockStyle.Fill;

            //
            // 禁止用户新增行
            //
            _grid.AllowUserToAddRows =
                false;

            //
            // 禁止删除
            //
            _grid.AllowUserToDeleteRows =
                false;

            //
            // 只读显示
            //
            _grid.ReadOnly =
                true;

            //
            // 不显示默认行头
            //
            _grid.RowHeadersVisible =
                false;

            //
            // 选择整行
            //
            _grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            //
            // 禁止多选
            //
            _grid.MultiSelect =
                false;

            //
            // 应用工业UI样式
            //
            UIGrid.BuildDataGrid(
                _grid);

            //
            // 创建列
            //
            BuildColumns();

            //
            // 添加控件
            //
            this.Controls.Add(
                _grid);

        }

        /// <summary>
        /// 创建Grid列
        ///
        /// 不依赖AutoGenerate
        ///
        /// 工业软件推荐明确控制列
        ///
        /// </summary>
        private void BuildColumns()
        {

            //
            // 时间
            //
            _grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {

                    HeaderText = "时间",

                    Name = "Time",

                    Width = 150,

                    AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.None

                });

            //
            // 方向
            //
            _grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {

                    HeaderText = "方向",

                    Name = "Direction",

                    Width = 70,

                    AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.None

                });

            //
            // Frame名称
            //
            _grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {

                    HeaderText = "Frame",

                    Name = "Frame",

                    AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.Fill

                });

            //
            // ID
            //
            _grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {

                    HeaderText = "ID",

                    Name = "ID",

                    Width = 100

                });

            //
            // 长度
            //
            _grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {

                    HeaderText = "Len",

                    Name = "Length",

                    Width = 60

                });

            //
            // Data
            //
            _grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {

                    HeaderText = "Data",

                    Name = "Data",

                    AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.Fill

                });

            //
            // 校验
            //
            _grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {

                    HeaderText = "Checksum",

                    Name = "Checksum",

                    Width = 90

                });

        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 添加一条Trace
        ///
        /// 事件驱动调用
        ///
        /// 不刷新全部数据
        ///
        /// </summary>
        public void AddTrace(
            LinTraceItem item)
        {

            if (item == null)
                return;



            lock (_buffer)
            {
                _buffer.Enqueue(item);
            }


        }

        private void Flush()
        {

            if (_buffer.Count == 0)
                return;


            List<LinTraceItem> list =
                new List<LinTraceItem>();


            lock (_buffer)
            {

                while (_buffer.Count > 0)
                {
                    list.Add(
                        _buffer.Dequeue());
                }

            }


            foreach (var item in list)
            {

                _grid.Rows.Add(
                    new object[]
                    {
                item.Time.ToString(
                    "HH:mm:ss.fff"),

                item.Direction,

                item.FrameName,

                $"0x{item.Id:X2}",

                item.Length,

                BitConverter.ToString(
                    item.Data),

                item.ChecksumOK?
                "OK":
                "ERROR"
                    });

            }


            while (_grid.Rows.Count > MaxRows)
            {
                _grid.Rows.RemoveAt(0);
            }

        }

        /// <summary>
        /// 更新Trace显示
        ///
        /// 调试界面刷新调用
        ///
        /// </summary>
        /// <param name="items">
        /// LinTraceLogger.Items
        /// </param>
        public void UpdateTrace(
            IEnumerable<LinTraceItem> items)
        {

            if (items == null)
                return;

            //
            // 更新缓存
            //
            _items.Clear();

            _items.AddRange(
                items);

            //
            // 刷新UI
            //
            RefreshGrid();

        }

        /// <summary>
        /// 清空显示
        ///
        /// </summary>
        public void Clear()
        {

            _items.Clear();

            _grid.Rows.Clear();

        }

        #endregion

        #region Grid刷新

        /// <summary>
        /// 根据缓存刷新Grid
        ///
        /// </summary>
        public void RefreshGrid()
        {

            //
            // 防止闪烁
            //
            _grid.SuspendLayout();

            try
            {

                //
                // 清空
                //
                _grid.Rows.Clear();

                foreach (var item in _items)
                {

                    //
                    // 添加一行
                    //
                    _grid.Rows.Add(
                        new object[]
                        {

                            item.Time
                                .ToString(
                                "HH:mm:ss.fff"),

                            item.Direction,

                            item.FrameName,

                            $"0x{item.Id:X2}",


                            item.Length,

                            item.Data == null
                            ?
                            ""
                            :
                            BitConverter
                            .ToString(
                                item.Data),

                            item.ChecksumOK
                            ?
                            "OK"
                            :
                            "ERROR"

                        });

                }

                while (_grid.Rows.Count > MaxRows)
                {
                    _grid.Rows.RemoveAt(0);
                }

            }
            finally
            {

                _grid.ResumeLayout();

            }

        }

        #endregion

    }

}
