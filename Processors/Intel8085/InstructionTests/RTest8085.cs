using Processors;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Windows.Forms;

namespace Intel8085.InstructionTests
{
    public abstract class RTest8085 : IInstructionTest
    {
        public string Name { set; get; }
        protected byte Opcode { set; get; }
        public InstructionCategoryEnum Category { set; get; }
        public static readonly byte FlagMask = 0xd5;

        protected Random mRnd = new Random();
        protected ushort generateMAddr()
        {//generate a random address between 0xd000 and 0xf000
            int rnd = mRnd.Next(0x1f00);
            return (ushort)(0xe000 + rnd);
        }

        protected BoardMessage mResponse;
        protected BoardMessage msg;
        protected byte[] Flags;

        public abstract byte[] usePrimaryRegs();
        public abstract byte[] useSecondaryRegs();
        public abstract void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors);

        public virtual void PerformTestFlags(TestSettings testSettings, statusUpdate callback) { }
        public virtual void OnResponse(TestSettings testSettings, byte[] cmdParams, byte[] cmdData) { }

        protected void CheckResultAFlags(BoardMessage msg, IProcessor processor, List<InstructionTestError> errors)
        {
            byte simA = (byte)processor.Registers.GetSingleRegister((byte)SingleRegisterEnums.a);
            byte brdA = (byte)msg.a;
            byte simF = (byte)((byte)processor.Registers.GetSingleRegister("Flags") & FlagMask);
            byte brdF = (byte)(msg.flags & FlagMask);
            if ((brdA != simA) || (brdF != simF))
            {
                errors.Add(new InstructionTestError { pReg = 0, sReg = 0, pVal = 0, Flags = 0, Opcode = Opcode, TestName = Name });
                return;
            }
        }

        protected void CheckResultHLFlags(BoardMessage msg, IProcessor processor, List<InstructionTestError> errors)
        {
            ushort simHL = (ushort)processor.Registers.GetDoubleRegister((byte)DoubleRegisterEnums.h);
            ushort brdHL = (ushort)msg.HL;
            byte simF = (byte)((byte)processor.Registers.GetSingleRegister("Flags") & FlagMask);
            byte brdF = (byte)(msg.flags & FlagMask);
            if ((brdHL != simHL) && (brdF != simF))
            {
                errors.Add(new InstructionTestError { pReg = 0, sReg = 0, pVal = 0, Flags = 0, Opcode = Opcode, TestName = Name });
                return;
            }
        }
        protected void CheckResultMem8(BoardMessage msg, IProcessor processor, ushort addr, List<InstructionTestError> errors)
        {
            byte simMem = (byte)processor.SystemMemory.GetMemory(addr, WordSize.OneByte, false);
            byte brdMem = (byte)msg.Data;
            byte simF = (byte)((byte)processor.Registers.GetSingleRegister("Flags") & FlagMask);
            byte brdF = (byte)(msg.flags & FlagMask);
            if ((simMem != brdMem) || (brdF != simF))
            {
                errors.Add(new InstructionTestError { pReg = 0, sReg = 0, pVal = 0, Flags = 0, Opcode = Opcode, TestName = Name });
                return;
            }
        }
        protected void CheckResultMem16(BoardMessage msg, IProcessor processor, ushort addr, List<InstructionTestError> errors)
        {
            ushort simMem = (ushort)processor.SystemMemory.GetMemory(addr, WordSize.TwoByte, false);
            ushort brdMem = msg.Data;
            byte simF = (byte)((byte)processor.Registers.GetSingleRegister("Flags") & FlagMask);
            byte brdF = (byte)(msg.flags & FlagMask);
            if ((simMem != brdMem) || (brdF != simF))
            {
                errors.Add(new InstructionTestError { pReg = 0, sReg = 0, pVal = 0, Flags = 0, Opcode = Opcode, TestName = Name });
                return;
            }
        }
    }
}