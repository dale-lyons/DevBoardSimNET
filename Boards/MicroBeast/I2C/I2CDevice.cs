using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static Terminal.Win32;
using System.Xml.Linq;

namespace MicroBeast.I2C
{
    public enum LineChangeState
    {
        NO_CHANGE,
        HIGH_TO_LOW,
        HIGH_TO_FLOAT,
        LOW_TO_HIGH,
        LOW_TO_FLOAT
    }

    public enum LineLevel
    {
        High, Float, Low
    }

    public abstract class I2CDevice
    {
        private byte i2cData = 0;
        protected byte i2cAddress = 0;
        private bool mRWBit;

        private byte count = 0;

        private enum I2CState
        {
            NONE,
            START_ADDRESS,
            RW_BIT,
            DATA_FRAME,
            ACK_BIT,
            STOP_BIT
        }
        private I2CState mI2CState = I2CState.NONE;

        private LineChangeState sclLineState;
        private LineChangeState sdaLineState;
        private LineChangeState sclLastLineState = LineChangeState.NO_CHANGE;
        private LineChangeState sdaLastLineState = LineChangeState.NO_CHANGE;
        private bool mLastScl = true;
        private bool mLastSda = true;

        public Queue<bool> BitsToSend;

        private LineLevel? forceSda;

        protected I2cBus mI2cBus;
        protected I2CDevice(I2cBus i2cBus)
        {
            mI2cBus = i2cBus;
        }

        public bool IsActive
        {
            get
            {
                return (CompareAddress(i2cAddress) != null);
            }
        }

        public void NextBit()
        {
            if (BitsToSend != null && BitsToSend.Count > 0)
                BitsToSend.Dequeue();
        }

        public LineLevel FindSDALevel()
        {
            if (forceSda.HasValue)
            {
                var ret = forceSda.Value;
                forceSda = null;
                return ret;
            }

            if (BitsToSend != null && BitsToSend.Count > 0)
            {
                bool bit = BitsToSend.Peek();
                return bit ? LineLevel.High : LineLevel.Low;
            }
            return LineLevel.Float;
        }

        public abstract I2CDevice CompareAddress(byte address);
        public abstract void onDataNotify(byte address, byte data, bool rwBit);
        public abstract void onReset();

        public void onTick(bool scl, bool sda)
        {
            sclLineState = setLineStates(scl, mLastScl);
            sdaLineState = setLineStates(sda, mLastSda);
            mLastScl = scl;
            mLastSda = sda;

            //test for start bit
            if (sdaLastLineState == LineChangeState.HIGH_TO_LOW && sclLineState == LineChangeState.HIGH_TO_LOW)
            {
                mI2CState = I2CState.START_ADDRESS;
                i2cData = 0;
                i2cAddress = 0;
                count = 7;
                return;
            }

            //test for stop bit
            if (sdaLineState == LineChangeState.LOW_TO_HIGH && sclLastLineState == LineChangeState.LOW_TO_HIGH)
            {
                mI2CState = I2CState.NONE;
                sdaLastLineState = sdaLineState;
                sclLastLineState = sclLineState;
                onReset();
                return;
            }
            sdaLastLineState = sdaLineState;
            sclLastLineState = sclLineState;

            if (!scl)
                return;
            if (sclLineState != LineChangeState.LOW_TO_HIGH)
                return;

            switch (mI2CState)
            {
                case I2CState.NONE:
                    break;
                case I2CState.START_ADDRESS:
                    i2cAddress <<= 1;
                    if (sda)
                        i2cAddress |= 0x01;
                    if (--count == 0)
                        mI2CState = I2CState.RW_BIT;
                    break;

                case I2CState.RW_BIT: // 1- read, 0 - write
                    mRWBit = sda;
                    mI2CState = I2CState.ACK_BIT;
                    break;

                case I2CState.ACK_BIT:
                    {
                        if (CompareAddress(i2cAddress) == null)
                        {
                            mI2CState = I2CState.NONE;
                            return;
                        }

                        forceSda = LineLevel.Low;
                        //mI2cBus.DeviceSda = LineLevel.Low;
                        mI2CState = I2CState.DATA_FRAME;
                        count = 8;
                        i2cData = 0;
                        return;
                    }

                case I2CState.DATA_FRAME:
                    {
                        //debug
                        //if (CompareAddress(i2cAddress) == null)
                        //    return;
                        //if (BitsToSend != null && BitsToSend.Count > 0)
                        //{
                        //    bool bit = BitsToSend.Dequeue();
                        //    mI2cBus.DeviceSda = bit ? LineLevel.High : LineLevel.Low;

                        //    if (--count == 0)
                        //    {
                        //        mI2CState = I2CState.ACK_BIT;
                        //    }
                        //    return;
                        //}
                        //else
                        //{
                        i2cData <<= 1;
                        if (sda)
                            i2cData |= 0x01;

                        if (--count == 0)
                        {
                            mI2CState = I2CState.ACK_BIT;
                            //                                if (i2cAddress == 0x6f && mRWBit) // 1- read, 0 - write
                            onDataNotify(i2cAddress, i2cData, mRWBit);// 1- read, 0 - write
                        }
                    }
                    break;

                default:
                    mI2CState = I2CState.NONE;
                    break;
            }
        }

        private static LineChangeState setLineStates(bool line, bool lastline)
        {
            if (!lastline && line)
                return LineChangeState.LOW_TO_HIGH;
            else if (lastline && !line)
                return LineChangeState.HIGH_TO_LOW;
            else
                return LineChangeState.NO_CHANGE;
        }

        public static bool lineLevelToBool(LineLevel lineLevel)
        {
            return (lineLevel == LineLevel.High);
        }

        public static LineLevel boolToLineLevel(bool line)
        {
            return line ? LineLevel.High : LineLevel.Low;
        }
    }
}