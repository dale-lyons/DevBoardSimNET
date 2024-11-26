using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

using Processors;
using Preferences;

namespace Intel8085
{
    public partial class Intel8085 : IProcessor
    {
        public delegate void executionCompleteHandler();

        private EventWaitHandle mAbort;
        private Thread mThread;
        public long CycleCount { get; set; }

        public event InvalidInstructionDelegate OnInvalidInstruction;

        public event CycleCountDelegate OnCycleCount;

        //        private int mResetFired;
        //        private int mTrapFired;
        //        private int m55Fired;
        //        private int m65Fired;
        //        private int m75Fired;

        public IRegisters Registers { get; set; }

        public string ProcessorName { get { return "Intel8085.Intel8085"; } }

        private Registers8085 Registers8085 { get { return Registers as Registers8085; } }

        public OpcodeTrace OpcodeTrace { get; set; } = new OpcodeTrace();

        public Breakpoints Breakpoints { get; private set; }

        private RegistersView mRegistersView;
        public virtual IRegistersView RegistersView { get { return mRegistersView; } }
        public bool IsRunning { get; private set; }
        public bool IsHalted { get; private set; }

        public void AddOpCodeOverride(uint opcodeMask, uint opCodeBase, opCodeOverrideEventHandler opCodeDelegate) { }

        public virtual IList<IInstructionTest> GetInstructionTests()
        {
            return InstructionTests.InstructionTests8085.Tests8085;
        }

        public enum ResetLevels : ushort
        {
            Trap = 0x0000,
            Rst0 = 0x0000,
            Rst1 = 0x0008,
            Rst2 = 0x0010,
            Rst3 = 0x0018,
            Rst4 = 0x0020,
            Rst45 = 0x0024,
            Rst5 = 0x0028,
            Rst55 = 0x002c,
            Rst6 = 0x0030,
            Rst65 = 0x0034,
            Rst7 = 0x0038,
            Rst75 = 0x003c
        }

        public enum InteruptRegisterFlags : byte
        {
            Rst55 = 0x10,
            Rst65 = 0x20,
            Rst75 = 0x40,
            All = Rst55 | Rst65 | Rst75
        }
        public enum InteruptMaskFlags : byte
        {
            Rst55 = 0x01,
            Rst65 = 0x02,
            Rst75 = 0x04,
            All = Rst55 | Rst65 | Rst75
        }

        private Intel8085ProcessorConfig mIntel8085ProcessorConfig;
        public virtual PreferencesBase Settings
        {
            get
            {
                return mIntel8085ProcessorConfig;
            }

        }
        public virtual void SaveSettings(PreferencesBase settings)
        {
            PreferencesBase.Save<Intel8085ProcessorConfig>(settings, Intel8085ProcessorConfig.Key);
        }

        public virtual string[] DoubleRegisters() { return null; }

