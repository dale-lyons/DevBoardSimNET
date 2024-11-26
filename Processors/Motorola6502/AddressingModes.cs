using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motorola6502
{
    public static class AddressingModes
    {
        public const int IMMED = 0; /* Immediate */
        public const int ABSOL = 1; /* Absolute */
        public const int ZEROP = 2; /* Zero Page */
        public const int IMPLI = 3; /* Implied */
        public const int INDIA = 4; /* Indirect Absolute */
        public const int ABSIX = 5; /* Absolute indexed with X */
        public const int ABSIY = 6; /* Absolute indexed with Y */
        public const int ZEPIX = 7; /* Zero page indexed with X */
        public const int ZEPIY = 8; /* Zero page indexed with Y */
        public const int INDIN = 9; /* Indexed indirect (with X) */
        public const int ININD = 10; /* Indirect indexed (with Y) */
        public const int RELAT = 11; /* Relative */
        public const int ACCUM = 12; /* Accumulator */
        public const int NONE = 13;

        private static int[] bitsToModeC00 = new int[8]
            {
                AddressingModes.IMMED,
                AddressingModes.ZEROP,
                AddressingModes.NONE,
                AddressingModes.ABSOL,
                AddressingModes.NONE,
                AddressingModes.ZEPIX,
                AddressingModes.NONE,
                AddressingModes.ABSIX
            };

        private static int[] bitsToModeC01 = new int[8]
            {
                AddressingModes.INDIN,
                AddressingModes.ZEROP,
                AddressingModes.IMMED,
                AddressingModes.ABSOL,
                AddressingModes.ININD,
                AddressingModes.ZEPIX,
                AddressingModes.ABSIY,
                AddressingModes.ABSIX
            };
        private static int[] bitsToModeC10 = new int[8]
            {
                AddressingModes.IMMED,
                AddressingModes.ZEROP,
                AddressingModes.ACCUM,
                AddressingModes.ABSOL,
                AddressingModes.ZEROP,
                AddressingModes.ZEPIX,
                AddressingModes.NONE,
                AddressingModes.ABSIX,
            };

        public static int addressModeFromOP(byte opcode)
        {
            byte ccmode = (byte)(opcode & 0x03);
            byte ccoff = (byte)((opcode & 0x1c) >> 2);

            switch (ccmode)
            {
                case 0x00:
                    return bitsToModeC00[ccoff];
                case 0x01:
                    return bitsToModeC01[ccoff];
                case 0x02:
                    return bitsToModeC10[ccoff];
                default:
                    return 0;
            }
        }
    }
}