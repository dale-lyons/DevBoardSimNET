using Processors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;

namespace Intel8085.InstructionTests
{
    public abstract class RTest8085_16 : RTest8085
    {
        private byte[] resonseBuff = new byte[18];
        //private byte[] msgBuff = new byte[29];

        protected static ushort[] DoubleInr1 = new ushort[32];
        protected static ushort[] DoubleInr2 = new ushort[32];
        protected static ushort[] RndVal8 = new ushort[32];
        protected static ushort[] FullVal8 = new ushort[256];
        protected static byte[] SingleH = new byte[] { (byte)DoubleRegisterEnums.h };

        protected static byte[] allRegisters = new byte[]
{
                (byte)DoubleRegisterEnums.b,
                (byte)DoubleRegisterEnums.d,
                (byte)DoubleRegisterEnums.h,
                (byte)DoubleRegisterEnums.sp
};
        protected static byte[] nullRegisters = new byte[] { (byte)DoubleRegisterEnums.b };
        protected static byte[] SingleRegA = new byte[] { (byte)DoubleRegisterEnums.b };

        private ushort pValv;
        private ushort sValv;
        protected ushort MAddr;

        public abstract ushort[] usePrimaryVals();
        public abstract ushort[] useSecondaryVals();

        public abstract BoardMessage formMessage(byte pReg, ushort pVal, byte sReg, ushort sVal);
        public abstract void formSim(IProcessor processor, byte pReg, ushort pVal, byte sReg, ushort sVal);

        public RTest8085_16(string name, InstructionCategoryEnum category, byte opcode)
        {
            Name = name;
            Opcode = opcode;
            Category = category;

            DoubleInr1[0] = 0;
            DoubleInr2[1] = 0;

            byte[] buffer = new byte[2];
            mRnd = new Random();
            for (int ii = 1; ii < DoubleInr1.Length - 1; ii++)
            {
                mRnd.NextBytes(buffer);
                DoubleInr1[ii] = BitConverter.ToUInt16(buffer, 0);
                mRnd.NextBytes(buffer);
                DoubleInr2[ii] = BitConverter.ToUInt16(buffer, 0);

                mRnd.NextBytes(buffer);
                RndVal8[ii] = (ushort)(byte)BitConverter.ToUInt16(buffer, 0);

            }
            DoubleInr1[DoubleInr1.Length - 1] = 0xffff;
            DoubleInr2[DoubleInr1.Length - 1] = 0xffff;

            for (int ii = 0; ii < FullVal8.Length; ii++)
                FullVal8[ii] = (byte)ii;
        }

        public override void PerformTestFlags(TestSettings testSettings, statusUpdate callback)
        {
            callback(StatusFields.TestStatus, Name);
            callback(StatusFields.Flag, "0");
            foreach (var pReg in usePrimaryRegs())
            {
                callback(StatusFields.PReg, pReg.ToString());
                var pVals = usePrimaryVals();
                for (int pVal = 0; pVal < pVals.Length; pVal++)
                {
                    callback(StatusFields.PVal, pVal.ToString("X4"));
                    foreach (var sReg in useSecondaryRegs())
                    {
                        callback(StatusFields.SReg, sReg.ToString());
                        var sVals = useSecondaryVals();
                        for (int sVal = 0; sVal < sVals.Length; sVal++)
                        {
                            callback(StatusFields.SVal, sVal.ToString("X4"));
                            if (testSettings.abort.WaitOne(0))
                                return;

                            MAddr = generateMAddr();
                            pValv = pVals[pVal];
                            sValv = sVals[sVal];
                            mResponse = null;
                            msg = null;
                            if (testSettings.src != null)
                            {
                                testSettings.src.Read(resonseBuff, 0, resonseBuff.Length);
                                mResponse = BoardMessage.FromStream(resonseBuff);
                                testSettings.src.Read(resonseBuff, 0, 6);
                                pValv = BitConverter.ToUInt16(resonseBuff, 0);
                                sValv = BitConverter.ToUInt16(resonseBuff, 2);
                                MAddr = BitConverter.ToUInt16(resonseBuff, 4);
                            }
                            else
                            {
                                pValv = pVals[pVal];
                                sValv = sVals[sVal];
                                msg = formMessage(pReg, pValv, sReg, sValv);
                                var stream = msg.ToStream();
                                testSettings.port.Write(stream, 0, stream.Length);
                            }

                            callback(StatusFields.Errors, testSettings.Errors.Count.ToString());
                            var s1 = DateTime.Now;
                            while (mResponse == null)
                            {
                                Application.DoEvents();
                                if ((DateTime.Now - s1).TotalSeconds > 100000)
                                {
                                    Debug.WriteLine(string.Format("Timeout!!"));
                                    break;
                                }
                            }
                            if (mResponse == null)
                            {
                                testSettings.Errors.Add(new InstructionTestError { pReg = (byte)pReg, sReg = (byte)sReg, pVal = 0, Flags = 0, Opcode = Opcode, TestName = Name });
                                return;
                            }
                            formSim(testSettings.processor, (byte)pReg, pValv, (byte)sReg, sValv);
                            testSettings.processor.ExecuteOne();
                            CheckResult(mResponse, testSettings.processor, (byte)pReg, (byte)sReg, testSettings.Errors);
                        }
                    }
                }
            }//pReg
            callback(StatusFields.TestStatus, "Finished");
            callback(StatusFields.Errors, testSettings.Errors.Count.ToString());
        }//PerformTestFlags

