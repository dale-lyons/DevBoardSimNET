using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO.Ports;

using Boards;
using Processors;
using Terminal;
using Preferences;
using GW8085SBC.Controls;

namespace GW8085SBC
{
    public class GW8085SBCBoard : IBoard
    {
        [Flags]
        private enum BoardControl : byte
        {
            RomPage0 = 0x01,
            RomPage1 = 0x02,
            RomPage2 = 0x04,
            RomBoot = 0x08,
            RomEnabled = 0x10,
            UserLED = 0x80
        }

        [Flags]
        private enum BoardStatus : byte
        {
            RPA0 = 0x01,
            RPA1 = 0x02,
            RPA2 = 0x04,
            RomBoot = 0x08,
            RomEnabled = 0x10,
            RomPage0 = 0x20,
            RomPage1 = 0x40,
            RomPage2 = 0x80
        }
        private byte UART_DataRegister = 0x00;//rw
        private byte UART_StatusRegister = 0x01;//r
        private byte UART_ControlRegister = 0x01;//w
        private byte BOARD_StatusRegister1 = 0x02;//r
        private byte BOARD_ControlRegister1 = 0x02;//w
        private byte BOARD_StatusRegister2 = 0x03;//r
        private byte BOARD_ControlRegister2 = 0x03;//w

        private CmdStateMachine mCmdStateMachine;

        private Panel mPluginPanel;

        private bool mROMBootEnabled;
        private bool mROMEnabled;
        private ushort mRomBase;
        private bool mRomWriteProtect;
        private GWTerminalPanel mGWTerminalPanel;

        private char? mKeypress;

        //private IMemoryBlock mRAMMemoryBlock;
        private IMemoryBlock[] mBankedMemoryBlocks = new IMemoryBlock[8];
        //private IMemoryBlock[] mMemoryMap;
        public string BoardName { get { return "GW8085SBC.GW8085SBCBoard"; } }

        private IBoardHost mBoardHost;
        public string ProcessorName { get { return "Intel8085.Intel8085"; } }
        //public string ProcessorName { get { return "ZilogZ80.ZilogZ80"; } }
        private IMemoryBlock mOriginalRAMBlock;

        private ISystemMemory mSystemMemory;

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

        private GW8085BoardConfig mGW8085BoardConfig;
        public PreferencesBase Settings { get { return mGW8085BoardConfig; } }
        public void SaveSettings(PreferencesBase settings)
        {
            PreferencesBase.Save<GW8085BoardConfig>(mGW8085BoardConfig, GW8085BoardConfig.Key);
        }

        public GW8085SBCBoard()
        {
            mSystemMemory = new SystemMemory();
            mSystemMemory.WordSize = WordSize.TwoByte;
            mSystemMemory.Endian = Endian.Little;
            mSystemMemory.Default(SystemMemory.SIXTY_FOURK);
            mOriginalRAMBlock = mSystemMemory.MemoryMap[0];
            mGW8085BoardConfig = PreferencesBase.Load<GW8085BoardConfig>(GW8085BoardConfig.Key);
        }
        public void Init(IBoardHost boardHost)
        {
            mBoardHost = boardHost;
            mBoardHost.Loaded += PluginHost_Loaded;
        }
        private void MGWTerminalPanel_OnKeypress(char ch)
        {
            mKeypress = ch;
            mBoardHost.FireInterupt("Rst75");
        }

        public void CycleCount(int count) { }

        public string Name { get { return "GW8085SBCBoard"; } }

        private void PluginHost_Loaded(object sender, EventArgs args)
        {
            mROMBootEnabled = mGW8085BoardConfig.BootROMEnabled;
            mROMEnabled = mGW8085BoardConfig.ResetROMEnabled;
            mRomBase = 0xf000;
            mRomWriteProtect = mGW8085BoardConfig.RomWriteProtect;

            byte[] romImage = null;
            if (mGW8085BoardConfig.UseCustomBootROM && File.Exists(mGW8085BoardConfig.RomImage))
            {
                romImage = File.ReadAllBytes(mGW8085BoardConfig.RomImage);
            }
            else
            {
                var assembly = Assembly.GetExecutingAssembly();
                string[] names = assembly.GetManifestResourceNames();
                var resourceName = "GW8085SBC.ROM.Monitor.bootImage.bin";
                romImage = Boards.Boards.loadResource(assembly, resourceName);
            }
            for (int ii = 0; ii < 8; ii++)
                mBankedMemoryBlocks[ii] = new MemoryBlock(mRomBase, romImage, (int)(ii * SystemMemory.FOURK), SystemMemory.FOURK, true);

            var ccp = Boards.Boards.loadResource(Assembly.GetExecutingAssembly(), "GW8085SBC.ROM.Monitor.CCP.bin");
            Terminal.SimFileAccess.ccp = ccp;

            //var currentThread = Thread.CurrentThread.ManagedThreadId;
            mPluginPanel = mBoardHost.RequestPanel(Name);

            mCmdStateMachine = new CmdStateMachine(executeCommand);
            mGWTerminalPanel = new GWTerminalPanel(mBoardHost, mCmdStateMachine, true);
            mGWTerminalPanel.Dock = DockStyle.Fill;
            mGWTerminalPanel.OnKeypress += MGWTerminalPanel_OnKeypress;
            mPluginPanel.Controls.Add(mGWTerminalPanel);

            //Setup based on switch settings SW2 - IOBASE
            byte ioBase = (byte)(mGW8085BoardConfig.SW2 << 2);
            UART_DataRegister = (byte)(ioBase + UART_DataRegister);
            UART_StatusRegister = (byte)(ioBase + UART_StatusRegister);
            UART_ControlRegister = (byte)(ioBase + UART_ControlRegister);
            BOARD_StatusRegister1 = (byte)(ioBase + BOARD_StatusRegister1);
            BOARD_ControlRegister1 = (byte)(ioBase + BOARD_ControlRegister1);
            BOARD_StatusRegister2 = (byte)(ioBase + BOARD_StatusRegister2);
            BOARD_ControlRegister2 = (byte)(ioBase + BOARD_ControlRegister2);

            mBoardHost.RequestPortAccess(UART_DataRegister, BOARD_ControlRegister2, portRead, portWrite);
            mGWTerminalPanel.SetPWRLED(true);
            selectBankedMemory((byte)mGW8085BoardConfig.BootRomPage);
            //ocessor.Breakpoints.Add(0xce1f, true);
            Processor.Registers.PC = 0;
            //mBoardHost.RequestMemoryAccess(0x0018, 0x0018, null, memaccess);
        }