        public Intel8085(ISystemMemory systemMemory)
        {
            SystemMemory = systemMemory;
            Registers = new Registers8085();
            mRegistersView = new RegistersView(this, Registers as Registers8085);

            Breakpoints = new Breakpoints(this);
            mPortOverrides = new Tuple<portAccessReadEventHandler, portAccessWriteEventHandler>[256];
            //mMemoryOverrides = new Tuple<memoryAccessReadEventHandler, memoryAccessWriteEventHandler>[256];
            mCodeOverrides = new codeAccessReadEventHandler[64 * 1024];
            mIntel8085ProcessorConfig = PreferencesBase.Load<Intel8085ProcessorConfig>(Intel8085ProcessorConfig.Key);

            mOpcodeFunctions[0x00] = Nop;                           //nop
            mOpcodeFunctions[0x01] = Lxi;                           //lxi b,data
            mOpcodeFunctions[0x02] = LdStax;                        //stax b
            mOpcodeFunctions[0x03] = DoubleRegister;                //inx b
            mOpcodeFunctions[0x04] = SingleRegDst;                  //inr b
            mOpcodeFunctions[0x05] = SingleRegDst;                  //dcr b
            mOpcodeFunctions[0x06] = Mvi;                           //mvi b,data
            mOpcodeFunctions[0x07] = NoParams;                      //rlc
            mOpcodeFunctions[0x08] = Invalid;                       //undocumented
            mOpcodeFunctions[0x09] = DoubleRegister;                //dad b
            mOpcodeFunctions[0x0a] = LdStax;                        //ldax b
            mOpcodeFunctions[0x0b] = DoubleRegister;                //dcx b
            mOpcodeFunctions[0x0c] = SingleRegDst;                  //inr c
            mOpcodeFunctions[0x0d] = SingleRegDst;                  //dcr c
            mOpcodeFunctions[0x0e] = Mvi;                           //mvi c,data
            mOpcodeFunctions[0x0f] = NoParams;                      //rrc

            mOpcodeFunctions[0x10] = Invalid;                       //undocumented
            mOpcodeFunctions[0x11] = Lxi;                           //lxi d,data
            mOpcodeFunctions[0x12] = LdStax;                        //stax d
            mOpcodeFunctions[0x13] = DoubleRegister;                //inx d
            mOpcodeFunctions[0x14] = SingleRegDst;                  //inr d
            mOpcodeFunctions[0x15] = SingleRegDst;                  //dcr d
            mOpcodeFunctions[0x16] = Mvi;                           //mvi d,data
            mOpcodeFunctions[0x17] = NoParams;                      //ral
            mOpcodeFunctions[0x18] = Invalid;                       //undocumented
            mOpcodeFunctions[0x19] = DoubleRegister;                //dad d
            mOpcodeFunctions[0x1a] = LdStax;                        //ldax d
            mOpcodeFunctions[0x1b] = DoubleRegister;                //dcx d
            mOpcodeFunctions[0x1c] = SingleRegDst;                  //inr e
            mOpcodeFunctions[0x1d] = SingleRegDst;                  //dcr e
            mOpcodeFunctions[0x1e] = Mvi;                           //mvi e,data
            mOpcodeFunctions[0x1f] = NoParams;                      //rar

            mOpcodeFunctions[0x20] = NoParams;                  //rim
            mOpcodeFunctions[0x21] = Lxi;                       //lxi h,data
            mOpcodeFunctions[0x22] = Data16;                    //shld addr
            mOpcodeFunctions[0x23] = DoubleRegister;            //inx h
            mOpcodeFunctions[0x24] = SingleRegDst;                 //inr h
            mOpcodeFunctions[0x25] = SingleRegDst;                 //dcr h
            mOpcodeFunctions[0x26] = Mvi;                       //mvi h,data
            mOpcodeFunctions[0x27] = NoParams;                  //daa
            mOpcodeFunctions[0x28] = Invalid;                    //undocumented
            mOpcodeFunctions[0x29] = DoubleRegister;            //dad h
            mOpcodeFunctions[0x2a] = Data16;                    //lhld addr
            mOpcodeFunctions[0x2b] = DoubleRegister;            //dcx h
            mOpcodeFunctions[0x2c] = SingleRegDst;                 //inr l
            mOpcodeFunctions[0x2d] = SingleRegDst;                 //dcr l
            mOpcodeFunctions[0x2e] = Mvi;                       //mvi l,data
            mOpcodeFunctions[0x2f] = NoParams;                  //cma

            mOpcodeFunctions[0x30] = NoParams;                  //sim
            mOpcodeFunctions[0x31] = Lxi;                       //lxi sp,data
            mOpcodeFunctions[0x32] = Data16;                    //sta addr
            mOpcodeFunctions[0x33] = DoubleRegister;            //inx sp
            mOpcodeFunctions[0x34] = SingleRegDst;              //inr m
            mOpcodeFunctions[0x35] = SingleRegDst;              //dcr m
            mOpcodeFunctions[0x36] = Mvi;                       //mvi m,data
            mOpcodeFunctions[0x37] = NoParams;                  //stc
            mOpcodeFunctions[0x38] = Invalid;                   //undocumented
            mOpcodeFunctions[0x39] = DoubleRegister;            //dad sp
            mOpcodeFunctions[0x3a] = Data16;                    //lda addr
            mOpcodeFunctions[0x3b] = DoubleRegister;            //dcx sp
            mOpcodeFunctions[0x3c] = SingleRegDst;              //inr a
            mOpcodeFunctions[0x3d] = SingleRegDst;              //dcr a
            mOpcodeFunctions[0x3e] = Mvi;                       //mvi a,data
            mOpcodeFunctions[0x3f] = NoParams;                  //cmc

            mOpcodeFunctions[0x40] = Mov;                       //mov b,b
            mOpcodeFunctions[0x41] = Mov;                       //mov b,c
            mOpcodeFunctions[0x42] = Mov;                       //mov b,d
            mOpcodeFunctions[0x43] = Mov;                       //mov b,e
            mOpcodeFunctions[0x44] = Mov;                       //mov b,h
            mOpcodeFunctions[0x45] = Mov;                       //mov b,l
            mOpcodeFunctions[0x46] = Mov;                       //mov b,m
            mOpcodeFunctions[0x47] = Mov;                       //mov b,a
            mOpcodeFunctions[0x48] = Mov;                       //mov c,b
            mOpcodeFunctions[0x49] = Mov;                       //mov c,c
            mOpcodeFunctions[0x4a] = Mov;                       //mov c,d
            mOpcodeFunctions[0x4b] = Mov;                       //mov c,e
            mOpcodeFunctions[0x4c] = Mov;                       //mov c,h
            mOpcodeFunctions[0x4d] = Mov;                       //mov c,l
            mOpcodeFunctions[0x4e] = Mov;                       //mov c,m
            mOpcodeFunctions[0x4f] = Mov;                       //mov c,a

            mOpcodeFunctions[0x50] = Mov;                       //mov d,b
            mOpcodeFunctions[0x51] = Mov;                       //mov d,c
            mOpcodeFunctions[0x52] = Mov;                       //mov d,d
            mOpcodeFunctions[0x53] = Mov;                       //mov d,e
            mOpcodeFunctions[0x54] = Mov;                       //mov d,h
            mOpcodeFunctions[0x55] = Mov;                       //mov d,l
            mOpcodeFunctions[0x56] = Mov;                       //mov d,m
            mOpcodeFunctions[0x57] = Mov;                       //mov d,a
            mOpcodeFunctions[0x58] = Mov;                       //mov e,b
            mOpcodeFunctions[0x59] = Mov;                       //mov e,c
            mOpcodeFunctions[0x5a] = Mov;                       //mov e,d
            mOpcodeFunctions[0x5b] = Mov;                       //mov e,e
            mOpcodeFunctions[0x5c] = Mov;                       //mov e,h
            mOpcodeFunctions[0x5d] = Mov;                       //mov e,l
            mOpcodeFunctions[0x5e] = Mov;                       //mov e,m
            mOpcodeFunctions[0x5f] = Mov;                       //mov e,a

            mOpcodeFunctions[0x60] = Mov;                       //mov h,b
            mOpcodeFunctions[0x61] = Mov;                       //mov h,c
            mOpcodeFunctions[0x62] = Mov;                       //mov h,d
            mOpcodeFunctions[0x63] = Mov;                       //mov h,e
            mOpcodeFunctions[0x64] = Mov;                       //mov h,h
            mOpcodeFunctions[0x65] = Mov;                       //mov h,l
            mOpcodeFunctions[0x66] = Mov;                       //mov h,m
            mOpcodeFunctions[0x67] = Mov;                       //mov h,a
            mOpcodeFunctions[0x68] = Mov;                       //mov l,b
            mOpcodeFunctions[0x69] = Mov;                       //mov l,c
            mOpcodeFunctions[0x6a] = Mov;                       //mov l,d
            mOpcodeFunctions[0x6b] = Mov;                       //mov l,e
            mOpcodeFunctions[0x6c] = Mov;                       //mov l,h
            mOpcodeFunctions[0x6d] = Mov;                       //mov l,l
            mOpcodeFunctions[0x6e] = Mov;                       //mov l,m
            mOpcodeFunctions[0x6f] = Mov;                       //mov l,a

            mOpcodeFunctions[0x70] = Mov;                       //mov m,b
            mOpcodeFunctions[0x71] = Mov;                       //mov m,c
            mOpcodeFunctions[0x72] = Mov;                       //mov m,d
            mOpcodeFunctions[0x73] = Mov;                       //mov m,e
            mOpcodeFunctions[0x74] = Mov;                       //mov m,h
            mOpcodeFunctions[0x75] = Mov;                       //mov m,l
            mOpcodeFunctions[0x76] = NoParams;                  //hlt
            mOpcodeFunctions[0x77] = Mov;                       //mov m,a
            mOpcodeFunctions[0x78] = Mov;                       //mov a,b
            mOpcodeFunctions[0x79] = Mov;                       //mov a,c
            mOpcodeFunctions[0x7a] = Mov;                       //mov a,d
            mOpcodeFunctions[0x7b] = Mov;                       //mov a,e
            mOpcodeFunctions[0x7c] = Mov;                       //mov a,h
            mOpcodeFunctions[0x7d] = Mov;                       //mov a,l
            mOpcodeFunctions[0x7e] = Mov;                       //mov a,m
            mOpcodeFunctions[0x7f] = Mov;                       //mov a,a

            mOpcodeFunctions[0x80] = SingleRegSrc;                 //add b
            mOpcodeFunctions[0x81] = SingleRegSrc;                 //add c
            mOpcodeFunctions[0x82] = SingleRegSrc;                 //add d
            mOpcodeFunctions[0x83] = SingleRegSrc;                 //add e
            mOpcodeFunctions[0x84] = SingleRegSrc;                 //add h
            mOpcodeFunctions[0x85] = SingleRegSrc;                 //add l
            mOpcodeFunctions[0x86] = SingleRegSrc;                 //add m
            mOpcodeFunctions[0x87] = SingleRegSrc;                 //add a
            mOpcodeFunctions[0x88] = SingleRegSrc;                 //adc b
            mOpcodeFunctions[0x89] = SingleRegSrc;                 //adc c
            mOpcodeFunctions[0x8a] = SingleRegSrc;                 //adc d
            mOpcodeFunctions[0x8b] = SingleRegSrc;                 //adc e
            mOpcodeFunctions[0x8c] = SingleRegSrc;                 //adc h
            mOpcodeFunctions[0x8d] = SingleRegSrc;                 //adc l
            mOpcodeFunctions[0x8e] = SingleRegSrc;                 //adc m
            mOpcodeFunctions[0x8f] = SingleRegSrc;                 //adc a

            mOpcodeFunctions[0x90] = SingleRegSrc;                 //sub b
            mOpcodeFunctions[0x91] = SingleRegSrc;                 //sub c
            mOpcodeFunctions[0x92] = SingleRegSrc;                 //sub d
            mOpcodeFunctions[0x93] = SingleRegSrc;                 //sub e
            mOpcodeFunctions[0x94] = SingleRegSrc;                 //sub h
            mOpcodeFunctions[0x95] = SingleRegSrc;                 //sub l
            mOpcodeFunctions[0x96] = SingleRegSrc;                 //sub m
            mOpcodeFunctions[0x97] = SingleRegSrc;                 //sub a
            mOpcodeFunctions[0x98] = SingleRegSrc;                 //sbb b
            mOpcodeFunctions[0x99] = SingleRegSrc;                 //sbb c
            mOpcodeFunctions[0x9a] = SingleRegSrc;                 //sbb d
            mOpcodeFunctions[0x9b] = SingleRegSrc;                 //sbb e
            mOpcodeFunctions[0x9c] = SingleRegSrc;                 //sbb h
            mOpcodeFunctions[0x9d] = SingleRegSrc;                 //sbb l
            mOpcodeFunctions[0x9e] = SingleRegSrc;                 //sbb m
            mOpcodeFunctions[0x9f] = SingleRegSrc;                 //sbb a

            mOpcodeFunctions[0xa0] = SingleRegSrc;                 //ana b
            mOpcodeFunctions[0xa1] = SingleRegSrc;                 //ana c
            mOpcodeFunctions[0xa2] = SingleRegSrc;                 //ana d
            mOpcodeFunctions[0xa3] = SingleRegSrc;                 //ana e
            mOpcodeFunctions[0xa4] = SingleRegSrc;                 //ana h
            mOpcodeFunctions[0xa5] = SingleRegSrc;                 //ana l
            mOpcodeFunctions[0xa6] = SingleRegSrc;                 //ana m
            mOpcodeFunctions[0xa7] = SingleRegSrc;                 //ana a
            mOpcodeFunctions[0xa8] = SingleRegSrc;                 //xra b
            mOpcodeFunctions[0xa9] = SingleRegSrc;                 //xra c
            mOpcodeFunctions[0xaa] = SingleRegSrc;                 //xra d
            mOpcodeFunctions[0xab] = SingleRegSrc;                 //xra e
            mOpcodeFunctions[0xac] = SingleRegSrc;                 //xra h
            mOpcodeFunctions[0xad] = SingleRegSrc;                 //xra l
            mOpcodeFunctions[0xae] = SingleRegSrc;                 //xra m
            mOpcodeFunctions[0xaf] = SingleRegSrc;                 //xra a

            mOpcodeFunctions[0xb0] = SingleRegSrc;                 //ora b
            mOpcodeFunctions[0xb1] = SingleRegSrc;                 //ora c
            mOpcodeFunctions[0xb2] = SingleRegSrc;                 //ora d
            mOpcodeFunctions[0xb3] = SingleRegSrc;                 //ora e
            mOpcodeFunctions[0xb4] = SingleRegSrc;                 //ora h
            mOpcodeFunctions[0xb5] = SingleRegSrc;                 //ora l
            mOpcodeFunctions[0xb6] = SingleRegSrc;                 //ora m
            mOpcodeFunctions[0xb7] = SingleRegSrc;                 //ora a
            mOpcodeFunctions[0xb8] = SingleRegSrc;                 //cmp b
            mOpcodeFunctions[0xb9] = SingleRegSrc;                 //cmp c
            mOpcodeFunctions[0xba] = SingleRegSrc;                 //cmp d
            mOpcodeFunctions[0xbb] = SingleRegSrc;                 //cmp e
            mOpcodeFunctions[0xbc] = SingleRegSrc;                 //cmp h
            mOpcodeFunctions[0xbd] = SingleRegSrc;                 //cmp l
            mOpcodeFunctions[0xbe] = SingleRegSrc;                 //cmp m
            mOpcodeFunctions[0xbf] = SingleRegSrc;                 //cmp a

            mOpcodeFunctions[0xc0] = Returns;                  //rnz
            mOpcodeFunctions[0xc1] = PushPop;                  //pop b
            mOpcodeFunctions[0xc2] = Jump;                   //jnz addr
            mOpcodeFunctions[0xc3] = Jump;                   //jmp addr
            mOpcodeFunctions[0xc4] = Call;                   //cnz addr
            mOpcodeFunctions[0xc5] = PushPop;                  //push b
            mOpcodeFunctions[0xc6] = Data8;                    //adi data
            mOpcodeFunctions[0xc7] = Rst;                      //rst 0
            mOpcodeFunctions[0xc8] = Returns;                  //rz
            mOpcodeFunctions[0xc9] = Returns;                  //ret
            mOpcodeFunctions[0xca] = Jump;                   //jz addr
            mOpcodeFunctions[0xcb] = Invalid;                  //undocumented
            mOpcodeFunctions[0xcc] = Call;                   //cz addr
            mOpcodeFunctions[0xcd] = Call;                   //call addr
            mOpcodeFunctions[0xce] = Data8;                    //aci data
            mOpcodeFunctions[0xcf] = Rst;                      //rst 1

            mOpcodeFunctions[0xd0] = Returns;                  //rnc
            mOpcodeFunctions[0xd1] = PushPop;                  //pop d
            mOpcodeFunctions[0xd2] = Jump;                     //jnc addr
            mOpcodeFunctions[0xd3] = Data8;                    //out data
            mOpcodeFunctions[0xd4] = Call;                     //cnc addr
            mOpcodeFunctions[0xd5] = PushPop;                  //push d
            mOpcodeFunctions[0xd6] = Data8;                    //sui data
            mOpcodeFunctions[0xd7] = Rst;                      //rst 2
            mOpcodeFunctions[0xd8] = Returns;                  //rc
            mOpcodeFunctions[0xd9] = Invalid;                  //undocumented
            mOpcodeFunctions[0xda] = Jump;                     //jc addr
            mOpcodeFunctions[0xdb] = Data8;                    //in data
            mOpcodeFunctions[0xdc] = Call;                     //cc addr
            mOpcodeFunctions[0xdd] = Invalid;                  //undocumented
            mOpcodeFunctions[0xde] = Data8;                    //sbi data
            mOpcodeFunctions[0xdf] = Rst;                      //rst 3

            mOpcodeFunctions[0xe0] = Returns;                  //rpo
            mOpcodeFunctions[0xe1] = PushPop;                  //pop h
            mOpcodeFunctions[0xe2] = Jump;                     //jpo addr
            mOpcodeFunctions[0xe3] = NoParamXCHGRegs;          //xthl
            mOpcodeFunctions[0xe4] = Call;                     //cpo addr
            mOpcodeFunctions[0xe5] = PushPop;                  //push h
            mOpcodeFunctions[0xe6] = Data8;                    //ani data
            mOpcodeFunctions[0xe7] = Rst;                      //rst 4
            mOpcodeFunctions[0xe8] = Returns;                  //rpe
            mOpcodeFunctions[0xe9] = NoParamXCHGRegs;          //pchl
            mOpcodeFunctions[0xea] = Jump;                     //jpe addr
            mOpcodeFunctions[0xeb] = NoParamXCHGRegs;          //xchg
            mOpcodeFunctions[0xec] = Call;                     //cpe addr
            mOpcodeFunctions[0xed] = Invalid;                  //undocumented
            mOpcodeFunctions[0xee] = Data8;                    //xri data
            mOpcodeFunctions[0xef] = Rst;                      //rst 5

            mOpcodeFunctions[0xf0] = Returns;                  //rp
            mOpcodeFunctions[0xf1] = PushPop;                  //pop psw
            mOpcodeFunctions[0xf2] = Jump;                     //jp addr
            mOpcodeFunctions[0xf3] = NoParams;                 //di
            mOpcodeFunctions[0xf4] = Call;                     //cp addr
            mOpcodeFunctions[0xf5] = PushPop;                  //push psw
            mOpcodeFunctions[0xf6] = Data8;                    //ori data
            mOpcodeFunctions[0xf7] = Rst;                      //rst 6
            mOpcodeFunctions[0xf8] = Returns;                  //rm
            mOpcodeFunctions[0xf9] = NoParamXCHGRegs;          //sphl
            mOpcodeFunctions[0xfa] = Jump;                     //jm addr
            mOpcodeFunctions[0xfb] = NoParams;                 //ei
            mOpcodeFunctions[0xfc] = Call;                     //cm addr
            mOpcodeFunctions[0xfd] = Invalid;                  //undocumented
            mOpcodeFunctions[0xfe] = Data8;                    //cpi data
            mOpcodeFunctions[0xff] = Rst;                      //rst 7

            //Undocumented 8085 opcodes ...
            if (mIntel8085ProcessorConfig.AllowUndocumentedOpcodes)
            {
                mOpcodeFunctions[0x08] = Undocumented;           //****** dsub ******
                mOpcodeFunctions[0x10] = Undocumented;           //****** arhl ******
                mOpcodeFunctions[0x18] = Undocumented;           //****** rdel ******
                mOpcodeFunctions[0x28] = Undocumented;           //****** ldhi ******
                mOpcodeFunctions[0x38] = Undocumented;           //****** ldsi ******
                mOpcodeFunctions[0xcb] = Undocumented;           //****** rstv ******
                mOpcodeFunctions[0xd9] = Undocumented;           //****** shlx ******
                mOpcodeFunctions[0xdd] = Undocumented;           //****** jnk ******
                mOpcodeFunctions[0xed] = Undocumented;           //****** lhlx ******
                mOpcodeFunctions[0xfd] = Undocumented;           //****** jk ******
            }
        }

