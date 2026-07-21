using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common
{
    // 1. 定义数据提供者（带事件）
    public class DataProvider
    {
        // 声明数据更新事件
        public event Action<string> DataUpdated;
        private string _currentData;
        public event Action<bool> StateUpdated;
        private bool _currentState;

        public string CurrentData
        {
            get => _currentData;
            set
            {
                _currentData = value;
                // 触发更新事件
                DataUpdated?.Invoke(value);
            }
        }

        public bool CurrentState
        {
            get => _currentState;
            set
            {
                _currentState = value;
                // 触发更新事件
                StateUpdated?.Invoke(value);
            }
        }
    }
}
