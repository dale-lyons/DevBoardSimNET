using MicroBeast.I2C;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static MicroBeast.I2C.I2cBus;

namespace MicroBeast
{
    public class PIO
    {
        public delegate void FireInterrup(byte vector);
        public event FireInterrup OnInterrupt;

        public LineLevel sclLineLevel;
        public LineLevel sdaLineLevel;

        public enum PullupLineType
        {
            None,
            Pullup,
            Pulldown
        }

        //        public delegate LineLevel? I2cBusNotification(bool scl, bool sda);
        //        public event I2cBusNotification onTick;

        private PullupLineType sclPullupType;
        private PullupLineType sdaPullupType;

        private bool mNextCtrlDir = false;

        private byte mDirection = 0x00;
        //private byte mPortDataOut = 0x00;
        //private byte mPortDataIn = 0x00;
        private byte mPortControl = 0x00;
        private byte mMode = 0x00;

        private bool mInterruptEnable;
        private byte mInterruptVector;
        private I2cBus mI2cBus;

        // PortB PB5: RTC Clock
        // PortB PB6: I2C Clock(scl)
        // PortB PB7: I2C Data (sda)
        //        private static byte I2C_CLK_BIT = 6;
        //        private static byte I2C_DATA_BIT = 7;
        private static byte I2C_CLK_MASK = 0x40;
        private static byte I2C_DATA_MASK = 0x80;

        public PIO(I2cBus i2cBus)
        {
            mI2cBus = i2cBus;
            sclPullupType = PullupLineType.None;
            sdaPullupType = PullupLineType.None;
        }
        public PIO(I2cBus i2cBus, PullupLineType scl, PullupLineType sda)
        {
            mI2cBus = i2cBus;
            sclPullupType = scl;
            sdaPullupType = sda;
        }
        //public LineLevel? onTick(bool scl, bool sda)
        //{
        //    return mI2cBus.onTick(scl, sda);
        //}

        public void FireInterrupt()
        {
            OnInterrupt?.Invoke(mInterruptVector);
        }

        public void Reset()
        {
            //mPortDataOut = 0;
            //mPortDataIn = 0;
            mDirection = 0;
            mNextCtrlDir = false;
            mMode = 0;
        }

        public byte Control
        {
            get { return mPortControl; }
            set
            {
                if (mNextCtrlDir)
                {
                    mNextCtrlDir = false;
                    Direction = value;
                    return;
                }

                mPortControl = value;
                if ((value & 0x0f) == 0x0f)
                {//Mode Control Word
                    mMode = (byte)((value & 0xf0) >> 6);
                    switch (mMode & 0x03)
                    {
                        case 0: // all output
                            Direction = 0xff;
                            break;
                        case 1: // all input
                            Direction = 0x00;
                            break;
                        case 2: // all input/output
                            mNextCtrlDir = true;
                            break;
                        case 3: // all input/output with individual control
                            mNextCtrlDir = true;
                            break;
                    }//switch
                }//control word
                else if ((value & 0x01) == 0x00)
                {//Interrupt Vector Word
                    mInterruptVector = value;
                }
                else if ((value & 0x07) == 0x07)
                {//Interrupt Control Word
                    mInterruptEnable = ((value & 0x80) != 0);
                }
                else if ((value & 0x03) == 0x03)
                {//Interrupt Disable Word
                }
            }
        }

        // 0 - output, 1 - input
        public byte Direction
        {
            get { return mDirection; }
            set
            {
                mDirection = value;
                // 0 - output, 1 - input
                if (((mDirection & I2C_CLK_MASK) != 0))
                {//scl is input, use true for pullup
                    mI2cBus.HostScl = true;
                }
                else
                {//scl is output, use data
                    mI2cBus.HostScl = ((mData & I2C_CLK_MASK) != 0);
                }

                if (((mDirection & I2C_DATA_MASK) != 0))
                {//sda is input, use true for pullup
                    mI2cBus.HostSda = true;
                }
                else
                {//sda is output, use data
                    mI2cBus.HostSda = ((mData & I2C_DATA_MASK) != 0);
                }
                mI2cBus.onTick();
            }
        }

        private LineLevel? mExternalSda;
        public LineLevel? ExternalSda
        {
            get
            { return mExternalSda; }
            set
            {
                mExternalSda = value;
                if (!mInterruptEnable)
                    return;
            }
        }

        private byte mData;
        public byte Data
        {
            set { mData = value; }
            get
            {
                byte ret = 0x00;
                // 0 - output, 1 - input
                byte direction = Direction;
                if (mMode == 0) //all output
                    direction = 0x00;
                else if (mMode == 1) // all input
                    direction = 0xff;

                bool sdaInput = ((direction & I2C_DATA_MASK) != 0);
                if (sdaInput)
                {
                    var dev = mI2cBus.ActiveDevice();
                    if (dev != null)
                    {
                        var level = dev.FindSDALevel();
                        bool sda = (level == LineLevel.High || level == LineLevel.Float);
                        if (sda)
                            ret |= I2C_DATA_MASK;

                        if (Direction != 0xff)
                        {
                            dev.NextBit();
                        }
                        return ret;
                    }
                    ret |= I2C_DATA_MASK;
                }
                return ret;
            }
        }


        //    // 0 - output, 1 - input
        //    bool sdaInput = ((direction & I2C_DATA_MASK) != 0);
        //    byte ret = 0x00;
        //    return ret;
        //    if (sdaInput)
        //    {
        //        ret = I2C_DATA_MASK;
        //        if (ExternalSda.HasValue)
        //        {
        //            if (ExternalSda == LineLevel.Low)
        //                ret = 0x00;
        //            else if (ExternalSda == LineLevel.High)
        //                ret = I2C_DATA_MASK;
        //            ExternalSda = null;
        //        }
        //        else if (mI2cBus.BytesToSend != null && mI2cBus.BytesToSend.Count > 0)
        //        {
        //            byte b = mI2cBus.BytesToSend[0];
        //            ret = (byte)(b & 0x80);
        //            if (direction != 0xff)
        //            {
        //                mI2cBus.BytesToSend[0] = (byte)(b << 1);
        //                mI2cBus.BitCount--;
        //                if (mI2cBus.BitCount == 1)
        //                    return 0x00;
        //                else if (mI2cBus.BitCount == 0)
        //                {
        //                    mI2cBus.BytesToSend.RemoveAt(0);
        //                    mI2cBus.BitCount = 9;

        //                    if (mI2cBus.BytesToSend.Count == 0)
        //                        mI2cBus.BytesToSend = null;
        //                }
        //            }
        //        }
        //    }
        //    ret |= (byte)(direction & mData);
        //    return ret;
        //}
        //set { mData = value; }
        //}
    }
}