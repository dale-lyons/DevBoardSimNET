using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Preferences;
using Processors;
using ARM7.Disassembler;

namespace ARM7
{
    public partial class ARM7 : IProcessor
    {
        private EventWaitHandle mAbort;
        private Thread mThread;
        private bool threadDone;
        public event InvalidInstructionDelegate OnInvalidInstruction;

        public bool IsRunning { get { return true; } }

        public ISystemMemory SystemMemory { get; set; }
        public GeneralPurposeRegisters GPR { get; private set; }
        //public VFP.FloatingPointRegisters FPR { get; private set; }
        public VFP.FloatingPointProcessor FPP { get; private set; }
        public IRegisters Registers { get; set; }
        public Breakpoints Breakpoints { get; private set; }

        public event CycleCountDelegate OnCycleCount;

        public string ProcessorName { get { return "ARM7.ARM7"; } }

        //this flag indicates if the cpu exception vectors are located in
        //"high" or "low" memory.
        public bool HighVectors { get; set; }

        //this ARMExceptions is set when the engine wants to raise an exception.
        //Each bit in the uint represents an exception.
        //This is kept as a single uint for effeciancy.
        //exception numbers are defined in:ARMExceptions
        protected ARMExceptions mRequestedException;
        
        public ulong CycleCount { get; set; }

        public CPSR CPSR { get { return GPR.CPSR; } }
        public IList<IInstructionTest> GetInstructionTests() { return null; }

        public string[] DoubleRegisters() { return null; }

        public Preferences.PreferencesBase Settings
        {
            get
            {
                return mARM7ProcessorConfig;
            }
        }
        public void SaveSettings(PreferencesBase settings) { }
        public string ParserName { get { return mARM7ProcessorConfig.ActiveParser; } }


        private OpcodeTrace opCodeTrace = new OpcodeTrace();
        public IRegistersView RegistersView { get { return new Controls.RegistersView(this); } }

        private List<Tuple<uint, uint, opCodeOverrideEventHandler>> mOpcodeOverrides;
        public void AddOpCodeOverride(uint opcodeMask, uint opcodeBase, opCodeOverrideEventHandler opCodeDelegate)
        {
            var t = new Tuple<uint, uint, opCodeOverrideEventHandler>(opcodeMask, opcodeBase, opCodeDelegate);
            mOpcodeOverrides.Add(t);
        }

        private ARM7ProcessorConfig mARM7ProcessorConfig;
        public ARM7(ISystemMemory systemMemory)
        {
            SystemMemory = systemMemory;
            mARM7ProcessorConfig = Preferences.PreferencesBase.Load<ARM7ProcessorConfig>(ARM7ProcessorConfig.Key);
            Registers = GPR = new GeneralPurposeRegisters();
            FPP = new VFP.FloatingPointProcessor(this);
            Breakpoints = new Breakpoints(this);
            mOpcodeOverrides = new List<Tuple<uint, uint, opCodeOverrideEventHandler>>();
        }
        public OpcodeTrace OpcodeTrace { get { return opCodeTrace; } }

        public bool IsHalted { get; set; }

        public void AddCodeAccess(uint startAddress, codeAccessReadEventHandler readDelegate)
        {
            throw new NotImplementedException();
        }

        public void AddPortOverride(byte startport, byte endPort, portAccessReadEventHandler readDelegate, portAccessWriteEventHandler writeDelegate)
        {
            throw new NotImplementedException();
        }

        public IBreakpoint CreateBreakpoint(uint address)
        {
            throw new NotImplementedException();
        }

        public IDisassembler CreateDisassembler(int topAddress)
        {
            return new ARMDisassembler();
        }

        //public IParser CreateParser()
        //{
        //    return Parsers.Parsers.GetParser(ParserName, this);
        //}

        public bool ExecuteOne()
        {
            WordSize opcodeSize = CPSR.tf ? WordSize.TwoByte : WordSize.FourByte;
            uint opcode = SystemMemory.GetMemory(Registers.PC, opcodeSize);

            //increment the PC by the current opcode size
            GPR.PC += (uint)opcodeSize;

            uint cycleCount;
            // check if this instruction has been overridden
            foreach (var t in mOpcodeOverrides)
            {
                if ((opcode & t.Item1) == t.Item2)
                {
                    cycleCount = t.Item3(this, new EventArgs());
                    OnCycleCount?.Invoke(cycleCount);
                    return false;
                }
            }

            if (CPSR.tf)
                cycleCount = ExecuteThumbInstruction(opcode);
            else
                cycleCount = ExecuteARMInstruction(opcode);

            SetExceptionState(mRequestedException);
            mRequestedException = ARMExceptions.None;
            OnCycleCount?.Invoke(cycleCount);
            return false;
        }

        public bool ExecuteUntilBreakpoint()
        {
            mAbort = new EventWaitHandle(false, EventResetMode.ManualReset);
            mThread = new Thread(new ThreadStart(thread_start));
            threadDone = false;
            mThread.Start();
            while (!threadDone)
                Application.DoEvents();
            mThread.Join();
            mThread = null;
            return false;
        }

