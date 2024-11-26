using Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8085.InstructionTests
{
    public class AciInstructionTest : RTest8085_8
    {
        public AciInstructionTest() : base("Aci", InstructionCategoryEnum.Arithmetic, 0xce, new byte[] { 0, 1 }) { }
        public override BoardMessage formMessage(byte pReg, byte pVal, byte sReg, byte sVal, byte flags)
        {
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_8085;
            ret.Opcode = Opcode;
            ret.setSinglereg(pReg, pVal);
            ret.useIMMData = (byte)BoardMessage.ImmDataSizeEnum.SByte;
            ret.ImmData = (ushort)sVal;
            ret.flags = flags;
            return ret;
        }

        public override byte[] usePrimaryVals() { return SingleInr1; }
        public override byte[] useSecondaryVals() { return SingleInr2; }
        public override byte[] usePrimaryRegs() { return SingleRegA; }
        public override byte[] useSecondaryRegs() { return SingleRegA; }

        public override void formSim(IProcessor processor, byte pReg, byte pVal, byte sReg, byte sVal, byte flags)
        {
            processor.Registers.SetSingleRegister(pReg, pVal);
            processor.Registers.SetSingleRegister("Flags", flags);
            processor.Registers.PC = 0xa000;
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, Opcode, false);
            processor.SystemMemory.SetMemory(0xa001, WordSize.OneByte, sVal, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            CheckResultAFlags(msg, processor, errors);
        }
    }
}