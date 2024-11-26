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
    public class LdMrInstructionTest : RTestZ80_16
    {//ld (ixy+d),r
        private ushort MAddr2;
        public LdMrInstructionTest()
            : base("LdMr", InstructionCategoryEnum.Z80, 0x70, nullFlags) { }
        public override BoardMessage formMessage(byte pReg, ushort pVal, byte sReg, ushort sVal, byte flags)
        {//ld (ixy+d),r
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_Z80;
            ret.PrebyteSize = (byte)BoardMessage.PrebyteSizeEnum.SOneB;
            Prebyte = (sReg == (byte)DoubleRegisterEnums.ix) ? (byte)0xdd : (byte)0xfd;
            ret.Prebyte1 = Prebyte;
            ret.Opcode = (byte)(pReg | Opcode);
            ret.useIMMData = (byte)BoardMessage.ImmDataSizeEnum.SByte;
            ret.ImmData = (byte)sVal;
            ret.setDblreg(sReg, MAddr);
            ret.setSinglereg(pReg, (byte)pVal);
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
        {//ld (ixy+d),r
            var sRegS = ((DoubleRegisterEnums)sReg).ToString();
            processor.Registers.SetDoubleRegister(sRegS, MAddr);
            processor.SystemMemory.SetMemory(MAddr2, WordSize.OneByte, pVal, false);
            processor.Registers.SetSingleRegister("Flags", flags);
            processor.Registers.SetSingleRegister(pReg, (byte)pVal);
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, Prebyte, false);
            byte opcode = (byte)(pReg | Opcode);
            processor.SystemMemory.SetMemory(0xa001, WordSize.OneByte, opcode, false);
            processor.SystemMemory.SetMemory(0xa002, WordSize.OneByte, sVal, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {//ld (ixy+d),r
            byte simA = (byte)processor.SystemMemory.GetMemory(MAddr2, WordSize.OneByte, false);
            byte brdA = (byte)msg.Data;
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