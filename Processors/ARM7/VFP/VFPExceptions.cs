using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARM7.VFP
{
    /// <summary>
    /// Defines a floating point exception
    /// </summary>
    public class FloatingPointException : Exception
    {
        /// <summary>
        /// FloatingPointException ctor
        /// </summary>
        /// <param name="msg">message to construct exception with</param>
        public FloatingPointException(string msg)
            : base(msg) { }
    }//class FloatingPointException

    /// <summary>
    /// Defines an invalid operation exception
    /// </summary>
    public class InvalidOperationFloatingPointException : FloatingPointException
    {
        /// <summary>
        /// InvalidOperationFloatingPointException ctor
        /// </summary>
        /// <param name="instruction">the instruction that caused the exception</param>
        /// <param name="singleType">true if single</param>
        public InvalidOperationFloatingPointException(string instruction, bool singleType)
            : base("Invalid Operation:" + instruction + (singleType ? "s" : "d")) { }
    }//class InvalidOperationFloatingPointException

    /// <summary>
    /// Defines an division by 0 exception
    /// </summary>
    public class DivisionByZeroFloatingPointException : FloatingPointException
    {
        /// <summary>
        /// DivisionByZeroFloatingPointException ctor
        /// </summary>
        /// <param name="instruction">the instruction that caused the exception</param>
        /// <param name="singleType">true if single</param>
        public DivisionByZeroFloatingPointException(string instruction, bool singleType)
            : base("Division By Zero:" + instruction + (singleType ? "s" : "d")) { }
    }//class DivisionByZeroFloatingPointException

    /// <summary>
    /// Defines an overflow exception
    /// </summary>
    public class OverflowFloatingPointException : FloatingPointException
    {
        /// <summary>
        /// OverflowFloatingPointException ctor
        /// </summary>
        /// <param name="instruction">the instruction that caused the exception</param>
        /// <param name="singleType">true if single</param>
        public OverflowFloatingPointException(string instruction, bool singleType)
            : base("Overflow:" + instruction + (singleType ? "s" : "d")) { }
    }//class OverflowFloatingPointException

    /// <summary>
    /// Defines an underflow exception
    /// </summary>
    public class UnderflowFloatingPointException : FloatingPointException
    {
        /// <summary>
        /// UnderflowFloatingPointException ctor
        /// </summary>
        /// <param name="instruction">the instruction that caused the exception</param>
        /// <param name="singleType">true if single</param>
        public UnderflowFloatingPointException(string instruction, bool singleType)
            : base("Underflow:" + instruction + (singleType ? "s" : "d")) { }
    }//class UnderflowFloatingPointException
}