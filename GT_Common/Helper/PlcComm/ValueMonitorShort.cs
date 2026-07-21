using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.PlcComm
{
    public class ValueMonitorShort
    {
        public PatchReader Plc { get; private set; }

        public int AddrIndex { get; private set; }

        
        /// <summary>
        /// 是否使用读取的第1个值作为依据判断高低变化
        /// </summary>
        public bool IsUseFirstValue { get; set; } = false;

        /// <summary>
        /// 初始值
        /// </summary>
        private short? originalValue = null;


        public event EventHandler ChangedHighEvent;
        public event EventHandler ChangedLowEvent;
        public event EventHandler<bool> ChangedEvent;


        public ValueMonitorShort(PatchReader plc, int addrIndex)
        {
            Plc = plc;
            AddrIndex = addrIndex;
            Plc.ValueUpdateEvent += Plc_ValueUpdateEvent;
        }


        private void Plc_ValueUpdateEvent(object sender, Int16Block e)
        {
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
    }
}
