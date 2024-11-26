using System;
using System.Collections.Generic;
using System.Text;

namespace ARM7.VFP
{
    //public delegate double fpTwoOperandD(double op1,double op2);
    //public delegate float fpTwoOperandS(float op1, float op2);

    /// <summary>
    /// Class to simulate a VFP(Vector Floating Point) Processor
    /// </summary>
    public partial class FloatingPointProcessor
    {
        private ARM7 mArm7;                      //reference back to ARM processor
        private FloatingPointRegisters _FPR;        //floating point registers
        private FPSCR _FPSCR;                       //floating point control register

        /// <summary>
        /// FloatingPointProcessor ctor
        /// create instance of VFP, obtain back reference to ARM
        /// </summary>
        /// <param name="jm"></param>
        //public FloatingPointProcessor(Jimulator Jm)
        public FloatingPointProcessor(ARM7 arm7)
        {
            mArm7 = arm7;
            _FPR = new FloatingPointRegisters();
            _FPSCR = new FPSCR();
        }

        ///<summary>Access to floating point registers</summary>
        public FloatingPointRegisters FPR { get { return _FPR; } }
        ///<summary>Access to floating point cpsr register</summary>
        public FPSCR FPSCR { get { return _FPSCR; } }

        /// <summary>
        /// handy function to inspect opcode and determine if instruction is single or double precision
        /// </summary>
        /// <param name="opcode">opcode to test</param>
        /// <returns>true if single</returns>
        public static bool isSingle(uint opcode)
        {
            return((opcode & 0x00000f00) == 0x00000a00);
        }

        /// <summary>
        /// handly function to extract a single or double precision register number from the opcode.
        /// inputs:
        /// opcode - the opcode to extract from
        /// shiftS1 - shift value for upper 4 bits if single (negative is shift left)
        /// shiftS2 - shift value for low bit if single
        /// shiftD  - shift value for 4 bit register number if double
        /// </summary>
        /// <param name="opcode">opcode to extract registers from</param>
        /// <param name="shiftS1">shift code 1</param>
        /// <param name="shiftS2">shift code 2</param>
        /// <param name="shiftD">shft d value</param>
        /// <returns>register number</returns>
        private static uint Unpack(uint opcode,int shiftS1, int shiftS2, int shiftD)
        {
            uint Fx;
            if (isSingle(opcode))
            {
                Fx = (shiftS1 >= 0) ? (opcode >> (shiftS1)) : (opcode << (-shiftS1));
                Fx &= 0x1e;
                Fx |= ((opcode >> shiftS2) & 0x01);
            }
            else
            {
                Fx = (opcode >> shiftD) & 0x0f;
            }
            return Fx;
        }//Unpack

        /// <summary>
        /// Extracts the Fn register from an opcode(first operand)
        /// </summary>
        /// <param name="opcode">opcode to extract register from</param>
        /// <returns>register number</returns>
        private static uint UnpackFn(uint opcode)
        {
            return Unpack(opcode, 15, 7, 16);
        }

        /// <summary>
        /// Extracts the Fd register from an opcode(destination register)
        /// </summary>
        /// <param name="opcode">opcode to extract register from</param>
        /// <returns>register number</returns>
        private static uint UnpackFd(uint opcode)
        {
            return Unpack(opcode, 11, 22, 12);
        }

        /// <summary>
        /// Extracts the Fn register from an opcode(destination register)
        /// </summary>
        /// <param name="opcode">opcode to extract register from</param>
        /// <returns>register number</returns>
        private static uint UnpackFm(uint opcode)
        {
            return Unpack(opcode, -1, 5, 0);
        }

        /// <summary>
        /// Extracts the Rd register, a general purpose register number
        /// </summary>
        /// <param name="opcode">opcode to extract register from</param>
        /// <returns>register number</returns>
        private static uint UnpackRd(uint opcode)
        {
            return (opcode >> 12) & 0x0f;
        }

        /// <summary>
        /// Extracts the Rn register, a general purpose register number
        /// </summary>
        /// <param name="opcode">opcode to extract register from</param>
        /// <returns>register number</returns>
        private static uint UnpackRn(uint opcode)
        {
            return (opcode >> 16) & 0x0f;
        }

        /// <summary>
        /// Extracts the PQRS bits
        /// </summary>
        /// <param name="opcode">opcode to extract register from</param>
        /// <returns>PQRS bits</returns>
        private static uint UnpackPQRS(uint opcode)
        {
            uint pqrs = 0;
            pqrs |= ((opcode >> 20) & 0x08);
            pqrs |= ((opcode >> 19) & 0x04);
            pqrs |= ((opcode >> 19) & 0x02);
            pqrs |= ((opcode >> 6) & 0x01);
            return pqrs;
        }

        /// <summary>
        /// Extracts the PUW bits(multi-load/store instructions)
        /// </summary>
        /// <param name="opcode">opcode to extract register from</param>
        /// <returns>PUW bits</returns>
        private static uint UnpackPUW(uint opcode)
        {
            return (((opcode & 0x01800000) >> 22) | ((opcode >> 21) & 0x01));
        }

        /// <summary>
        /// processor reset.
        /// </summary>
        public void reset()
        {
            this._FPR.reset();          //reset floating point registers
            this._FPSCR.reset();        //reset control register
        }

        /// <summary>
        /// Execute a floating point instruction.
        /// Called from main simulator execute function when a floating point
        /// instruction is detected.
        /// Here we determine the class of instruction and dispatch it
        /// </summary>
        /// <param name="opcode">opcode to execute</param>
        /// <returns>number of clock cyces used</returns>
        public uint Execute(uint opcode)
        {
            try
            {
                //must be a coprocessor 0xa or 0xb instruction. If not we have an undefined instruction
                uint coprocessor = opcode & 0x00000f00;
                if (coprocessor != 0x00000a00 && coprocessor != 0x00000b00)
                {
                    return 0;
                }
                bool singleType = isSingle(opcode);

                //one of these 3 classes of instructions
                if ((opcode & 0x0f000010) == 0x0e000000)
                    return data_processing(opcode, singleType);
                else if ((opcode & 0x0e000000) == 0x0c000000)
                    return load_store(opcode);
                else if ((opcode & 0x0f000070) == 0x0e000010)
                    return register_transfer(opcode);
                else
                    return 0;
            }
            catch (FloatingPointException)
            {
                //catch any floating point exceptions here. Dump message to console
                //and stop simulator
//                _jm.WriteLineConsole("Floating point exception occurred:" + ex.Message);
//                _jm.StopSimulation();
                return 0;
            }
        }//Execute

        /// <summary>
        /// Create a string from a double value. Uses a simple FP format
        /// suitable for the registers view and swi instructions.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string DoubleToString(double value)
        {
            return double.IsNaN(value) ? "NaN" : value.ToString("0.###E+0");
        }

        /// <summary>
        /// Create a string from a float value. Uses a simple FP format
        /// suitable for the registers view and swi instructions.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FloatToString(float value)
        {
            return float.IsNaN(value) ? "NaN" : value.ToString("0.###E+0");
        }

    }//class FloatingPointProcessor
}