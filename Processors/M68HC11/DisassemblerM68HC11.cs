using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using Processors;

namespace M68HC11
{
    public class InvalidOpcodeException : Exception
    {
        public uint Address { get; set; }
        public byte Opcode { get; set; }
    }

    public partial class DisassemblerM68HC11 : IDisassembler
    {
        private uint TopAddress { get; set; }
        private ISystemMemory SystemMemory { get { return mProcessor.SystemMemory; } }
        private IProcessor mProcessor;
        public DisassemblerM68HC11(IProcessor processor, uint topAddress)
        {
            TopAddress = topAddress;
            mProcessor = processor;
        }

        /// <summary>
        /// Is the byte at pos an instruction?
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="code"></param>
        /// <param name="pc"></param>
        /// <returns></returns>
        public int Align(uint pos, ISystemMemory code)
        {
            return 0;
            //uint newAddr = 0;
            //uint p1, p2;
            //uint pc = pos;
            //if (pos == TopAddress)
            //    return 0;
            //else if (pos < TopAddress)
            //{
            //    p1 = pos;
            //    p2 = TopAddress;
            //}
            //else
            //{// pos >= TopAddress
            //    p1 = TopAddress;
            //    p2 = pos;
            //}

            //while (p1 < p2)
            //{
            //    ProcessInstruction(p1, code, out newAddr);
            //    if (newAddr == p2)
            //        return 0;
            //    p1 = newAddr;
            //}
            //if (newAddr > p2)
            //    return (int)(newAddr - p2);
            //else
            //    return (int)(-(newAddr - p2));
        }

        public CodeLine ProcessInstruction(uint startAddr, ISystemMemory code, out uint newAddr)
        {
            newAddr = 0;
            byte opcode = code[startAddr];

            string str = string.Empty;
            var cl = new CodeLine();

            try
            {
                str = Page0(opcode, startAddr, out newAddr);
                cl.CodeLineType = CodeLine.CodeLineTypes.Code;
                cl.Address = startAddr;
                cl.Length = (int)(newAddr - startAddr);
            }
            catch (InvalidOpcodeException e)
            {
                str = "Invalid(" + e.Address.ToString("X4") + ":" + e.Opcode.ToString("X2");
                cl.CodeLineType = CodeLine.CodeLineTypes.Comment;
                newAddr = startAddr + 1;
            }
            cl.Text = str;
            return cl;
        }

