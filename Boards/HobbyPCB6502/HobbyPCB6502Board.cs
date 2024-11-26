using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Boards;
using Processors;
using Terminal;
using static Terminal.CmdStateMachine;
using System.Threading;
using System.Windows.Forms;
using HobbyPCB6502.Controls;
using Preferences;

namespace HobbyPCB6502
{
    public class HobbyPCB6502Board : IBoard
    {
        /// <summary>
        /// these are NES addresses for the uart.
        /// </summary>
        private const uint UART_DataRegister = 0x8001;
        private const uint UART_StatusRegister = 0x8000;

        private ISystemMemory mSystemMemory;
        private IBoardHost mBoardHost;
        private Panel mPluginPanel;
        private CmdStateMachine mCmdStateMachine;
        private HobbyTerminalPanel mHobbyTerminalPanel;

        private IMemoryBlock mRAMMemoryBlock;
        private IMemoryBlock mROMMemoryBlock;

        private char? mKeypress;

        private BoardPreferencesConfig mBoardPreferencesConfig;

        private IProcessor mProcessor;
        public IProcessor Processor
        {
            get
            {
                if (mProcessor == null)
                {
                    mProcessor = Processors.Processors.GetProcessor(ProcessorName, mSystemMemory);
                }
                return mProcessor;
            }
        }

        public string Name { get { return "HobbySBC6502Board"; } }
        public string BoardName { get { return "HobbyPCB6502.HobbyPCB6502Board"; } }

        public string ProcessorName { get { return "Motorola6502.Motorola6502"; } }

        public PreferencesBase Settings { get { return new BoardPreferencesConfig(); } set { } }
        public void SaveSettings(PreferencesBase settings)
        {
            PreferencesBase.Save<BoardPreferencesConfig>(mBoardPreferencesConfig, BoardPreferencesConfig.Key);
        }

        public HobbyPCB6502Board()
        {
            mSystemMemory = new SystemMemory();
            mSystemMemory.WordSize = WordSize.TwoByte;
            mSystemMemory.Endian = Endian.Little;
        }

        public void Init(IBoardHost boardHost)
        {
            mBoardHost = boardHost;
            mBoardHost.Loaded += MBoardHost_Loaded;
        }

        private void MBoardHost_Loaded(object sender, EventArgs e)
        {
            mBoardPreferencesConfig = PreferencesBase.Load<BoardPreferencesConfig>(BoardPreferencesConfig.Key);
            //byte[] romImage = null;
            //romImage = File.ReadAllBytes(@"C:\Projects\ddtC\Hobby6502\rom1.bin");
            if (mBoardPreferencesConfig.UseCustomBootROM && File.Exists(mBoardPreferencesConfig.RomImage))
            {
                //romImage = File.ReadAllBytes(mBoardPreferencesConfig.RomImage);
            }
            else
            {
                //var assembly = Assembly.GetExecutingAssembly();
                //string[] names = assembly.GetManifestResourceNames();
                //var resourceName = "HobbyPCB6502.ROM.PBW65C02CEGMON.BIN";
                //romImage = Boards.Boards.loadResource(assembly, resourceName);
            }
            mRAMMemoryBlock = mBoardHost.SystemMemory.CreateMemoryBlock(0, null, 0, SystemMemory.THIRTYTWOK, true);
            mROMMemoryBlock = mBoardHost.SystemMemory.CreateMemoryBlock(0x8000, null, 0, SystemMemory.THIRTYTWOK, true);
            mBoardHost.RequestMemoryAccess(0x8000, 0x8002, portRead, portWrite);
            //mBoardHost.RequestMemoryAccess(0x2000, 0x2003, ppuRead, ppuWrite);

            var currentThread = Thread.CurrentThread.ManagedThreadId;
            mPluginPanel = mBoardHost.RequestPanel(Name);

            mCmdStateMachine = new CmdStateMachine(executeCommand);

            mHobbyTerminalPanel = new HobbyTerminalPanel(mBoardHost, mCmdStateMachine, true);
            mHobbyTerminalPanel.Dock = DockStyle.Fill;
            mHobbyTerminalPanel.OnKeypress += MHobbyTerminalPanel_OnKeypress;
            mPluginPanel.Controls.Add(mHobbyTerminalPanel);

            uint addr = mBoardHost.SystemMemory.GetMemory(0xfffc, WordSize.TwoByte, false);
            mBoardHost.Processor.Registers.PC = addr;
            //Processor.Breakpoints.Add(0xe6b8, false);
        }

        private void MHobbyTerminalPanel_OnKeypress(char ch)
        {
            mKeypress = ch;
            //mBoardHost.FireInterupt("Rst75");
        }

        //private uint ppuRead(object sender, MemoryAccessReadEventArgs args)
        //{
        //    return 0xff;
        //}
        //private void ppuWrite(object sender, MemoryAccessWriteEventArgs args) { }

        private void executeCommand(List<byte> cmd, List<byte> data)
        {
        }

        private uint portRead(object sender, MemoryAccessReadEventArgs args)
        {
            uint ret = 0x02;
            switch (args.Address)
            {
                case UART_StatusRegister:
                    if (mKeypress.HasValue)
                    {
                        ret |= 0x01;
                    }
                    break;
                case UART_DataRegister:
                    if (mKeypress.HasValue)
                    {
                        ret = (byte)(char)mKeypress;
                        mKeypress = null;
                        return ret;
                    }
                    break;
            }//switch
            return ret;
        }
        private void portWrite(object sender, MemoryAccessWriteEventArgs args)
        {
            if (args.Address == UART_StatusRegister)
            {
            }
            else if (args.Address == UART_DataRegister)
            {
                if (!mCmdStateMachine.portWrite((byte)args.Value))
                    mHobbyTerminalPanel.Feed((byte)args.Value);
            }
        }
    }
}