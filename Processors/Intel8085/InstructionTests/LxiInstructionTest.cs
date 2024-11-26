using Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8085.InstructionTests
{
    public class LxiInstructionTest : RTest8085_16
    {
        public LxiInstructionTest()
        : base("Lxi", InstructionCategoryEnum.DataTransfer, 0x01) { }
        public override BoardMessage formMessage(byte pReg, ushort pVal, byte sReg, ushort sVal)
        {
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_8085;
            ret.Opcode = (byte)((sReg << 4) | Opcode);
            ret.setDblreg(pReg, 0);
            ret.useIMMData = (byte)BoardMessage.ImmDataSizeEnum.SWord;
            ret.ImmData = pVal;
            return ret;
        }
        public override ushort[] usePrimaryVals() { return DoubleInr1; }
        public override ushort[] useSecondaryVals() { return new ushort[] { 0 }; }
        public override byte[] usePrimaryRegs() { return allRegisters; }
        public override byte[] useSecondaryRegs() { return nullRegisters; }

        public override void formSim(IProcessor processor, byte pReg, ushort pVal, byte sReg, ushort sVal)
        {
            processor.Registers.SetDoubleRegister(sReg, 0);
            byte opcode = (byte)((sReg << 4) | Opcode);
            processor.Registers.PC = 0xa000;
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, opcode, false);
            processor.SystemMemory.SetMemory(0xa001, WordSize.TwoByte, pVal, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            CheckResultRegFlags(msg, processor, sReg, errors);
        }
    }
}