        public void AddMemoryAccess(uint startAddress, uint endAddress, memoryAccessReadEventHandler readDelegate, memoryAccessWriteEventHandler writeDelegate)
        {
            SystemMemory.AddMemoryOverride(startAddress, endAddress, readDelegate, writeDelegate);
        }

        public void AddPortOverride(byte startPort, byte EndPort, portAccessReadEventHandler readDelegate, portAccessWriteEventHandler writeDelegate)
        {
            for (int ii = startPort; ii <= EndPort; ii++)
                mPortOverrides[ii] = new Tuple<portAccessReadEventHandler, portAccessWriteEventHandler>(readDelegate, writeDelegate);
        }
        public void AddCodeAccess(uint startAddress, codeAccessReadEventHandler readDelegate)
        {
            mCodeOverrides[startAddress] = readDelegate;
        }

        public void RaiseRstLevel(ResetLevels level)
        {
            Push((ushort)Registers.PC);
            Registers.PC = (ushort)level;
            InterruptsEnabled = false;
        }

        public virtual void OnRaiseInterrupt(string interrupt)
        {
            if ((string.Compare(interrupt, "ResetFired") == 0) ||
                (string.Compare(interrupt, "TrapFired") == 0))
            {
                RaiseRstLevel(ResetLevels.Trap);
                return;
            }

            if (!InterruptsEnabled)
                return;

            switch (interrupt)
            {
                case "Rst0":
                    RaiseRstLevel(ResetLevels.Rst0);
                    break;
                case "Rst45":
                    RaiseRstLevel(ResetLevels.Rst45);
                    break;
                case "Rst55":
                    if ((InteruptMask & (byte)InteruptRegisterFlags.Rst55) == 0)
                        return;
                    InteruptMask |= (byte)InteruptRegisterFlags.Rst55;
                    RaiseRstLevel(ResetLevels.Rst55);
                    break;
                case "Rst65":
                    if ((InteruptMask & (byte)InteruptRegisterFlags.Rst65) == 0)
                        InteruptMask |= (byte)InteruptRegisterFlags.Rst65;
                    RaiseRstLevel(ResetLevels.Rst65);
                    break;
                case "Rst7":
                    RaiseRstLevel(ResetLevels.Rst7);
                    break;
                case "Rst75":
                    if ((InteruptMask & (byte)InteruptRegisterFlags.Rst75) == 0)
                        InteruptMask |= (byte)InteruptRegisterFlags.Rst75;
                    RaiseRstLevel(ResetLevels.Rst75);
                    break;
            }
        }

