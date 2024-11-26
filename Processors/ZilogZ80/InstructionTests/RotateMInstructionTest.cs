using Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intel8085;
using Intel8085.InstructionTests;

namespace ZilogZ80.InstructionTests
{
    public class RotateMInstructionTest : RTestZ80_16
    {
        private ushort MAddr2;
        private bool mParityBitDefined;
        public RotateMInstructionTest(string name, byte opcode, bool parityBitDefined=true)
                 : base(name, InstructionCategoryEnum.Z80, opcode, carryFlags)
        {
            mParityBitDefined = parityBitDefined;
        }

        public override BoardMessage formMessage(byte pReg, ushort pVal, byte sReg, ushort sVal, byte flags)
        {//rlc (i?+d)
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_Z80;
            ret.PrebyteSize = (byte)BoardMessage.PrebyteSizeEnum.STwoB;
            Prebyte = (pReg == (byte)DoubleRegisterEnums.ix) ? (byte)0xdd : (byte)0xfd;
            ret.Prebyte1 = Prebyte;
            ret.Prebyte2 = 0xcb;
            ret.Opcode = (byte)sVal;

            MAddr = generateMAddr();
            MAddr2 = (ushort)(MAddr + ((byte)sVal).SignExtend());

            ret.useIMMData = (byte)BoardMessage.ImmDataSizeEnum.SByte;
            //var buf = new byte[] { Opcode, (byte)sVal };
            ret.ImmData = Opcode;
            ret.setDblreg(pReg, MAddr);
            ret.Addr = MAddr2;
            ret.Data = pVal;
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
            processor.Registers.PC = 0xa000;
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, Prebyte, false);
            processor.SystemMemory.SetMemory(0xa001, WordSize.OneByte, 0xcb, false);
            processor.SystemMemory.SetMemory(0xa002, WordSize.OneByte, sVal, false);
            processor.SystemMemory.SetMemory(0xa003, WordSize.OneByte, Opcode, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            var simA = (byte)processor.Registers.GetSingleRegister((byte)SingleRegisterEnums.a);
            var brdA = msg.a;
            var simM = (byte)processor.SystemMemory.GetMemory(MAddr2, WordSize.OneByte, false);
            var brdM = (byte)msg.Data;

            byte flagMask = FlagMask;
            if (!mParityBitDefined)
                flagMask &= 0xfb;

            byte simF = (byte)((byte)processor.Registers.GetSingleRegister("Flags") & flagMask);
            byte brdF = (byte)(msg.flags & flagMask);
            if ((simA != brdA) || (brdM != simM) || (brdF != simF))
            {
                errors.Add(new InstructionTestError { pReg = pReg, sReg = sReg, pVal = 0, Flags = 0, Opcode = Opcode, TestName = Name });
                return;
            }
        }
    }
}