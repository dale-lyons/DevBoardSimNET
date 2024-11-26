using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Processors
{
    public static class MyExtensions
    {
        public static ushort SignExtend(this byte b)
        {
            ushort ret = b;
            if (b.Bit7())
                ret |= 0xff00;
            return ret;
        }
        public static bool Bit0(this byte b)
        {
            return ((b & 0x01) != 0);
        }
        public static bool Bit3(this byte b)
        {
            return ((b & 0x08) != 0);
        }
        public static bool Bit6(this byte b)
        {
            return ((b & 0x40) != 0);
        }
        public static bool Bit7(this byte b)
        {
            return ((b & 0x80) != 0);
        }
        public static byte Lsn(this byte b)
        {
            return (byte)(b & 0x0f);
        }
        public static byte Msn(this byte b)
        {
            return (byte)((b>>4) & 0x0f);
        }

        public static bool Bit15(this ushort b)
        {
            return ((b & 0x8000) != 0);
        }

        public static byte OnesComplement(this byte b)
        {
            return (byte)~b;
        }

        public static byte TwosComplement(this byte b)
        {
            byte r = b.OnesComplement();
            return (byte)(r + 1);
        }
    }
}