        public bool ExecuteOne()
        {
            lock (mPendingInterrupt)
            {
                if(!string.IsNullOrEmpty(mPendingInterrupt))
                {
                    OnRaiseInterrupt(mPendingInterrupt);
                    mPendingInterrupt = string.Empty;
                    return false;
                }
            }//lock

            OpcodeTrace.Feed(Registers.PC);
            //if (mCodeOverrides[Registers.PC] != null)
            //{
            //    mCodeOverrides[Registers.PC](this, new CodeAccessReadEventArgs(Registers.PC));
            //    return false;
            //}
            byte opcode = (byte)SystemMemory.GetMemory(Registers.PC++, WordSize.OneByte, true);
            uint cycleCount = mOpcodeFunctions[opcode](opcode);
            addCycles(cycleCount);

            if (IsHalted)
                Registers.PC--;

            //            if (mEnableEINext && (opcode != (byte)MnemonicEnums.ei))
            if (mEnableEINext)
            {
                mEnableEINext = false;
                InterruptsEnabled = true;
            }
            return IsHalted;
        }

        protected void addCycles(uint cycles)
        {
            OnCycleCount?.Invoke(cycles);
            CycleCount += cycles;
        }

        private string mPendingInterrupt = string.Empty;

        public void FireInterupt(string interruptType)
        {
            lock (mPendingInterrupt)
            {
                mPendingInterrupt = interruptType;
            }
        }

