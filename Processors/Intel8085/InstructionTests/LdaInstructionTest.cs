using Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8085.InstructionTests
{
    public class LdaInstructionTest : RTest8085_8
    {
        public LdaInstructionTest() : base("Lda", InstructionCategoryEnum.DataTransfer, 0x3a, nullFlags) { }
        public override BoardMessage formMessage(byte pReg, byte pVal, byte sReg, byte sVal, byte flags)
        {//lda 0x1234
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_8085;
            ret.Opcode = Opcode;

            //lda 0x1234
            ret.useIMMData = (byte)BoardMessage.ImmDataSizeEnum.SWord;
            ret.ImmData = MAddr;
            ret.Addr = MAddr;
            ret.Data = sVal;
            return ret;
        }

        public override byte[] usePrimaryVals() { return nullSingle; }
        public override byte[] useSecondaryVals() { return FullSingle; }
        public override byte[] usePrimaryRegs() { return nullRegisters; }
        public override byte[] useSecondaryRegs() { return SingleRegA; }

        public override void formSim(IProcessor processor, byte pReg, byte pVal, byte sReg, byte sVal, byte flags)
        {
            //lda 0x1234
            processor.Registers.SetSingleRegister("Flags", flags);
            processor.Registers.PC = 0xa000;
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, Opcode, false);
            processor.SystemMemory.SetMemory(0xa001, WordSize.TwoByte, MAddr, false);
            processor.SystemMemory.SetMemory(MAddr, WordSize.OneByte, sVal, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            CheckResultAFlags(msg, processor, errors);
        }
    }
}