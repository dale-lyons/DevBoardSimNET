using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Processors;

namespace ZilogZ80
{
    class DisassemblerZ80 : IDisassembler
    {
        public static char HexFormat = 'h';
        private IProcessor mProcesor;

        public DisassemblerZ80(IProcessor processor)
        {
            mProcesor = processor;
        }

        int IDisassembler.Align(uint pos, ISystemMemory code)
        {
            //ushort alignedPC = (ushort)pos;
            //if (TestAlignment(alignedPC, code, (ushort)pc))
            //    return alignedPC;

            //alignedPC--;
            //if (TestAlignment(alignedPC, code, (ushort)pc))
            //    return alignedPC;

            //alignedPC--;
            //if (TestAlignment(alignedPC, code, (ushort)pc))
            //    return alignedPC;

            //alignedPC--;
            //if (TestAlignment(alignedPC, code, (ushort)pc))
            //    return alignedPC;

            return 0;
        }
        public static bool TestAlignment(ushort startAddr, ISystemMemory code, ushort pc)
        {
            return true;
            /*
            if (startAddr < pc)
            {
                ushort addr = startAddr;
                while (addr < pc)
                {
                    byte opcode = code[addr];
                    int count = 0;
                    //                    mOpcodeFunctions[opcode](pc, code, opcode, out count);
                    addr += (ushort)count;
                }
                return (addr == pc);
            }
            else
            {
                ushort addr = pc;
                while (addr < startAddr)
                {
                    byte opcode = code[addr];
                    int count = 0;
                    //                    mOpcodeFunctions[opcode](pc, code, opcode, out count);
                    addr += (ushort)count;
                }
                return (addr == startAddr);
            }
            */
        }
        public struct iasm
        {
            public string Asm;
            public int t_states;
            public int t_states2;
        }

        CodeLine IDisassembler.ProcessInstruction(uint startAddr, ISystemMemory code, out uint newAddr)
        {
            var addr = startAddr;

            if (addr == 0x14a5)
            {

            }
            byte opc = (byte)code.GetMemory(addr++, WordSize.OneByte, false);
            OpCode dasm = OpCodes.dasm_base[opc];
            iasm i;
            byte disp_u = 0;
            bool have_disp = false;

            switch (opc)
            {
                case 0xDD:
                case 0xFD:
                    byte next = (byte)code.GetMemory(addr++, WordSize.OneByte, false);
                    if ((next | 0x20) == 0xFD || next == 0xED)
                    {
                        i.Asm = "NOP*";
                        i.t_states = 4;
                        dasm = null;
                    }
                    else if (next == 0xCB)
                    {
                        disp_u = (byte)code.GetMemory(addr++, WordSize.OneByte, false);
                        next = (byte)code.GetMemory(addr++, WordSize.OneByte, false);

                        dasm = (opc == 0xDD) ? OpCodes.dasm_ddcb[next] : OpCodes.dasm_fdcb[next];
                        have_disp = true;
                    }
                    else
                    {
                        dasm = (opc == 0xDD) ? OpCodes.dasm_dd[next] : OpCodes.dasm_fd[next];
                        if (dasm.mnemonic == null) //mirrored instructions
                        {
                            dasm = OpCodes.dasm_base[next];
                            i.t_states = 4;
                            i.t_states2 = 4;
                        }
                    }
                    break;

                case 0xED:
                    next = (byte)code.GetMemory(addr++, WordSize.OneByte, false);
                    dasm = OpCodes.dasm_ed[next];
                    if (dasm.mnemonic == null)
                    {
                        i.Asm = "NOP*";
                        i.t_states = 8;
                        dasm = null;
                    }
                    break;

                case 0xCB:
                    next = (byte)code.GetMemory(addr++, WordSize.OneByte, false);
                    dasm = OpCodes.dasm_cb[next];
                    break;

                default:
                    dasm = OpCodes.dasm_base[opc];
                    break;
            }
            if(dasm == null)
            {
                var invalid = new CodeLine("INVALID", startAddr, (int)(addr - startAddr));
                invalid.CodeLineType = CodeLine.CodeLineTypes.Comment;
                newAddr = addr;
                return invalid;

            }

            var sb = new StringBuilder();
            foreach (var ch in dasm.mnemonic)
            {
                switch (ch)
                {
                    case '@':
                        {
                            var lo = (byte)code.GetMemory(addr++, WordSize.OneByte, false);
                            var hi = (byte)code.GetMemory(addr++, WordSize.OneByte, false);
                            ushort val = (ushort)(lo + hi * 0x100);
                            sb.Append(FormatWord(val));
                            break;
                        }

                    case '$':
                    case '%':
                        if(!have_disp)
                            disp_u = (byte)code.GetMemory(addr++, WordSize.OneByte, false);
                        var disp = (disp_u & 0x80) != 0 ? -(((~disp_u) & 0x7f) + 1) : disp_u;

                        //var lott = (byte)code.GetMemory(addr++, WordSize.OneByte, false);
                        sb.Append(FormatByte((byte)disp));
                        break;

                    case '#':
                        {
                            var lo = (byte)code.GetMemory(addr++, WordSize.OneByte, false);
                            sb.Append(FormatByte(lo));
                            //if (lo >= 0x20 && lo <= 0x7f)
                            //    i.Comment = string.Format("'{0}'", (char)lo);
                            //i.byte_val = lo;
                            break;
                        }

                    default:
                        if (ch == ' ')
                            sb.Append('\t');
                        else
                            sb.Append(ch);
                        break;
                }

            }
            var ret = new CodeLine(sb.ToString(), startAddr, (int)(addr - startAddr));
            ret.CodeLineType = CodeLine.CodeLineTypes.Code;
            newAddr = addr;

            return ret;
        }

        public static string FormatWord(ushort w)
        {
            return string.Format("{0:X4}", w);
        }//FormatWord

        public static string FormatByte(byte b)
        {
            return string.Format("{0:X2}", b);
        }//FormatByte

        public uint Align(uint pos, ISystemMemory code, uint pc)
        {
            return 0;
        }
    }
}