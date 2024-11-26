using Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8085.InstructionTests
{
    public class MviInstructionTest : RTest8085_8
    {
        public MviInstructionTest() : base("Mvi", InstructionCategoryEnum.DataTransfer, 0x06, nullFlags) { }
        public override BoardMessage formMessage(byte pReg, byte pVal, byte sReg, byte sVal, byte flags)
        {
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_8085;
            ret.Opcode = formOpcode(pReg, Opcode);
            setBrdRegister(ret, pReg, pVal);
            ret.useIMMData = (byte)BoardMessage.ImmDataSizeEnum.SByte;
            ret.ImmData = sVal;
            return ret;
        }
        private byte formOpcode(byte reg, byte opcode) { return (byte)((reg << 3) | Opcode); }

        public override byte[] usePrimaryVals() { return nullSingle; }
        public override byte[] useSecondaryVals() { return SingleInr2; }
        public override byte[] usePrimaryRegs() { return allRegisters; }
        public override byte[] useSecondaryRegs() { return nullRegisters; }

        public override void formSim(IProcessor processor, byte pReg, byte pVal, byte sReg, byte sVal, byte flags)
        {
            setSimRegister(processor, pReg, pVal);
            processor.Registers.PC = 0xa000;
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, formOpcode(pReg, Opcode), false);
            processor.SystemMemory.SetMemory(0xa001, WordSize.OneByte, sVal, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            CheckResultRegFlags(msg, processor, pReg, errors);
        }
    }
}