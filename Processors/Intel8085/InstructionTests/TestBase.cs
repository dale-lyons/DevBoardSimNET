using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Windows.Forms;
using System.IO.Ports;
using System.Diagnostics;
using System.Threading;
using Processors;

namespace Intel8085.InstructionTests
{
    public enum TestInstructionEnum
    {
        OneRpTest = 0,
        ARTest = 1,
        ADTest = 2,
        ATest = 3,
        RTest = 4,
    }


    public abstract class TestBase : IInstructionTest
    {
        protected static readonly byte[] nullFlags = new byte[] { 0 };
        protected bool done;

        protected static ushort[] DoubleInr1 = new ushort[32];
        protected static ushort[] DoubleInr2 = new ushort[32];

        protected BoardMessage mResponse;

        public InstructionCategoryEnum Category { get; set; }
        public string Name { get; set; }

        public const ushort TestAddress = 0xa000;
        public byte Opcode { get; set; }
        private byte[] mFlags;

        public TestBase(string name, InstructionCategoryEnum category, byte opcode, byte[] flags)
        {
            Name = name;
            Category = category;
            Opcode = opcode;
            mFlags = flags; ;

            DoubleInr1[0] = 0;
            DoubleInr2[1] = 0;

            byte[] buffer = new byte[2];
            Random rnd = new Random();
            for (int ii = 1; ii < DoubleInr1.Length - 1; ii++)
            {
                rnd.NextBytes(buffer);
                DoubleInr1[ii] = BitConverter.ToUInt16(buffer, 0);
                rnd.NextBytes(buffer);
                DoubleInr2[ii] = BitConverter.ToUInt16(buffer, 0);
            }
            DoubleInr1[DoubleInr1.Length - 1] = 0xffff;
            DoubleInr2[DoubleInr1.Length - 1] = 0xffff;
        }

        public override string ToString()
        {
            return Name;
        }
        public void OnResponse(byte[] cmdParams, byte[] cmdData)
        {
            mResponse = BoardMessage.FromStream(cmdParams);
            done = true;
        }
        public abstract byte[] useFlags { get; }
        public abstract DoubleRegisterEnums[] useDblRegP { get; }
        public abstract DoubleRegisterEnums[] useDblRegS { get; }
        public abstract SingleRegisterEnums[] useSngRegP { get; }
        public abstract SingleRegisterEnums[] useSngRegS { get; }

        private bool SendAndWait(byte[] buffer, SerialPort port)
        {
            while (true)
            {
                var s1 = DateTime.Now;
                done = false;
                port.Write(buffer, 0, buffer.Length);
                while (!done)
                {
                    Application.DoEvents();
                    if ((DateTime.Now - s1).TotalSeconds > 10)
                    {
                        Debug.WriteLine(string.Format("Timeout!!"));
                        break;
                    }
                }
                return done;
            }
        }

        public abstract DoubleRegisterEnums[] useDblReg { get; }

        public void PerformTestFlags(TestSettings testSettings, statusUpdate callback)
        {
            BoardMessage msg = new BoardMessage();
            //msg.tst = (byte)TestInstructionEnum.ARTest;
            callback(StatusFields.TestStatus, Name);

            foreach (var flag in useFlags)
            {
                callback(StatusFields.CurrentFlag, flag.ToString("X2"));
                foreach(var dblRegP in useDblRegP)
                {//b,d,h,sp
                    foreach (var dblRegS in useDblRegS)
                    {//b,d,h,sp




                        msg.Opcode = (byte)((reg << 4) | 0x09);
                    callback(StatusFields.CurrentReg, reg.ToString("X2"));

                    for (byte AVal = 0; AVal <= byte.MaxValue; AVal++)
                    {
                        callback(StatusFields.CurrentA, AVal.ToString("X2"));
                        msg.a = AVal;
                        for (byte RVal = 0; RVal <= byte.MaxValue; RVal++)
                        {
                            callback(StatusFields.CurrentR, RVal.ToString("X2"));
                            msg.setSinglereg(reg, RVal);
                            if (testSettings.abort.WaitOne(0))
                                return;

                            var msgB = msg.ToStream();
                            if (!SendAndWait(msgB, testSettings.port))
                                return;

                            byte brdA = mResponse.a;
                            byte brdF = mResponse.flags;

                            //check result
                            testSettings.processor.Registers.SetSingleRegister((byte)SingleRegisterEnums.a, AVal);
                            testSettings.processor.Registers.SetSingleRegister(reg, RVal);
                            testSettings.processor.Registers.PC = TestAddress;
                            testSettings.processor.SystemMemory.SetMemory(TestAddress, WordSize.OneByte, msg.Opcode, false);
                            testSettings.processor.ExecuteOne();
                            byte simA = (byte)testSettings.processor.Registers.GetSingleRegister((byte)SingleRegisterEnums.a);
                            byte simF = (byte)testSettings.processor.Registers.GetSingleRegister("Flags");

                            if ((simA != brdA) || (simF != brdF))
                            {
                                return;
                            }
                        }//for RVal
                    }//for AVal
                }//for reg
            }//flag
            callback(StatusFields.TestStatus, "Finished");
            callback(StatusFields.Errors, testSettings.Errors.Count.ToString());
        }
    }
}