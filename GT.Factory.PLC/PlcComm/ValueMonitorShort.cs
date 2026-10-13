using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.PLC.PlcComm
{
    public class ValueMonitorShort
    {
        public PatchReader Plc { get; private set; }

        public string AddrIndex { get; private set; }

        /// <summary>
        /// 是否使用读取的第1个值作为依据判断高低变化
        /// </summary>
        public bool IsUseFirstValue { get; set; } = false;

        /// <summary>
        /// 初始值
        /// </summary>
        private short? originalValue = null;
        private bool _disposed = false;

        public event EventHandler ChangedHighEvent;
        public event EventHandler ChangedLowEvent;
        public event EventHandler<bool> ChangedEvent;

        ///// <summary>
        ///// 使用 int 地址构造
        ///// </summary>
        //public ValueMonitorShort(PatchReader plc, string addrIndex)
        //{
        //    Plc = plc;
        //    AddrIndex = addrIndex;
        //    Plc.ValueUpdateEvent += Plc_ValueUpdateEvent;
        //}

        /// <summary>
        /// 使用 string 地址构造（自动转换为 int）
        /// 例如："D100" → 100
        /// </summary>
        public ValueMonitorShort(PatchReader plc, string address)
        {
            Plc = plc;
            AddrIndex = address;
            Plc.ValueUpdateEvent += Plc_ValueUpdateEvent;
        }

        private void Plc_ValueUpdateEvent(object sender, Int16Block e)
        {
            if (_disposed) return;

            short newValue = e.S(AddrIndex);
            if (newValue != originalValue)
            {
                if (originalValue == null && IsUseFirstValue)
                {
                    originalValue = newValue;
                    return;
                }

                originalValue = newValue;

                ChangedEvent?.BeginInvoke(this, newValue == 1, null, null);

                if (newValue == 1)
                {
                    ChangedHighEvent?.BeginInvoke(this, new EventArgs(), null, null);
                }
                else
                {
                    ChangedLowEvent?.BeginInvoke(this, new EventArgs(), null, null);
                }
            }
        }

        /// <summary>
        /// 退订 PLC 更新事件，防止热重载后旧监听继续被触发
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Plc.ValueUpdateEvent -= Plc_ValueUpdateEvent;
            ChangedHighEvent = null;
            ChangedLowEvent = null;
            ChangedEvent = null;
        }
    }
}
