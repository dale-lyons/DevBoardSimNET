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
    public class AddInstructionTest : RTestZ80_16
    {
        public AddInstructionTest()
                : base("Add(ixy,rr)", InstructionCategoryEnum.Z80, 0x09, new byte[] { 0x00, 0x01, 0x10, 0x11 }) { }

        public override BoardMessage formMessage(byte pReg, ushort pVal, byte sReg, ushort sVal, byte flags)
        {//add ix,de
            Prebyte = (pReg == (byte)DoubleRegisterEnums.ix) ? (byte)0xdd : (byte)0xfd;
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_Z80;
            ret.PrebyteSize = (byte)BoardMessage.PrebyteSizeEnum.SOneB;
            ret.Prebyte1 = Prebyte;
            ret.Opcode = (byte)((Dbltoz80Dbl(sReg) << 4) | Opcode);
            ret.setDblreg(pReg, pVal);
            ret.setDblreg(sReg, sVal);
            ret.flags = flags;
            return ret;
        }

        public override ushort[] usePrimaryVals() { return new ushort[] { 0 }; }
        public override ushort[] useSecondaryVals() { return SecondaryValues; }


        public override byte[] usePrimaryRegs() { return new byte[] { (byte)DoubleRegisterEnums.ix, (byte)DoubleRegisterEnums.iy }; }
        public override byte[] useSecondaryRegs()
        {
            return new byte[]
            {
                (byte)DoubleRegisterEnums.b,
                (byte)DoubleRegisterEnums.d,
                mIndexRegister,
                (byte)DoubleRegisterEnums.sp
            };
        }

        public override void formSim(IProcessor processor, byte pReg, ushort pVal, byte sReg, ushort sVal, byte flags)
        {
            var pRegS = ((DoubleRegisterEnums)pReg).ToString();
            var sRegS = ((DoubleRegisterEnums)sReg).ToString();
            processor.Registers.SetSingleRegister("Flags", flags);
            byte opcode = (byte)((Dbltoz80Dbl(sReg) << 4) | Opcode);
            processor.Registers.SetDoubleRegister(pRegS, pVal);
            processor.Registers.SetDoubleRegister(sRegS, sVal);
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, Prebyte, false);
            processor.SystemMemory.SetMemory(0xa001, WordSize.OneByte, opcode, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            var pRegS = ((DoubleRegisterEnums)pReg).ToString();
            ushort simA = (ushort)processor.Registers.GetDoubleRegister(pRegS);
            ushort brdA = msg.getDblreg(pReg);
            byte simF = (byte)((byte)processor.Registers.GetSingleRegister("Flags") & FlagMask);
            byte brdF = (byte)(msg.flags & FlagMask);
            //Debug
            simF &= 0xef;
            brdF &= 0xef;
            if ((brdA != simA) || (brdF != simF))
            {
                errors.Add(new InstructionTestError { pReg = pReg, sReg = sReg, pVal = 0, Flags = 0, Opcode = Opcode, TestName = Name });
                return;
            }
        }
    }
}