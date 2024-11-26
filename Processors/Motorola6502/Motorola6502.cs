using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Windows.Forms;
using Processors;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using Preferences;

namespace Motorola6502
{
    public partial class Motorola6502 : IProcessor
    {
        private EventWaitHandle mAbort;
        private Thread mThread;
        private bool threadDone;
        public long CycleCount { get; set; }

        private string mPendingInterrupt = string.Empty;

        public event CycleCountDelegate OnCycleCount;

        public ISystemMemory SystemMemory { get; set; }

        public bool IsRunning { get { return true; } }

        public string ProcessorName { get { return "Motorola6502.Motorola6502"; } }

        public IRegisters Registers { get; set; }

        private RegistersView mRegistersView;
        public IRegistersView RegistersView { get { return mRegistersView; } }

        public Breakpoints Breakpoints { get; private set; }

        private Registers6502 Registers6502 { get { return Registers as Registers6502; } }

        private OpcodeTrace opCodeTrace = new OpcodeTrace();
        public bool IsHalted { get; set; }
        public string ParserName { get; set; }

        public string[] DoubleRegisters() { return null; }

        private Motorola6502ProcessorConfig mMotorola6502ProcessorConfig;
        public PreferencesBase Settings { get { return mMotorola6502ProcessorConfig; }
        }
        public void SaveSettings(PreferencesBase settings)
        {
            PreferencesBase.Save<Motorola6502ProcessorConfig>(settings, Motorola6502ProcessorConfig.Key);
        }
        public IList<IInstructionTest> GetInstructionTests() { return null; }

        public void AddOpCodeOverride(uint opcodeMask, uint opCodeBase, opCodeOverrideEventHandler opCodeDelegate) { }

        public Motorola6502(ISystemMemory systemMemory)
        {
            SystemMemory = systemMemory;
            Registers = new Registers6502();
            Registers6502.SP = 0x40;
            mRegistersView = new RegistersView(this, Registers as Registers6502);
            Breakpoints = new Breakpoints(this);
            mMotorola6502ProcessorConfig = PreferencesBase.Load<Motorola6502ProcessorConfig>(Motorola6502ProcessorConfig.Key);

            mOpcodeFunctions = new ExecuteInstruction[]
            {
/* 00-0f */ Brk,    Ora,    Invalid,    Invalid,    TsbC,       Ora,    Asl,    Invalid,    Php,    Ora,    Asl,    Invalid,    Invalid,    Ora,    Asl,    Invalid,
/* 10-1f */ Bpl,    Ora,    Invalid,    Invalid,    Invalid,    Ora,    Asl,    Invalid,    Clc,    Ora,    Invalid,Invalid,    Invalid,    Ora,    Asl,    Invalid,
/* 20-2f */ Jsr,    And,    Invalid,    Invalid,    Bit,        And,    Rol,    Invalid,    Plp,    And,    Rol,    Invalid,    Bit,        And,    Rol,    Invalid,
/* 30-3f */ Bmi,    And,    Invalid,    Invalid,    Invalid,    And,    Rol,    Invalid,    Sec,    And,    DecaC,  Invalid,    Invalid,    And,    Rol,    Invalid,
/* 40-4f */ Rti,    Eor,    Invalid,    Invalid,    Invalid,    Eor,    Lsr,    Invalid,    Pha,    Eor,    Lsr,    Invalid,    Jmp,        Eor,    Lsr,    Invalid,
/* 50-5f */ Bvc,    Eor,    Invalid,    Invalid,    Invalid,    Eor,    Lsr,    Invalid,    Cli,    Eor,    PhyC,   Invalid,    Invalid,    Eor,    Lsr,    Invalid,
/* 60-6f */ Rts,    Adc,    Invalid,    Invalid,    StzC,       Adc,    Ror,    Invalid,    Pla,    Adc,    Ror,    Invalid,    Jmp,        Adc,    Ror,    Invalid,
/* 70-7f */ Bvs,    Adc,    Invalid,    Invalid,    Invalid,    Adc,    Ror,    Invalid,    Sei,    Adc,    PlyC,   Invalid,    Jmp,        Adc,    Ror,    Invalid,
/* 80-8f */ BraC,   Sta,    Invalid,    Invalid,    Sty,        Sta,    Stx,    Invalid,    Dey,    BitC,   Txa,    Invalid,    Sty,        Sta,    Stx,    Invalid,
/* 90-9f */ Bcc,    Sta,    StaC,       Invalid,    Sty,        Sta,    Stx,    Invalid,    Tya,    Sta,    Txs,    Invalid,    Invalid,    Sta,    Invalid,Invalid,
/* a0-af */ Ldy,    Lda,    Ldx,        Invalid,    Ldy,        Lda,    Ldx,    Invalid,    Tay,    Lda,    Tax,    Invalid,    Ldy,        Lda,    Ldx,    Invalid,
/* b0-bf */ Bcs,    Lda,    LdaC,       Invalid,    Ldy,        Lda,    Ldx,    Invalid,    Clv,    Lda,    Tsx,    Invalid,    Ldy,        Lda,    Ldx,    Invalid,
/* c0-cf */ Cpy,    Cmp,    Invalid,    Invalid,    Cpy,        Cmp,    Dec,    Invalid,    Iny,    Cmp,    Dex,    Invalid,    Cpy,        Cmp,    Dex,    Invalid,
/* d0-df */ Bne,    Cmp,    Invalid,    Invalid,    Invalid,    Cmp,    Dec,    Invalid,    Cld,    Cmp,    PhxC,   Invalid,    Invalid,    Cmp,    Dex,    Invalid,
/* e0-ef */ Cpx,    Sbc,    Invalid,    Invalid,    Cpx,        Sbc,    Inc,    Invalid,    Inx,    Sbc,    Nop,    Invalid,    Cpx,        Sbc,    Inc,    Invalid,
/* f0-ff */ Beq,    Sbc,    Invalid,    Invalid,    Invalid,    Sbc,    Inc,    Invalid,    Sed,    Sbc,    PlxC,   Invalid,    Invalid,    Sbc,    Inc,    Invalid
            };
        }

