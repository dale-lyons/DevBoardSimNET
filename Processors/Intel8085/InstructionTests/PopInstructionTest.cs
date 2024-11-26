using Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8085.InstructionTests
{
    public class PopInstructionTest : RTest8085_16
    {
        public PopInstructionTest()
            : base("Pop", InstructionCategoryEnum.DataTransfer, 0xc1) { }

        public override BoardMessage formMessage(byte pReg, ushort pVal, byte sReg, ushort sVal)
        {
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_8085;
            byte reg = sReg;
            if (sReg == (byte)DoubleRegisterEnums.psw)
                reg = (byte)DoubleRegisterEnums.sp;
            ret.Opcode = (byte)((reg << 4) | Opcode);
            ret.flags = 0;
            ret.setDblreg((byte)DoubleRegisterEnums.sp, MAddr);
            ret.Addr = MAddr;
            ret.Data = sVal;
            return ret;
        }

        public override ushort[] usePrimaryVals() { return new ushort[] { 0 }; }
        public override ushort[] useSecondaryVals() { return DoubleInr1; }
        public override byte[] usePrimaryRegs() { return nullRegisters; }

        public override byte[] useSecondaryRegs()
        {
            return new byte[] { (byte)DoubleRegisterEnums.b, (byte)DoubleRegisterEnums.d, (byte)DoubleRegisterEnums.h, (byte)DoubleRegisterEnums.psw };
        }

        public override void formSim(IProcessor processor, byte pReg, ushort pVal, byte sReg, ushort sVal)
        {
            processor.Registers.SetSingleRegister("Flags", 0);
            processor.Registers.SetDoubleRegister((byte)DoubleRegisterEnums.sp, MAddr);
            byte reg = sReg;
            if (sReg == (byte)DoubleRegisterEnums.psw)
                reg = (byte)DoubleRegisterEnums.sp;
            byte opcode = (byte)((reg << 4) | Opcode);
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, opcode, false);
            processor.SystemMemory.SetMemory(MAddr, WordSize.TwoByte, sVal, false);
            processor.Registers.PC = 0xa000;
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            CheckResultMem16(msg, processor, (ushort)MAddr, errors);
        }
    }
}