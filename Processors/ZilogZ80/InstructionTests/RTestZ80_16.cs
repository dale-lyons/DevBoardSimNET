using Intel8085;
using Processors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Intel8085.InstructionTests;
using System.IO;

namespace ZilogZ80.InstructionTests
{
    //public enum z80IndexRegisterEnum
    //{
    //    IX,
    //    IY
    //}

    public abstract class RTestZ80_16 : RTestZ80
    {
        public RTestZ80_16(string name, InstructionCategoryEnum category, byte opcode, byte[] flags)
                    : base(name, InstructionDataSizeEnum.Double, category, 0, opcode, flags) { }

        protected byte mIndexRegister;
        protected ushort pValv;
        protected ushort sValv;

        public abstract BoardMessage formMessage(byte pReg, ushort pVal, byte sReg, ushort sVal, byte flags);
        public abstract void formSim(IProcessor processor, byte pReg, ushort pVal, byte sReg, ushort sVal, byte flag);

        protected void setDblRegister(BoardMessage ret, byte reg, ushort pVal)
        {
            ret.setDblreg(Dbltoz80Dbl(reg), pVal);
        }
        protected void setDblRegister(IProcessor processor, byte reg, ushort pVal)
        {
            processor.Registers.SetDoubleRegister(Dbltoz80Dbl(reg), pVal);
        }

        protected byte Dbltoz80Dbl(byte reg)
        {
            //b = 0x00,
            //d = 0x01,
            //h = 0x02,
            //sp = 0x03,
            //psw = 0x04,
            //ix = 0x05,
            //iy = 0x06
            switch (reg)
            {
                case (byte)DoubleRegisterEnums.b:
                case (byte)DoubleRegisterEnums.d:
                case (byte)DoubleRegisterEnums.h:
                case (byte)DoubleRegisterEnums.sp:
                    return reg;
                case (byte)DoubleRegisterEnums.ix:
                case (byte)DoubleRegisterEnums.iy:
                    return (byte)DoubleRegisterEnums.h;
                default:
                    throw new Exception();
            }
        }

        public override void PerformTestFlags(TestSettings testSettings, statusUpdate callback)
        {
            MAddr = generateMAddr();
            callback(StatusFields.TestStatus, Name);
            foreach (var flag in Flags)
            {
                callback(StatusFields.Flag, flag.ToString("X2"));
                foreach (var pReg in usePrimaryRegs())
                {
                    mIndexRegister = (pReg == (byte)DoubleRegisterEnums.ix) ? pReg : (byte)DoubleRegisterEnums.iy;
                    callback(StatusFields.PReg, pReg.ToString());
                    var pVals = usePrimaryVals();
                    for (int pVal = 0; pVal < pVals.Length; pVal++)
                    {
                        callback(StatusFields.PVal, pVal.ToString("X2"));
                        foreach (var sReg in useSecondaryRegs())
                        {
                            callback(StatusFields.SReg, sReg.ToString());
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
                                if (testSettings.src != null)
                                {
                                    testSettings.src.Read(responseBuff, 0, responseBuff.Length);
                                    mResponse = BoardMessage.FromStream(responseBuff);
                                    testSettings.src.Read(responseBuff, 0, 6);
                                    pValv = BitConverter.ToUInt16(responseBuff, 0);
                                    sValv = BitConverter.ToUInt16(responseBuff, 2);
                                    MAddr = BitConverter.ToUInt16(responseBuff, 4);
                                }
                                else
                                {
                                    var msg = formMessage(pReg, pValv, sReg, sValv, flag);
                                    var stream = msg.ToStream();
                                    testSettings.port.Write(stream, 0, stream.Length);
                                }

                                var s1 = DateTime.Now;
                                while (mResponse == null)
                                {
                                    Application.DoEvents();
                                    if ((DateTime.Now - s1).TotalSeconds > 5)
                                    {
                                        Debug.WriteLine(string.Format("Timeout!!"));
                                        break;
                                    }
                                }
                                if (mResponse == null)
                                    throw new Exception();

                                testSettings.processor.Registers.PC = 0xa000;
                                formSim(testSettings.processor, pReg, pValv, sReg, sValv, flag);
                                testSettings.processor.ExecuteOne();
                                CheckResult(mResponse, testSettings.processor, pReg, sReg, testSettings.Errors);
                                callback(StatusFields.Errors, testSettings.Errors.Count.ToString());
                            }
                        }
                    }
                }//pReg
            }//flags
            callback(StatusFields.TestStatus, "Finished");
            callback(StatusFields.Errors, testSettings.Errors.Count.ToString());
        }//PerformTestFlags

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