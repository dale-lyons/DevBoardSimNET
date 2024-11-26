using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Processors;

namespace Motorola6502.Disassembler
{
    public class Disassembler : IDisassembler
    {
        private static Dictionary<byte, OpCode> opLookup = new Dictionary<byte, OpCode>();

        static Disassembler()
        {
            foreach (OpCode op in OpCodes.table)
            {
                opLookup[op.opcode] = op;
            }
        }

        public int Align(uint pos, ISystemMemory code)
        {
            throw new NotImplementedException();
        }

        public CodeLine ProcessInstruction(uint startAddr, ISystemMemory code, out uint newAddr)
        {
            CodeLine ret = null;
            int len = 2;

            var opcode = (byte)code.GetMemory(startAddr, WordSize.OneByte, false);
            if(!opLookup.ContainsKey(opcode))
            {
                newAddr = (uint)(startAddr + 1);
                ret = new CodeLine("Bad opcode", startAddr, 1);
                ret.CodeLineType = CodeLine.CodeLineTypes.Code;
                return ret;
            }

            OpCode op = opLookup[opcode];
            var sb = new StringBuilder(op.Mnemonic);
            sb.Append(" ");

            var next1 = (byte)code.GetMemory(startAddr + 1, WordSize.OneByte, false);
            var next2 = (byte)code.GetMemory(startAddr + 2, WordSize.OneByte, false);

            switch (op.Mode)
            {
                case AddressingModes.IMMED:
                    {//lda #10
                        sb.Append("#" + next1.ToString("X2"));
                    }
                    break;
                case AddressingModes.ABSOL:
                    {//lda $1234
                        sb.Append("$" + next2.ToString("X2") + next1.ToString("X2"));
                        len = 3;
                    }
                    break;
                case AddressingModes.ZEROP:
                    {//lda $12
                        sb.Append("$" + next1.ToString("X2"));
                    }
                    break;
                case AddressingModes.IMPLI:
                    {//tax
                        len = 1;
                    }
                    break;
                case AddressingModes.INDIA:
                    {
                        sb.Append("$" + next2.ToString("X2") + next1.ToString("X2"));
                        len = 3;
                    }
                    break;
                case AddressingModes.ABSIX:
                    {
                        sb.Append("$" + next2.ToString("X2") + next1.ToString("X2") + ",X");
                        len = 3;
                    }
                    break;
                case AddressingModes.ABSIY:
                    {
                        sb.Append("$" + next2.ToString("X2") + next1.ToString("X2") + ",Y");
                        len = 3;
                    }
                    break;
                case AddressingModes.ZEPIX:
                    {
                        sb.Append("$" + next1.ToString("X2") + ",X");
                    }
                    break;
                case AddressingModes.ZEPIY:
                    {
                        sb.Append("$" + next1.ToString("X2") + ",Y");
                    }
                    break;
                case AddressingModes.INDIN:
                    {
                        sb.Append("($" + next1.ToString("X2") + ",X)");
                    }
                    break;
                case AddressingModes.ININD:
                    {
                        sb.Append("($" + next1.ToString("X2") + "),Y");
                    }
                    break;
                case AddressingModes.RELAT:
                    {
//                        sb.Append("($" + next1.ToString("X2") + "),Y");
                        sb.Append("($" + next1.ToString("X2") + ")");
                    }
                    break;
                default:
                    throw new Exception();

                //case AddressingModes.ACCUM:
                //    {
                //        sb.Append("A");
                //    }
                //    break;
            };
            ret = new CodeLine(sb.ToString(), startAddr, len);
            ret.CodeLineType = CodeLine.CodeLineTypes.Code;
            newAddr = (uint)(startAddr + len);
            return ret;
        }

    }
}