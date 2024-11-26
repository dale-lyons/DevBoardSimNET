using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using Processors;

namespace Boards
{
    public class UnloadEventArgs : EventArgs
    {
    }

    public class CyclesEventArgs : EventArgs
    {
        public uint CycleCount { get; private set; }
        public CyclesEventArgs(uint count)
        {
            CycleCount = count;
        }
    }

    public interface IBoardHost
    {
        Panel RequestPanel(string title);
        /// <summary>
        /// The Load delegate is called when all the plugins have been loaded and their init methods called.
        /// </summary>
        //pluginLoadHandlerDelegate Load { get; set; }
        event EventHandler<EventArgs> Loaded;

        ///// <summary>
        ///// The Start delegate is called when the simulator starts a simulation session
        ///// </summary>
        event EventHandler<EventArgs> StartSimulation;

        ///// <summary>
        ///// The Stop delegate is called when the simulator is stopped.
        ///// </summary>
        event EventHandler<EventArgs> StopSimulation;

        ///// <summary>
        ///// The Restart delegate is called when the simulator is restarting
        ///// </summary>
        //event EventHandler<EventArgs> Restart;

        ///// <summary>
        ///// The Unload delegate is called when the simulator is shutting down
        ///// </summary>
        event EventHandler<UnloadEventArgs> Unload;

        /// <summary>
        /// The Cycles delegate is called when an instruction is finished executing.
        /// The number of cycles expired is reported.
        /// </summary>
        event EventHandler<CyclesEventArgs> Cycles;
        void FireInterupt(string interruptType);

        IProcessor Processor { get; }
        ISystemMemory SystemMemory { get; }

        //object LoadConfig(string key, Type t);
        //void SaveConfig(string key, Type t, object config);

        void FireCycles(uint count);
        void FireStartSimulation();
        void FireSopSimulation();
        void FireUnload();
        void FireLoaded();

        void RequestPortAccess(byte startport, byte endPort, portAccessReadEventHandler readDelegate, portAccessWriteEventHandler writeDelegate);
        void RequestMemoryAccess(uint startAddress, uint endAddress, memoryAccessReadEventHandler readDelegate, memoryAccessWriteEventHandler writeDelegate);
        void RequestCodeAccess(uint startAddress, codeAccessReadEventHandler readDelegate);
        void LoadBinaryCode(uint addr, byte[] bytes);
        void LoadSourceCode(string filename);
    }
}