        private static byte[] callOPCodes = new byte[]
        {
            (byte)MnemonicEnums.call,
            (byte)MnemonicEnums.cc,
            (byte)MnemonicEnums.cm,
            (byte)MnemonicEnums.cnc,
            (byte)MnemonicEnums.cnz,
            (byte)MnemonicEnums.cp,
            (byte)MnemonicEnums.cpe,
            (byte)MnemonicEnums.cpo,
            (byte)MnemonicEnums.cz
        };

        private bool IsNextInstructionCall()
        {
            byte opcode = (byte)SystemMemory.GetMemory(this.Registers.PC, WordSize.OneByte, true);
            foreach (var b in callOPCodes)
                if (b == opcode)
                    return true;
            return false;
        }

        private bool IsNextInstructionRst()
        {
            byte opcode = (byte)SystemMemory.GetMemory(this.Registers.PC, WordSize.OneByte, true);
            return ((opcode & 0xc7) == 0xc7);
        }

        public void Reset()
        {
        }

        private bool threadDone;
        public bool ExecuteUntilBreakpoint()
        {
            IsRunning = true;
            mAbort = new EventWaitHandle(false, EventResetMode.ManualReset);
            mThread = new Thread(new ParameterizedThreadStart(thread_start));
            threadDone = false;
            mThread.Start(null);
            while (!threadDone)
                Application.DoEvents();
            mThread.Join();
            mThread = null;
            IsRunning = false;
            return false;
        }

        private void thread_start(object param)
        {
            //var executionComplete = param as executionCompleteHandler;
            while (!mAbort.WaitOne(0))
            {
                if (this.Breakpoints.CodeHit(this.Registers.PC))
                    break;
                if (this.ExecuteOne())
                    break;
            }
            threadDone = true;
        }//thread_start

        public void Stop()
        {
            if (mAbort != null)
                mAbort.Set();
        }

        public virtual bool StepInto()
        {
            return ExecuteOne();
        }

        public virtual bool StepOver()
        {
            bool IsCall = IsNextInstructionCall();
            bool IsRst = IsNextInstructionRst();
            if (IsCall || IsRst)
            {
                int opCodeLen = IsCall ? 3 : 1;
                uint addr = (uint)(Registers8085.PC + opCodeLen);
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

        public IBreakpoint CreateBreakpoint(uint address)
        {
            return new Breakpoint(address, false);
        }

        public virtual string ParserName { get { return mIntel8085ProcessorConfig.ActiveParser; } }

        public virtual IDisassembler CreateDisassembler(int topAddress)
        {
            return new Disassembler8085(this);
        }
    }
}