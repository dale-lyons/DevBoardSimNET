using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARM7
{
    /// <summary>
    /// delegate defines a callback when one of the 4 status bits changes(o,c,n,z)
    /// or either of the interrupt enables changes(irq,fiq)
    /// </summary>
    /// <param name="flag"></param>
    public delegate void CPSRChangedDelegate(CPSR.CPUFlag flag);

    public class CPSR
    {
        /// <summary>
        /// Defines the number of operating modes of the CPU
        /// </summary>
        public const int NumberCPUModes = 7;

        /// <summary>
        /// defines the default cpu mode on reset.
        /// </summary>
        public const CPUModeEnum DefaultMode = CPUModeEnum.System;

        /// <summary>
        /// the 7 modes of operation of the CPU
        /// The numbers MUST start at 0 and increment by only 1 as the spsr array
        /// is created with the size of _numCPUModes and this enum is used to index into it.
        /// </summary>
        public enum CPUModeEnum : int
        {
            /// <summary>Supervisor Mode</summary>
            Supervisor = 0,
            /// <summary>Abort Mode</summary>
            Abort = 1,
            /// <summary>Undefined Mode</summary>
            Undefined = 2,
            /// <summary>IRQ Mode</summary>
            IRQ = 3,
            /// <summary>FIQ Mode</summary>
            FIQ = 4,
            /// <summary>User Mode</summary>
            User = 5,
            /// <summary>System Mode</summary>
            System = 6
        }

        /// <summary>
        /// This class defines an operating mode of the CPU
        /// Some routines provide the conversion of cpsr/spsr bits to CPU mode and
        /// mode to bits. These bits are the mode bits in the cpsr register.
        /// </summary>
        private class CPUMode
        {
            /// <summary>
            /// defines the bit pattern for each CPU mode
            /// </summary>
            private static uint[] _bits = new uint[] { 0x13, 0x17, 0x1b, 0x12, 0x11, 0x10, 0x1f };

            private readonly CPUModeEnum _mode;
            /// <summary>
            /// ctor takes a mode as param and saves
            /// </summary>
            public CPUMode(CPUModeEnum mode)
            {
                _mode = mode;
            }

            /// <summary>
            /// return the current mode as an Enum
            /// </summary>
            public CPUModeEnum Mode { get { return _mode; } }

            /// <summary>
            /// return the current mode as an integer
            /// </summary>
            public int Index { get { return (int)_mode; } }

            /// <summary>
            /// return the current mode as a bit pattern in the cpsr register
            /// the mode is an index into the static array of bits
            /// </summary>
            public uint Bits { get { return (_bits[(int)_mode]); } }

            /// <summary>
            /// this function converts a given cpu bit pattern to a cpu mode.
            /// We will scan the bit pattern array looking for the pattern where
            /// the lower 5 bits matches the given pattern. If found, the mode
            /// enum is returned. If no pattern found return default mode.
            /// </summary>
            /// <param name="bits"></param>
            /// <returns></returns>
            public static CPUMode bitsTocpuMode(uint bits)
            {
                for (int ii = 0; ii < _bits.Length; ii++)
                {
                    if ((bits & 0x1f) == _bits[ii])
                    {
                        return new CPUMode((CPUModeEnum)ii);
                    }
                }
                return new CPUMode(DefaultMode);
            }//bitsTocpuMode
        }//class CPUMode

        /// <summary>
        /// Helper class to handle a single bit for the status bits
        /// the 2 interrupt disable bits(I and F), and the thumb bit(T)
        /// </summary>
        public class CPUFlag
        {
            private readonly int _index;
            private readonly uint _mask;
            private bool _state;
            /// <summary>
            /// CPUFlag ctor
            /// </summary>
            /// <param name="index"></param>
            /// <param name="mask"></param>
            public CPUFlag(int index, uint mask)
            {
                _index = index;
                _mask = mask;
                //_state = false;
            }
            /// <summary>Returns the flag index</summary>
            public int Index { get { return _index; } }
            /// <summary>Returns the flag mask</summary>
            public uint Mask { get { return _mask; } }
            /// <summary>Returns the flag state</summary>
            public bool State { get { return _state; } set { _state = value; } }
        }//class CPUFlag

        //create the status and interrupt disable bit structures
        private CPUFlag _negative = new CPUFlag(0, 0x80000000);
        private CPUFlag _zero = new CPUFlag(1, 0x40000000);
        private CPUFlag _carry = new CPUFlag(2, 0x20000000);
        private CPUFlag _overflow = new CPUFlag(3, 0x10000000);
        private CPUFlag _irqDisable = new CPUFlag(4, 0x00000080);
        private CPUFlag _fiqDisable = new CPUFlag(5, 0x00000040);
        //corrected the thumb bit value and index-dale
        //private CPUFlag _thumb = new CPUFlag(5, 0x00000100);
        private CPUFlag _thumb = new CPUFlag(6, 0x00000020);

        /// <summary>
        /// current cpu mode
        /// </summary>
        private CPUMode _cpuMode;

        /// <summary>
        /// this array holds the spsr for the 7 cpu modes.
        /// Note that only 5 of the cpu modes requires a spsr.
        /// 2 of the array entries(User and System  mode) will always be 0 and never accessed.
        /// they are there simply to allow simple indexing into the spsr array by the enum value.
        /// </summary>
        private uint[] _spsr;

        /// <summary>
        /// handler to call when the cpsr is written to.
        /// </summary>
        private CPSRChangedDelegate _CPSRChangedHandler;

        /// <summary>
        /// CPSR ctor
        /// </summary>
		public CPSR()
        {
            //create the spsr array based on the number of entries in the cpu mode enum
            _spsr = new uint[NumberCPUModes];

            //zeros out all flags, sets cpu mode to default, disable interrupts
            this.reset();
        }

        /// <summary>
        /// Perform a reset operation. Put CPSR into a default state
        /// </summary>
        public void reset()
        {
            //all CPSR flags to false
            _negative.State = false;
            _zero.State = false;
            _carry.State = false;
            _overflow.State = false;

            //disable all interrupts
            _irqDisable.State = true;
            _fiqDisable.State = true;

            //ARM (non-Thumb) mode of execution
            _thumb.State = false;

            //cpu mode to system
            _cpuMode = new CPUMode(DefaultMode);

            //clear out spsr registers to 0
            Array.Clear(_spsr, 0, _spsr.Length);
        }

        /// <summary>
        /// calls the cpsr status bit changed handler if set
        /// </summary>
        /// <param name="flag"></param>
        private void fireChangedHandler(CPUFlag flag)
        {
            if (_CPSRChangedHandler != null)
                _CPSRChangedHandler(flag);
        }//fireChangedHandler

        /// <summary>
        /// Converts the internal CPSR data items to a 32bit integer and vice-versa
        /// Allows us to maintain the internal state of the CPSR as discrete data items
        /// and show them as a 32bit int to the outside.
        /// </summary>
		public uint Flags
        {
            get
            {
                uint flags = 0;
                if (_negative.State) flags |= _negative.Mask;
                if (_zero.State) flags |= _zero.Mask;
                if (_carry.State) flags |= _carry.Mask;
                if (_overflow.State) flags |= _overflow.Mask;
                if (_irqDisable.State) flags |= _irqDisable.Mask;   //irq enable bit
                if (_fiqDisable.State) flags |= _fiqDisable.Mask;   //fiq enable bit
                if (_thumb.State) flags |= _thumb.Mask;   //thumb mode bit
                flags |= _cpuMode.Bits;
                return flags;
            }
            set
            {
                this.nf = ((value & _negative.Mask) != 0);
                this.zf = ((value & _zero.Mask) != 0);
                this.cf = ((value & _carry.Mask) != 0);
                this.vf = ((value & _overflow.Mask) != 0);
                this.IRQDisable = ((value & _irqDisable.Mask) != 0);
                this.FIQDisable = ((value & _fiqDisable.Mask) != 0);
                this.tf = ((value & _thumb.Mask) != 0);
                _cpuMode = CPUMode.bitsTocpuMode(value);
            }
        }

        /// <summary>
        /// Return the current cpu operating mode as an Enum
        /// </summary>
        public CPUModeEnum Mode { get { return _cpuMode.Mode; } }

        /// <summary>
        /// FIQ disabled flag(NOTE:true means FIQ is disabled!)
        /// if set, fire the cpsr changed handler
        /// </summary>
        public bool FIQDisable
        {
            get { return _fiqDisable.State; }
            set { _fiqDisable.State = value; fireChangedHandler(_fiqDisable); }
        }

        /// <summary>
        /// IRQ disabled flag(NOTE:true means IRQ is disabled!)
        /// if set, fire the cpsr changed handler
        /// </summary>
        public bool IRQDisable
        {
            get { return _irqDisable.State; }
            set { _irqDisable.State = value; fireChangedHandler(_irqDisable); }
        }

        /// <summary>
        /// Zero flag
        /// </summary>
		public bool zf
        {
            get { return _zero.State; }
            set { _zero.State = value; fireChangedHandler(_zero); }
        }

        /// <summary>
        /// Carry flag
        /// </summary>
		public bool cf
        {
            get { return _carry.State; }
            set { _carry.State = value; fireChangedHandler(_carry); }
        }

        /// <summary>
        /// Negative flag
        /// </summary>
		public bool nf
        {
            get { return _negative.State; }
            set { _negative.State = value; fireChangedHandler(_negative); }
        }

        /// <summary>
        /// Overflow flag
        /// </summary>
		public bool vf
        {
            get { return _overflow.State; }
            set { _overflow.State = value; fireChangedHandler(_overflow); }
        }

        /// <summary>
        /// Thumb mode flag
        /// </summary>
        public bool tf
        {
            get { return _thumb.State; }
            set { _thumb.State = value; fireChangedHandler(_thumb); }
        }

        public void set_flags_sub(uint operand1, uint operand2, uint result, bool carry)
        {
            set_NZ(result);
            this.cf = !((result > operand1) || ((result == operand1) && (!carry)));
            //set_CF(a, rd, carry);
            this.vf = Utils.msb((operand1 ^ operand2) & (operand1 ^ result));
            //set_VF_SUB(a, b, rd);
        }
        public void set_flags_add(uint operand1, uint operand2, uint result, bool carry)
        {
            set_NZ(result);
            this.cf = !((result > operand1) || ((result == operand1) && (!carry)));
            //set_CF(a, rd, carry);
            this.vf = Utils.msb(~(operand1 ^ operand2) & (operand1 ^ result));
            //set_VF_ADD(a, b, rd);
        }

        /// <summary>
        /// Negative flag
        /// </summary>
        /// <param name="value"></param>
        public void set_NZ(uint value)
        {
            this.zf = (value == 0);
            this.nf = Utils.msb(value);
        }

        /// <summary>
        /// Carry flag
        /// </summary>
        /// <param name="a"></param>
        /// <param name="rd"></param>
        /// <param name="carry"></param>
        //private void set_CF(uint a, uint rd, bool carry)
        //{//Two ways result can equal an operand
        //    this.cf = !((rd > a) || ((rd == a) && (!carry)));
        //}

        /// <summary>
        /// Sets the overflow flag based on the result of a subtraction
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="rd"></param>
        //        private void set_VF_SUB(uint a, uint b, uint rd)
        //        {
        //            this.vf = Utils.msb((a ^ b) & (a ^ rd));
        ////			this.vf=false;
        ////			if ((((a ^ b) & (a ^ rd)) & bit_31) != 0)
        ////				this.vf=true;
        //        }

        /// <summary>
        /// Sets the overflow flag based on the result of a addition
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="rd"></param>
        //        private void set_VF_ADD(uint a, uint b, uint rd)
        //        {
        //            this.vf = Utils.msb(~(a ^ b) & (a ^ rd));
        //            //			this.vf=false;
        ////			if (((~(a ^ b) & (a ^ rd)) & bit_31) != 0)
        ////				this.vf=true;
        //        }

        /// <summary>
        /// Delegate to set for notification of cpsr changes. This delegate will be
        /// called whenever a change occurs in the cpsr.
        /// </summary>
        public CPSRChangedDelegate CPSRChangedHandler
        {
            //get { return _CPSRChangedHandler; }
            set { _CPSRChangedHandler = value; }
        }

        /// <summary>
        /// Return the spsr register for the current cpu mode
        /// User and System mode have no spsr, so use a 0
        /// (manual says UNPREDICTABLE)
        /// </summary>
        public uint SPSR { get { return _spsr[_cpuMode.Index]; } set { _spsr[_cpuMode.Index] = value; } }

        /// <summary>
        /// Switches the CPU into one of 7 modes. Save the cspr to the spsr, switch to the new mode.
        /// </summary>
        /// <param name="cpuMode"></param>
        public void SwitchCPUMode(CPSR.CPUModeEnum cpuMode)
        {
            //save the current cspr register into the spsr table based on the current mode
            _spsr[(int)cpuMode] = Flags;

            //switch cpu to the new mode
            _cpuMode = new CPUMode(cpuMode);
        }//SetCPUMode


        /// <summary>
        /// Restore the cpu back to its original mode before the interrupt occurred.
        /// Retrieve the cpsr back from the spsr table based on the current cpu mode and restore cpsr.
        /// Based on the mode in the cpsr, switch the cpu back to that mode.
        /// </summary>
        public void RestoreCPUMode()
        {
            //retrieve the original cpsr back from the spsr table
            uint cpsr = _spsr[_cpuMode.Index];
            Flags = cpsr;

            //switch the cpu back to the original mode from the mode bits in the cpsr
            _cpuMode = CPUMode.bitsTocpuMode(cpsr);
        }//SetCPUModeReturn
    }//class CPSR
}