        private void thread_start()
        {
            while (!mAbort.WaitOne(0))
            {
                this.ExecuteOne();
                if (this.Breakpoints.CodeHit(this.Registers.PC))
                    break;
            }
            var bp = Breakpoints.Breakpoint(this.Registers.PC);
            if (bp != null)
                if (bp.Temporary)
                    this.Breakpoints.Remove(this.Registers.PC);

            threadDone = true;
        }//thread_start

        public void FireInterupt(string interruptType)
        {
            switch (interruptType)
            {
                case "SWI":
                    mRequestedException = ARMExceptions.SoftwareInterrupt;
                    break;
                case "IRQ":
                    mRequestedException = ARMExceptions.IRQ;
                    break;
                case "FIQ":
                    mRequestedException = ARMExceptions.FIQ;
                    break;
            }
        }

        public void Reset()
        {
            mRequestedException = ARMExceptions.Reset;
        }

        public bool StepInto()
        {
            ExecuteOne();
            return false;
        }

        public bool StepOver()
        {
            bool IsBL = IsNextInstructionBL();
            if (!IsBL)
                StepInto();
            else
            {
                Breakpoints.Add(GPR.PC + 4, true);
                ExecuteUntilBreakpoint();
            }
            return false;
        }

        public void Stop()
        {
            if (mAbort != null)
                mAbort.Set();
            Application.DoEvents();
        }

        private bool IsNextInstructionBL()
        {
            uint opcode = SystemMemory.GetMemory(GPR.PC, WordSize.FourByte, false);
            return ((opcode & 0x0f000000) == 0x0b000000 || (opcode & 0x0ff000f0) == 0x01200030);
        }

        public void AddMemoryAccess(uint startAddress, uint endAddress, memoryAccessReadEventHandler readDelegate, memoryAccessWriteEventHandler writeDelegate)
        {
        }

        /// <summary>
        /// This enum specifies the exception types of the ARM processor.
        /// Each is a separate bit so we can represent more than one exception
        /// being raised during a single instruction execution.
        /// </summary>
        [Flags]
        public enum ARMExceptions
        {
            None = 0x00,
            Reset = 0x01,
            UndefinedInstruction = 0x02,
            SoftwareInterrupt = 0x04,
            PreFetchAbort = 0x08,
            DataAbort = 0x10,
            IRQ = 0x20,
            FIQ = 0x40
        }

        //Switches the CPU into an exception state. Each of the 7 exceptions that can be
        //raised are unique. The CPU is switched into a new cpu mode, and interrupts
        //may be disabled. The PC is set to the exception vector.
        private void SetExceptionState(ARMExceptions armException)
        {
            uint newPC;
            switch (armException)
            {
                case ARMExceptions.None: return;
                case ARMExceptions.Reset:
                    CPSR.SwitchCPUMode(CPSR.CPUModeEnum.Supervisor);
                    CPSR.IRQDisable = true;
                    CPSR.FIQDisable = true;
                    newPC = 0x00000000;
                    break;

                case ARMExceptions.UndefinedInstruction:
                    CPSR.SwitchCPUMode(CPSR.CPUModeEnum.Undefined);
                    CPSR.IRQDisable = true;
                    newPC = 0x00000004;
                    break;

                case ARMExceptions.SoftwareInterrupt:
                    CPSR.SwitchCPUMode(CPSR.CPUModeEnum.Supervisor);
                    CPSR.IRQDisable = true;
                    newPC = 0x00000008;
                    break;

                case ARMExceptions.PreFetchAbort:
                    CPSR.SwitchCPUMode(CPSR.CPUModeEnum.Abort);
                    CPSR.IRQDisable = true;
                    newPC = 0x0000000c;
                    break;

                case ARMExceptions.DataAbort:
                    CPSR.SwitchCPUMode(CPSR.CPUModeEnum.Abort);
                    CPSR.IRQDisable = true;
                    newPC = 0x00000010;
                    break;

                case ARMExceptions.IRQ:
                    CPSR.SwitchCPUMode(CPSR.CPUModeEnum.IRQ);
                    CPSR.IRQDisable = true;
                    newPC = 0x00000018;
                    break;

                case ARMExceptions.FIQ:
                    CPSR.SwitchCPUMode(CPSR.CPUModeEnum.FIQ);
                    CPSR.IRQDisable = true;
                    CPSR.FIQDisable = true;
                    newPC = 0x0000001c;
                    break;
                default: throw new Exception("bad exception type");
            }//switch

            //save the PC into the new modes banked lr
            GPR.LR = GPR.PC;

            //if the "high" vectors mode is on, then set the top 16bits to a 1 of the new pc.
            //GPR.PC = (Settings as ProcessorSettings).HighVectors ? (newPC | 0xffff0000) : newPC;
            GPR.PC = newPC;

            //always process the exception in ARM mode. When the exception returns and reloads
            //the cpsr from the scpsr, thumb mode will be reenabled if that was the starting mode.
            CPSR.tf = false;
        }//SetExceptionState
    }//class ARM7
}