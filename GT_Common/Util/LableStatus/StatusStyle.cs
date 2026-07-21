using GT_Common.MyEnum;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Util.LableStatus
{
    public static class StatusStyle
    {
        public static Color GetBackColor(ProductStatus status)
        {
            switch (status)
            {
                case ProductStatus.OK:
                    return Color.LimeGreen;
                case ProductStatus.NG:
                    return Color.Red;
                default:
                    return Color.LightGoldenrodYellow;
            }
        }

        public static Color GetFontColor(ProductStatus status)
        {
            switch (status)
            {
                case ProductStatus.OK:
                    return Color.LimeGreen;
                case ProductStatus.NG:
                    return Color.Red;
                default:
                    return Color.Orange;
            }
        }


        public static string GetDisplayText(ProductStatus status)
        {
            switch (status)
            {
                case ProductStatus.OK:
                    return "PASS";
                case ProductStatus.NG:
                    return "FAIL";
                default:
                    return "WAIT";
            }
        }
    }
}
