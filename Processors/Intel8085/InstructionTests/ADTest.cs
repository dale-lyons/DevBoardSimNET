using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.IO.Ports;
using System.Diagnostics;

using Processors;
using System.Threading;

namespace Intel8085.InstructionTests
{
    /// <summary>
    /// aci, adi
    /// </summary>
    public class ADTest : TestBase
    {
        private byte[] mFlags;
        public ADTest(string name, byte opcode, byte[] flags)
        {
            mFlags = flags;
            if (mFlags == null)
                mFlags = new byte[] { 0 };

            Name = name;
            Opcode = opcode;
            mResponse = new byte[4];
        }

        public override void PerformTestFlags(TestSettings testSettings, statusUpdate callback)
        {
            callback(StatusFields.TestStatus, Name);
            for (int ii = 0; ii < mFlags.Length; ii++)
            {
                callback(StatusFields.CurrentFlag, mFlags[ii].ToString("X2"));
                var buffer = new byte[6];
                buffer[0] = (byte)(buffer.Length - 1);
                buffer[1] = (byte)TestInstructionEnum.ADTest;
                buffer[2] = Opcode;
                buffer[4] = mFlags[ii];             //flags

                for (int AVal = 0; AVal < 256; AVal++)
                {
                    callback(StatusFields.CurrentA, AVal.ToString("X2"));
                    callback(StatusFields.Errors, testSettings.Errors.Count.ToString());

                    buffer[3] = (byte)AVal;                //A Register Value
                    for (int RVal = 0; RVal < 256; RVal++)
                    {
                        if (testSettings.abort.WaitOne(0))
                            break;

                        if (testSettings.src != null)
                        {
                            testSettings.src.Read(mResponse, 0, 3);
                        }
                        else
                        {
                            buffer[5] = (byte)RVal;            //param value
                            if (!SendAndWait(buffer, testSettings.port))
                                return;
                        }

                        byte rflags = mResponse[2];
                        byte ra = mResponse[1];

                        if (testSettings.dst != null)
                            testSettings.dst.Write(mResponse, 0, 3);

                        testSettings.processor.Registers.SetSingleRegister("A", (byte)AVal);
                        testSettings.processor.Registers.SetSingleRegister("Flags", mFlags[ii]);
                        testSettings.processor.Registers.PC = TestAddress;
                        testSettings.processor.SystemMemory.SetMemory(TestAddress, WordSize.OneByte, Opcode, false);
                        testSettings.processor.SystemMemory.SetMemory(TestAddress + 1, WordSize.OneByte, (byte)RVal, false);
                        testSettings.processor.ExecuteOne();

                        byte sima = (byte)testSettings.processor.Registers.GetSingleRegister("A");
                        byte simf = (byte)testSettings.processor.Registers.GetSingleRegister("Flags");
                        CheckResult(ra, rflags, sima, simf, testSettings.Errors, 0, (byte)AVal, (byte)RVal, Opcode);


                    }//for RegVal
                }//for AVal
            }//flags
            callback(StatusFields.TestStatus, "Finished");
            callback(StatusFields.Errors, testSettings.Errors.Count.ToString());
        }//PerformTestFlags
    }
}