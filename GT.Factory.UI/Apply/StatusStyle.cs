using GT.Factory.UI.Enum;
using GT.Factory.UI.Theme;
using System.Drawing;

namespace GT.Factory.UI.Apply
{
    public static class StatusStyle
    {
        public static Color GetBackColor(ProductStatus status)
        {
            switch (status)
            {
                case ProductStatus.OK:
                    return UITheme.Success;
                case ProductStatus.NG:
                    return UITheme.Danger;
                default:
                    return UITheme.Warning;
            }
        }

        public static Color GetFontColor(ProductStatus status)
        {
            switch (status)
            {
                case ProductStatus.OK:
                    return UITheme.Warning;
                case ProductStatus.NG:
                    return UITheme.Warning;
                default:
                    return UITheme.Warning;
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