        //public void Init(Motorola6502ProcessorConfig processorSettings)
        //{
        //}

        public OpcodeTrace OpcodeTrace { get { return opCodeTrace; } }

        protected void addCycles(uint cycles)
        {
            OnCycleCount?.Invoke(cycles);
            CycleCount += cycles;
        }

        public bool ExecuteOne()
        {
            try
            {
                opCodeTrace.Feed(Registers.PC);
                if (!string.IsNullOrEmpty(mPendingInterrupt))
                {
                    OnRaiseInterrupt(mPendingInterrupt);
                    mPendingInterrupt = string.Empty;
                    return false;
                }
                if(InteruptsDisabled)
                {

                }
                //if (mCodeOverrides[Registers.PC] != null)
                //{
                //    mCodeOverrides[Registers.PC](this, new CodeAccessReadEventArgs(Registers.PC));
                //    return false;
                //}
                byte opcode = (byte)SystemMemory.GetMemory(Registers.PC, WordSize.OneByte, true);
                uint cycleCount = mOpcodeFunctions[opcode](opcode);
                addCycles(cycleCount);

                if (IsHalted)
                    Registers.PC--;

                return IsHalted;
            }
            catch (InvalidInstructionException ex)
            {
                Console.WriteLine(ex.ToString());
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return true;
            }
        }

        public void OnRaiseInterrupt(string interrupt)
        {

        }

        public event InvalidInstructionDelegate OnInvalidInstruction;

        public void AddCodeAccess(uint startAddress, codeAccessReadEventHandler readDelegate)
        {
            throw new NotImplementedException();
        }

        public void AddMemoryAccess(uint startAddress, uint endAddress, memoryAccessReadEventHandler readDelegate, memoryAccessWriteEventHandler writeDelegate)
        {
            SystemMemory.AddMemoryOverride(startAddress, endAddress, readDelegate, writeDelegate);
        }

        public void AddPortOverride(byte startport, byte endPort, portAccessReadEventHandler readDelegate, portAccessWriteEventHandler writeDelegate)
        {
            throw new NotImplementedException();
        }

        public bool ExecuteUntilBreakpoint()
        {
            lock (this)
            {
                mAbort = new EventWaitHandle(false, EventResetMode.ManualReset);
                mThread = new Thread(new ParameterizedThreadStart(thread_start));
                threadDone = false;
                mThread.Start(null);
                while (!threadDone)
                    Application.DoEvents();
                mThread.Join();
                mThread = null;
            }
            return false;
        }

        private void thread_start(object param)
        {
            //var executionComplete = param as executionCompleteHandler;
            while (!mAbort.WaitOne(0) && !IsHalted)
            {
                if (this.Breakpoints.CodeHit(this.Registers.PC))
                    break;
                this.ExecuteOne();
            }
            threadDone = true;
        }//thread_start

        public void FireInterupt(string interruptType)
        {
            throw new NotImplementedException();
        }


        public void Reset()
        {
            ushort addr = (ushort)SystemMemory.GetMemory(0xfffc, WordSize.TwoByte, false);
            this.Registers.PC = addr;
        }

        private static Dictionary<byte, bool> callOPCodes = new Dictionary<byte, bool>()
        {
            { 0x20, true }
        };
        private bool IsNextInstructionCall()
        {
            byte opcode = (byte)SystemMemory.GetMemory(this.Registers.PC, WordSize.OneByte, true);
            return callOPCodes.ContainsKey(opcode);
        }

        public bool StepInto()
        {
            return ExecuteOne();
        }

        public bool StepOver()
        {
            bool IsCall = IsNextInstructionCall();
            if (IsCall)
            {
                int opCodeLen = IsCall ? 3 : 1;
                uint addr = (uint)(Registers6502.PC + opCodeLen);
                Breakpoints.Add(addr, true);
                bool halted = ExecuteUntilBreakpoint();
                Breakpoints.Remove(addr);
                return halted;
            }
            else
            {
                return StepInto();
            }

        }

        public void Stop()
        {
            if(mAbort != null)
                mAbort.Set();
        }

        public IBreakpoint CreateBreakpoint(uint address)
        {
            return new Breakpoint(address, false);
        }

        //public IParser CreateParser()
        //{
        //    return null;
        //    //return new Parser6502(this);
        //}

        public IDisassembler CreateDisassembler(int topAddress)
        {
            return new Disassembler.Disassembler();
        }

    }
}