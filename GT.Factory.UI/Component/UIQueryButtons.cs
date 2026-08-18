using GT.Factory.UI.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT.Factory.UI.Component
{
    /// <summary>
    /// 查询按钮区域（查询 + 导出）
    /// </summary>
    public class UIQueryButtons : FlowLayoutPanel
    {
        public Button BtnSearch { get; }
        public Button BtnExport { get; }

        public UIQueryButtons()
        {
            this.AutoSize = true;
            this.FlowDirection = FlowDirection.LeftToRight;

            BtnSearch = UIButton.Primary("查询");
            BtnExport = UIButton.Default("导出");

            this.Controls.Add(BtnSearch);
            this.Controls.Add(BtnExport);
        }
    }
}
