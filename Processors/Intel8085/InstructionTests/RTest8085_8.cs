using Processors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Intel8085.InstructionTests
{
    public abstract class RTest8085_8 : RTest8085
    {
        private byte[] resonseBuff = new byte[18];

        private byte pValv;
        private byte sValv;
        protected ushort MAddr;

        protected static byte[] SingleInr1 = new byte[32];
        protected static byte[] SingleInr2 = new byte[32];
        protected static byte[] nullFlags = new byte[] { 0 };
        protected static byte[] FullSingle = new byte[256];
        protected static byte[] nullSingle = new byte[] { 0 };

        protected static byte[] allRegisters = new byte[]
{
                (byte)SingleRegisterEnums.a,
                (byte)SingleRegisterEnums.b,
                (byte)SingleRegisterEnums.c,
                (byte)SingleRegisterEnums.d,
                (byte)SingleRegisterEnums.e,
                (byte)SingleRegisterEnums.h,
                (byte)SingleRegisterEnums.l,
                (byte)SingleRegisterEnums.m
};
        protected static byte[] nullRegisters = new byte[] { (byte)SingleRegisterEnums.a };
        protected static byte[] SingleRegA = new byte[] { (byte)SingleRegisterEnums.a };

        public abstract byte[] usePrimaryVals();
        public abstract byte[] useSecondaryVals();

        public abstract BoardMessage formMessage(byte pReg, byte pVal, byte sReg, byte sVal, byte flags);
        public abstract void formSim(IProcessor processor, byte pReg, byte pVal, byte sReg, byte sVal, byte flags);

        public byte getSimRegister(IProcessor processor, byte reg, ushort addr)
        {
            if (reg == (byte)SingleRegisterEnums.m)
                return (byte)processor.SystemMemory.GetMemory(addr, WordSize.OneByte, false);
            else
                return (byte)processor.Registers.GetSingleRegister(reg);
        }

        public void setSimRegister(IProcessor processor, byte reg, byte sVal)
        {
            if (reg == (byte)SingleRegisterEnums.m)
            {
                processor.SystemMemory.SetMemory(MAddr, WordSize.OneByte, sVal, false);
                processor.Registers.SetDoubleRegister((byte)DoubleRegisterEnums.h, MAddr);
            }
            else
                processor.Registers.SetSingleRegister(reg, sVal);
        }

        public byte getBrdRegister(BoardMessage msg, byte reg, ushort addr)
        {
            if (reg == (byte)SingleRegisterEnums.m)
                return (byte)msg.Data;
            else
                return msg.getSinglereg(reg);
        }

        public void setBrdRegister(BoardMessage ret, byte reg, byte sVal)
        {
            if (reg == (byte)SingleRegisterEnums.m)
            {
                ret.HL = MAddr;
                ret.Addr = MAddr;
                ret.Data = sVal;
            }
            else
                ret.setSinglereg(reg, sVal);
        }

        public RTest8085_8(string name, InstructionCategoryEnum category, byte opcode, byte[] flags)
        {
            Name = name;
            Opcode = opcode;
            Category = category;
            Flags = flags;

            mRnd.NextBytes(SingleInr1);
            mRnd.NextBytes(SingleInr2);
            SingleInr1[0] = 0;
            SingleInr2[0] = 0;
            SingleInr1[SingleInr1.Length - 1] = 0xff;
            SingleInr2[SingleInr1.Length - 1] = 0xff;

            for (int ii = 0; ii < FullSingle.Length; ii++)
                FullSingle[ii] = (byte)ii;
        }

        public override void PerformTestFlags(TestSettings testSettings, statusUpdate callback)
        {
            callback(StatusFields.TestStatus, Name);
            foreach (var flag in Flags)
            {
                callback(StatusFields.Flag, flag.ToString("X2"));
                foreach (var pReg in usePrimaryRegs())
                {
                    callback(StatusFields.PReg, pReg.ToString());
                    var pVals = usePrimaryVals();
                    for (int pVal = 0; pVal < pVals.Length; pVal++)
                    {
                        callback(StatusFields.PVal, pVal.ToString("X2"));
                        foreach (var sReg in useSecondaryRegs())
                        {
                            callback(StatusFields.SReg, sReg.ToString());
                            if (pReg == (byte)SingleRegisterEnums.m && sReg == (byte)SingleRegisterEnums.m)
                                continue;
                            var sVals = useSecondaryVals();
                            for (int sVal = 0; sVal < sVals.Length; sVal++)
                            {
                                callback(StatusFields.SVal, sVal.ToString("X2"));
                                if (testSettings.abort.WaitOne(0))
                                    return;

                                pValv = pVals[pVal];
                                sValv = sVals[sVal];
                                MAddr = generateMAddr();

                                mResponse = null;
                                msg = null;
                                if (testSettings.src != null)
                                {
                                    testSettings.src.Read(resonseBuff, 0, resonseBuff.Length);
                                    mResponse = BoardMessage.FromStream(resonseBuff);
                                    testSettings.src.Read(resonseBuff, 0, 4);
                                    pValv = resonseBuff[0];
                                    sValv = resonseBuff[1];
                                    MAddr = BitConverter.ToUInt16(resonseBuff, 2);
                                }
                                else
                                {
                                    var msg = formMessage(pReg, pValv, sReg, sValv, flag);
                                    var stream = msg.ToStream();
                                    testSettings.port.Write(stream, 0, stream.Length);
                                }//else

                                callback(StatusFields.Errors, testSettings.Errors.Count.ToString());
                                var s1 = DateTime.Now;
                                while (mResponse == null)
                                {
                                    Application.DoEvents();
                                    if ((DateTime.Now - s1).TotalSeconds > 10)
                                    {
                                        Debug.WriteLine(string.Format("Timeout!!"));
                                        break;
                                    }
                                }//while
                                if (mResponse == null)
                                    throw new Exception();
                            }//for sVal
                            formSim(testSettings.processor, pReg, pValv, sReg, sValv, flag);
                            testSettings.processor.ExecuteOne();
                            CheckResult(mResponse, testSettings.processor, pReg, sReg, testSettings.Errors);
                        }//for sReg
                    }//for pVal
                }//pReg
            }//for flags
            callback(StatusFields.TestStatus, "Finished");
            callback(StatusFields.Errors, testSettings.Errors.Count.ToString());
        }//PerformTestFlags

        protected void CheckResultRegFlags(BoardMessage msg, IProcessor processor, byte reg, List<InstructionTestError> errors)
        {
            byte simA = getSimRegister(processor, reg, MAddr);
            byte brdA = getBrdRegister(msg, reg, MAddr);
            byte simF = (byte)((byte)processor.Registers.GetSingleRegister("Flags") & FlagMask);
            byte brdF = (byte)(msg.flags & FlagMask);
            if ((brdA != simA) || (brdF != simF))
            {
                errors.Add(new InstructionTestError { pReg = 0, sReg = reg, pVal = 0, Flags = 0, Opcode = Opcode, TestName = Name });
                return;
            }
        }
        protected void CheckResultRegRegFlags(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            byte simA = getSimRegister(processor, pReg, MAddr);
            byte brdA = getBrdRegister(msg, pReg, MAddr);
            byte simF = (byte)((byte)processor.Registers.GetSingleRegister("Flags") & FlagMask);
            byte brdF = (byte)(msg.flags & FlagMask);
            if ((brdA != simA) || (brdF != simF))
            {
                errors.Add(new InstructionTestError { pReg = 0, sReg = 0, pVal = 0, Flags = 0, Opcode = Opcode, TestName = Name });
                return;
            }
        }

        public override void OnResponse(TestSettings testSettings, byte[] cmdParams, byte[] cmdData)
        {
            if (mResponse != null)
                return;

            if (cmdParams.Length != 18)
                throw new Exception();

            if (testSettings.dst != null)
            {
                testSettings.dst.Write(cmdParams, 0, cmdParams.Length);
                testSettings.dst.WriteByte(pValv);
                testSettings.dst.WriteByte(sValv);
                testSettings.dst.Write(BitConverter.GetBytes(MAddr), 0, 2);
            }
            mResponse = BoardMessage.FromStream(cmdParams);
        }

    }
}