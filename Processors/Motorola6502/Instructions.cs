using Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Motorola6502
{
    public partial class Motorola6502
    {
        protected delegate uint ExecuteInstruction(byte opcode);
        protected ExecuteInstruction[] mOpcodeFunctions;

        private bool InteruptsDisabled = true;
        private void setZ(byte data) { Registers6502.Zero = (data == 0); }
        private void setC(byte reg, byte mem) { Registers6502.Carry = (reg >= mem); }
        private void setV(byte op) { Registers6502.Overflow = ((op & 0x40) != 0); }
        private void setN(byte op) { Registers6502.Negative = ((op & 0x80) != 0); }

        private void setNV(byte data)
        {
            setN(data);
            setV(data);
        }

        private void setZN(byte data)
        {
            setZ(data);
            setN(data);
        }

        private void setZNV(byte data)
        {
            setZ(data);
            setN(data);
            setV(data);
        }

        //C->m0 m7->C
        private byte rol(byte op)
        {
            bool carry = ((op & 0x80) != 0);
            byte result = (byte)(op << 1);
            result |= Registers6502.Carry ? (byte)0x01 : (byte)0;
            Registers6502.Carry = carry;
            return result;
        }

        //C->m7; m0->C
        private byte ror(byte op)
        {
            bool carry = ((op & 0x01) != 0);
            byte result = (byte)(op >> 1);
            result |= Registers6502.Carry ? (byte)0x80 : (byte)0;
            return result;
        }

        //0->m7; m0->C
        private byte lsr(byte op)
        {
            Registers6502.Carry = ((op & 0x01) != 0);
            byte result = (byte)(op >> 1);
            return result;
        }

        //m7->C; 0->m0
        private byte asl(byte op)
        {
            Registers6502.Carry = ((op & 0x80) != 0);
            return (byte)(op << 1);
        }

        private uint branchIf(bool flag)
        {
            if (!flag)
            {
                Registers6502.PC += 2;
                return 3;
            }

            byte off = (byte)SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.OneByte, true);
            bool negative = ((off & 0x80) != 0);
            ushort offset = off;
            if (negative)
                offset |= 0xff00;

            ushort addr = (ushort)Registers6502.PC;
            addr += offset;
            addr += 2;
            Registers6502.PC = addr;
            return 4;
        }

        private uint Bne(byte opcode) { return branchIf(!Registers6502.Zero); }
        private uint Beq(byte opcode) { return branchIf(Registers6502.Zero); }
        private uint Bpl(byte opcode) { return branchIf(!Registers6502.Negative); }
        private uint Bmi(byte opcode) { return branchIf(Registers6502.Negative); }
        private uint Bvc(byte opcode) { return branchIf(!Registers6502.Overflow); }
        private uint Bvs(byte opcode) { return branchIf(Registers6502.Overflow); }
        private uint Bcc(byte opcode) { return branchIf(!Registers6502.Carry); }
        private uint Bcs(byte opcode) { return branchIf(Registers6502.Carry); }

        private uint Inc(byte opcode) { return incdec(1, opcode); }
        private uint Dec(byte opcode) { return incdec(byte.MaxValue, opcode); }

        private uint incdec(byte opcode, byte offset)
        {
            uint byteCount = 0;
            uint cycles = 2;
            ushort addr = getAddress(opcode, out byteCount);
            byte result = (byte)SystemMemory.GetMemory(addr, Processors.WordSize.OneByte, true);
            result += offset;
            SystemMemory.SetMemory(addr, Processors.WordSize.OneByte, result);
            setZN(result);
            Registers6502.PC += byteCount;
            return cycles;
        }

        private uint Sta(byte opcode)
        {
            uint cycles;
            uint byteCount = 0;
            putByte(opcode, out cycles, out byteCount, Registers6502.A);
            Registers6502.PC += byteCount;
            return cycles;
        }

        private uint Stx(byte opcode)
        {
            uint cycles;
            uint byteCount = 0;
            putByte(opcode, out cycles, out byteCount, Registers6502.X);
            Registers6502.PC += byteCount;
            return cycles;
        }

        private uint Sty(byte opcode)
        {
            uint cycles;
            uint byteCount = 0;
            putByte(opcode, out cycles, out byteCount, Registers6502.Y);
            Registers6502.PC += byteCount;
            return cycles;
        }

        private uint Adc(byte opcode)
        {
            uint cycles;
            uint byteCount;
            byte op = getByte(opcode, out cycles, out byteCount);
            Registers6502.A += op;
            if (Registers6502.Carry)
                Registers6502.A++;
            Registers6502.PC += byteCount;
            setZN(Registers6502.A);
            return cycles;
        }

        private uint Ora(byte opcode)
        {
            uint cycles;
            uint byteCount;
            byte op = getByte(opcode, out cycles, out byteCount);
            Registers6502.A |= op;
            Registers6502.PC += byteCount;
            setZN(Registers6502.A);
            return cycles;
        }

        private uint Asl(byte opcode)
        {
            uint byteCount = 0;
            uint cycles = 2;
            byte result = getByte(opcode, out cycles, out byteCount);
            result = asl(result);
            putByte(opcode, out cycles, out byteCount, result);
            setZN(result);
            Registers6502.PC += byteCount;
            return cycles;
        }
        private uint And(byte opcode)
        {
            uint cycles;
            uint byteCount;
            byte op = getByte(opcode, out cycles, out byteCount);
            Registers6502.A &= op;
            Registers6502.PC += byteCount;
            setZN(Registers6502.A);
            return cycles;
        }

        private uint Cmp(byte opcode)
        {
            uint cycles;
            uint byteCount;
            byte op = getByte(opcode, out cycles, out byteCount);
            var result = (byte)(Registers6502.A - op);
            Registers6502.PC += byteCount;
            setZN(result);
            setC(Registers6502.A, op);
            return cycles;
        }

        private uint Cpx(byte opcode)
        {
            uint cycles;
            uint byteCount;
            byte op = getByte(opcode, out cycles, out byteCount);
            var result = (byte)(Registers6502.X - op);
            Registers6502.PC += byteCount;
            setZN(result);
            setC(Registers6502.X, op);
            return cycles;
        }

        private uint Cpy(byte opcode)
        {
            uint cycles;
            uint byteCount;
            byte op = getByte(opcode, out cycles, out byteCount);
            var result = (byte)(Registers6502.Y - op);
            Registers6502.PC += byteCount;
            setZN(result);
            setC(Registers6502.Y, op);
            return cycles;
        }

        private uint Eor(byte opcode)
        {
            uint cycles;
            uint byteCount;
            byte op = getByte(opcode, out cycles, out byteCount);
            Registers6502.A ^= op;
            Registers6502.PC += byteCount;
            setZN(Registers6502.A);
            return cycles;
        }

        private uint Lda(byte opcode)
        {
            uint cycles;
            uint byteCount;
            Registers6502.A = getByte(opcode, out cycles, out byteCount);
            Registers6502.PC += byteCount;
            setZN(Registers6502.A);
            return cycles;
        }

        private uint Ldx(byte opcode)
        {
            uint cycles;
            uint byteCount;
            Registers6502.X = getByte(opcode, out cycles, out byteCount);
            Registers6502.PC += byteCount;
            setZN(Registers6502.A);
            return cycles;
        }

        private uint Ldy(byte opcode)
        {
            uint cycles;
            uint byteCount;
            Registers6502.Y = getByte(opcode, out cycles, out byteCount);
            Registers6502.PC += byteCount;
            setZN(Registers6502.A);
            return cycles;
        }

        private uint Sbc(byte opcode)
        {
            uint cycles;
            uint byteCount;
            byte ra = Registers6502.A;
            byte op = getByte(opcode, out cycles, out byteCount);
            Registers6502.A = (byte)(Registers6502.A - op);
            if (Registers6502.Carry)
                Registers6502.A--;
            Registers6502.PC += byteCount;
            setZN(Registers6502.A);
            setC(ra, op);
            return cycles;
        }

        private uint Lsr(byte opcode)
        {
            uint cycles;
            uint byteCount;
            byte op = getByte(opcode, out cycles, out byteCount);
            Registers6502.A = lsr(op);
            Registers6502.PC += byteCount;
            setZN(Registers6502.A);
            return cycles;
        }

        private uint Rol(byte opcode)
        {
            uint byteCount = 0;
            uint cycles = 2;
            byte result = getByte(opcode, out cycles, out byteCount);
            result = rol(result);
            putByte(opcode, out cycles, out byteCount, result);
            setZN(result);
            Registers6502.PC += byteCount;
            return cycles;
        }

        private uint Ror(byte opcode)
        {
            uint cycles;
            uint byteCount;
            byte op = getByte(opcode, out cycles, out byteCount);
            Registers6502.A = ror(op);
            Registers6502.PC += byteCount;
            setZN(Registers6502.A);
            return cycles;
        }

        private uint Bit(byte opcode)
        {
            uint cycles;
            uint byteCount;
            byte op = getByte(opcode, out cycles, out byteCount);
            setV(op);
            byte result = (byte)(Registers6502.A & op);
            setZN(result);
            Registers6502.PC += byteCount;
            return cycles;
        }

        private uint Php(byte opcode)
        {
            Push(Registers6502.GetStatusFlags(true, true));
            Registers6502.PC++;
            return 3;
        }

        private uint Clc(byte opcode)
        {
            Registers6502.Carry = false;
            Registers6502.PC++;
            return 2;
        }

        private uint Jsr(byte opcode)
        {
            ushort dest = (ushort)SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.TwoByte, true);
            Push((ushort)(Registers6502.PC + 3));
            Registers6502.PC = dest;
            return 4;
        }

        private byte Pop()
        {
            Registers6502.SP++;
            ushort addr = (ushort)(Registers6502.SP | 0x0100);
            return (byte)SystemMemory.GetMemory(addr, Processors.WordSize.OneByte, true);
        }

        private ushort PopW()
        {
            byte[] buff = new byte[2];
            buff[0] = Pop();
            buff[1] = Pop();
            return BitConverter.ToUInt16(buff, 0);
        }

        private void Push(byte data)
        {
            ushort addr = (ushort)(Registers6502.SP | 0x0100);
            SystemMemory.SetMemory(addr, Processors.WordSize.OneByte, data, true);
            Registers6502.SP--;
        }
        private void Push(ushort data)
        {
            byte b = (byte)(data >> 8 & 0xff);
            Push(b);
            b = (byte)(data & 0xff);
            Push(b);
        }

        private uint Plp(byte opcode)
        {
            Registers6502.SetStatusFlags(Pop());
            Registers6502.PC++;
            return 3;
        }

        private uint Sec(byte opcode)
        {
            Registers6502.Carry = true;
            Registers6502.PC++;
            return 2;
        }

        private uint Pha(byte opcode)
        {
            Push(Registers6502.A);
            Registers6502.PC++;
            return 3;
        }

        private uint Jmp(byte opcode)
        {
            uint cycles = 0;
            var addressMode = AddressingModes.addressModeFromOP(opcode);
            if (opcode == 0x6c)
                addressMode = AddressingModes.NONE;
            else if (opcode == 0x7c)
                addressMode = AddressingModes.ABSIX;

            var addr = (ushort)SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.TwoByte, true);

            if (addressMode == AddressingModes.ABSOL)
            {
                Registers6502.PC = addr;
                cycles = 3;
            }
            else if (addressMode == AddressingModes.NONE)
            {
                Registers6502.PC = (ushort)SystemMemory.GetMemory(addr, Processors.WordSize.TwoByte, true);
                cycles = 5;
            }
            else if (addressMode == AddressingModes.ABSIX)
            {
                ushort ind = (ushort)(addr + Registers6502.X);
                ushort dest = (ushort)SystemMemory.GetMemory(ind, Processors.WordSize.TwoByte, true);
                Registers6502.PC = dest;
                cycles = 5;
            }
            return cycles;
        }


        private uint Cli(byte opcode)
        {
            Registers6502.PC++;
            InteruptsDisabled = false;
            return 4;
        }
        private uint Rts(byte opcode)
        {
            Registers6502.PC = PopW();
            return 4;
        }

        private uint Pla(byte opcode)
        {
            Registers6502.A = Pop();
            Registers6502.PC++;
            setZN(Registers6502.A);
            return 4;
        }
        private uint Sei(byte opcode)
        {
            Registers6502.PC++;
            InteruptsDisabled = true;
            return 4;
        }

        private uint Dey(byte opcode)
        {
            Registers6502.PC++;
            Registers6502.Y--;
            setZN(Registers6502.Y);
            return 4;
        }
        private uint Txa(byte opcode)
        {
            Registers6502.PC++;
            Registers6502.A = Registers6502.X;
            setZN(Registers6502.A);
            return 3;
        }


        private uint Tya(byte opcode)
        {
            Registers6502.PC++;
            Registers6502.A = Registers6502.Y;
            setZN(Registers6502.A);
            return 4;
        }

        private uint Txs(byte opcode)
        {
            Registers6502.PC++;
            Registers6502.SP = Registers6502.X;
            return 2;
        }

        private uint Tay(byte opcode)
        {
            Registers6502.Y = Registers6502.A;
            setZN(Registers6502.Y);
            Registers6502.PC++;
            return 2;
        }
        private uint Tax(byte opcode)
        {
            Registers6502.X = Registers6502.A;
            setZN(Registers6502.X);
            Registers6502.PC++;
            return 2;
        }
        private uint Clv(byte opcode)
        {
            Registers6502.Overflow = false;
            Registers6502.PC++;
            return 4;
        }
        private uint Tsx(byte opcode)
        {
            Registers6502.X = Registers6502.SP;
            Registers6502.PC++;
            return 4;
        }

        private uint Iny(byte opcode)
        {
            Registers6502.Y++;
            Registers6502.PC++;
            setZN(Registers6502.Y);
            return 2;
        }
        private uint Dex(byte opcode)
        {
            Registers6502.X--;
            Registers6502.PC++;
            setZN(Registers6502.X);
            return 2;
        }

        private uint Cld(byte opcode)
        {
            Registers6502.Decimal = false;
            Registers6502.PC++;
            return 4;
        }

        private uint Inx(byte opcode)
        {
            Registers6502.X++;
            Registers6502.PC++;
            setZN(Registers6502.X);
            return 2;
        }

        private uint Nop(byte opcode)
        {
            Registers6502.PC++;
            return 4;
        }

        private uint Sed(byte opcode)
        {
            Registers6502.Decimal = true;
            Registers6502.PC++;
            return 4;
        }

        private ushort getAddress(byte opcode, out uint byteCount)
        {
            byteCount = 2;
            byte baseA = 0;
            ushort baseB = 0;
            var addressMode = AddressingModes.addressModeFromOP(opcode);
            switch (addressMode)
            {
                //(zero page, Y)
                case AddressingModes.ZEPIY:
                    baseA = (byte)SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.OneByte);
                    return (ushort)(baseA + Registers6502.Y);

                //(zero page, X)
                case AddressingModes.ZEPIX:
                    baseA = (byte)SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.OneByte);
                    return (ushort)(baseA + Registers6502.X);

                //zero page
                case AddressingModes.ZEROP:
                    return (ushort)(byte)SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.OneByte);

                //#immediate
                case AddressingModes.IMMED:
                    return (ushort)(Registers6502.PC + 1);

                //absolute
                case AddressingModes.ABSOL:
                    byteCount = 3;
                    return (ushort)SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.TwoByte);

                //(zero page), Y
                case AddressingModes.ININD:
                    baseA = (byte)SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.OneByte);
                    baseB = (ushort)SystemMemory.GetMemory(baseA, Processors.WordSize.TwoByte, true);
                    return (ushort)(baseB + Registers6502.Y);

                //zero page, X
                case AddressingModes.INDIN:
                    baseA = (byte)SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.OneByte);
                    baseB = (ushort)SystemMemory.GetMemory(baseA, Processors.WordSize.TwoByte, true);
                    return (ushort)(baseB + Registers6502.X);
                //absolute, Y
                case AddressingModes.ABSIY:
                    return (ushort)(SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.TwoByte) + Registers6502.Y);

                //absolute, X
                case AddressingModes.ABSIX:
                    return (ushort)(SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.TwoByte) + Registers6502.X);

                case AddressingModes.IMPLI:
                case AddressingModes.ACCUM:
                    return 0;

                default: throw new NotImplementedException();
            }

        }

        private byte getByte(byte opcode, out uint cycles, out uint byteCount)
        {
            cycles = 2;
            byteCount = 2;
            ushort addr = 0;
            byte baseA = 0;
            ushort baseB = 0;
            var addressMode = AddressingModes.addressModeFromOP(opcode);
            switch (addressMode)
            {
                //(zero page, X)
                case AddressingModes.INDIN:
                    cycles = 6;
                    baseA = (byte)SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.OneByte);
                    baseB = (ushort)SystemMemory.GetMemory(baseA, Processors.WordSize.TwoByte, true);
                    addr = (ushort)(baseB + Registers6502.X);
                    return (byte)SystemMemory.GetMemory(addr, Processors.WordSize.OneByte);

                //zero page
                case AddressingModes.ZEROP:
                    cycles = 3;
                    addr = (ushort)SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.OneByte);
                    return (byte)SystemMemory.GetMemory(addr, Processors.WordSize.OneByte);

                //#immediate
                case AddressingModes.IMMED:
                    addr = (ushort)(Registers6502.PC + 1);
                    return (byte)SystemMemory.GetMemory(addr, Processors.WordSize.OneByte);

                //absolute
                case AddressingModes.ABSOL:
                    byteCount = 3;
                    cycles = 4;
                    addr = (ushort)SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.TwoByte);
                    return (byte)SystemMemory.GetMemory(addr, Processors.WordSize.OneByte);

                //(zero page), Y
                case AddressingModes.ININD:
                    cycles = 5;
                    baseA = (byte)SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.OneByte);
                    baseB = (ushort)SystemMemory.GetMemory(baseA, Processors.WordSize.TwoByte, true);
                    addr = (ushort)(baseB + Registers6502.Y);
                    return (byte)SystemMemory.GetMemory(addr, Processors.WordSize.OneByte);

                //zero page, X
                case AddressingModes.ZEPIX:
                    cycles = 6;
                    baseA = (byte)SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.OneByte);
                    addr = (ushort)(baseA + Registers6502.X);
                    return (byte)SystemMemory.GetMemory(addr, Processors.WordSize.OneByte);

                //zero page, Y
                case AddressingModes.ZEPIY:
                    cycles = 6;
                    baseA = (byte)SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.OneByte);
                    addr = (ushort)(baseA + Registers6502.Y);
                    return (byte)SystemMemory.GetMemory(addr, Processors.WordSize.OneByte);

                //absolute, Y
                case AddressingModes.ABSIY:
                    byteCount = 3;
                    cycles = 4;
                    addr = (ushort)(SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.TwoByte) + Registers6502.Y);
                    return (byte)SystemMemory.GetMemory(addr, Processors.WordSize.OneByte);
                //absolute, X
                case AddressingModes.ABSIX:
                    byteCount = 3;
                    cycles = 4;
                    addr = (ushort)(SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.TwoByte) + Registers6502.X);
                    return (byte)SystemMemory.GetMemory(addr, Processors.WordSize.OneByte);

                case AddressingModes.IMPLI:
                case AddressingModes.ACCUM:
                    byteCount = 1;
                    return Registers6502.A;
                default:
                    throw new Exception();
            }//switch
        }//getByte

        private void putByte(byte opcode, out uint cycles, out uint byteCount, byte data)
        {
            cycles = 2;
            byteCount = 2;
            ushort addr = getAddress(opcode, out byteCount);
            var mode = AddressingModes.addressModeFromOP(opcode);
            switch (mode)
            {
                //(zero page, X)
                case AddressingModes.INDIN:
                    SystemMemory.SetMemory(addr, Processors.WordSize.OneByte, data);
                    break;

                //zero page
                case AddressingModes.ZEROP:
                    SystemMemory.SetMemory(addr, Processors.WordSize.OneByte, data);
                    break;

                //#immediate
                case AddressingModes.IMMED:
                    throw new NotImplementedException();

                //absolute
                case AddressingModes.ABSOL:
                    byteCount = 3;
                    cycles = 3;
                    SystemMemory.SetMemory(addr, Processors.WordSize.OneByte, data);
                    break;
                //(zero page), Y
                case AddressingModes.ININD:
                    byteCount = 2;
                    SystemMemory.SetMemory(addr, Processors.WordSize.OneByte, data);
                    break;

                //zero page, X
                case AddressingModes.ZEPIX:
                    byteCount = 2;
                    SystemMemory.SetMemory(addr, Processors.WordSize.OneByte, data);
                    break;
                //zero page, Y
                case AddressingModes.ZEPIY:
                    SystemMemory.SetMemory(addr, Processors.WordSize.OneByte, data);
                    break;
                //absolute, Y
                case AddressingModes.ABSIY:
                    byteCount = 3;
                    SystemMemory.SetMemory(addr, Processors.WordSize.OneByte, data);
                    break;
                //absolute, X
                case AddressingModes.ABSIX:
                    byteCount = 3;
                    SystemMemory.SetMemory(addr, Processors.WordSize.OneByte, data);
                    break;
                case AddressingModes.ACCUM:
                    byteCount = 1;
                    Registers6502.A = data;
                    break;
                default: throw new NotImplementedException();
            }//switch
        }//putByte

        private uint Rti(byte opcode)
        {
            throw new NotImplementedException();
        }

        private uint Invalid(byte opcode)
        {
            IsHalted = true;
            OnInvalidInstruction?.Invoke(opcode, Registers6502.PC);
            throw new InvalidInstructionException(Registers6502.PC, opcode);
//            return 1;
        }
        private uint Brk(byte opcode) { throw new NotImplementedException(); }

    }
}