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
    public class ExMTestInstruction : RTestZ80_16
    {//ex (sp), ixy
        public ExMTestInstruction()
                : base("ExM", InstructionCategoryEnum.Z80, 0xe3, nullFlags) { }
        public override BoardMessage formMessage(byte pReg, ushort pVal, byte sReg, ushort sVal, byte flags)
        {//ex (sp), ixy
            var ret = new BoardMessage();
            MAddr = generateMAddr();
            ret.tst = (byte)TestInstructionEnum.RpTest_Z80;
            ret.PrebyteSize = (byte)BoardMessage.PrebyteSizeEnum.SOneB;
            Prebyte = (pReg == (byte)DoubleRegisterEnums.ix) ? (byte)0xdd : (byte)0xfd;
            ret.setDblreg((byte)DoubleRegisterEnums.sp, MAddr);
            ret.Prebyte1 = Prebyte;
            ret.Opcode = Opcode;
            ret.Addr = MAddr;
            ret.Data = pVal;
            ret.setDblreg(pReg, sVal);
            ret.flags = flags;
            return ret;
        }

        public override ushort[] usePrimaryVals() { return PrimaryValues; }
        public override ushort[] useSecondaryVals() { return SecondaryValues; }
        public override byte[] usePrimaryRegs() { return new byte[] { (byte)DoubleRegisterEnums.ix, (byte)DoubleRegisterEnums.iy }; }
        public override byte[] useSecondaryRegs() { return SingleRegA; }

        public override void formSim(IProcessor processor, byte pReg, ushort pVal, byte sReg, ushort sVal, byte flags)
        {//ex (sp), ixy
            processor.Registers.SetDoubleRegister((byte)DoubleRegisterEnums.sp, MAddr);
            processor.SystemMemory.SetMemory(MAddr, WordSize.TwoByte, pVal, false);
            var pRegS = ((DoubleRegisterEnums)pReg).ToString();
            processor.Registers.SetDoubleRegister(pRegS, sVal);
            processor.Registers.SetSingleRegister("Flags", flags);
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, Prebyte, false);
            processor.SystemMemory.SetMemory(0xa001, WordSize.OneByte, Opcode, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            var pRegS = ((DoubleRegisterEnums)pReg).ToString();
            ushort simA = (ushort)processor.Registers.GetDoubleRegister(pRegS);
            ushort brdA = msg.getDblreg(pReg);
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