        protected void CheckResultRegFlags(BoardMessage msg, IProcessor processor, byte reg, List<InstructionTestError> errors)
        {
            ushort simSReg = (ushort)processor.Registers.GetDoubleRegister(reg);
            ushort brdSReg = msg.getDblreg(reg);
            byte simF = (byte)((byte)processor.Registers.GetSingleRegister("Flags") & FlagMask);
            byte brdF = (byte)(msg.flags & FlagMask);
            if ((simSReg != brdSReg) || (brdF != simF))
            {
                errors.Add(new InstructionTestError { pReg = 0, sReg = reg, pVal = 0, Flags = 0, Opcode = Opcode, TestName = Name });
                return;
            }
        }
        protected void CheckResultRegRegFlags(BoardMessage msg, IProcessor processor, byte pReg, byte sReg, List<InstructionTestError> errors)
        {
            ushort simPReg = (ushort)processor.Registers.GetDoubleRegister(pReg);
            ushort brdPReg = msg.getDblreg(pReg);
            ushort simSReg = (ushort)processor.Registers.GetDoubleRegister(sReg);
            ushort brdSReg = msg.getDblreg(sReg);
            byte simF = (byte)((byte)processor.Registers.GetSingleRegister("Flags") & FlagMask);
            byte brdF = (byte)(msg.flags & FlagMask);

            if ((simPReg != brdPReg) || simSReg != brdSReg || (brdF != simF))
            {
                errors.Add(new InstructionTestError { pReg = pReg, sReg = sReg, pVal = 0, Flags = 0, Opcode = Opcode, TestName = Name });
                return;
            }
        }
        protected void CheckResultRegMemFlags(BoardMessage msg, IProcessor processor, byte reg, ushort addr, List<InstructionTestError> errors)
        {
            ushort simReg = (ushort)processor.Registers.GetDoubleRegister(reg);
            ushort brdReg = msg.getDblreg(reg);
            ushort simMem = (ushort)processor.SystemMemory.GetMemory(addr, WordSize.TwoByte, false);
            ushort brdMem = msg.Data;
            byte simF = (byte)((byte)processor.Registers.GetSingleRegister("Flags") & FlagMask);
            byte brdF = (byte)(msg.flags & FlagMask);
            if ((simReg != brdReg) || (simMem != brdMem) || (simF != brdF))
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
                testSettings.dst.Write(BitConverter.GetBytes(pValv), 0, 2);
                testSettings.dst.Write(BitConverter.GetBytes(sValv), 0, 2);
                testSettings.dst.Write(BitConverter.GetBytes(MAddr), 0, 2);
            }
            mResponse = BoardMessage.FromStream(cmdParams);
        }

    }
}