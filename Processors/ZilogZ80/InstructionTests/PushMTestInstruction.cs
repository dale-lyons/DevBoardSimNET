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
    public class PushMTestInstruction : RTestZ80_16
    {
        //private ushort MAddr2;
        public PushMTestInstruction()
                        : base("PushM", InstructionCategoryEnum.Z80, 0xe5, nullFlags) { }
        public override BoardMessage formMessage(byte pReg, ushort pVal, byte sReg, ushort sVal, byte flags)
        {//push ixy
            var ret = new BoardMessage();
            MAddr = generateMAddr();
            ret.tst = (byte)TestInstructionEnum.RpTest_Z80;
            ret.PrebyteSize = (byte)BoardMessage.PrebyteSizeEnum.SOneB;
            Prebyte = (pReg == (byte)DoubleRegisterEnums.ix) ? (byte)0xdd : (byte)0xfd;
            ret.setDblreg((byte)DoubleRegisterEnums.sp, MAddr);
            ret.Prebyte1 = Prebyte;
            ret.Opcode = Opcode;
            ret.Addr = (ushort)(MAddr-2);
            ret.Data = pVal;
            ret.setDblreg(pReg, pVal);
            ret.flags = flags;
            return ret;
        }

        public override ushort[] usePrimaryVals() { return PrimaryValues; }
        public override ushort[] useSecondaryVals() { return new ushort[] { 0 }; }
        public override byte[] usePrimaryRegs() { return new byte[] { (byte)DoubleRegisterEnums.ix, (byte)DoubleRegisterEnums.iy }; }
        public override byte[] useSecondaryRegs() { return SingleRegA; }

        public override void formSim(IProcessor processor, byte pReg, ushort pVal, byte sReg, ushort sVal, byte flags)
        {//push ixy
            var pRegS = ((DoubleRegisterEnums)pReg).ToString();
            processor.Registers.SetDoubleRegister(pRegS, pVal);
            processor.Registers.SetDoubleRegister((byte)DoubleRegisterEnums.sp, MAddr);
            processor.Registers.SetSingleRegister("Flags", flags);
            processor.SystemMemory.SetMemory(0xa000, WordSize.OneByte, Prebyte, false);
            processor.SystemMemory.SetMemory(0xa001, WordSize.OneByte, Opcode, false);
        }

        public override void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {//push ixy
            var pRegS = ((DoubleRegisterEnums)pReg).ToString();
            ushort simSP = (ushort)processor.Registers.GetDoubleRegister((byte)DoubleRegisterEnums.sp);
            ushort brdSP = msg.getDblreg((byte)DoubleRegisterEnums.sp);

            ushort simIXY = (ushort)processor.Registers.GetDoubleRegister(pRegS);
            ushort brdIXY = msg.getDblreg(pReg);

            ushort simData = (ushort)processor.SystemMemory.GetMemory((uint)(MAddr-2), WordSize.TwoByte, false);
            ushort brdData = msg.Data;

            byte simF = (byte)((byte)processor.Registers.GetSingleRegister("Flags") & FlagMask);
            byte brdF = (byte)(msg.flags & FlagMask);
            if ((simSP != brdSP) || (simIXY != brdIXY) || (simData != brdData) || (brdF != simF) || (simIXY != simData))
            {
                errors.Add(new InstructionTestError { pReg = pReg, sReg = sReg, pVal = 0, Flags = 0, Opcode = Opcode, TestName = Name });
                return;
            }
        }
    }
}