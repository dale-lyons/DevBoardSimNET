using Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8085.InstructionTests
{
    public class DcrInstructionTest : RTest8085_8
    {
        public DcrInstructionTest() : base("Dcr", InstructionCategoryEnum.Arithmetic, 0x05, nullFlags) { }
        public override BoardMessage formMessage(byte pReg, byte pVal, byte sReg, byte sVal, byte flags)
        {
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_8085;
            ret.Opcode = (byte)(sReg << 3 | Opcode);
            setBrdRegister(ret, sReg, sVal);
            return ret;
        }

        public override byte[] usePrimaryVals() { return nullSingle; }
        public override byte[] useSecondaryVals() { return FullSingle; }
        public override byte[] usePrimaryRegs() { return nullRegisters; }
        public override byte[] useSecondaryRegs() { return allRegisters; }

        public override void formSim(IProcessor processor, byte pReg, byte pVal, byte sReg, byte sVal, byte flags)
        {
            setSimRegister(processor, pReg, pVal);
            setSimRegister(processor, sReg, sVal);
            processor.Registers.SetSingleRegister("Flags", flags);
            byte opcode = (byte)(sReg << 3 | Opcode);
            processor.Registers.PC = 0xa000;
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, opcode, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            CheckResultRegFlags(msg, processor, sReg, errors);
        }
    }
}