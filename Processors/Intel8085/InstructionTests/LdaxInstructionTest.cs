using Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Intel8085.InstructionTests
{
    public class LdaxInstructionTest : RTest8085_16
    {
        public LdaxInstructionTest()
                : base("Ldax", InstructionCategoryEnum.DataTransfer, 0x0a) { }
        public override BoardMessage formMessage(byte pReg, ushort pVal, byte sReg, ushort sVal)
        {
            //ldax d
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_8085;
            ret.Opcode = (byte)(((sReg & 0x01) << 4) | Opcode);
            //MAddr = generateMAddr();

            ret.setDblreg(sReg, MAddr);
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
            processor.Registers.SetDoubleRegister((byte)sReg, MAddr);
            byte opcode = (byte)(((sReg & 0x01) << 4) | Opcode);
            processor.Registers.PC = 0xa000;
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, opcode, false);
            processor.SystemMemory.SetMemory(MAddr, WordSize.OneByte, sVal, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            CheckResultAFlags(msg, processor, errors);
        }
    }
}