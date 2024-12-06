using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Processors;
using System.Drawing;
using Preferences;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.ConstrainedExecution;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace M68HC11
{
    public partial class M68HC11 : IProcessor
    {
        private byte preByte;
        public event InvalidInstructionDelegate OnInvalidInstruction;

        private EventWaitHandle mAbort;
        private Thread mThread;
        private bool threadDone;
        public OpcodeTrace OpcodeTrace { get; set; } = new OpcodeTrace();

        public bool IsRunning { get { return true; } }

        public string ProcessorName { get { return "M68HC11.M68HC11"; } }

        public IRegisters Registers { get; set; }

        public ISystemMemory SystemMemory { get; set; }

        public event CycleCountDelegate OnCycleCount;
        public ulong CycleCount { get; set; }
        public Breakpoints Breakpoints { get; private set; }

        private IRegistersView mIRegistersView;
        public IRegistersView RegistersView { get { return mIRegistersView; } }
        public void AddOpCodeOverride(uint opcodeMask, uint opCodeBase, opCodeOverrideEventHandler opCodeDelegate) { }

        private M68HC11ProcessorConfig mM68HC11ProcessorConfig;
        public string[] DoubleRegisters() { return null; }
        public IList<IInstructionTest> GetInstructionTests() { return null; }
        public string ParserName { get; set; }

        public PreferencesBase Settings
        {
            get { return mM68HC11ProcessorConfig; }
        }
        public virtual void SaveSettings(PreferencesBase settings)
        {
            PreferencesBase.Save<M68HC11ProcessorConfig>(settings, M68HC11ProcessorConfig.Key);
        }

        public void AddCodeAccess(uint startAddress, codeAccessReadEventHandler readDelegate)
        {
            //mCodeOverrides[startAddress] = readDelegate;
        }

        private RegistersM68HC11 RegistersM68HC11 { get { return Registers as RegistersM68HC11; } }
        public M68HC11(ISystemMemory systemMemory)
        {
            SystemMemory = systemMemory;
            Registers = new RegistersM68HC11();
            mIRegistersView = new M68HC11RegistersView(this, RegistersM68HC11);
            mM68HC11ProcessorConfig = PreferencesBase.Load<M68HC11ProcessorConfig>(M68HC11ProcessorConfig.Key);
            Breakpoints = new Breakpoints(this);
            if(mM68HC11ProcessorConfig.StoponInvalid)
                SystemMemory.OnInvalidMemoryAccess += SystemMemory_OnInvalidMemoryAccess;
            mOpcodeFunctions[0x00] = Test;
            mOpcodeFunctions[0x01] = Nop;
            mOpcodeFunctions[0x02] = Idiv;
            mOpcodeFunctions[0x03] = Fdiv;
            mOpcodeFunctions[0x04] = LsrD;
            mOpcodeFunctions[0x05] = AslD;
            mOpcodeFunctions[0x06] = Tap;
            mOpcodeFunctions[0x07] = Tpa;
            mOpcodeFunctions[0x08] = Inx;
            mOpcodeFunctions[0x09] = Dex;
            mOpcodeFunctions[0x0a] = Clv;
            mOpcodeFunctions[0x0b] = Sev;
            mOpcodeFunctions[0x0c] = Clc;
            mOpcodeFunctions[0x0d] = Sec;
            mOpcodeFunctions[0x0e] = Cli;
            mOpcodeFunctions[0x0f] = Sei;
            mOpcodeFunctions[0x10] = Sba;
            mOpcodeFunctions[0x11] = Cba;
            mOpcodeFunctions[0x12] = Brset;
            mOpcodeFunctions[0x13] = Brclr;
            mOpcodeFunctions[0x14] = Bset;
            mOpcodeFunctions[0x15] = Bclr;
            mOpcodeFunctions[0x16] = Tab;
            mOpcodeFunctions[0x17] = Tpa;
            mOpcodeFunctions[0x18] = Invalid;
            mOpcodeFunctions[0x19] = Daa;
            mOpcodeFunctions[0x1a] = Invalid;
            mOpcodeFunctions[0x1b] = Aba;
            mOpcodeFunctions[0x1c] = Bset;
            mOpcodeFunctions[0x1d] = Bclr;
            mOpcodeFunctions[0x1e] = Brset;
            mOpcodeFunctions[0x1f] = Brclr;
            mOpcodeFunctions[0x20] = Bra;
            mOpcodeFunctions[0x21] = Brn;
            mOpcodeFunctions[0x22] = Bhi;
            mOpcodeFunctions[0x23] = Bls;
            mOpcodeFunctions[0x24] = Bcc;
            mOpcodeFunctions[0x25] = Bcs;
            mOpcodeFunctions[0x26] = Bne;
            mOpcodeFunctions[0x27] = Beq;
            mOpcodeFunctions[0x28] = Bvc;
            mOpcodeFunctions[0x29] = Bvs;
            mOpcodeFunctions[0x2a] = Bpl;
            mOpcodeFunctions[0x2b] = Bmi;
            mOpcodeFunctions[0x2c] = Bge;
            mOpcodeFunctions[0x2d] = Blt;
            mOpcodeFunctions[0x2e] = Bgt;
            mOpcodeFunctions[0x2f] = Ble;
            mOpcodeFunctions[0x30] = Tsx;
            mOpcodeFunctions[0x31] = Ins;
            mOpcodeFunctions[0x32] = Pula;
            mOpcodeFunctions[0x33] = Pulb;
            mOpcodeFunctions[0x34] = Des;
            mOpcodeFunctions[0x35] = Txs;
            mOpcodeFunctions[0x36] = Psha;
            mOpcodeFunctions[0x37] = Pshb;
            mOpcodeFunctions[0x38] = Pulx;
            mOpcodeFunctions[0x39] = Rts;
            mOpcodeFunctions[0x3a] = Abx;
            mOpcodeFunctions[0x3b] = Rti;
            mOpcodeFunctions[0x3c] = Pshx;
            mOpcodeFunctions[0x3d] = Mul;
            mOpcodeFunctions[0x3e] = Wai;
            mOpcodeFunctions[0x3f] = Swi;
            mOpcodeFunctions[0x40] = NegA;
            mOpcodeFunctions[0x41] = Invalid;
            mOpcodeFunctions[0x42] = Invalid;
            mOpcodeFunctions[0x43] = ComA;
            mOpcodeFunctions[0x44] = LsrA;
            mOpcodeFunctions[0x45] = Invalid;
            mOpcodeFunctions[0x46] = RorA;
            mOpcodeFunctions[0x47] = AsrA;
            mOpcodeFunctions[0x48] = AslA;
            mOpcodeFunctions[0x49] = Rola;
            mOpcodeFunctions[0x4a] = DecA;
            mOpcodeFunctions[0x4b] = Invalid;
            mOpcodeFunctions[0x4c] = IncA;
            mOpcodeFunctions[0x4d] = Tsta;
            mOpcodeFunctions[0x4e] = Invalid;
            mOpcodeFunctions[0x4f] = ClrA;
            mOpcodeFunctions[0x50] = NegB;
            mOpcodeFunctions[0x51] = Invalid;
            mOpcodeFunctions[0x52] = Invalid;
            mOpcodeFunctions[0x53] = ComB;
            mOpcodeFunctions[0x54] = LsrA;
            mOpcodeFunctions[0x55] = Invalid;
            mOpcodeFunctions[0x56] = RorB;
            mOpcodeFunctions[0x57] = AsrB;
            mOpcodeFunctions[0x58] = AslB;
            mOpcodeFunctions[0x59] = Rolb;
            mOpcodeFunctions[0x5a] = DecB;
            mOpcodeFunctions[0x5b] = Invalid;
            mOpcodeFunctions[0x5c] = IncB;
            mOpcodeFunctions[0x5d] = Tstb;
            mOpcodeFunctions[0x5e] = Invalid;
            mOpcodeFunctions[0x5f] = ClrB;
            mOpcodeFunctions[0x60] = Neg;
            mOpcodeFunctions[0x61] = Invalid;
            mOpcodeFunctions[0x62] = Invalid;
            mOpcodeFunctions[0x63] = Com;
            mOpcodeFunctions[0x64] = Lsr;
            mOpcodeFunctions[0x65] = Invalid;
            mOpcodeFunctions[0x66] = Ror;
            mOpcodeFunctions[0x67] = Asr;
            mOpcodeFunctions[0x68] = Asl;
            mOpcodeFunctions[0x69] = Rol;
            mOpcodeFunctions[0x6a] = Dec;
            mOpcodeFunctions[0x6b] = Invalid;
            mOpcodeFunctions[0x6c] = Inc;
            mOpcodeFunctions[0x6d] = Tst;
            mOpcodeFunctions[0x6e] = Jmp;
            mOpcodeFunctions[0x6f] = Clr;
            mOpcodeFunctions[0x70] = Neg;
            mOpcodeFunctions[0x71] = Invalid;
            mOpcodeFunctions[0x72] = Invalid;
            mOpcodeFunctions[0x73] = Com;
            mOpcodeFunctions[0x74] = Lsr;
            mOpcodeFunctions[0x75] = Invalid;
            mOpcodeFunctions[0x76] = Ror;
            mOpcodeFunctions[0x77] = Asr;
            mOpcodeFunctions[0x78] = Asl;
            mOpcodeFunctions[0x79] = Rol;
            mOpcodeFunctions[0x7a] = Dec;
            mOpcodeFunctions[0x7b] = Invalid;
            mOpcodeFunctions[0x7c] = Inc;
            mOpcodeFunctions[0x7d] = Tst;
            mOpcodeFunctions[0x7e] = Jmp;
            mOpcodeFunctions[0x7f] = Clr;
            mOpcodeFunctions[0x80] = Suba;
            mOpcodeFunctions[0x81] = Cmpa;
            mOpcodeFunctions[0x82] = Sbca;
            mOpcodeFunctions[0x83] = Cpd;
            mOpcodeFunctions[0x84] = Anda;
            mOpcodeFunctions[0x85] = BitA;
            mOpcodeFunctions[0x86] = Ldaa;
            mOpcodeFunctions[0x87] = Invalid;
            mOpcodeFunctions[0x88] = EorA;
            mOpcodeFunctions[0x89] = Adca;
            mOpcodeFunctions[0x8a] = Oraa;
            mOpcodeFunctions[0x8b] = Adda;
            mOpcodeFunctions[0x8c] = Cpx;
            mOpcodeFunctions[0x8d] = Bsr;
            mOpcodeFunctions[0x8e] = Lds;
            mOpcodeFunctions[0x8f] = XGDX;
            mOpcodeFunctions[0x90] = Suba;
            mOpcodeFunctions[0x91] = Cmpa;
            mOpcodeFunctions[0x92] = Sbca;
            mOpcodeFunctions[0x93] = Cpd;
            mOpcodeFunctions[0x94] = Anda;
            mOpcodeFunctions[0x95] = BitA;
            mOpcodeFunctions[0x96] = Ldaa;
            mOpcodeFunctions[0x97] = Staa;
            mOpcodeFunctions[0x98] = EorA;
            mOpcodeFunctions[0x99] = Adca;
            mOpcodeFunctions[0x9a] = Oraa;
            mOpcodeFunctions[0x9b] = Adda;
            mOpcodeFunctions[0x9c] = Cpx;
            mOpcodeFunctions[0x9d] = Jsr;
            mOpcodeFunctions[0x9e] = Lds;
            mOpcodeFunctions[0x9f] = Sts;
            mOpcodeFunctions[0xa0] = Suba;
            mOpcodeFunctions[0xa1] = Cmpa;
            mOpcodeFunctions[0xa2] = Sbca;
            mOpcodeFunctions[0xa3] = Cpd;
            mOpcodeFunctions[0xa4] = Anda;
            mOpcodeFunctions[0xa5] = BitA;
            mOpcodeFunctions[0xa6] = Ldaa;
            mOpcodeFunctions[0xa7] = Staa;
            mOpcodeFunctions[0xa8] = EorA;
            mOpcodeFunctions[0xa9] = Adca;
            mOpcodeFunctions[0xaa] = Oraa;
            mOpcodeFunctions[0xab] = Adda;
            mOpcodeFunctions[0xac] = Cpx;
            mOpcodeFunctions[0xad] = Jsr;
            mOpcodeFunctions[0xae] = Lds;
            mOpcodeFunctions[0xaf] = Sts;
            mOpcodeFunctions[0xb0] = Suba;
            mOpcodeFunctions[0xb1] = Cmpa;
            mOpcodeFunctions[0xb2] = Sbca;
            mOpcodeFunctions[0xb3] = Cpd;
            mOpcodeFunctions[0xb4] = Anda;
            mOpcodeFunctions[0xb5] = BitA;
            mOpcodeFunctions[0xb6] = Ldaa;
            mOpcodeFunctions[0xb7] = Staa;
            mOpcodeFunctions[0xb8] = EorA;
            mOpcodeFunctions[0xb9] = Adca;
            mOpcodeFunctions[0xba] = Oraa;
            mOpcodeFunctions[0xbb] = Adda;
            mOpcodeFunctions[0xbc] = Cpx;
            mOpcodeFunctions[0xbd] = Jsr;
            mOpcodeFunctions[0xbe] = Lds;
            mOpcodeFunctions[0xbf] = Sts;
            mOpcodeFunctions[0xc0] = Subb;
            mOpcodeFunctions[0xc1] = Cmpb;
            mOpcodeFunctions[0xc2] = Sbcb;
            mOpcodeFunctions[0xc3] = Addd;
            mOpcodeFunctions[0xc4] = Andb;
            mOpcodeFunctions[0xc5] = BitB;
            mOpcodeFunctions[0xc6] = Ldab;
            mOpcodeFunctions[0xc7] = Invalid;
            mOpcodeFunctions[0xc8] = EorB;
            mOpcodeFunctions[0xc9] = Adcb;
            mOpcodeFunctions[0xca] = Orab;
            mOpcodeFunctions[0xcb] = Addb;
            mOpcodeFunctions[0xcc] = Ldd;
            mOpcodeFunctions[0xcd] = Invalid;
            mOpcodeFunctions[0xce] = Ldx;
            mOpcodeFunctions[0xcf] = Stop;
            mOpcodeFunctions[0xd0] = Subb;
            mOpcodeFunctions[0xd1] = Cmpb;
            mOpcodeFunctions[0xd2] = Sbcb;
            mOpcodeFunctions[0xd3] = Addd;
            mOpcodeFunctions[0xd4] = Andb;
            mOpcodeFunctions[0xd5] = BitB;
            mOpcodeFunctions[0xd6] = Ldab;
            mOpcodeFunctions[0xd7] = Stab;
            mOpcodeFunctions[0xd8] = EorB;
            mOpcodeFunctions[0xd9] = Adcb;
            mOpcodeFunctions[0xda] = Orab;
            mOpcodeFunctions[0xdb] = Addb;
            mOpcodeFunctions[0xdc] = Ldd;
            mOpcodeFunctions[0xdd] = Std;
            mOpcodeFunctions[0xde] = Ldx;
            mOpcodeFunctions[0xdf] = Stx;
            mOpcodeFunctions[0xe0] = Subb;
            mOpcodeFunctions[0xe1] = Cmpb;
            mOpcodeFunctions[0xe2] = Sbcb;
            mOpcodeFunctions[0xe3] = Addd;
            mOpcodeFunctions[0xe4] = Andb;
            mOpcodeFunctions[0xe5] = BitB;
            mOpcodeFunctions[0xe6] = Ldab;
            mOpcodeFunctions[0xe7] = Stab;
            mOpcodeFunctions[0xe8] = EorB;
            mOpcodeFunctions[0xe9] = Adcb;
            mOpcodeFunctions[0xea] = Orab;
            mOpcodeFunctions[0xeb] = Addb;
            mOpcodeFunctions[0xec] = Ldd;
            mOpcodeFunctions[0xed] = Std;
            mOpcodeFunctions[0xee] = Ldx;
            mOpcodeFunctions[0xef] = Stx;
            mOpcodeFunctions[0xf0] = Subb;
            mOpcodeFunctions[0xf1] = Cmpb;
            mOpcodeFunctions[0xf2] = Sbcb;
            mOpcodeFunctions[0xf3] = Addd;
            mOpcodeFunctions[0xf4] = Andb;
            mOpcodeFunctions[0xf5] = BitB;
            mOpcodeFunctions[0xf6] = Ldab;
            mOpcodeFunctions[0xf7] = Stab;
            mOpcodeFunctions[0xf8] = EorB;
            mOpcodeFunctions[0xf9] = Adcb;
            mOpcodeFunctions[0xfa] = Orab;
            mOpcodeFunctions[0xfb] = Addb;
            mOpcodeFunctions[0xfc] = Ldd;
            mOpcodeFunctions[0xfd] = Std;
            mOpcodeFunctions[0xfe] = Ldx;
            mOpcodeFunctions[0xff] = Stx;
        }

        private void SystemMemory_OnInvalidMemoryAccess(uint address)
        {
            IsHalted = true;
        }

        public Panel RegistersViewPanel { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
            return new DisassemblerM68HC11(this, (uint)topAddress);
        }

        //public IParser CreateParser()
        //{
        //    return Parsers.Parsers.GetParser(ParserName, this);
        //}


        public void FireInterupt(string interruptType)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public bool StepInto()
        {
            return ExecuteOne();
        }

        private bool IsNextInstructionCall(ref int opCodeLen)
        {
            byte opcode = (byte)SystemMemory.GetMemory(RegistersM68HC11.PC, WordSize.OneByte, false);
            switch(opcode)
            {
                case 0x18:
                    {
                        opcode = (byte)SystemMemory.GetMemory(RegistersM68HC11.PC + 1, WordSize.OneByte, false);
                        if (opcode == 0xad)
                        {
                            opCodeLen = 3;
                            return true;
                        }
                        return false;
                    }
                case 0xad:
                case 0x9d: opCodeLen = 2; return true;
                case 0xbd: opCodeLen = 3; return true;
                default:
                    return false;

            }
        }
        private bool IsNextInstructionRst()
        {
            byte opcode = (byte)SystemMemory.GetMemory(RegistersM68HC11.PC, WordSize.OneByte, false);
            return opcode == 0x3f;
        }

        public bool StepOver()
        {
            int opCodeLen = 0;
            bool IsCall = IsNextInstructionCall(ref opCodeLen);
            bool IsRst = IsNextInstructionRst();
            if (IsCall || IsRst)
            {
                uint addr = (uint)(RegistersM68HC11.PC + opCodeLen);
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
            if (mAbort != null)
                mAbort.Set();
        }

        public bool ExecuteOne()
        {
            OpcodeTrace.Feed(Registers.PC);
            byte opcode = SystemMemory[(ushort)Registers.PC++];
            if (opcode == 0x18 || opcode == 0x1a || opcode == 0xcd)
            {
                preByte = opcode;
                opcode = SystemMemory[(ushort)Registers.PC++];
            }
            uint cycleCount = mOpcodeFunctions[opcode](opcode);
            CycleCount += cycleCount;
            preByte = 0;
            OnCycleCount?.Invoke(cycleCount);
            return IsHalted;
        }

        public bool ExecuteUntilBreakpoint()
        {
            mAbort = new EventWaitHandle(false, EventResetMode.ManualReset);
            mThread = new Thread(new ParameterizedThreadStart(thread_start));
            threadDone = false;
            mThread.Start(null);
            while (!threadDone)
                Application.DoEvents();
            mThread.Join();
            mThread = null;
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
                if (IsHalted)
                    break;
            }
            threadDone = true;
        }//thread_start

        public void AddMemoryAccess(uint startAddress, uint endAddress, memoryAccessReadEventHandler readDelegate, memoryAccessWriteEventHandler writeDelegate)
        {
            SystemMemory.AddMemoryOverride(startAddress, endAddress, readDelegate, writeDelegate);
        }
    }
}