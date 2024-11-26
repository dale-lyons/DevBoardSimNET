using Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8085.InstructionTests
{
    public class sphlInstructionTest : RTest8085_16
    {
        public sphlInstructionTest()
                    : base("Sphl", InstructionCategoryEnum.DataTransfer, 0xf9) { }

        public override BoardMessage formMessage(byte pReg, ushort pVal, byte sReg, ushort sVal)
        {
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_8085;
            ret.Opcode = Opcode;
            ret.setDblreg(pReg, pVal);
            ret.setDblreg(sReg, sVal);
            return ret;
        }

        public override ushort[] usePrimaryVals() { return DoubleInr1; }
        public override ushort[] useSecondaryVals() { return DoubleInr2; }
        public override byte[] usePrimaryRegs() { return nullRegisters; }
        public override byte[] useSecondaryRegs() { return nullRegisters; }

        public override void formSim(IProcessor processor, byte pReg, ushort pVal, byte sReg, ushort sVal)
        {
            processor.Registers.PC = 0xa000;
            processor.Registers.SetDoubleRegister(pReg, pVal);
            processor.Registers.SetDoubleRegister(sReg, sVal);
            byte opcode = Opcode;
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, opcode, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            CheckResultRegRegFlags(msg, processor, pReg, sReg, errors);
        }
    }
}