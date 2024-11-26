using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Processors;

namespace BenEater
{
    public partial class BenEater
    {
        private delegate uint ExecuteInstruction(byte opcode);

        //private byte[] mBuff2 = new byte[2];

        public enum CPUFlags : byte
        {
            Zero = 0x0c,
            Carry = 0x0d,
            Parity = 0x0e,
            Sign = 0x0f
        }

        private ExecuteInstruction[] mOpcodeFunctions = new ExecuteInstruction[256];
        //        private Tuple<pluginPortAccessReadEventHandler, pluginPortAccessWriteEventHandler>[] mPortOverrides;

        private uint Nop(byte opcode) { return 4; }
        private uint Lda(byte opcode)
        {
            byte val = (byte)(opcode & 0x0f);
            RegistersBenEater.A = (byte)SystemMemory.GetMemory(val, WordSize.OneByte);
            return 4;
        }
        private uint Add(byte opcode)
        {
            byte addr = (byte)(opcode & 0x0f);
            byte val = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            int result = RegistersBenEater.A + val;
            RegistersBenEater.Carry = (result > byte.MaxValue);
            RegistersBenEater.A = (byte)result;
            return 4;
        }
        private uint Sub(byte opcode)
        {
            byte addr = (byte)(opcode & 0x0f);
            byte val = (byte)SystemMemory.GetMemory(addr, WordSize.OneByte, true);
            RegistersBenEater.A -= val;
            return 4;
        }

        private uint Sta(byte opcode)
        {
            byte addr = (byte)(opcode & 0x0f);
            SystemMemory.SetMemory(addr, WordSize.OneByte, RegistersBenEater.A);
            return 4;
        }
        private uint Ldi(byte opcode)
        {
            byte val = (byte)(opcode & 0x0f);
            RegistersBenEater.A = val;
            return 4;
        }
        private uint Jmp(byte opcode)
        {
            byte addr = (byte)(opcode & 0x0f);
            RegistersBenEater.PC = addr;
            return 4;
        }
        private uint Jc(byte opcode)
        {
            if (!RegistersBenEater.Carry)
                return 4;

            return Jmp(opcode);
        }
        private uint Out(byte opcode)
        {
            RegistersBenEater.OUT = RegistersBenEater.A;
            return 4;
        }
        private uint Hlt(byte opcode) { IsHalted = true; return 4; }
    }
}