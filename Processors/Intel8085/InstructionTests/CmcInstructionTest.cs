﻿using Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intel8085.InstructionTests
{
    public class CmcInstructionTest : RTest8085_8
    {
        public CmcInstructionTest() : base("Cmc", InstructionCategoryEnum.Logical, 0x3f, new byte[] { 0x00, 0x01 }) { }
        public override BoardMessage formMessage(byte pReg, byte pVal, byte sReg, byte sVal, byte flags)
        {
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_8085;
            ret.Opcode = Opcode;
            ret.setSinglereg(pReg, pVal);
            ret.flags = flags;
            return ret;
        }

        public override byte[] usePrimaryVals() { return FullSingle; }
        public override byte[] useSecondaryVals() { return new byte[] { 0 }; }
        public override byte[] usePrimaryRegs() { return nullRegisters; }
        public override byte[] useSecondaryRegs() { return SingleRegA; }

        public override void formSim(IProcessor processor, byte pReg, byte pVal, byte sReg, byte sVal, byte flags)
        {
            processor.Registers.SetSingleRegister(pReg, pVal);
            processor.Registers.SetSingleRegister("Flags", flags);
            processor.Registers.PC = 0xa000;
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, Opcode, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            CheckResultAFlags(msg, processor, errors);
        }
    }
}