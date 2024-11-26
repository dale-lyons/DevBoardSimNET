using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Preferences;

namespace Processors
{
    public delegate uint opCodeOverrideEventHandler(object sender, EventArgs args);
    public delegate void codeAccessReadEventHandler(object sender, CodeAccessReadEventArgs args);
    public delegate void memoryAccessWriteEventHandler(object sender, MemoryAccessWriteEventArgs args);
    public delegate uint memoryAccessReadEventHandler(object sender, MemoryAccessReadEventArgs args);
    public delegate void portAccessWriteEventHandler(object sender, PortAccessWriteEventArgs args);
    public delegate uint portAccessReadEventHandler(object sender, PortAccessReadEventArgs args);
    public delegate void CycleCountDelegate(uint count);

    public class MemoryAccessWriteEventArgs : MemoryAccessReadEventArgs
    {
        public object Value { get; private set; }
        public MemoryAccessWriteEventArgs(uint address, WordSize size, object value)
            : base(address, size)
        {
            Value = value;
        }
    }

    public class MemoryAccessReadEventArgs : EventArgs
    {
        //the address, and size of the read
        public uint Address { get; private set; }
        public WordSize Size { get; private set; }

        public MemoryAccessReadEventArgs(uint address, WordSize size)
        {
            Address = address;
            Size = size;
        }
    }

    public class CodeAccessReadEventArgs : EventArgs
    {
        public uint Address { get; private set; }

        public CodeAccessReadEventArgs(uint address)
        {
            Address = address;
        }
    }

    public class PortAccessReadEventArgs : EventArgs
    {
        //the address, and size of the read
        public ushort Port { get; private set; }
        public PortAccessReadEventArgs(ushort port)
        {
            Port = port;
        }
    }
    public class PortAccessWriteEventArgs : EventArgs
    {
        //the address, and size of the read
        public ushort Port { get; private set; }
        public byte Data { get; private set; }
        public PortAccessWriteEventArgs(ushort port, byte data)
        {
            Port = port;
            Data = data;
        }
    }

    public enum SimRunningState
    {
        Idle,
        Running,
        Stopped
    }

    public delegate void StatusUpdateDelegate(SimRunningState simRunningState);
    public delegate void InvalidInstructionDelegate(uint opcode, uint pc);

    public interface IProcessor
    {
        PreferencesBase Settings { get; }
        void SaveSettings(PreferencesBase settings);
        string ParserName { get; }
        IBreakpoint CreateBreakpoint(uint address);
        ISystemMemory SystemMemory { get; set; }
        OpcodeTrace OpcodeTrace { get; }
        //IParser CreateParser();

        IDisassembler CreateDisassembler(int topAddress);

        IRegisters Registers { get; set; }

        Breakpoints Breakpoints { get; }

        bool ExecuteOne();
        bool ExecuteUntilBreakpoint();
        bool StepInto();
        bool StepOver();
        void Stop();
        void Reset();

        void FireInterupt(string interruptType);

        void AddPortOverride(byte startport, byte endPort, portAccessReadEventHandler readDelegate, portAccessWriteEventHandler writeDelegate);
        void AddMemoryAccess(uint startAddress, uint endAddress, memoryAccessReadEventHandler readDelegate, memoryAccessWriteEventHandler writeDelegate);
        void AddCodeAccess(uint startAddress, codeAccessReadEventHandler readDelegate);
        void AddOpCodeOverride(uint opcodeMask, uint opCodeBase, opCodeOverrideEventHandler opCodeDelegate);

        bool IsHalted { get; }
        bool IsRunning { get; }

        event CycleCountDelegate OnCycleCount;
        event InvalidInstructionDelegate OnInvalidInstruction;
        IRegistersView RegistersView { get; }
        long CycleCount { get; set; }
        string[] DoubleRegisters();

        IList<IInstructionTest> GetInstructionTests();
        string ProcessorName { get; }
    }

    public enum WordSize
    {
        OneByte = 1,
        TwoByte = 2,
        FourByte = 4
    }

    public enum Endian
    {
        Little,
        Big
    }
}