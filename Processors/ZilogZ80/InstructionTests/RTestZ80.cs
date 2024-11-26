using Intel8085;
using Processors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Intel8085.InstructionTests;

namespace ZilogZ80.InstructionTests
{
    public enum InstructionDataSizeEnum
    {
        Single = 0,
        Double = 1
    }

    public abstract class RTestZ80 : IInstructionTest
    {
        public byte FlagMask { get { return (byte)0xd7; } }

        public string Name { set; get; }
        protected byte Prebyte { set; get; }
        protected byte[] Flags { set; get; }
        protected byte Opcode { set; get; }
        public InstructionCategoryEnum Category { set; get; }
        public InstructionDataSizeEnum InstructionDataSize { set; get; }

        protected static ushort[] PrimaryValues = new ushort[32];
        protected static ushort[] SecondaryValues = new ushort[32];
        protected static ushort[] StartValues = new ushort[32];

        public static byte[] arithFlags = new byte[] { 0x00, 0x01, 0x10, 0x11 };
        public static byte[] carryFlags = new byte[] { 0x00, 0x01 };

        protected static byte[] nullFlags = new byte[] { 0 };
        protected static byte[] FullSingle = new byte[256];
        protected static byte[] nullSingle = new byte[] { 0 };
        protected static byte[] SingleRegA = new byte[] { (byte)SingleRegisterEnums.a };
        protected static byte[] allRegs8 = new byte[]
            {
                (byte)SingleRegisterEnums.b,
                (byte)SingleRegisterEnums.c,
                (byte)SingleRegisterEnums.d,
                (byte)SingleRegisterEnums.e,
                (byte)SingleRegisterEnums.h,
                (byte)SingleRegisterEnums.l,
                (byte)SingleRegisterEnums.a,
            };

        protected ushort MAddr;
        protected Random mRnd = new Random();
        protected BoardMessage mResponse;
        protected byte[] responseBuff = new byte[18];

        public virtual ushort[] usePrimaryVals() { return new ushort[] { 0 }; }
        public virtual ushort[] useSecondaryVals() { return SecondaryValues; }
        public virtual byte[] usePrimaryRegs() { return new byte[] { 0 }; }
        public virtual byte[] useSecondaryRegs() { return new byte[] { 0 }; }

        public abstract void CheckResult(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors);

        public RTestZ80(string name, InstructionDataSizeEnum instructionDataSize, InstructionCategoryEnum category, byte prebyte, byte opcode, byte[] flags)
        {
            Name = name;
            InstructionDataSize = instructionDataSize;
            Category = category;
            Prebyte = prebyte;
            Opcode = opcode;
            Flags = flags;
            GenerateRandomDataValues();
        }
        public virtual void OnResponse(TestSettings testSettings, byte[] cmdParams, byte[] cmdData) { }

        //generate a randome sequence of byte offsets
        public ushort[] generateds(Random rnd)
        {
            byte[] buff = new byte[2];
            var ret = new ushort[32];
            for (int ii = 0; ii < ret.Length; ii++)
            {
                rnd.NextBytes(buff);
                var val = (byte)BitConverter.ToUInt16(buff, 0);
                ret[ii] = val;
                StartValues[ii] = (byte)ii;
            }
            return ret;
        }

        public void GenerateRandomDataValues()
        {
            generateDataValues(InstructionDataSize, mRnd, PrimaryValues);
            generateDataValues(InstructionDataSize, mRnd, SecondaryValues);
        }

        protected static void generateDataValues(InstructionDataSizeEnum instructionDataSize, Random rnd, ushort[] values)
        {
            byte[] buff = new byte[2];
            for (int ii = 0; ii < values.Length; ii++)
            {
                rnd.NextBytes(buff);
                var val = BitConverter.ToUInt16(buff, 0);
                if (instructionDataSize == InstructionDataSizeEnum.Single)
                    values[ii] = (byte)val;
                else
                    values[ii] = val;
            }
        }

        protected static byte getSimRegister(IProcessor processor, byte reg, ushort addr)
        {
            if (reg == (byte)SingleRegisterEnums.m)
                return (byte)processor.SystemMemory.GetMemory(addr, WordSize.OneByte, false);
            else
                return (byte)processor.Registers.GetSingleRegister(reg);
        }
        protected static void setSimRegister(IProcessor processor, byte reg, byte sVal, ushort addr)
        {
            if (reg == (byte)SingleRegisterEnums.m)
            {
                processor.Registers.SetDoubleRegister((byte)DoubleRegisterEnums.h, addr);
                processor.SystemMemory.SetMemory(addr, WordSize.OneByte, sVal, false);
            }
            else
                processor.Registers.SetSingleRegister(reg, sVal);
        }
        protected static byte getBrdRegister(BoardMessage msg, byte reg, ushort addr)
        {
            if (reg == (byte)SingleRegisterEnums.m)
                return (byte)msg.Data;
            else
                return msg.getSinglereg(reg);
        }

        protected static void setBrdRegister(BoardMessage ret, byte reg, byte sVal, ushort addr)
        {
            if (reg == (byte)SingleRegisterEnums.m)
            {
                ret.HL = addr;
                ret.Addr = addr;
                ret.Data = sVal;
            }
            else
                ret.setSinglereg(reg, sVal);
        }

        protected ushort generateMAddr()
        {//generate a random address between 0xc000 and 0xffff
            int val = mRnd.Next(0xff00 - 0xc100);
            return (ushort)(0xc100 + val);
        }
        public virtual void PerformTestFlags(TestSettings testSettings, statusUpdate callback) { }
    }
}