using GT.Factory.LIN.ProjectDebug;
using GT.Factory.UI;
using GT.Factory.UI.Style;
using GT.Factory.UI.Theme;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;


namespace GT.Factory.LIN.UI.Controls
{


    /// <summary>
    /// LIN Frame数据监控面板
    ///
    /// 用于显示当前LIN总线Frame缓存数据。
    ///
    ///
    /// 数据来源:
    ///
    /// LinDebugSession.Frames
    ///
    ///
    /// 显示:
    ///
    /// Frame
    /// ID
    /// Length
    /// Data
    ///
    ///
    /// 示例:
    ///
    /// MotorCmd
    /// 0x10
    /// 8
    /// 01-02-03-04-00-00-00-00
    ///
    ///
    /// 注意:
    ///
    /// 本控件:
    ///
    /// 只显示数据
    ///
    ///
    /// 不负责:
    ///
    /// Frame发送
    /// Signal解析
    /// LIN通信
    ///
    /// </summary>
    public class LinFrameDataPanel :
        UserControl
    {



        #region 控件


        /// <summary>
        /// Frame数据显示表格
        ///
        /// 使用GT.Factory.UI统一Grid规范
        ///
        /// </summary>
        private DataGridView _grid;


        #endregion



        #region 数据缓存


        /// <summary>
        /// 当前显示Frame集合
        ///
        /// 不直接绑定外部List
        ///
        /// 防止UI修改Core数据
        ///
        /// </summary>
        private readonly List<LinFrameInfo> _frames;


        #endregion


        #region 构造

        /// <summary>
        /// 创建Frame Data监控控件
        ///
        /// 完全代码创建
        ///
        /// 不使用Designer
        ///
        /// </summary>
        public LinFrameDataPanel()
        {

            _frames =
                new List<LinFrameInfo>();


            Initialize();

        }


        #endregion



        #region UI初始化



        /// <summary>
        /// 初始化界面
        ///
        /// 使用GT.Factory.UI样式
        ///
        /// </summary>
        private void Initialize()
        {


            //
            // UserControl基础属性
            //
            Dock =
                DockStyle.Fill;


            BackColor =
                UITheme.BgBase;



            //
            // 创建DataGrid
            //
            _grid =
                new DataGridView();



            _grid.Dock =
                DockStyle.Fill;



            //
            // 数据显示
            //
            // 不允许用户编辑
            //
            _grid.ReadOnly =
                true;



            //
            // 禁止新增
            //
            _grid.AllowUserToAddRows =
                false;



            //
            // 禁止删除
            //
            _grid.AllowUserToDeleteRows =
                false;



            //
            // 隐藏行头
            //
            _grid.RowHeadersVisible =
                false;



            //
            // 整行选择
            //
            _grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;



            //
            // 使用公共工业UI样式
            //
            UIGrid.BuildDataGrid(
                _grid);



            //
            // 创建列
            //
            BuildColumns();



            Controls.Add(
                _grid);

        }




        /// <summary>
        /// 创建显示列
        ///
        /// 明确控制列
        ///
        /// 不使用AutoGenerate
        ///
        /// </summary>
        private void BuildColumns()
        {


            //
            // Frame名称
            //
            _grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {

                    HeaderText =
                        "Frame",

                    Name =
                        "Frame",

                    Width =
                        200

                });



            //
            // Frame ID
            //
            _grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {

                    HeaderText =
                        "ID",

                    Name =
                        "ID",

                    Width =
                        80

                });



            //
            // 数据长度
            //
            _grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {

                    HeaderText =
                        "Length",

                    Name =
                        "Length",

                    Width =
                        80

                });



            //
            // 原始Data
            //
            _grid.Columns.Add(
                new DataGridViewTextBoxColumn
                {

                    HeaderText =
                        "Data",

                    Name =
                        "Data",

                    AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.Fill

                });


        }



        #endregion


        #region 数据接口


        /// <summary>
        /// 加载Frame列表
        ///
        /// 数据来源:
        ///
        /// LinDebugSession.Frames
        ///
        /// </summary>
        public void LoadFrames(
            IEnumerable<LinFrameInfo> frames)
        {

            _frames.Clear();

            if (frames != null)
            {

                _frames.AddRange(
                    frames);

            }


            RefreshData();

        }


        /// <summary>
        /// 刷新Frame Data显示
        ///
        /// 调试周期调用
        ///
        /// 数据来源:
        ///
        /// LinRuntime.FrameCache
        ///
        /// </summary>
        public void RefreshData()
        {

            if (_grid.InvokeRequired)
            {

                _grid.BeginInvoke(
                    new Action(
                        RefreshData));

                return;

            }
            _grid.SuspendLayout();

            _grid.Rows.Clear();

            foreach (var frame in _frames)
            {

                _grid.Rows.Add(
                    new object[]
                    {

                        frame.Name,


                        $"0x{frame.Id:X2}",


                        frame.Length,


                        FormatData(
                            frame.Data)

                    });

            }

            _grid.Refresh();

            _grid.ResumeLayout();

        }


        public void UpdateFrame(
            LinFrameInfo frame)
        {

           

            if (frame == null)
                return;


            if (_grid.InvokeRequired)
            {

                _grid.BeginInvoke(
                    new Action<LinFrameInfo>(
                        UpdateFrame),
                    frame);

                return;

            }


            Console.WriteLine(
$"Frame Update:{frame.Name} Data={BitConverter.ToString(frame.Data)}");

            foreach (DataGridViewRow row in _grid.Rows)
            {

                if (row.Cells["Frame"].Value
                    .ToString()
                    ==
                    frame.Name)
                {

                    row.Cells["Data"].Value =
                        FormatData(frame.Data);


                    return;

                }

            }

        }


        #endregion


        #region 私有方法


        /// <summary>
        /// 格式化Frame数据
        ///
        /// byte[]
        ///
        /// 转:
        ///
        /// 01-02-03-04
        ///
        /// </summary>
        private string FormatData(
            byte[] data)
        {

            if (data == null ||
               data.Length == 0)
            {
                return "";
            }


            return BitConverter
                .ToString(data);

        }


        #endregion

    }

}