        private void memaccess(object sender, MemoryAccessWriteEventArgs args) { }

        private uint portRead(object sender, PortAccessReadEventArgs args)
        {
            byte ret = 0;
            if (args.Port == BOARD_StatusRegister1 || args.Port == BOARD_StatusRegister2)
            {
                ret = (byte)(mGW8085BoardConfig.SW3 & 0x07);
                if (mROMEnabled)
                    ret |= 0x10;

                if (mROMBootEnabled)
                    ret |= 0x08;
            }
            else if (args.Port == UART_StatusRegister)
            {
                ret = 0x01;
                if ((!mCmdStateMachine.AvailableToSendBytes) && (!mKeypress.HasValue))
                    return ret;
                ret |= 0x02;
            }
            else if (args.Port == UART_DataRegister)
            {
                if (mKeypress.HasValue)
                {
                    ret = (byte)(char)mKeypress;
                    mKeypress = null;
                    return ret;
                }

                if (mCmdStateMachine.AvailableToSendBytes)
                {
                    byte data;
                    if (mCmdStateMachine.portRead(out data))
                        return data;
                }
            }
            return ret;
        }

        private void portWrite(object sender, PortAccessWriteEventArgs args)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("Output to Port:{0} Value:{1}", args.Port, args.Data));
            if ((args.Port == BOARD_ControlRegister1) || (args.Port == BOARD_ControlRegister2))
            {
                mROMEnabled = ((args.Data & 0x10) != 0);
                mGWTerminalPanel.SetROMLED(mROMEnabled);

                mROMBootEnabled = ((args.Data & 0x08) != 0);

                mGWTerminalPanel.SetUSRLED((args.Data & 0x80) != 0);
                selectBankedMemory((byte)(args.Data & 0x07));
            }
            else if (args.Port == UART_ControlRegister)
            {
            }
            else if (args.Port == UART_DataRegister)
            {
                //Debug.WriteLine(string.Format("Port Write-{0:X2}", args.Data));
                if (!mCmdStateMachine.portWrite(args.Data))
                    mGWTerminalPanel.Feed((char)args.Data);
            }
        }

        private void executeCommand(List<byte> cmd, List<byte> data)
        {
            switch (cmd[0])
            {// load cpm
                case (byte)'r':
                    {
                        var fileData = Terminal.SimFileAccess.readDiskSector(cmd.ToArray());
                        mCmdStateMachine.SendBytes(fileData);
                    }
                    break;
                case (byte)'w':
                    {
                        Terminal.SimFileAccess.writeDiskSector(cmd.ToArray(), data.ToArray());
                    }
                    break;
                case (byte)'e':
                    mCmdStateMachine.IsEscapeOn = true;
                    break;

                default:
                    break;

            }//switch

        }//executeCommand

        private delegate void ledDelegate(int page);
        private void selectBankedMemory(int page)
        {
            var memoryMap = mSystemMemory.MemoryMap;

            mGWTerminalPanel.SelectedBank(page);
            if (mROMEnabled)
            {
                var block = mBankedMemoryBlocks[page].Clone() as MemoryBlock;
                block.StartAddress = mRomBase;
                //map the selected 4K page into the ROM window
                for (int ii = 0; ii < SystemMemory.FOURK; ii++)
                    memoryMap[ii + mRomBase] = block;
            }
            else
            {
                //if the ROM has been disabled, restore the 64K RAM pages and disable the rom window
                for (int ii = 0; ii < SystemMemory.SIXTY_FOURK; ii++)
                    memoryMap[ii] = mOriginalRAMBlock;
                return;
            }

            if (mROMBootEnabled)
            {
                var block = mBankedMemoryBlocks[page].Clone() as MemoryBlock;
                block.StartAddress = 0;
                //if the ROM boot enable is on, copy the selected page into all of ram
                for (int ii = 0; ii < SystemMemory.FOURK; ii++)
                    memoryMap[ii] = block;
            }
        }//selectBankedMemory
    }
}