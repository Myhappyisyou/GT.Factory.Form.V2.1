using GT_Common.MyEnum;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.Util.LableStatus
{
    public class StatusLabel : Label
    {
        private ProductStatus _currentStatus = ProductStatus.Standby;
        private ProductStatus _lastTestResult = ProductStatus.Standby;

        public ProductStatus CurrentStatus
        {
            get { return _currentStatus; }
            set
            {
                if (_currentStatus == value) return; // 🚫 状态没变就跳过
                _currentStatus = value;
                if (value != ProductStatus.Standby)
                {
                    _lastTestResult = value;
                }
                UpdateAppearance();
            }
        }

        private void UpdateAppearance()
        {
            //this.BackColor = StatusStyle.GetBackColor(_currentStatus);
            this.ForeColor= StatusStyle.GetFontColor(_currentStatus);
            this.Text = (_currentStatus == ProductStatus.Standby)
                ? StatusStyle.GetDisplayText(ProductStatus.Standby)
                : string.Format("上次结果: {0}", StatusStyle.GetDisplayText(_lastTestResult));

            this.Font = new Font("Microsoft YaHei", 24f, FontStyle.Bold);
            this.TextAlign = ContentAlignment.MiddleCenter;
            //this.AutoSize = false;
            //this.Size = new Size(120, 40);
        }

        public StatusLabel()
        {
            UpdateAppearance();
            //this.BorderStyle = BorderStyle.FixedSingle;
        }
    }
}
