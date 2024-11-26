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
    public class LogicOperationTestInstruction : RTestZ80_16
    {//adc a,(ixy+d)
        private ushort MAddr2;
        public LogicOperationTestInstruction(string name, byte opcode, byte[] flags)
                        : base(name, InstructionCategoryEnum.Z80, opcode, flags) { }
        public override BoardMessage formMessage(byte pReg, ushort pVal, byte sReg, ushort sVal, byte flags)
        {
            var ret = new BoardMessage();
            MAddr2 = (ushort)(MAddr + ((byte)pVal).SignExtend());
            ret.tst = (byte)TestInstructionEnum.RpTest_Z80;
            ret.PrebyteSize = (byte)BoardMessage.PrebyteSizeEnum.SOneB;
            Prebyte = (pReg == (byte)DoubleRegisterEnums.ix) ? (byte)0xdd : (byte)0xfd;
            ret.Prebyte1 = Prebyte;
            ret.Opcode = Opcode;
            ret.useIMMData = (byte)BoardMessage.ImmDataSizeEnum.SByte;
            ret.ImmData = (byte)pVal;
            ret.Addr = MAddr2;
            ret.Data = (byte)sVal;
            ret.setDblreg(pReg, MAddr);
            ret.setSinglereg(sReg, (byte)pVal);
            ret.flags = flags;
            return ret;
        }

        public override ushort[] usePrimaryVals()
        {
            var ret = new ushort[FullSingle.Length];
            for(int ii=0; ii<FullSingle.Length; ii++)
                ret[ii] = (byte)FullSingle[ii];
            return ret;
        }
        public override ushort[] useSecondaryVals() { return StartValues; }
        public override byte[] usePrimaryRegs() { return new byte[] { (byte)DoubleRegisterEnums.ix, (byte)DoubleRegisterEnums.iy }; }
        public override byte[] useSecondaryRegs() { return SingleRegA; }

        public override void formSim(IProcessor processor, byte pReg, ushort pVal, byte sReg, ushort sVal, byte flags)
        {
            var pRegS = ((DoubleRegisterEnums)pReg).ToString();
            processor.Registers.SetDoubleRegister(pRegS, MAddr);
            processor.Registers.SetSingleRegister("Flags", flags);
            processor.Registers.SetSingleRegister((byte)SingleRegisterEnums.a, (byte)pVal);
            processor.SystemMemory.SetMemory(MAddr2, WordSize.OneByte, sVal, false);
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, Prebyte, false);
            processor.SystemMemory.SetMemory(0xa001, WordSize.OneByte, Opcode, false);
            processor.SystemMemory.SetMemory(0xa002, WordSize.OneByte, (byte)pVal, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            byte simA = (byte)processor.Registers.GetSingleRegister((byte)SingleRegisterEnums.a);
            byte brdA = msg.a;
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