using Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Motorola6502
{
    [Flags]
    public enum CyclesAdjustments : byte
    {
        CYCLES_CROSS_PAGE_ADDS_ONE = 0x01,
        CYCLES_BRANCH_TAKEN_ADDS_ONE = 0x02,
        Both = 0x03
    }

    public class OpCode
    {
        public byte opcode { get; set; }
        public string Mnemonic { get; set; }
        public int Mode { get; set; }
        public int Cycles { get; set; }
        public CyclesAdjustments CyclesAdjustment { get; set; }

    }

    public static class OpCodes
    {
        public static OpCode LookupOpcode(string mnemonic, int mode)
        {
            foreach (var op in table)
            {
                if (op.Mnemonic != mnemonic)
                    continue;

                if (isRelativeModeOnly(mnemonic))
                    return op;

                if (op.Mode == mode)
                    return op;
            }
            return null;
        }

        private static Dictionary<string, bool> relativeOnly = new Dictionary<string, bool>
        {
            { "BCC", true },
            { "BCS", true },
            { "BEQ", true },
            { "BMI", true },
            { "BNE", true },
            { "BPL", true },
            { "BRK", true },
            { "BVC", true },
            { "BVS", true }
        };
        public static bool isRelativeModeOnly(string mnemonic)
        {
            return relativeOnly.ContainsKey(mnemonic.ToUpper());
        }

        public static OpCode[] table = new OpCode[]
            {
                new OpCode { opcode = 0x69, Mnemonic = "ADC", Mode = AddressingModes.IMMED, Cycles = 2},
                new OpCode { opcode = 0x65, Mnemonic = "ADC", Mode = AddressingModes.ZEROP, Cycles = 3},
                new OpCode { opcode = 0x75, Mnemonic = "ADC", Mode = AddressingModes.ZEPIX, Cycles = 4},
                new OpCode { opcode = 0x6d, Mnemonic = "ADC", Mode = AddressingModes.ABSOL, Cycles = 4},
                new OpCode { opcode = 0x7d, Mnemonic = "ADC", Mode = AddressingModes.ABSIX, Cycles = 4, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},
                new OpCode { opcode = 0x79, Mnemonic = "ADC", Mode = AddressingModes.ABSIY, Cycles = 4, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},
                new OpCode { opcode = 0x61, Mnemonic = "ADC", Mode = AddressingModes.INDIN, Cycles = 6},
                new OpCode { opcode = 0x71, Mnemonic = "ADC", Mode = AddressingModes.ININD, Cycles = 5, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},

                new OpCode { opcode = 0x29, Mnemonic = "AND", Mode = AddressingModes.IMMED, Cycles = 2},
                new OpCode { opcode = 0x25, Mnemonic = "AND", Mode = AddressingModes.ZEROP, Cycles = 3},
                new OpCode { opcode = 0x35, Mnemonic = "AND", Mode = AddressingModes.ZEPIX, Cycles = 4},
                new OpCode { opcode = 0x2d, Mnemonic = "AND", Mode = AddressingModes.ABSOL, Cycles = 4},
                new OpCode { opcode = 0x3d, Mnemonic = "AND", Mode = AddressingModes.ABSIX, Cycles = 4, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},
                new OpCode { opcode = 0x39, Mnemonic = "AND", Mode = AddressingModes.ABSIY, Cycles = 4, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},
                new OpCode { opcode = 0x21, Mnemonic = "AND", Mode = AddressingModes.INDIN, Cycles = 6},
                new OpCode { opcode = 0x31, Mnemonic = "AND", Mode = AddressingModes.ININD, Cycles = 5, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},

                new OpCode { opcode = 0x0a, Mnemonic = "ASL", Mode = AddressingModes.IMPLI, Cycles = 2},
                new OpCode { opcode = 0x06, Mnemonic = "ASL", Mode = AddressingModes.ZEROP, Cycles = 5},
                new OpCode { opcode = 0x16, Mnemonic = "ASL", Mode = AddressingModes.ZEPIX, Cycles = 6},
                new OpCode { opcode = 0x0e, Mnemonic = "ASL", Mode = AddressingModes.ABSOL, Cycles = 6},
                new OpCode { opcode = 0x1e, Mnemonic = "ASL", Mode = AddressingModes.ABSIX, Cycles = 7},

                new OpCode { opcode = 0x90, Mnemonic = "BCC", Mode = AddressingModes.RELAT, Cycles = 2, CyclesAdjustment = CyclesAdjustments.Both},
                new OpCode { opcode = 0xb0, Mnemonic = "BCS", Mode = AddressingModes.RELAT, Cycles = 2, CyclesAdjustment = CyclesAdjustments.Both},
                new OpCode { opcode = 0xf0, Mnemonic = "BEQ", Mode = AddressingModes.RELAT, Cycles = 2, CyclesAdjustment = CyclesAdjustments.Both},

                new OpCode { opcode = 0x24, Mnemonic = "BIT", Mode = AddressingModes.ZEROP, Cycles = 3},
                new OpCode { opcode = 0x2c, Mnemonic = "BIT", Mode = AddressingModes.ABSOL, Cycles = 4},

                new OpCode { opcode = 0x30, Mnemonic = "BMI", Mode = AddressingModes.RELAT, Cycles = 2, CyclesAdjustment = CyclesAdjustments.Both},
                new OpCode { opcode = 0xd0, Mnemonic = "BNE", Mode = AddressingModes.RELAT, Cycles = 2, CyclesAdjustment = CyclesAdjustments.Both},
                new OpCode { opcode = 0x10, Mnemonic = "BPL", Mode = AddressingModes.RELAT, Cycles = 2, CyclesAdjustment = CyclesAdjustments.Both},
                new OpCode { opcode = 0x00, Mnemonic = "BRK", Mode = AddressingModes.RELAT, Cycles = 7},

                new OpCode { opcode = 0x50, Mnemonic = "BVC", Mode = AddressingModes.RELAT, Cycles = 2, CyclesAdjustment = CyclesAdjustments.Both},
                new OpCode { opcode = 0x70, Mnemonic = "BVS", Mode = AddressingModes.RELAT, Cycles = 2, CyclesAdjustment = CyclesAdjustments.Both},

                new OpCode { opcode = 0x18, Mnemonic = "CLC", Mode = AddressingModes.IMPLI, Cycles = 2},
                new OpCode { opcode = 0xd8, Mnemonic = "CLD", Mode = AddressingModes.IMPLI, Cycles = 2},
                new OpCode { opcode = 0x58, Mnemonic = "CLI", Mode = AddressingModes.IMPLI, Cycles = 2},
                new OpCode { opcode = 0xb8, Mnemonic = "CLV", Mode = AddressingModes.IMPLI, Cycles = 2},

                new OpCode { opcode = 0xc9, Mnemonic = "CMP", Mode = AddressingModes.IMMED, Cycles = 2},
                new OpCode { opcode = 0xc5, Mnemonic = "CMP", Mode = AddressingModes.ZEROP, Cycles = 3},
                new OpCode { opcode = 0xd5, Mnemonic = "CMP", Mode = AddressingModes.ZEPIX, Cycles = 4},
                new OpCode { opcode = 0xcd, Mnemonic = "CMP", Mode = AddressingModes.ABSOL, Cycles = 4},
                new OpCode { opcode = 0xdd, Mnemonic = "CMP", Mode = AddressingModes.ABSIX, Cycles = 4, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},
                new OpCode { opcode = 0xd9, Mnemonic = "CMP", Mode = AddressingModes.ABSIY, Cycles = 4, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},
                new OpCode { opcode = 0xc1, Mnemonic = "CMP", Mode = AddressingModes.INDIN, Cycles = 6},
                new OpCode { opcode = 0xd1, Mnemonic = "CMP", Mode = AddressingModes.ININD, Cycles = 5, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},

                new OpCode { opcode = 0xe0, Mnemonic = "CPX", Mode = AddressingModes.IMMED, Cycles = 2},
                new OpCode { opcode = 0xe4, Mnemonic = "CPX", Mode = AddressingModes.ZEROP, Cycles = 3},
                new OpCode { opcode = 0xec, Mnemonic = "CPX", Mode = AddressingModes.ABSOL, Cycles = 4},
                new OpCode { opcode = 0xc0, Mnemonic = "CPY", Mode = AddressingModes.IMMED, Cycles = 2},
                new OpCode { opcode = 0xc4, Mnemonic = "CPY", Mode = AddressingModes.ZEROP, Cycles = 3},
                new OpCode { opcode = 0xcc, Mnemonic = "CPY", Mode = AddressingModes.ABSOL, Cycles = 4},

                new OpCode { opcode = 0xc6, Mnemonic = "DEC", Mode = AddressingModes.ZEROP, Cycles = 5},
                new OpCode { opcode = 0xd6, Mnemonic = "DEC", Mode = AddressingModes.ZEPIX, Cycles = 6},
                new OpCode { opcode = 0xce, Mnemonic = "DEC", Mode = AddressingModes.ABSOL, Cycles = 6},
                new OpCode { opcode = 0xde, Mnemonic = "DEC", Mode = AddressingModes.ABSIX, Cycles = 7},

                new OpCode { opcode = 0xca, Mnemonic = "DEX", Mode = AddressingModes.IMPLI, Cycles = 2},
                new OpCode { opcode = 0x88, Mnemonic = "DEY", Mode = AddressingModes.IMPLI, Cycles = 2},

                new OpCode { opcode = 0x49, Mnemonic = "EOR", Mode = AddressingModes.IMMED, Cycles = 2},
                new OpCode { opcode = 0x45, Mnemonic = "EOR", Mode = AddressingModes.ZEROP, Cycles = 3},
                new OpCode { opcode = 0x55, Mnemonic = "EOR", Mode = AddressingModes.ZEPIX, Cycles = 4},
                new OpCode { opcode = 0x4d, Mnemonic = "EOR", Mode = AddressingModes.ABSOL, Cycles = 4},
                new OpCode { opcode = 0x5d, Mnemonic = "EOR", Mode = AddressingModes.ABSIX, Cycles = 4, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},
                new OpCode { opcode = 0x59, Mnemonic = "EOR", Mode = AddressingModes.ABSIY, Cycles = 4, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},
                new OpCode { opcode = 0x41, Mnemonic = "EOR", Mode = AddressingModes.INDIN, Cycles = 6},
                new OpCode { opcode = 0x51, Mnemonic = "EOR", Mode = AddressingModes.ININD, Cycles = 5, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},

                new OpCode { opcode = 0xe6, Mnemonic = "INC", Mode = AddressingModes.ZEROP, Cycles = 5},
                new OpCode { opcode = 0xf6, Mnemonic = "INC", Mode = AddressingModes.ZEPIX, Cycles = 6},
                new OpCode { opcode = 0xee, Mnemonic = "INC", Mode = AddressingModes.ABSOL, Cycles = 6},
                new OpCode { opcode = 0xfe, Mnemonic = "INC", Mode = AddressingModes.ABSIX, Cycles = 7},

                new OpCode { opcode = 0xe8, Mnemonic = "INX", Mode = AddressingModes.IMPLI, Cycles = 2},
                new OpCode { opcode = 0xc8, Mnemonic = "INY", Mode = AddressingModes.IMPLI, Cycles = 2},

                new OpCode { opcode = 0x4c, Mnemonic = "JMP", Mode = AddressingModes.ABSOL, Cycles = 3},
                new OpCode { opcode = 0x6c, Mnemonic = "JMP", Mode = AddressingModes.INDIA, Cycles = 5},
                new OpCode { opcode = 0x20, Mnemonic = "JSR", Mode = AddressingModes.ABSOL, Cycles = 6},

                new OpCode { opcode = 0xa9, Mnemonic = "LDA", Mode = AddressingModes.IMMED, Cycles = 2},
                new OpCode { opcode = 0xa5, Mnemonic = "LDA", Mode = AddressingModes.ZEROP, Cycles = 3},
                new OpCode { opcode = 0xb5, Mnemonic = "LDA", Mode = AddressingModes.ZEPIX, Cycles = 4},
                new OpCode { opcode = 0xad, Mnemonic = "LDA", Mode = AddressingModes.ABSOL, Cycles = 4},
                new OpCode { opcode = 0xbd, Mnemonic = "LDA", Mode = AddressingModes.ABSIX, Cycles = 4, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},
                new OpCode { opcode = 0xb9, Mnemonic = "LDA", Mode = AddressingModes.ABSIY, Cycles = 4, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},
                new OpCode { opcode = 0xa1, Mnemonic = "LDA", Mode = AddressingModes.INDIN, Cycles = 6},
                new OpCode { opcode = 0xb1, Mnemonic = "LDA", Mode = AddressingModes.ININD, Cycles = 5, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},

                new OpCode { opcode = 0xa2, Mnemonic = "LDX", Mode = AddressingModes.IMMED, Cycles = 2},
                new OpCode { opcode = 0xa6, Mnemonic = "LDX", Mode = AddressingModes.ZEROP, Cycles = 3},
                new OpCode { opcode = 0xb6, Mnemonic = "LDX", Mode = AddressingModes.ZEPIY, Cycles = 4},
                new OpCode { opcode = 0xae, Mnemonic = "LDX", Mode = AddressingModes.ABSOL, Cycles = 4},
                new OpCode { opcode = 0xbe, Mnemonic = "LDX", Mode = AddressingModes.ABSIY, Cycles = 4, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},
                new OpCode { opcode = 0xa0, Mnemonic = "LDY", Mode = AddressingModes.IMMED, Cycles = 2},
                new OpCode { opcode = 0xa4, Mnemonic = "LDY", Mode = AddressingModes.ZEROP, Cycles = 3},
                new OpCode { opcode = 0xb4, Mnemonic = "LDY", Mode = AddressingModes.ZEPIX, Cycles = 4},
                new OpCode { opcode = 0xac, Mnemonic = "LDY", Mode = AddressingModes.ABSOL, Cycles = 4},
                new OpCode { opcode = 0xbc, Mnemonic = "LDY", Mode = AddressingModes.ABSIX, Cycles = 4, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},

                new OpCode { opcode = 0x4a, Mnemonic = "LSR", Mode = AddressingModes.IMPLI, Cycles = 2},
                new OpCode { opcode = 0x46, Mnemonic = "LSR", Mode = AddressingModes.ZEROP, Cycles = 5},
                new OpCode { opcode = 0x56, Mnemonic = "LSR", Mode = AddressingModes.ZEPIX, Cycles = 6},
                new OpCode { opcode = 0x4e, Mnemonic = "LSR", Mode = AddressingModes.ABSOL, Cycles = 6},
                new OpCode { opcode = 0x5e, Mnemonic = "LSR", Mode = AddressingModes.ABSIX, Cycles = 7},

                new OpCode { opcode = 0xea, Mnemonic = "NOP", Mode = AddressingModes.IMPLI, Cycles = 2},

                new OpCode { opcode = 0x09, Mnemonic = "ORA", Mode = AddressingModes.IMMED, Cycles = 2},
                new OpCode { opcode = 0x05, Mnemonic = "ORA", Mode = AddressingModes.ZEROP, Cycles = 3},
                new OpCode { opcode = 0x15, Mnemonic = "ORA", Mode = AddressingModes.ZEPIX, Cycles = 4},
                new OpCode { opcode = 0x0d, Mnemonic = "ORA", Mode = AddressingModes.ABSOL, Cycles = 4},
                new OpCode { opcode = 0x1d, Mnemonic = "ORA", Mode = AddressingModes.ABSIX, Cycles = 4, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},
                new OpCode { opcode = 0x19, Mnemonic = "ORA", Mode = AddressingModes.ABSIY, Cycles = 4, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},
                new OpCode { opcode = 0x01, Mnemonic = "ORA", Mode = AddressingModes.INDIN, Cycles = 6},
                new OpCode { opcode = 0x11, Mnemonic = "ORA", Mode = AddressingModes.ININD, Cycles = 5, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},

                new OpCode { opcode = 0x48, Mnemonic = "PHA", Mode = AddressingModes.IMPLI, Cycles = 3},
                new OpCode { opcode = 0x08, Mnemonic = "PHP", Mode = AddressingModes.IMPLI, Cycles = 3},
                new OpCode { opcode = 0x5a, Mnemonic = "PHY", Mode = AddressingModes.IMPLI, Cycles = 3},
                new OpCode { opcode = 0x68, Mnemonic = "PLA", Mode = AddressingModes.IMPLI, Cycles = 4},
                new OpCode { opcode = 0x28, Mnemonic = "PLP", Mode = AddressingModes.IMPLI, Cycles = 4},
                new OpCode { opcode = 0x7a, Mnemonic = "PLY", Mode = AddressingModes.IMPLI, Cycles = 3},

                new OpCode { opcode = 0x2a, Mnemonic = "ROL", Mode = AddressingModes.IMPLI, Cycles = 2},
                new OpCode { opcode = 0x26, Mnemonic = "ROL", Mode = AddressingModes.ZEROP, Cycles = 5},
                new OpCode { opcode = 0x36, Mnemonic = "ROL", Mode = AddressingModes.ZEPIX, Cycles = 6},
                new OpCode { opcode = 0x2e, Mnemonic = "ROL", Mode = AddressingModes.ABSOL, Cycles = 6},
                new OpCode { opcode = 0x3e, Mnemonic = "ROL", Mode = AddressingModes.ABSIX, Cycles = 7},

                new OpCode { opcode = 0x6a, Mnemonic = "ROR", Mode = AddressingModes.IMPLI, Cycles = 2},
                new OpCode { opcode = 0x66, Mnemonic = "ROR", Mode = AddressingModes.ZEROP, Cycles = 5},
                new OpCode { opcode = 0x76, Mnemonic = "ROR", Mode = AddressingModes.ZEPIX, Cycles = 6},
                new OpCode { opcode = 0x6e, Mnemonic = "ROR", Mode = AddressingModes.ABSOL, Cycles = 6},
                new OpCode { opcode = 0x7e, Mnemonic = "ROR", Mode = AddressingModes.ABSIX, Cycles = 7},

                new OpCode { opcode = 0x40, Mnemonic = "RTI", Mode = AddressingModes.IMPLI, Cycles = 6},
                new OpCode { opcode = 0x60, Mnemonic = "RTS", Mode = AddressingModes.IMPLI, Cycles = 6},

                new OpCode { opcode = 0xe9, Mnemonic = "SBC", Mode = AddressingModes.IMMED, Cycles = 2},
                new OpCode { opcode = 0xe5, Mnemonic = "SBC", Mode = AddressingModes.ZEROP, Cycles = 3},
                new OpCode { opcode = 0xf5, Mnemonic = "SBC", Mode = AddressingModes.ZEPIX, Cycles = 4},
                new OpCode { opcode = 0xed, Mnemonic = "SBC", Mode = AddressingModes.ABSOL, Cycles = 4},
                new OpCode { opcode = 0xfd, Mnemonic = "SBC", Mode = AddressingModes.ABSIX, Cycles = 4, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},
                new OpCode { opcode = 0xf9, Mnemonic = "SBC", Mode = AddressingModes.ABSIY, Cycles = 4, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},
                new OpCode { opcode = 0xe1, Mnemonic = "SBC", Mode = AddressingModes.INDIN, Cycles = 6},
                new OpCode { opcode = 0xf1, Mnemonic = "SBC", Mode = AddressingModes.ININD, Cycles = 5, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},

                new OpCode { opcode = 0x38, Mnemonic = "SEC", Mode = AddressingModes.IMPLI, Cycles = 2},
                new OpCode { opcode = 0xf8, Mnemonic = "SED", Mode = AddressingModes.IMPLI, Cycles = 2},
                new OpCode { opcode = 0x78, Mnemonic = "SEI", Mode = AddressingModes.IMPLI, Cycles = 2},

                new OpCode { opcode = 0x85, Mnemonic = "STA", Mode = AddressingModes.ZEROP, Cycles = 3},
                new OpCode { opcode = 0x95, Mnemonic = "STA", Mode = AddressingModes.ZEPIX, Cycles = 4},
                new OpCode { opcode = 0x8d, Mnemonic = "STA", Mode = AddressingModes.ABSOL, Cycles = 4},
                new OpCode { opcode = 0x9d, Mnemonic = "STA", Mode = AddressingModes.ABSIX, Cycles = 4, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},
                new OpCode { opcode = 0x99, Mnemonic = "STA", Mode = AddressingModes.ABSIY, Cycles = 4, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},
                new OpCode { opcode = 0x81, Mnemonic = "STA", Mode = AddressingModes.INDIN, Cycles = 6},
                new OpCode { opcode = 0x91, Mnemonic = "STA", Mode = AddressingModes.ININD, Cycles = 5, CyclesAdjustment = CyclesAdjustments.CYCLES_CROSS_PAGE_ADDS_ONE},

                new OpCode { opcode = 0x86, Mnemonic = "STX", Mode = AddressingModes.ZEROP, Cycles = 3},
                new OpCode { opcode = 0x96, Mnemonic = "STX", Mode = AddressingModes.ZEPIY, Cycles = 4},
                new OpCode { opcode = 0x8e, Mnemonic = "STX", Mode = AddressingModes.ABSOL, Cycles = 4},

                new OpCode { opcode = 0x84, Mnemonic = "STY", Mode = AddressingModes.ZEROP, Cycles = 3},
                new OpCode { opcode = 0x94, Mnemonic = "STY", Mode = AddressingModes.ZEPIX, Cycles = 4},
                new OpCode { opcode = 0x8c, Mnemonic = "STY", Mode = AddressingModes.ABSOL, Cycles = 4},

                new OpCode { opcode = 0xaa, Mnemonic = "TAX", Mode = AddressingModes.IMPLI, Cycles = 2},
                new OpCode { opcode = 0xa8, Mnemonic = "TAY", Mode = AddressingModes.IMPLI, Cycles = 2},

                new OpCode { opcode = 0xba, Mnemonic = "TSX", Mode = AddressingModes.IMPLI, Cycles = 2},
                new OpCode { opcode = 0x8a, Mnemonic = "TXA", Mode = AddressingModes.IMPLI, Cycles = 2},
                new OpCode { opcode = 0x9a, Mnemonic = "TXS", Mode = AddressingModes.IMPLI, Cycles = 2},
                new OpCode { opcode = 0x98, Mnemonic = "TYA", Mode = AddressingModes.IMPLI, Cycles = 2},
            };
    }
}