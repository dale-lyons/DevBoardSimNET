using Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8085.InstructionTests
{
    public class AdcInstructionTest : RTest8085_8
    {
        public AdcInstructionTest() : base("Adc", InstructionCategoryEnum.Arithmetic, 0x88, new byte[] { 0, 1 }) { }

        public override BoardMessage formMessage(byte pReg, byte pVal, byte sReg, byte sVal, byte flags)
        {
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_8085;
            ret.Opcode = (byte)(sReg | Opcode);
            setBrdRegister(ret, pReg, pVal);
            setBrdRegister(ret, sReg, sVal);
            ret.flags = flags;
            return ret;
        }

        public override byte[] usePrimaryVals() { return SingleInr1; }
        public override byte[] useSecondaryVals() { return SingleInr2; }
        public override byte[] usePrimaryRegs() { return SingleRegA; }
        public override byte[] useSecondaryRegs() { return allRegisters; }

        public override void formSim(IProcessor processor, byte pReg, byte pVal, byte sReg, byte sVal, byte flags)
        {
            setSimRegister(processor, pReg, pVal);
            setSimRegister(processor, sReg, sVal);

            //processor.Registers.SetSingleRegister(pReg, pVal);
            //if (sReg == (byte)SingleRegisterEnums.m)
            //{
            //    processor.SystemMemory.SetMemory(MAddr, WordSize.OneByte, sVal, false);
            //    processor.Registers.SetDoubleRegister((byte)DoubleRegisterEnums.h, MAddr);
            //}
            //else
            //    processor.Registers.SetSingleRegister(sReg, sVal);

            processor.Registers.SetSingleRegister("Flags", flags);
            byte opcode = (byte)(sReg | Opcode);
            processor.Registers.PC = 0xa000;
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, opcode, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            CheckResultAFlags(msg, processor, errors);
        }
    }
}