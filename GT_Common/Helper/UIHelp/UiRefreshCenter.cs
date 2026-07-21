using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GT_Common.Helper.UIHelp
{
    public static class UiRefreshCenter
    {
        public static event Action OnRefresh;
        private static readonly Timer _timer;
        private static bool _isInitialized = false;

        static UiRefreshCenter()
        {
            _timer = new Timer { Interval = 1000 };
            _timer.Tick += (s, e) =>
            {
                // ✅ 核心：最小化/后台时直接跳过所有刷新
                if (UiState.IsPaused || !UiState.IsVisible)
                    return;

                try { OnRefresh?.Invoke(); }
                catch { }
            };
        }

        // 程序启动时调用一次
        public static void Start()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            if (!_isInitialized)
            {
                _isInitialized = true;
                _timer.Start();
            }
        }

        public static void RequestRefresh()
        {
            if (UiState.IsPaused || !UiState.IsVisible)
                return;
            OnRefresh?.Invoke();
        }
    }
}
