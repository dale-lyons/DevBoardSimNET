using Intel8085.InstructionTests;
using Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8085.InstructionTests
{
    public class InxInstructionTest : RTest8085_16
    {
        public InxInstructionTest()
                    : base("Inx", InstructionCategoryEnum.Arithmetic, 0x03) { }

        public override BoardMessage formMessage(byte pReg, ushort pVal, byte sReg, ushort sVal)
        {
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_8085;
            ret.flags = 0;
            ret.Opcode = (byte)((sReg << 4) | Opcode);
            ret.setDblreg(pReg, pVal);
            ret.setDblreg(sReg, sVal);
            return ret;
        }
        public override ushort[] usePrimaryVals() { return DoubleInr1; }
        public override ushort[] useSecondaryVals() { return new ushort[] { 0 }; }
        public override byte[] usePrimaryRegs() { return allRegisters; }
        public override byte[] useSecondaryRegs() { return nullRegisters; }

        public override void formSim(IProcessor processor, byte pReg, ushort pVal, byte sReg, ushort sVal)
        {
            processor.Registers.PC = 0xa000;
            processor.Registers.SetDoubleRegister(sReg, sVal);
            processor.Registers.SetSingleRegister("Flags", 0);
            byte opcode = (byte)((sReg << 4) | Opcode);
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, opcode, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            CheckResultRegFlags(msg, processor, sReg, errors);
        }
    }
}