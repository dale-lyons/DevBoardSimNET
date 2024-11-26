using Processors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Intel8085.InstructionTests
{
    public class DadInstructionTest : RTest8085_16
    {
        public DadInstructionTest() 
            : base("Dad", InstructionCategoryEnum.Arithmetic, 0x09) { }

        public override BoardMessage formMessage(byte pReg, ushort pVal, byte sReg, ushort sVal)
        {
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_8085;
            ret.Opcode = (byte)((sReg << 4) | Opcode);
            ret.setDblreg(pReg, pVal);
            ret.setDblreg(sReg, sVal);
            return ret;
        }

        //public override ushort[] usePrimaryVals() { return DoubleInr1; }
        //public override ushort[] useSecondaryVals() { return DoubleInr2; }

        public override ushort[] usePrimaryVals() { return new ushort[] { 0x1122 }; }
        public override ushort[] useSecondaryVals() { return new ushort[] { 0x3344 }; }
        public override byte[] usePrimaryRegs() { return SingleH; }
        public override byte[] useSecondaryRegs()
        {
            return new byte[] { (byte)DoubleRegisterEnums.b, (byte)DoubleRegisterEnums.d, (byte)DoubleRegisterEnums.h, (byte)DoubleRegisterEnums.sp };
        }

        public override void formSim(IProcessor processor, byte pReg, ushort pVal, byte sReg, ushort sVal)
        {
            processor.Registers.PC = 0xa000;
            processor.Registers.SetDoubleRegister(pReg, pVal);
            processor.Registers.SetDoubleRegister(sReg, sVal);
            byte opcode = (byte)((sReg << 4) | Opcode);
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, opcode, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            CheckResultHLFlags(msg, processor, errors);
        }
    }
}