using GT.Factory.UI.Theme;
using System;
using System.Windows.Forms;

namespace GT.Factory.UI.Style
{

    /// <summary>
    /// GT.Factory统一ListBox样式。
    ///
    /// 负责:
    ///
    /// 1. 创建ListBox
    /// 2. 应用UITheme
    /// 3. 统一列表视觉
    ///
    ///
    /// 业务项目:
    ///
    /// 负责:
    ///
    /// 1. 数据加载
    /// 2. Selected事件
    /// 3. Model绑定
    ///
    /// </summary>
    public static class UIListBox
    {


        #region 创建


        /// <summary>
        /// 创建标准ListBox。
        ///
        /// 示例:
        ///
        /// var list =
        ///     UIListBox.Create();
        ///
        /// </summary>
        public static ListBox Create()
        {

            var list =
                new ListBox();


            ApplyStyle(
                list);


            return list;

        }


        #endregion



        #region 样式


        /// <summary>
        /// 应用统一ListBox样式。
        /// </summary>
        public static void ApplyStyle(
            ListBox list)
        {

            if (list == null)
                throw new ArgumentNullException(
                    nameof(list));


            //
            // 布局
            //
            list.Dock =
                DockStyle.Fill;


            //
            // 背景
            //
            list.BackColor =
                UITheme.BgBase;


            //
            // 文字
            //
            list.ForeColor =
                UITheme.TextMain;


            //
            // 字体
            //
            list.Font =
                UITheme.BodyFont;


            //
            // 边框
            //
            list.BorderStyle =
                BorderStyle.None;


            //
            // 行高度
            //
            list.ItemHeight =
                36;


            //
            // 单选
            //
            list.SelectionMode =
                SelectionMode.One;


            //
            // 不自动裁剪高度
            //
            list.IntegralHeight =
                false;

        }


        #endregion



        #region 数据辅助


        /// <summary>
        /// 清空列表。
        /// </summary>
        public static void Clear(
            ListBox list)
        {

            if (list == null)
                return;


            list.Items.Clear();

        }



        /// <summary>
        /// 添加列表项。
        ///
        /// 可以直接添加Model。
        /// </summary>
        public static void Add(
            ListBox list,
            object item)
        {

            if (list == null ||
               item == null)
                return;


            list.Items.Add(
                item);

        }



        #endregion

    }

}
