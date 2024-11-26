using Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intel8085.InstructionTests;

namespace ZilogZ80.InstructionTests
{
    public class RotateInstructionTest : RTestZ80_8
    {
        public RotateInstructionTest(string name, byte prebyte, byte opcode)
            : base(name, InstructionCategoryEnum.Z80, prebyte, opcode, new byte[] { 0x00, 0x01, 0x10, 0x11 }) { }
        public override BoardMessage formMessage(byte pReg, byte pVal, byte sReg, byte sVal, byte flags)
        {
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_Z80;
            ret.PrebyteSize = (byte)BoardMessage.PrebyteSizeEnum.SOneB;
            ret.Prebyte1 = Prebyte;
            ret.Opcode = (byte)(sReg | Opcode);
            setBrdRegister(ret, sReg, sVal, MAddr);
            ret.flags = flags;
            return ret;
        }

        public override ushort[] usePrimaryVals() { return PrimaryValues; }
        public override ushort[] useSecondaryVals() { return SecondaryValues; }
        public override byte[] usePrimaryRegs() { return SingleRegA; }
        public override byte[] useSecondaryRegs() { return allRegs8; }

        public override void formSim(IProcessor processor, byte pReg, byte pVal, byte sReg, byte sVal, byte flags)
        {
            processor.Registers.SetSingleRegister("Flags", flags);
            setSimRegister(processor, sReg, sVal, MAddr);
            byte opcode = (byte)(sReg | Opcode);
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, Prebyte, false);
            processor.SystemMemory.SetMemory(0xa001, WordSize.OneByte, opcode, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            byte simA = getSimRegister(processor, sReg, MAddr);
            byte brdA = getBrdRegister(msg, sReg, MAddr);
            byte simF = (byte)((byte)processor.Registers.GetSingleRegister("Flags") & FlagMask);
            byte brdF = (byte)(msg.flags & FlagMask);
            if ((brdA != simA) || (brdF != simF))
            {
                errors.Add(new InstructionTestError { pReg = pReg, sReg = sReg, pVal = 0, Flags = 0, Opcode = Opcode, TestName = Name });
                return;
            }
        }
    }
}