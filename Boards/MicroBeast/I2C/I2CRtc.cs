using Boards;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static MicroBeast.Z84C20;

namespace MicroBeast.I2C
{
    public class I2CRtc : I2CDevice
    {
        private byte mAddress;
        private uint mCycleCountInt = 0;
        private uint mCycleCountSec = 0;
        // sec,min,hrs,dow,day,month,year
        //Jan 18,2024: 13:34:42
        //note:rtc uses BCD numbers
        private byte[] mTime = new byte[] { 0x42, 0x34, 0x13, 0x01, 0x18, 0x01, 0x24 };

        public I2CRtc(I2cBus i2cBus, byte address) : base(i2cBus)
        {
            mAddress = address;

            //load current time into RTC
            DateTime now = DateTime.Now;
            mTime[0] = decToBCD((byte)now.Second);
            mTime[1] = decToBCD((byte)now.Minute);
            mTime[2] = decToBCD((byte)now.Hour);
            mTime[3] = decToBCD((byte)now.DayOfWeek);
            mTime[4] = decToBCD((byte)now.Day);
            mTime[5] = decToBCD((byte)now.Month);
            mTime[6] = decToBCD((byte)(now.Year-2000));
        }
        public override void onReset()
        {
            i2cAddress = 0;
            BitsToSend = null;
        }

        public override void onDataNotify(byte address, byte data, bool rwBit)// 1- read, 0 - write
        {
            switch (data)
            {
                case 0x00:
                    BitsToSend = new Queue<bool>();
                    foreach (byte tm in mTime)
                    {
                        byte tm2 = tm;
                        for (int ii = 0; ii < 8; ii++)
                        {
                            bool bit = ((tm2 & 0x80) != 0);
                            BitsToSend.Enqueue(bit);
                            tm2 <<= 1;
                        }
                    }
                    break;
                case 0x07:
                    break;
            }
        }

        public override I2CDevice CompareAddress(byte address)
        {// || (address == 0x50) || (address == 0x53))
            if (address == mAddress)
                return this;
            return null;
        }

        private static int CYCLES_PER_SEC = 4000000;
        private static int CYCLES_PER_INT = 8000000 / 32;
        public void Cycles(object sender, CyclesEventArgs e)
        {
            mCycleCountInt += e.CycleCount;
            mCycleCountSec += e.CycleCount;
            if (mCycleCountSec >= CYCLES_PER_SEC)
            {
                mCycleCountSec = 0;
                bool overflow;
                mTime[0] = incBCD(mTime[0], 60, out overflow);
                if (overflow)
                {
                    mTime[1] = incBCD(mTime[1], 60, out overflow);
                    if (overflow)
                        mTime[2] = incBCD(mTime[1], 24, out overflow);

                }
            }
            if (mCycleCountInt >= CYCLES_PER_INT)
            {
                mCycleCountInt = 0;
                mI2cBus.FireInterrupt();
            }
        }

        private static byte decToBCD(byte num)
        {
            byte high = (byte)(num / 10);
            byte low = (byte)(num % 10);
            byte ret = (byte)((high << 4) + low);
            return ret;
        }

        private static byte bcdToDec(byte num)
        {
            byte high = (byte)((num & 0xf0) >> 4);
            byte low = (byte)(num & 0x0f);
            return (byte)((high * 10) + low);
        }

        private static byte incBCD(byte num, byte max, out bool overflow)
        {
            overflow = false;
            byte valD = (byte)(bcdToDec(num) + 1);
            if (valD >= max)
            {
                overflow = true;
                return 0;
            }
            return decToBCD(valD);
        }
    }
}