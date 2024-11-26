using Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8085.InstructionTests
{
    public class xchgInstructionTest : RTest8085_16
    {
        public xchgInstructionTest()
                : base("Xchg", InstructionCategoryEnum.DataTransfer, 0xeb) { }

        public override BoardMessage formMessage(byte pReg, ushort pVal, byte sReg, ushort sVal)
        {//Xchg
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_8085;
            ret.Opcode = Opcode;
            ret.setDblreg(pReg, pVal);
            ret.setDblreg(sReg, sVal);
            return ret;
        }

        public override ushort[] usePrimaryVals() { return DoubleInr1; }
        public override ushort[] useSecondaryVals() { return DoubleInr2; }
        public override byte[] usePrimaryRegs() { return new byte[] { (byte)DoubleRegisterEnums.d }; }
        public override byte[] useSecondaryRegs() { return new byte[] { (byte)DoubleRegisterEnums.h }; }

        public override void formSim(IProcessor processor, byte pReg, ushort pVal, byte sReg, ushort sVal)
        {
            processor.Registers.PC = 0xa000;
            processor.Registers.SetDoubleRegister(pReg, pVal);
            processor.Registers.SetDoubleRegister(sReg, sVal);
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, Opcode, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            CheckResultRegRegFlags(msg, processor, pReg, sReg, errors);
        }
    }
}