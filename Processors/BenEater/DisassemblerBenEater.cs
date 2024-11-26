using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Processors;

namespace BenEater
{
    public class DisassemblerBenEater : IDisassembler
    {
        private delegate string ExecuteInstruction(ushort pc, ISystemMemory code, byte opcode, out int count);

        private IProcessor mProcesor;
        public DisassemblerBenEater(IProcessor procesor)
        {
            mProcesor = procesor;
        }

        private static ExecuteInstruction[] mOpcodeFunctions = new ExecuteInstruction[16]
            {
                Nop,                       //nop            0x00
                Lda,                       //lxi b,data
                Add,                    //stax b
                Sub,            //inx b
                Sta,                 //inr b
                Ldi,                 //dcr b
                Jmp,                       //mvi b,data
                Jc,                  //rlc
                Nop,
                Nop,            //dad b
                Nop,                      //ldax b
                Nop,            //dcx b
                Nop,                 //inr c
                Nop,                 //dcr c
                Out,                       //mvi c,data
                Hlt,                  //rrc
            };

        private static string Nop(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 1;
            return "nop";
        }
        private static string Lda(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 1;
            return "lda";
        }
        private static string Add(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 1;
            return "add";
        }
        private static string Sub(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 1;
            return "sub";
        }
        private static string Sta(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 1;
            return "sta";
        }
        private static string Ldi(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 1;
            return "ldi";
        }
        private static string Jmp(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 1;
            return "jmp";
        }
        private static string Jc(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 1;
            return "jc";
        }
        private static string Out(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 1;
            return "out";
        }
        private static string Hlt(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 1;
            return "hlt";
        }

        public int Align(uint pos, ISystemMemory code)
        {
            return 0;
        }

        public CodeLine ProcessInstruction(uint startAddr, ISystemMemory code, out uint newAddr)
        {
            byte opcode = code[(ushort)startAddr];
            byte inst = (byte)(((int)opcode & (int)0xf0) >> 4);

            int count = 0;
            string opcodeStr = mOpcodeFunctions[inst]((ushort)startAddr, code, opcode, out count);
            newAddr = (uint)(startAddr + count);
            var cl = new CodeLine(opcodeStr, (ushort)startAddr, count);
            cl.CodeLineType = CodeLine.CodeLineTypes.Code;
            return cl;
        }
    }
}