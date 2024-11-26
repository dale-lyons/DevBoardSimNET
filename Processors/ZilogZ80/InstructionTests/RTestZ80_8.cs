using Intel8085.InstructionTests;
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

namespace ZilogZ80.InstructionTests
{
    public abstract class RTestZ80_8 : RTestZ80
    {
        public RTestZ80_8(string name, InstructionCategoryEnum category, byte prebyte, byte opcode, byte[] flags)
            : base(name, InstructionDataSizeEnum.Single, category, prebyte, opcode, flags) { }

        protected byte pValv;
        protected byte sValv;

        public abstract BoardMessage formMessage(byte pReg, byte pVal, byte sReg, byte sVal, byte flags);
        public abstract void formSim(IProcessor processor, byte pReg, byte pVal, byte sReg, byte sVal, byte flag);

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

                                pValv = (byte)pVals[pVal];
                                sValv = (byte)sVals[pVal];
                                MAddr = generateMAddr();

                                mResponse = null;
                                if (testSettings.src != null)
                                {
                                    testSettings.src.Read(responseBuff, 0, responseBuff.Length);
                                    mResponse = BoardMessage.FromStream(responseBuff);
                                    testSettings.src.Read(responseBuff, 0, 4);
                                    pValv = responseBuff[0];
                                    sValv = responseBuff[1];
                                    MAddr = BitConverter.ToUInt16(responseBuff, 2);
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
                                }//while
                                if (mResponse == null)
                                    throw new Exception();

                                testSettings.processor.Registers.PC = 0xa000;
                                formSim(testSettings.processor, pReg, pValv, sReg, sValv, flag);
                                testSettings.processor.ExecuteOne();
                                CheckResult(mResponse, testSettings.processor, pReg, sReg, testSettings.Errors);
                                callback(StatusFields.Errors, testSettings.Errors.Count.ToString());
                            }//for sVal
                        }//for sReg
                    }//for pVal
                }//pReg
            }//for flags
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
                testSettings.dst.WriteByte(pValv);
                testSettings.dst.WriteByte(sValv);
                testSettings.dst.Write(BitConverter.GetBytes(MAddr), 0, 2);
            }
            mResponse = BoardMessage.FromStream(cmdParams);
        }
    }
}