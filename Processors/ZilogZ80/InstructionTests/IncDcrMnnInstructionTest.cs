using Intel8085;
using Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Intel8085.InstructionTests;

namespace ZilogZ80.InstructionTests
{
    public class IncDcrMnnInstructionTest : RTestZ80_16
    {
        private ushort MAddr2;
        public IncDcrMnnInstructionTest(string name, byte opcode)
                 : base(name + "(Ixyd)", InstructionCategoryEnum.Z80, opcode, new byte[] { 0x00 }) { }

        public override BoardMessage formMessage(byte pReg, ushort pVal, byte sReg, ushort sVal, byte flags)
        {//inc (i?+d)
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_Z80;
            ret.PrebyteSize = (byte)BoardMessage.PrebyteSizeEnum.SOneB;
            Prebyte = (pReg == (byte)DoubleRegisterEnums.ix) ? (byte)0xdd : (byte)0xfd;
            ret.Prebyte1 = Prebyte;
            ret.Opcode = Opcode;
            ret.useIMMData = (byte)BoardMessage.ImmDataSizeEnum.SByte;
            ret.ImmData = (byte)sVal;
            ret.setDblreg(pReg, MAddr);
            MAddr2 = (ushort)(MAddr + ((byte)sVal).SignExtend());
            ret.Addr = MAddr2;
            ret.Data = (byte)pVal;
            ret.flags = flags;
            return ret;
        }

        public override ushort[] usePrimaryVals() { return StartValues; }
        public override ushort[] useSecondaryVals() { return generateds(mRnd); }

        public override byte[] usePrimaryRegs() { return new byte[] { (byte)DoubleRegisterEnums.ix, (byte)DoubleRegisterEnums.iy }; }
        public override byte[] useSecondaryRegs() { return new byte[] { (byte)DoubleRegisterEnums.ix }; }

        public override void formSim(IProcessor processor, byte pReg, ushort pVal, byte sReg, ushort sVal, byte flags)
        {//inc (i?+d)
            var pRegS = ((DoubleRegisterEnums)pReg).ToString();
            processor.Registers.SetDoubleRegister(pRegS, MAddr);
            processor.SystemMemory.SetMemory(MAddr2, WordSize.OneByte, pVal, false);
            processor.Registers.SetSingleRegister("Flags", flags);
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, Prebyte, false);
            processor.SystemMemory.SetMemory(0xa001, WordSize.OneByte, Opcode, false);
            processor.SystemMemory.SetMemory(0xa002, WordSize.OneByte, sVal, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            var simA = (byte)processor.SystemMemory.GetMemory(MAddr2, WordSize.OneByte, false);
            var brdA = (byte)msg.Data;
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