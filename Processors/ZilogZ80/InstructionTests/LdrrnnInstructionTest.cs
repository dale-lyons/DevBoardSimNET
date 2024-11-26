using Intel8085;
using Processors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intel8085.InstructionTests;

namespace ZilogZ80.InstructionTests
{
    public class LdrrnnInstructionTest : RTestZ80_16
    {
        public LdrrnnInstructionTest()
        : base("Ldrrnn(Ixy)", InstructionCategoryEnum.Z80, 0x01, new byte[] { 0x00 }) { }

        public override BoardMessage formMessage(byte pReg, ushort pVal, byte sReg, ushort sVal, byte flags)
        {//ld ixy,nnnn
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_Z80;
            ret.PrebyteSize = (byte)BoardMessage.PrebyteSizeEnum.SOneB;
            Prebyte = (pReg == (byte)DoubleRegisterEnums.ix) ? (byte)0xdd : (byte)0xfd;
            ret.Prebyte1 = Prebyte;
            ret.Opcode = (byte)((Dbltoz80Dbl(sReg) << 4) | Opcode);
            ret.useIMMData = (byte)BoardMessage.ImmDataSizeEnum.SWord;
            ret.ImmData = sVal;
            ret.flags = flags;
            return ret;
        }

        public override ushort[] usePrimaryVals() { return new ushort[] { 0 }; }
        public override ushort[] useSecondaryVals() { return SecondaryValues; }

        public override byte[] usePrimaryRegs() { return new byte[] { (byte)DoubleRegisterEnums.ix, (byte)DoubleRegisterEnums.iy }; }

        public override byte[] useSecondaryRegs() { return new byte[] { (byte)DoubleRegisterEnums.ix }; }

        public override void formSim(IProcessor processor, byte pReg, ushort pVal, byte sReg, ushort sVal, byte flags)
        {//ld ixy,nnnn
            processor.Registers.SetSingleRegister("Flags", flags);
            byte opcode = (byte)((Dbltoz80Dbl(sReg) << 4) | Opcode);
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, Prebyte, false);
            processor.SystemMemory.SetMemory(0xa001, WordSize.OneByte, opcode, false);
            processor.SystemMemory.SetMemory(0xa002, WordSize.TwoByte, sVal, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            var pRegS = ((DoubleRegisterEnums)pReg).ToString();
            ushort simA = (ushort)processor.Registers.GetDoubleRegister(pRegS);
            ushort brdA = msg.getDblreg((byte)mIndexRegister);
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