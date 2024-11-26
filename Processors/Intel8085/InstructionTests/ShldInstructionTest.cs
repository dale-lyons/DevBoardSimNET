using Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8085.InstructionTests
{
    public class ShldInstructionTest : RTest8085_16
    {
        public ShldInstructionTest()
            : base("Shld", InstructionCategoryEnum.DataTransfer, 0x22) { }
        public override BoardMessage formMessage(byte pReg, ushort pVal, byte sReg, ushort sVal)
        {
            //Shld xxxx
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_8085;
            ret.Opcode = Opcode;

            //MAddr = generateMAddr();
            ret.useIMMData = (byte)BoardMessage.ImmDataSizeEnum.SWord;
            ret.ImmData = MAddr;
            ret.HL = sVal;
            ret.Addr = MAddr;
            return ret;
        }
        public override ushort[] usePrimaryVals() { return new ushort[] { 0 }; }
        public override ushort[] useSecondaryVals() { return DoubleInr2; }
        public override byte[] usePrimaryRegs() { return nullRegisters; }
        public override byte[] useSecondaryRegs() { return nullRegisters; }

        public override void formSim(IProcessor processor, byte pReg, ushort pVal, byte sReg, ushort sVal)
        {
            processor.Registers.PC = 0xa000;
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, Opcode, false);
            processor.SystemMemory.SetMemory(0xa001, WordSize.TwoByte, MAddr, false);
            processor.Registers.SetDoubleRegister((byte)DoubleRegisterEnums.h, sVal);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            CheckResultMem16(msg, processor, (ushort)MAddr, errors);
        }
    }
}