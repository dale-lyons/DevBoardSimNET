using Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8085.InstructionTests
{
    public class xthlInstructionTest : RTest8085_16
    {
        public xthlInstructionTest()
            : base("Xthl", InstructionCategoryEnum.DataTransfer, 0xe3) { }

        public override BoardMessage formMessage(byte pReg, ushort pVal, byte sReg, ushort sVal)
        {
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_8085;
            ret.Opcode = Opcode;
            ret.setDblreg((byte)DoubleRegisterEnums.h, pVal);
            ret.setDblreg((byte)DoubleRegisterEnums.sp, MAddr);
            ret.Addr = MAddr;
            ret.Data = sVal;
            return ret;
        }

        public override ushort[] usePrimaryVals() { return new ushort[] { 0 }; }
        public override ushort[] useSecondaryVals() { return DoubleInr2; }
        public override byte[] usePrimaryRegs() { return SingleH; }
        public override byte[] useSecondaryRegs() { return nullRegisters; }

        public override void formSim(IProcessor processor, byte pReg, ushort pVal, byte sReg, ushort sVal)
        {//Xthl
            processor.Registers.SetDoubleRegister((byte)DoubleRegisterEnums.h, pVal);
            processor.Registers.SetDoubleRegister((byte)DoubleRegisterEnums.sp, MAddr);
            processor.Registers.PC = 0xa000;
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, Opcode, false);
            processor.SystemMemory.SetMemory(MAddr, WordSize.TwoByte, sVal, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            CheckResultRegMemFlags(msg, processor, pReg, MAddr, errors);
        }
    }
}