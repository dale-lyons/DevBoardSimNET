using System;
using System.Windows.Forms;

using Boards;
using Processors;
using Parsers;

namespace DevBoardSim.NET
{
    public class BoardHost : IBoardHost
    {
        private IBoard mIBoard;
        private GUI.Board.BoardView mBoardView;
        private Form1 mForm;
        public ISystemMemory SystemMemory { get { return Processor.SystemMemory; } }

        public BoardHost(IBoard board, GUI.Board.BoardView boardView, Form1 form)
        {
            mIBoard = board;
            mBoardView = boardView;
            mForm = form;
        }

        public IProcessor Processor { get { return mIBoard.Processor; } }

        public Panel RequestPanel(string title)
        {
            return mBoardView.RequestPanel(title);
        }

        public void FireInterupt(string interruptType) { Processor.FireInterupt(interruptType); }

        /// <summary>
        /// The Load delegate is called when all the plugins have been loaded and their init methods called.
        /// </summary>
        //pluginLoadHandlerDelegate Load { get; set; }
        public event EventHandler<EventArgs> Loaded;

        ///// <summary>
        ///// The Start delegate is called when the simulator starts a simulation session
        ///// </summary>
        public event EventHandler<EventArgs> StartSimulation;

        ///// <summary>
        ///// The Stop delegate is called when the simulator is stopped.
        ///// </summary>
        public event EventHandler<EventArgs> StopSimulation;

        ///// <summary>
        ///// The Restart delegate is called when the simulator is restarting
        ///// </summary>
        //event EventHandler<EventArgs> Restart;

        ///// <summary>
        ///// The Unload delegate is called when the simulator is shutting down
        ///// </summary>
        public event EventHandler<UnloadEventArgs> Unload;

        /// <summary>
        /// The Cycles delegate is called when an instruction is finished executing.
        /// The number of cycles expired is reported.
        /// </summary>
        public event EventHandler<CyclesEventArgs> Cycles;

        public void FireCycles(uint count)
        {
            Cycles?.Invoke(this, new CyclesEventArgs(count));
        }

        public void FireStartSimulation()
        {
            StartSimulation?.Invoke(this, new EventArgs());
        }
        public void FireSopSimulation()
        {
            StopSimulation?.Invoke(this, new EventArgs());
        }

        public void FireUnload()
        {
            Unload?.Invoke(this, new UnloadEventArgs());
        }

        public void FireLoaded()
        {
            Loaded?.Invoke(this, new EventArgs());
        }

        public void RequestPortAccess(byte startport, byte endPort, portAccessReadEventHandler readDelegate, portAccessWriteEventHandler writeDelegate)
        {
            Processor.AddPortOverride(startport, endPort, readDelegate, writeDelegate);
        }

        public void RequestMemoryAccess(uint startAddress, uint endAddress, memoryAccessReadEventHandler readDelegate, memoryAccessWriteEventHandler writeDelegate)
        {
            Processor.AddMemoryAccess(startAddress, endAddress, readDelegate, writeDelegate);
        }

        public void RequestCodeAccess(uint startAddress, codeAccessReadEventHandler readDelegate)
        {
            Processor.AddCodeAccess(startAddress, readDelegate);
        }

        public void LoadBinaryCode(uint addr, byte[] bytes)
        {
            mForm.LoadBinaryCode(addr, bytes);
        }

        public void LoadSourceCode(string filename)
        {
            var parser = Parsers.Parsers.GetParser(mForm.CodeView.Processor.ParserName, mForm.CodeView.Processor);
            //var parser = mForm.CodeView.Processor.CreateParser();
            var parseSettings = new ParseSettings { Hex = true, Listing = true, Bin = true, Path = filename };
            parser.Parse(filename, parseSettings);
            if (parser.Errors.Count > 0)
            {
                return;
            }
            mForm.CodeView.AddCompiledCode(parser, false);
            mForm.RefreshAllViews();
        }
    }
}