        public string Page0(byte opcode, uint addr, out uint newAddr)
        {
            newAddr = addr;
            switch (opcode)
            {
                // No parameter:
                case (byte)OpcodesEnum.OP_ABA:
                case (byte)OpcodesEnum.OP_ABX:
                case (byte)OpcodesEnum.OP_ASLA:
                case (byte)OpcodesEnum.OP_ASLB:
                case (byte)OpcodesEnum.OP_ASLD:
                case (byte)OpcodesEnum.OP_ASRA:
                case (byte)OpcodesEnum.OP_ASRB:
                case (byte)OpcodesEnum.OP_CLI:
                case (byte)OpcodesEnum.OP_CLRA:
                case (byte)OpcodesEnum.OP_CLRB:
                case (byte)OpcodesEnum.OP_CLV:
                case (byte)OpcodesEnum.OP_DAA:
                case (byte)OpcodesEnum.OP_DECA:
                case (byte)OpcodesEnum.OP_DECB:
                case (byte)OpcodesEnum.OP_DES:
                case (byte)OpcodesEnum.OP_DEX:
                case (byte)OpcodesEnum.OP_FDIV:
                case (byte)OpcodesEnum.OP_IDIV:
                case (byte)OpcodesEnum.OP_INCA:
                case (byte)OpcodesEnum.OP_INCB:
                case (byte)OpcodesEnum.OP_INS:
                case (byte)OpcodesEnum.OP_INX:
                case (byte)OpcodesEnum.OP_LSRA:
                case (byte)OpcodesEnum.OP_LSRB:
                case (byte)OpcodesEnum.OP_LSRD:
                case (byte)OpcodesEnum.OP_MUL:
                case (byte)OpcodesEnum.OP_PSHA:
                case (byte)OpcodesEnum.OP_PSHB:
                case (byte)OpcodesEnum.OP_PSHX:
                case (byte)OpcodesEnum.OP_PULA:
                case (byte)OpcodesEnum.OP_PULB:
                case (byte)OpcodesEnum.OP_PULX:
                case (byte)OpcodesEnum.OP_ROLA:
                case (byte)OpcodesEnum.OP_ROLB:
                case (byte)OpcodesEnum.OP_RORA:
                case (byte)OpcodesEnum.OP_RORB:
                case (byte)OpcodesEnum.OP_RTS:
                case (byte)OpcodesEnum.OP_SBA:
                case (byte)OpcodesEnum.OP_SEI:
                case (byte)OpcodesEnum.OP_TAB:
                case (byte)OpcodesEnum.OP_TBA:
                case (byte)OpcodesEnum.OP_TSX:
                case (byte)OpcodesEnum.OP_TSTA:
                case (byte)OpcodesEnum.OP_TSTB:
                case (byte)OpcodesEnum.OP_XGDX:
                case (byte)OpcodesEnum.OP_TAP:
                case (byte)OpcodesEnum.OP_STOP:
                    {
                        newAddr = addr + 1;
                        return opcodeToString(opcode);
                    }

                // Immediate one byte:
                case (byte)OpcodesEnum.OP_ADCA_IMM:
                case (byte)OpcodesEnum.OP_ADDA_IMM:
                case (byte)OpcodesEnum.OP_ADDB_IMM:
                case (byte)OpcodesEnum.OP_ANDA_IMM:
                case (byte)OpcodesEnum.OP_ANDB_IMM:
                case (byte)OpcodesEnum.OP_CMPA_IMM:
                case (byte)OpcodesEnum.OP_CMPB_IMM:
                case (byte)OpcodesEnum.OP_EORA_IMM:
                case (byte)OpcodesEnum.OP_EORB_IMM:
                case (byte)OpcodesEnum.OP_LDAA_IMM:
                case (byte)OpcodesEnum.OP_LDAB_IMM:
                case (byte)OpcodesEnum.OP_ORAA_IMM:
                case (byte)OpcodesEnum.OP_SUBA_IMM:
                case (byte)OpcodesEnum.OP_SUBB_IMM:
                case (byte)OpcodesEnum.OP_BITA_IMM:
                case (byte)OpcodesEnum.OP_BITB_IMM:
                    {
                        newAddr = addr + 2;
                        byte data = SystemMemory[addr + 1];
                        return opcodeToString(opcode) + "\t#$" + data.ToString("X2");
                    }
                // Direct:
                case (byte)OpcodesEnum.OP_ADCA_DIR:
                case (byte)OpcodesEnum.OP_ADDA_DIR:
                case (byte)OpcodesEnum.OP_ADDB_DIR:
                case (byte)OpcodesEnum.OP_ADDD_DIR:
                case (byte)OpcodesEnum.OP_ANDA_DIR:
                case (byte)OpcodesEnum.OP_ANDB_DIR:
                case (byte)OpcodesEnum.OP_CMPA_DIR:
                case (byte)OpcodesEnum.OP_CMPB_DIR:
                case (byte)OpcodesEnum.OP_CPX_DIR:
                case (byte)OpcodesEnum.OP_JSR_DIR:
                case (byte)OpcodesEnum.OP_LDAA_DIR:
                case (byte)OpcodesEnum.OP_LDAB_DIR:
                case (byte)OpcodesEnum.OP_LDD_DIR:
                case (byte)OpcodesEnum.OP_LDX_DIR:
                case (byte)OpcodesEnum.OP_ORAA_DIR:
                case (byte)OpcodesEnum.OP_ORAB_DIR:
                case (byte)OpcodesEnum.OP_STAA_DIR:
                case (byte)OpcodesEnum.OP_STAB_DIR:
                case (byte)OpcodesEnum.OP_STD_DIR:
                case (byte)OpcodesEnum.OP_STX_DIR:
                case (byte)OpcodesEnum.OP_SUBA_DIR:
                case (byte)OpcodesEnum.OP_SUBB_DIR:
                case (byte)OpcodesEnum.OP_BITA_DIR:
                case (byte)OpcodesEnum.OP_BITB_DIR:
                    {
                        byte data = SystemMemory[addr + 1];
                        newAddr = addr + 2;
                        return opcodeToString(opcode) + "\t$" + data.ToString("X2");
                    }
                // Relative (signed):
                case (byte)OpcodesEnum.OP_BCC:
                case (byte)OpcodesEnum.OP_BCS:
                case (byte)OpcodesEnum.OP_BEQ:
                case (byte)OpcodesEnum.OP_BGE:
                case (byte)OpcodesEnum.OP_BGT:
                case (byte)OpcodesEnum.OP_BHI:
                //	case OP_BHS: // Same as BCC
                //	case OP_BLO:
                case (byte)OpcodesEnum.OP_BLS:
                case (byte)OpcodesEnum.OP_BLE:
                case (byte)OpcodesEnum.OP_BLT:
                case (byte)OpcodesEnum.OP_BMI:
                case (byte)OpcodesEnum.OP_BNE:
                case (byte)OpcodesEnum.OP_BPL:
                case (byte)OpcodesEnum.OP_BRA:
                case (byte)OpcodesEnum.OP_BSR:
                case (byte)OpcodesEnum.OP_BVC:
                case (byte)OpcodesEnum.OP_BVS:
                    {// calculate destination address:
                        byte offset = SystemMemory[addr + 1];
                        ushort dest = (ushort)(addr + offset + 2);
                        newAddr = addr + 2;
                        return opcodeToString(opcode) + "\t$" + offset.ToString("X2") + "(" + dest.ToString("X4") + ")";
                        //int offset = int8_t(data[++pc]);
                        //std::cout << Mnenomic(op) << "\t$" << std::hex << pc + offset + 1 + epromStart;
                    }
                // Immediate two byte:
                case (byte)OpcodesEnum.OP_CPX_IMM:
                case (byte)OpcodesEnum.OP_LDD_IMM:
                case (byte)OpcodesEnum.OP_LDX_IMM:
                case (byte)OpcodesEnum.OP_LDS_IMM:
                case (byte)OpcodesEnum.OP_ADDD_IMM:
                    {
                        ushort value = (ushort)SystemMemory.GetMemory(addr + 1, WordSize.TwoByte, false);
                        newAddr = addr + 3;
                        return opcodeToString(opcode) + "\t#" + AddressString(value);
                    }
                // Extended (two byte) parameter:
                case (byte)OpcodesEnum.OP_CLR_EXT:
                case (byte)OpcodesEnum.OP_INC_EXT:
                case (byte)OpcodesEnum.OP_JMP_EXT:
                case (byte)OpcodesEnum.OP_JSR_EXT:
                case (byte)OpcodesEnum.OP_LDAA_EXT:
                case (byte)OpcodesEnum.OP_LDAB_EXT:
                case (byte)OpcodesEnum.OP_LDX_EXT:
                case (byte)OpcodesEnum.OP_LSR_EXT:
                case (byte)OpcodesEnum.OP_STAA_EXT:
                case (byte)OpcodesEnum.OP_STAB_EXT:
                case (byte)OpcodesEnum.OP_TST_EXT:
                case (byte)OpcodesEnum.OP_ANDA_EXT:
                case (byte)OpcodesEnum.OP_ASL_EXT:
                case (byte)OpcodesEnum.OP_BITA_EXT:
                    {
                        ushort value = (ushort)SystemMemory.GetMemory((uint)(addr + 1), WordSize.TwoByte, false);
                        newAddr = (uint)(addr + 3);
                        return opcodeToString(opcode) + "\t" + AddressString(value);
                    }

                case (byte)OpcodesEnum.OP_PRE_BYTE_18:
                    {
                        byte b = (byte)SystemMemory[addr + 1];
                        byte b2 = (byte)SystemMemory[addr + 2];
                        switch (b)
                        {
                            case 0x08:
                                newAddr = addr + 1;
                                return opcodeToString(b);
                            case 0x3a:
                                return "ABY";
                            case 0x68:
                            case 0x6f:
                                {
                                    newAddr = addr + 3;
                                    if (b != 0)
                                        return opcodeToString(b) + "\t$" + b2.ToString("X2") + ",Y";
                                    else
                                        return opcodeToString(b) + "\tY";
                                }
                            case 0x7d:
                            case 0xa1:
                            case 0xa4:
                            case 0xa5:
                            case 0xe5:
                            case 0xa6:
                                newAddr = addr + 3;
                                return opcodeToString(b) + "\t$" + b2.ToString("X2") + ",Y";
                            case 0xce:
                                {
                                    ushort value = (ushort)SystemMemory.GetMemory(addr + 2, WordSize.TwoByte, false);
                                    newAddr = addr + 4;
                                    return "LDY" + "\t#" + AddressString(value);
                                }
                            //case 0xde:
                            case 0xfe:
                                {
                                    ushort data = (ushort)SystemMemory.GetMemory(addr + 2, WordSize.TwoByte, false);
                                    newAddr = addr + 4;
                                    return "LDY" + "\t#" + data.ToString("X4");
                                }
                            default:
                                throw new Exception();
                                //newAddr = addr + 2;
                                //return "";
                        }
                    }
                case (byte)OpcodesEnum.OP_PRE_BYTE_1A:
                    {
                        byte b = (byte)SystemMemory.GetMemory(addr + 1, WordSize.OneByte, false);
                        switch (b)
                        {
                            case 0x83:
                                {
                                    newAddr = addr + 4;
                                    byte b1 = SystemMemory[addr + 1];
                                    return "CPD" + "\t#$" + b1.ToString("X2");
                                }
                            case 0x93:
                                {
                                    newAddr = addr + 3;
                                    byte b1 = SystemMemory[addr + 2];
                                    return "CPD" + "\t$" + b1.ToString("X2");
                                }
                            case 0xef:
                                {
                                    newAddr = addr + 3;
                                    byte b1 = SystemMemory[addr + 2];
                                    return "STY" + "\t$" + b1.ToString("X2") + ",X";
                                }
                            case 0xee:
                                {
                                    newAddr = addr + 3;
                                    byte b1 = SystemMemory[addr + 2];
                                    return opcodeToString(b) + "\t$" + b1.ToString("X2") + ",X";
                                }
                            default:
                                throw new Exception();
                        }
                    }

                case (byte)OpcodesEnum.OP_PRE_BYTE_CD:
                    {
                        byte b = (byte)SystemMemory.GetMemory(addr + 1, WordSize.OneByte, false);
                        switch (b)
                        {
                            case 0xee:
                                {
                                    newAddr = addr + 3;
                                    b = SystemMemory[addr + 2];
                                    if (b != 0)
                                        return "LDX\t$" + b.ToString("X2") + ",Y";
                                    else
                                        return "LDX\tY";
                                }
                            default:
                                throw new Exception();
                                //                                newAddr = addr + 2;
                                //                                return "";
                        }
                    }

                // Indexed:
                case (byte)OpcodesEnum.OP_ADCB_IND_X:
                case (byte)OpcodesEnum.OP_CLR_IND:
                case (byte)OpcodesEnum.OP_CMPA_IND_X:
                case (byte)OpcodesEnum.OP_INC_IND_X:
                case (byte)OpcodesEnum.OP_JMP_IND_X:
                case (byte)OpcodesEnum.OP_JSR_IND:
                case (byte)OpcodesEnum.OP_LDX_IND:
                case (byte)OpcodesEnum.OP_LDAA_IND_X:
                case (byte)OpcodesEnum.OP_LDAB_IND_X:
                case (byte)OpcodesEnum.OP_ORAA_IND_X:
                case (byte)OpcodesEnum.OP_ORAB_IND_X:
                case (byte)OpcodesEnum.OP_STAA_IND_X:
                case (byte)OpcodesEnum.OP_STAB_IND_X:
                case (byte)OpcodesEnum.OP_SUBA_IND_X:
                case (byte)OpcodesEnum.OP_SUBB_IND_X:
                case (byte)OpcodesEnum.OP_ANDA_IND_X:
                case (byte)OpcodesEnum.OP_ASL_IND_X:
                case (byte)OpcodesEnum.OP_ROL_IND:
                    {
                        newAddr = addr + 2;
                        var b1 = SystemMemory[addr + 1];
                        return opcodeToString(opcode) + "\t$" + b1.ToString("X2") + ",X";

                    }
                case (byte)OpcodesEnum.OP_BRCLR_IND_X:
                    {
                        newAddr = addr + 4;
                        var b1 = SystemMemory[addr + 1];
                        var b2 = SystemMemory[addr + 2];
                        var b3 = SystemMemory[addr + 3];
                        return opcodeToString(opcode) + "\t$" + b1.ToString("X2") + ",X," + b2.ToString("X2") + "," + (newAddr + b3).ToString("X4");
                    }
                default:
                    throw new InvalidOpcodeException() { Address = addr, Opcode = opcode };
            }
        }
    }
}