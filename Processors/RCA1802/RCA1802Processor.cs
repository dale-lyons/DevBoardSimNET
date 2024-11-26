using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Processors;

namespace RCA1802
{
    public class RCA1802Processor : IProcessor
    {
        public event InvalidInstructionDelegate OnInvalidInstruction;

        public event CycleCountDelegate OnCycleCount;

        public ISystemMemory SystemMemory { get; set; }

        public System.Windows.Forms.Panel RegistersViewPanel => throw new NotImplementedException();

        public IRegisters Registers { get; set; }

        public Breakpoints Breakpoints { get; set; }

        public bool IsHalted { get; set; }

        public void AddMemoryAccess(uint startAddress, uint endAddress, memoryAccessReadEventHandler readDelegate, memoryAccessWriteEventHandler writeDelegate)
        {

        }

        public void AddPortOverride(byte startport, byte endPort, portAccessReadEventHandler readDelegate, portAccessWriteEventHandler writeDelegate)
        {
            throw new NotImplementedException();
        }

        public IBreakpoint CreateBreakpoint(uint address)
        {
            throw new NotImplementedException();
        }

        public IDisassembler CreateDisassembler()
        {
            throw new NotImplementedException();
        }

        public IParser CreateParser()
        {
            throw new NotImplementedException();
        }

        public bool ExecuteOne()
        {
            OnCycleCount?.Invoke(0);
            return false;
        }
        public long CycleCount { get; set; }

        public bool ExecuteUntilBreakpoint(StatusUpdateDelegate callback)
        {
            throw new NotImplementedException();
        }

        public void FireInterupt(string interruptType)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public bool StepInto(StatusUpdateDelegate callback)
        {
            throw new NotImplementedException();
        }

        public bool StepOver(StatusUpdateDelegate callback)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
