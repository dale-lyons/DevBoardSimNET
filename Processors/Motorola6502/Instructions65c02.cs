using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motorola6502
{
    public partial class Motorola6502
    {
        /// <summary>
        /// sta ($xx)
        /// </summary>
        /// <param name="opcode"></param>
        /// <returns></returns>
        private uint StaC(byte opcode)
        {
            byte baseA = (byte)SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.OneByte);
            ushort addr = (ushort)SystemMemory.GetMemory(baseA, Processors.WordSize.TwoByte, true);
            SystemMemory.SetMemory(addr, Processors.WordSize.OneByte, Registers6502.A, true);
            Registers6502.PC += 2;
            return 4;
        }

        /// <summary>
        /// bra r
        /// </summary>
        /// <param name="opcode"></param>
        /// <returns></returns>
        private uint BraC(byte opcode)
        {
            byte off = (byte)SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.OneByte, true);
            bool negative = ((off & 0x80) != 0);
            ushort offset = off;
            if (negative)
                offset |= 0xff00;

            ushort dest = (ushort)(Registers6502.PC + offset + 2);
            Registers6502.PC = dest;
            return 6;
        }

        /// <summary>
        /// stz r
        /// </summary>
        /// <param name="opcode"></param>
        /// <returns></returns>
        private uint StzC(byte opcode)
        {
            byte baseA = (byte)SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.OneByte);
            ushort addr = (ushort)SystemMemory.GetMemory(baseA, Processors.WordSize.TwoByte, true);
            SystemMemory.SetMemory(addr, Processors.WordSize.OneByte, 0, true);
            Registers6502.PC += 2;
            return 6;
        }

        /// <summary>
        /// phx
        /// </summary>
        /// <param name="opcode"></param>
        /// <returns></returns>
        private uint PhxC(byte opcode)
        {
            Push(Registers6502.X);
            Registers6502.PC++;
            return 3;
        }

        /// <summary>
        /// phx
        /// </summary>
        /// <param name="opcode"></param>
        /// <returns></returns>
        private uint BitC(byte opcode)
        {
            byte op = (byte)SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.OneByte);
            setNV(op);
            byte result = (byte)(Registers6502.A & op);
            Registers6502.Negative = (result == 0);
            Registers6502.PC += 2;
            return 2;
        }

        /// <summary>
        /// phx
        /// </summary>
        /// <param name="opcode"></param>
        /// <returns></returns>
        private uint LdaC(byte opcode)
        {
            ushort addr = (byte)SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.OneByte);
            ushort src = (ushort)SystemMemory.GetMemory(addr, Processors.WordSize.TwoByte);
            Registers6502.A = (byte)SystemMemory.GetMemory(src, Processors.WordSize.OneByte);
            setZNV(Registers6502.A);
            Registers6502.PC += 2;
            return 4;
        }

        /// <summary>
        /// phx
        /// </summary>
        /// <param name="opcode"></param>
        /// <returns></returns>
        private uint PlxC(byte opcode)
        {
            Registers6502.X = Pop();
            Registers6502.PC++;
            setZN(Registers6502.X);
            return 3;
        }
        private uint PlyC(byte opcode)
        {
            Registers6502.Y = Pop();
            Registers6502.PC++;
            setZN(Registers6502.Y);
            return 3;
        }

        /// <summary>
        /// phx
        /// </summary>
        /// <param name="opcode"></param>
        /// <returns></returns>
        private uint TsbC(byte opcode)
        {
            byte op = (byte)SystemMemory.GetMemory(Registers6502.PC + 1, Processors.WordSize.OneByte);
            byte result = (byte)(Registers6502.A & op);
            setZ(result);
            Registers6502.PC += 2;
            return 3;
        }

        private uint PhyC(byte opcode)
        {
            Push(Registers6502.Y);
            Registers6502.PC++;
            return 3;
        }

        private uint DecaC(byte opcode)
        {
            Registers6502.A--;
            Registers6502.PC++;
            setZN(Registers6502.A);
            return 3;
        }
    }
}