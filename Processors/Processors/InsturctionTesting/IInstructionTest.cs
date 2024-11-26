using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.IO.Ports;
using Processors;
using System.Threading;

namespace Processors
{
    public class TestSettings
    {
        public SerialPort port { get; set; }
        public FileStream src { get; set; }
        public FileStream dst { get; set; }
        public IProcessor processor { get; set; }
        public List<InstructionTestError> Errors { get; set; }
        public string boardID { get; set; }
        //public statusUpdate callback { get; set; }
        public EventWaitHandle abort { get; set; }
    }

    public enum TestInstructionEnum
    {
        RpTest_8085 = 0,
        RpTest_Z80 = 1
    }

    public class InstructionTestError
    {
        public byte pReg;
        public byte sReg;
        public byte pVal;
        public byte sVal;
        public byte Flags;
        public byte Opcode;
        public string TestName;
    }

    public enum InstructionCategoryEnum
    {
        DataTransfer,
        Arithmetic,
        Logical,
        Branching,
        Control,
        Z80
    }

    public enum StatusFields
    {
        TestStatus,
        Flag,
        PReg,
        PVal,
        SReg,
        SVal,
        Errors
    }

    public delegate void statusUpdate(StatusFields field, string val);

    public interface IInstructionTest
    {
        string Name { get; }
        void PerformTestFlags(TestSettings testSettings, statusUpdate callback);
        void OnResponse(TestSettings testSettings, byte[] cmdParams, byte[] cmdData);
        InstructionCategoryEnum Category { get; }
        string ToString();
    }
}