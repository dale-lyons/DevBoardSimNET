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
    public class LdrrMnnInstructionTest : RTestZ80_16
    {//ld i?,(nnnn)
        public LdrrMnnInstructionTest()
                    : base("LdrrMnn()", InstructionCategoryEnum.Z80, 0x2a, new byte[] { 0x00 }) { }

        public override BoardMessage formMessage(byte pReg, ushort pVal, byte sReg, ushort sVal, byte flags)
        {//ld i?,(nnnn)
            var ret = new BoardMessage();
            ret.tst = (byte)TestInstructionEnum.RpTest_Z80;
            ret.PrebyteSize = (byte)BoardMessage.PrebyteSizeEnum.SOneB;
            Prebyte = (sReg == (byte)DoubleRegisterEnums.ix) ? (byte)0xdd : (byte)0xfd;
            ret.Prebyte1 = Prebyte;
            ret.Opcode = Opcode;
            ret.useIMMData = (byte)BoardMessage.ImmDataSizeEnum.SWord;
            ret.ImmData = MAddr;
            ret.Addr = MAddr;
            ret.Data = pVal;
            ret.flags = flags;
            return ret;
        }

        public override ushort[] usePrimaryVals() { return SecondaryValues; }
        public override ushort[] useSecondaryVals() { return new ushort[] { 0 }; }

        public override byte[] usePrimaryRegs() { return new byte[] { 0 }; }
        public override byte[] useSecondaryRegs() { return new byte[] { (byte)DoubleRegisterEnums.ix, (byte)DoubleRegisterEnums.iy }; }

        public override void formSim(IProcessor processor, byte pReg, ushort pVal, byte sReg, ushort sVal, byte flags)
        {//ld i?,(nnnn)
            processor.Registers.SetSingleRegister("Flags", flags);
            processor.Registers.PC = 0xa000;
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, Prebyte, false);
            processor.SystemMemory.SetMemory(0xa001, WordSize.OneByte, Opcode, false);
            processor.SystemMemory.SetMemory(0xa002, WordSize.TwoByte, MAddr, false);
            processor.SystemMemory.SetMemory(MAddr, WordSize.TwoByte, pVal, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            var sRegS = ((DoubleRegisterEnums)sReg).ToString();
            ushort simA = (ushort)processor.Registers.GetDoubleRegister(sRegS);
            ushort brdA = msg.getDblreg(sReg);
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