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
    public class LdrMInstructionTest : RTestZ80_16
    {//ld r,(ixy+d)
        private ushort MAddr2;
        public LdrMInstructionTest()
            : base("LdrM", InstructionCategoryEnum.Z80, 0x46, nullFlags) { }
        public override BoardMessage formMessage(byte pReg, ushort pVal, byte sReg, ushort sVal, byte flags)
        {//ld r,(I?+d)
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_Z80;
            ret.PrebyteSize = (byte)BoardMessage.PrebyteSizeEnum.SOneB;
            Prebyte = (sReg == (byte)DoubleRegisterEnums.ix) ? (byte)0xdd : (byte)0xfd;
            ret.Prebyte1 = Prebyte;
            ret.Opcode = (byte)((pReg << 3) | Opcode);
            ret.useIMMData = (byte)BoardMessage.ImmDataSizeEnum.SByte;
            ret.ImmData = (byte)sVal;
            ret.setDblreg(sReg, MAddr);
            MAddr2 = (ushort)(MAddr + ((byte)sVal).SignExtend());
            ret.Addr = MAddr2;
            ret.Data = (byte)pVal;
            ret.flags = flags;
            return ret;
        }

        public override ushort[] usePrimaryVals() { return StartValues; }
        public override ushort[] useSecondaryVals() { return generateds(mRnd); }
        public override byte[] usePrimaryRegs() { return allRegs8; }
        public override byte[] useSecondaryRegs() { return new byte[] { (byte)DoubleRegisterEnums.ix, (byte)DoubleRegisterEnums.iy }; }

        public override void formSim(IProcessor processor, byte pReg, ushort pVal, byte sReg, ushort sVal, byte flags)
        {//ld r,(ixy+d)
            var sRegS = ((DoubleRegisterEnums)sReg).ToString();
            processor.Registers.SetDoubleRegister(sRegS, MAddr);
            processor.SystemMemory.SetMemory(MAddr2, WordSize.OneByte, pVal, false);
            processor.Registers.SetSingleRegister("Flags", flags);
            byte opcode = (byte)((pReg << 3) | Opcode);
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, Prebyte, false);
            processor.SystemMemory.SetMemory(0xa001, WordSize.OneByte, opcode, false);
            processor.SystemMemory.SetMemory(0xa002, WordSize.OneByte, sVal, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {//ld r,(ixy+d)
            byte simA = (byte)processor.Registers.GetSingleRegister(pReg);
            byte brdA = msg.getSinglereg(pReg);
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