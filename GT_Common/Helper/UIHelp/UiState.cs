using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.UIHelp
{
    public static class UiState
    {
        public static bool IsPaused { get; private set; }
        public static bool IsVisible { get; set; } = true;

        public static void SetPaused(bool paused)
        {
            IsPaused = paused;
        }
    }
}
