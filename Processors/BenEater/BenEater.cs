using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Processors;
using System.Drawing;
using Preferences;
using System.Threading;

namespace BenEater
{
    public partial class BenEater : IProcessor
    {
        public event InvalidInstructionDelegate OnInvalidInstruction;

        private EventWaitHandle mAbort;
        private Thread mThread;
        private bool threadDone;
        public OpcodeTrace OpcodeTrace { get; set; } = new OpcodeTrace();

        public IRegisters Registers { get; set; }

        public ISystemMemory SystemMemory { get; set; }

        public event CycleCountDelegate OnCycleCount;
        public long CycleCount { get; set; }
        private codeAccessReadEventHandler[] mCodeOverrides;
        public Breakpoints Breakpoints { get; private set; }

        private IRegistersView mIRegistersView;
        public IRegistersView RegistersView { get { return mIRegistersView; } }
        public string[] DoubleRegisters() { return null; }

        public BenEater(ISystemMemory systemMemory)
        {
            SystemMemory = systemMemory;
        }
        public void AddOpCodeOverride(uint opcodeMask, uint opCodeBase, opCodeOverrideEventHandler opCodeDelegate) { }

        private BenEaterProcessorConfig mBenEaterProcessorConfig;
        public PreferencesBase Settings
        {
            get { return mBenEaterProcessorConfig; }
        }
        public void SaveSettings(PreferencesBase settings)
        {
            PreferencesBase.Save<BenEaterProcessorConfig>(settings, BenEaterProcessorConfig.Key);
        }

        public void AddCodeAccess(uint startAddress, codeAccessReadEventHandler readDelegate)
        {
            mCodeOverrides[startAddress] = readDelegate;
        }


        private RegistersBenEater RegistersBenEater {  get { return Registers as RegistersBenEater; } }
        public BenEater()
        {
            Registers = new RegistersBenEater();
            SystemMemory = new SystemMemory();
            mCodeOverrides = new codeAccessReadEventHandler[16];
            mIRegistersView = new BenEaterRegistersView(this, RegistersBenEater);
            mBenEaterProcessorConfig = PreferencesBase.Load<BenEaterProcessorConfig>(BenEaterProcessorConfig.Key);
            Breakpoints = new Breakpoints(this);

            mOpcodeFunctions[0] = Nop;
            mOpcodeFunctions[1] = Lda;
            mOpcodeFunctions[2] = Add;
            mOpcodeFunctions[3] = Sub;
            mOpcodeFunctions[4] = Sta;
            mOpcodeFunctions[5] = Ldi;
            mOpcodeFunctions[6] = Jmp;
            mOpcodeFunctions[7] = Jc;
            mOpcodeFunctions[8] = Nop;
            mOpcodeFunctions[9] = Nop;
            mOpcodeFunctions[10] = Nop;
            mOpcodeFunctions[11] = Nop;
            mOpcodeFunctions[12] = Nop;
            mOpcodeFunctions[13] = Nop;
            mOpcodeFunctions[14] = Out;
            mOpcodeFunctions[15] = Hlt;
        }

        public Panel RegistersViewPanel { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //public Control Main { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        //public WordSize WordSize { get { return WordSize.OneByte; } }
        //public Endian Endian { get { return Endian.Little; } }

        public bool IsHalted { get; set; }

        public void AddPortOverride(byte startport, byte endPort, portAccessReadEventHandler readDelegate, portAccessWriteEventHandler writeDelegate)
        {
            throw new NotImplementedException();
        }

        public IBreakpoint CreateBreakpoint(uint address)
        {
            return new Breakpoint(address, false);
        }

        public IDisassembler CreateDisassembler(int topAddress)
        {
            return new DisassemblerBenEater(this);
        }

        public IParser CreateParser()
        {
            return new Processors.BenEater.Assembler.ParserBenEater(this);
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
            callback?.Invoke(SimRunningState.Running);
            bool halted = ExecuteOne();
            callback?.Invoke(SimRunningState.Stopped);
            return halted;
        }

        public bool StepOver(StatusUpdateDelegate callback)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            if (mAbort != null)
                mAbort.Set();
        }

        public bool ExecuteOne()
        {
            byte opcode = SystemMemory[(ushort)Registers.PC++];
            uint cycleCount = mOpcodeFunctions[(opcode & 0xf0) >> 4](opcode);
            OnCycleCount?.Invoke(cycleCount);
            return IsHalted;
        }

        public bool ExecuteUntilBreakpoint(StatusUpdateDelegate callback)
        {
            callback(SimRunningState.Running);
            mAbort = new EventWaitHandle(false, EventResetMode.ManualReset);
            mThread = new Thread(new ParameterizedThreadStart(thread_start));
            threadDone = false;
            mThread.Start(null);
            while (!threadDone)
                Application.DoEvents();
            mThread.Join();
            mThread = null;
            callback(SimRunningState.Stopped);
            return false;
        }

        private void thread_start(object param)
        {
            //var executionComplete = param as executionCompleteHandler;
            while (!mAbort.WaitOne(0))
            {
                if (this.Breakpoints.CodeHit(this.Registers.PC))
                    break;
                this.ExecuteOne();
            }
            threadDone = true;
        }//thread_start

        public void AddMemoryAccess(uint startAddress, uint endAddress, memoryAccessReadEventHandler readDelegate, memoryAccessWriteEventHandler writeDelegate)
        {

        }
    }
}