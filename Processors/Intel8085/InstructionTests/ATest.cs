using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using System.Diagnostics;

using Processors;

namespace Intel8085.InstructionTests
{
    public class ATest : TestBase
    {
        private byte[] mFlags;
        public ATest(string name, byte opcode, byte[] flags)
        {
            Name = name;
            Opcode = opcode;
            mFlags = flags;
            if (mFlags == null)
                mFlags = new byte[] { 0 };
        }

        public override void PerformTestFlags(TestSettings testSettings, statusUpdate callback)
        {
            var buffer = new byte[5];
            buffer[0] = (byte)(buffer.Length - 1);
            buffer[1] = (byte)TestInstructionEnum.ATest;
            buffer[2] = Opcode;

            foreach (var flag in mFlags)
            {
                callback(StatusFields.CurrentReg, flag.ToString("X2"));
                buffer[4] = flag;
                for (int AVal = 0x0; AVal < 256; AVal++)
                {
                    callback(StatusFields.CurrentA, AVal.ToString("X2"));
                    callback(StatusFields.Errors, testSettings.Errors.Count.ToString());
                    buffer[3] = (byte)AVal;

                    if (testSettings.abort.WaitOne(0))
                        break;

                    if (testSettings.src != null)
                    {
                        testSettings.src.Read(mResponse, 0, 3);
                    }
                    else
                    {
                        if (!SendAndWait(buffer, testSettings.port))
                            return;
                    }
                    if (testSettings.dst != null)
                        testSettings.dst.Write(mResponse, 0, 3);

                    byte rflags = mResponse[2];
                    byte ra = mResponse[1];

                    testSettings.processor.Registers.SetSingleRegister("A", (byte)AVal);
                    testSettings.processor.Registers.SetSingleRegister("Flags", flag);
                    testSettings.processor.Registers.PC = TestAddress;
                    testSettings.processor.SystemMemory.SetMemory(TestAddress, WordSize.OneByte, Opcode, false);
                    testSettings.processor.ExecuteOne();

                    byte sima = (byte)testSettings.processor.Registers.GetSingleRegister("A");
                    byte simf = (byte)testSettings.processor.Registers.GetSingleRegister("Flags");
                    CheckResult(ra, rflags, sima, simf, testSettings.Errors, flag, (byte)AVal, (byte)0, Opcode);
                }//AVal
            }//flag
            callback(StatusFields.TestStatus, "Finished");
            callback(StatusFields.Errors, testSettings.Errors.Count.ToString());
        }
    }
}