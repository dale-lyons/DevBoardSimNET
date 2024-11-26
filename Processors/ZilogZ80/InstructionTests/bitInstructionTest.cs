using Intel8085;
using Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intel8085.InstructionTests;

namespace ZilogZ80.InstructionTests
{
    public class bitInstructionTest : RTestZ80_8
    {
        public bitInstructionTest() : base("bit", InstructionCategoryEnum.Z80, 0xcb, 0x40, nullFlags) { }

        //ignore the P/V flag and S flag, undefined for bit instruction
        private byte FlagMaskOverride = 0x84;

        public override BoardMessage formMessage(byte pReg, byte pVal, byte sReg, byte sVal, byte flags)
        {
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_Z80;
            ret.PrebyteSize = (byte)BoardMessage.PrebyteSizeEnum.SOneB;
            ret.Prebyte1 = Prebyte;
            ret.Opcode = (byte)((pReg << 3) | sReg | Opcode);
            setBrdRegister(ret, sReg, sVal, MAddr);
            ret.flags = flags;
            return ret;
        }

        public override ushort[] usePrimaryVals() { return new ushort[] { 0, 1, 2, 3, 4, 5, 6, 7 }; }
        public override ushort[] useSecondaryVals() { return SecondaryValues; }

        public override byte[] usePrimaryRegs() { return allRegs8; }
        public override byte[] useSecondaryRegs() { return allRegs8; }
        public override void formSim(IProcessor processor, byte pReg, byte pVal, byte sReg, byte sVal, byte flags)
        {
            processor.Registers.SetSingleRegister("Flags", flags);
            setSimRegister(processor, sReg, sVal, MAddr);
            byte opcode = (byte)((pReg << 3) | sReg | Opcode);
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, Prebyte, false);
            processor.SystemMemory.SetMemory(0xa001, WordSize.OneByte, opcode, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            byte simF = (byte)((byte)processor.Registers.GetSingleRegister("Flags") & FlagMask);
            byte brdF = (byte)(msg.flags & FlagMask);

            brdF &= (byte)~FlagMaskOverride;
            if (brdF != simF)
            {
                errors.Add(new InstructionTestError { pReg = pReg, sReg = sReg, pVal = 0, Flags = 0, Opcode = Opcode, TestName = Name });
                return;
            }
        }
    }
}