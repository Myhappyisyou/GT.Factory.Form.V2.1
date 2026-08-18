using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.UI.Theme
{
    /// <summary>
    /// UI主题定义（颜色 / 字体 / 尺寸）
    /// ⚠ 不允许在业务代码中直接写颜色/字体，必须统一从这里取
    /// </summary>
    public static class UITheme
    {
        // ===== 颜色定义 =====

        /// <summary>主背景色（整个系统背景）</summary>
        public static Color Background = Color.FromArgb(18, 25, 38);

        /// <summary>卡片背景（Panel / 容器）</summary>
        public static Color Card = Color.FromArgb(28, 38, 55);

        /// <summary>输入框背景</summary>
        public static Color Input = Color.FromArgb(45, 55, 72);

        /// <summary>主文本颜色</summary>
        public static Color Text = Color.White;

        /// <summary>次级文本（提示/弱化）</summary>
        public static Color SubText = Color.FromArgb(180, 180, 180);

        /// <summary>边框颜色</summary>
        public static Color Border = Color.FromArgb(70, 80, 100);


        public static readonly Color BgHover =
            Color.FromArgb(35, 45, 60);
        // ===== 字体定义 =====

        /// <summary>默认字体</summary>
        public static Font FontNormal = new Font("Microsoft YaHei UI", 10F);

        /// <summary>较大字体（按钮/重要信息）</summary>
        public static Font FontLarge = new Font("Microsoft YaHei UI", 12F);

        /// <summary>标题字体</summary>
        public static Font FontTitle = new Font("Microsoft YaHei UI", 18F, FontStyle.Bold);

        // ===== 尺寸规范 =====

        /// <summary>统一控件高度（所有输入框/按钮必须一致）</summary>
        public static int ControlHeight = 34;

        /// <summary>控件间距</summary>
        public static int ControlSpacing = 10;


        // ==========================================
        // 1. 背景体系 (由深到浅，用于构建物理机器面板的层次感)
        // ==========================================

        /// <summary>
        /// 最底层背景色 (深空黑) - 适用于主窗体 (Form) 的底色
        /// </summary>
        public static readonly Color BgBase = Color.FromArgb(11, 15, 25);

        /// <summary>
        /// 卡片背景色 - 适用于 Panel、弹窗、分组框，与底层拉开对比度
        /// </summary>
        public static readonly Color BgCard = Color.FromArgb(21, 27, 40);

        /// <summary>
        /// 输入区域背景色 - 适用于 TextBox, ComboBox 以及未激活的暗色按钮
        /// </summary>
        public static readonly Color BgInput = Color.FromArgb(31, 41, 61);


        // ==========================================
        // 2. 品牌与语义色 (赛博工业风高饱和度色彩，用于状态指示)
        // ==========================================

        /// <summary>
        /// 主操作色 (科技蓝) - 适用于【查询】、【登录】等引导用户点击的主按钮
        /// </summary>
        public static readonly Color Primary = Color.FromArgb(0, 163, 255);

        /// <summary>
        /// 荧光青 (边框/高亮状态) - 适用于控件获得焦点时的发光边框
        /// </summary>
        public static readonly Color CyanGlow = Color.FromArgb(0, 240, 255);

        /// <summary>
        /// 成功/合格色 (霓虹绿) - 适用于 OK 状态、设备在线状态
        /// </summary>
        public static readonly Color Success = Color.FromArgb(0, 255, 157);

        /// <summary>
        /// 危险/报警色 (警示红) - 适用于 NG 状态、设备掉线、退出/取消操作
        /// </summary>
        public static readonly Color Danger = Color.FromArgb(255, 0, 60);

        /// <summary>
        /// 警告/待机色 (橙色) - 适用于进行中、异常拦截提示
        /// </summary>
        public static readonly Color Warning = Color.FromArgb(255, 165, 0);


        // ==========================================
        // 3. 文本体系 (保证深色背景下的极佳可读性)
        // ==========================================

        /// <summary>
        /// 高亮主文本 (纯白偏蓝) - 用于按钮文字、输入框输入的内容
        /// </summary>
        public static readonly Color TextMain = Color.FromArgb(224, 230, 237);

        /// <summary>
        /// 弱化/提示文本 (灰白) - 用于 Label 标签、水印占位符、表头文字
        /// </summary>
        public static readonly Color TextMuted = Color.FromArgb(139, 148, 158);


        // ==========================================
        // 4. 排版令牌 (Typography)
        // ==========================================

        private const string FontFamily = "微软雅黑";

        /// <summary>
        /// 等宽字体，工业控制专属，能让连续跳动的数字条码绝对对齐，不晃眼
        /// </summary>
        private const string CodeFontFamily = "Consolas";

        /// <summary>页面大标题，24号粗体</summary>
        public static readonly Font TitleFont = new Font(FontFamily, 24F, FontStyle.Bold);

        /// <summary>弹窗标题、大号操作按钮，16号粗体</summary>
        public static readonly Font HeaderFont = new Font(FontFamily, 16F, FontStyle.Bold);

        /// <summary>常规正文、文本输入框、普通按钮，12号常规体</summary>
        public static readonly Font BodyFont = new Font(FontFamily, 12F, FontStyle.Regular);

        /// <summary>辅助提示、数据表格的表头，10号常规体</summary>
        public static readonly Font SmallFont = new Font(FontFamily, 10F, FontStyle.Regular);

        /// <summary>产品码、工单号等数字显示专属字体，14号等宽</summary>
        public static readonly Font CodeFont = new Font(CodeFontFamily, 14F, FontStyle.Regular);
    }
}
