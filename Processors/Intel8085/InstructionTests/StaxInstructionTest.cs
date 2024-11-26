using Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8085.InstructionTests
{
    public class StaxInstructionTest : RTest8085_16
    {
        public StaxInstructionTest()
                : base("Stax", InstructionCategoryEnum.DataTransfer, 0x02) { }
        public override BoardMessage formMessage(byte pReg, ushort pVal, byte sReg, ushort sVal)
        {
            //Stax d
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_8085;
            ret.Opcode = (byte)(((sReg & 0x01) << 4) | Opcode);
            //MAddr = generateMAddr();

            if (sReg == (byte)DoubleRegisterEnums.d)
                ret.DE = MAddr;
            else
                ret.BC = MAddr;

            ret.a = (byte)sVal;
            ret.Addr = MAddr;
            ret.Data = sVal;
            return ret;
        }
        public override ushort[] usePrimaryVals() { return new ushort[] { 0 }; }
        public override ushort[] useSecondaryVals() { return FullVal8; }
        public override byte[] usePrimaryRegs() { return nullRegisters; }
        public override byte[] useSecondaryRegs()
        {
            return new byte[] { (byte)DoubleRegisterEnums.b, (byte)DoubleRegisterEnums.d };
        }

        public override void formSim(IProcessor processor, byte pReg, ushort pVal, byte sReg, ushort sVal)
        {
            if (sReg == (byte)DoubleRegisterEnums.d)
                processor.Registers.SetDoubleRegister((byte)DoubleRegisterEnums.d, MAddr);
            else
                processor.Registers.SetDoubleRegister((byte)DoubleRegisterEnums.b, MAddr);

            processor.Registers.SetSingleRegister((byte)SingleRegisterEnums.a, (byte)sVal);
            byte opcode = (byte)(((sReg & 0x01) << 4) | Opcode);
            processor.Registers.PC = 0xa000;
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, opcode, false);
            processor.SystemMemory.SetMemory(MAddr, WordSize.TwoByte, sVal, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            CheckResultMem16(msg, processor, (ushort)MAddr, errors);
        }
    }
}