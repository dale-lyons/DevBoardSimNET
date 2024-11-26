using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z80MembershipCard
{
    public class SerialPortStateMachine
    {
        private SPStates mState = SPStates.Idle;
        public IList<byte> BytesToSend { get; private set; } = new List<byte>();

        private byte? sendingbyte;
        private int count;
        public void Writebit()
        {
            switch (mState)
            {
                case SPStates.Idle:
                    break;



            }
        }

        public LineState Readbit()
        {
            switch (mState)
            {
                case SPStates.Idle:
                    if (BytesToSend.Count <= 0)
                        return LineState.None;
                    sendingbyte = BytesToSend[0];
                    BytesToSend.RemoveAt(0);
                    mState = SPStates.wStartbit;
                    return LineState.High;
                case SPStates.wStartbit:
                    count = 8;
                    mState = SPStates.wDatabits;
                    return LineState.Low;
                case SPStates.wDatabits:
                    if (sendingbyte == null)
                    {
                        mState = SPStates.Idle;
                        return LineState.None;
                    }
                    LineState ret = ((sendingbyte & 0x01) != 0) ? LineState.High : LineState.Low;
                    sendingbyte >>= 1;
                    count--;
                    if (count <= 0)
                        mState = SPStates.wStopbits1;
                    return ret;
                case SPStates.wStopbits1:
                    sendingbyte = null;
                    mState = SPStates.wStopbits2;
                    return LineState.High;
                case SPStates.wStopbits2:
                    mState = SPStates.Idle;
                    return LineState.High;
                default:
                    mState = SPStates.Idle;
                    sendingbyte = null;
                    return LineState.None;

            }//switch
        }

        private enum SPStates
        {
            Idle,
            wStartbit,
            wDatabits,
            wStopbits1,
            wStopbits2,
            rStartbit,
            rDatabits,
            rStopbits
        }

        public enum LineState : byte
        {
            High = 0x80,
            Low = 0x00,
            None = 0xff
        }
    }
}
