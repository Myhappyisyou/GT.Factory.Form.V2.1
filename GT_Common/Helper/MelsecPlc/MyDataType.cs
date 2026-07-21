using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT_Common.Helper.MelsecPlc
{
    public class SinglePlcData
    {
        public MelsecPlcComm plc { get; private set; }

        public int Index { get; private set; }


        public SinglePlcData(MelsecPlcComm _plc, int _index)
        {
            plc = _plc;
            Index = _index;
        }
    }

    public class BatchPlcData : SinglePlcData
    {
        public int ValueLength { get; private set; } = 1;

        public BatchPlcData(MelsecPlcComm _plc, int _index, int _valueLength)
             : base(_plc, _index)
        {
            ValueLength = _valueLength;
        }
    }

    public class ShortPlcData : SinglePlcData
    {
        public short Data
        {
            get
            {
                HslCommunication.Core.ByteTransformBase baseOperator = new HslCommunication.Core.ByteTransformBase();
                return baseOperator.TransInt16(plc.Data.AllValueRead, (Index - plc.Data.ValueStartIndex) * 2);
            }
        }

        public ShortPlcData(MelsecPlcComm _plc, int _index)
            : base(_plc, _index)
        {
        }
    }

    public class FloatPlcData : SinglePlcData
    {
        public double Data
        {
            get
            {
                HslCommunication.Core.ByteTransformBase baseOperator = new HslCommunication.Core.ByteTransformBase();
                return baseOperator.TransSingle(plc.Data.AllValueRead, (Index - plc.Data.ValueStartIndex) * 2);
            }
        }

        public FloatPlcData(MelsecPlcComm _plc, int _index)
            : base(_plc, _index)
        {
        }
    }

    public class DoublePlcData : SinglePlcData
    {
        public double Data
        {
            get
            {
                HslCommunication.Core.ByteTransformBase baseOperator = new HslCommunication.Core.ByteTransformBase();
                return baseOperator.TransDouble(plc.Data.AllValueRead, (Index - plc.Data.ValueStartIndex) * 2);
            }
        }

        public DoublePlcData(MelsecPlcComm _plc, int _index)
            : base(_plc, _index)
        {
        }
    }

    public class Int32PlcData : SinglePlcData
    {
        public double Data
        {
            get
            {
                HslCommunication.Core.ByteTransformBase baseOperator = new HslCommunication.Core.ByteTransformBase();
                return baseOperator.TransInt32(plc.Data.AllValueRead, (Index - plc.Data.ValueStartIndex) * 2);
            }
        }

        public Int32PlcData(MelsecPlcComm _plc, int _index)
            : base(_plc, _index)
        {
        }
    }

    public class StringPlcData : BatchPlcData
    {

        public string Data
        {
            get
            {
                HslCommunication.Core.ByteTransformBase baseOperator = new HslCommunication.Core.ByteTransformBase();
                return baseOperator.TransString(plc.Data.AllValueRead, (Index - plc.Data.ValueStartIndex) * 2, ValueLength, Encoding.ASCII);
            }
        }

        public StringPlcData(MelsecPlcComm _plc, int _index, int _valueLength)
            : base(_plc, _index, _valueLength)
        {

        }
    }

    public class SignalPlcData : ShortPlcData
    {
        public event EventHandler HighSignalTrigEvent;
        public event EventHandler LowSignalTrigEvent;


        private short originalValue = 0;

        public SignalPlcData(MelsecPlcComm _plc, int _index)
            : base(_plc, _index)
        {
            plc.ValueUpdateEvent += Plc_ValueUpdateEvent;
        }


        private void Plc_ValueUpdateEvent(object sender, Dvalue e)
        {
            if (Data != originalValue)
            {
                originalValue = Data;
                if (originalValue == (short)1)
                {
                    //HighSignalTrigEvent?.Invoke(this, null);
                    HighSignalTrigEvent?.BeginInvoke(this, null, null, null);
                }
                else
                {
                    LowSignalTrigEvent?.BeginInvoke(this, null, null, null);
                }
            }
        }
    }

}
