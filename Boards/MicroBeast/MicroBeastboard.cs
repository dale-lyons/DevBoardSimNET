using System;
using System.Collections.Generic;

using Boards;
using Processors;
using Preferences;
using System.Reflection;
using System.Windows.Forms;
using Terminal;
using MicroBeast.Controls;
using MicroBeast.I2C;
using static Terminal.CmdStateMachine;

namespace MicroBeast
{
    public class MicroBeastboard : IBoard
    {
        const int UART_TX_RX = 0x20;         //  receiver buffer, Write: transmitter buffer
        const int UART_INT_ENABLE = 0x21;    //  Interrupt enable register
        const int UART_INT_ID = 0x22;        // Read: Interrupt identification register
        const int UART_FIFO_CTRL = 0x22;     //Write: FIFO Control register
        const int UART_LINE_CTRL = 0x23;     // Line control register
        const int UART_MODEM_CTRL = 0x24;    //Modem control
        const int UART_LINE_STATUS = 0x25;   //Line status
        const int UART_MODEM_STATUS = 0x26;   //Modem status
        const int UART_SCRATCH = 0x27;       //Scratch register

        const int PIO_A_DATA = 0x10;
        const int PIO_A_CTRL = 0x12;
        const int PIO_B_DATA = 0x11;
        const int PIO_B_CTRL = 0x13;

        const int IO_MEM_0 = 0x70;              //Page 0: 0000h - 3fffh
        const int IO_MEM_1 = 0x71;              //Page 1: 4000h - 7fffh
        const int IO_MEM_2 = 0x72;              //Page 2: 8000h - bfffh
        const int IO_MEM_3 = 0x73;              //Page 3: c000h - ffffh
        const int IO_MEM_CTRL = 0x74;           //Paging enable register

        char? mKeypress;
        public string BoardName { get { return "MicroBeast.MicroBeastboard"; } }
        private IBoardHost mBoardHost;
        public string ProcessorName { get { return "ZilogZ80.ZilogZ80"; } }
        public string Name { get { return "MicroBeastBoard"; } }

        private CmdStateMachine mCmdStateMachine;
        private MicroBeastTerminalPanel mMicroBeastTerminalPanel;

        private ISystemMemory mSystemMemory;
        private IProcessor mProcessor;
        private Z84C20 mZ84C20;
        private I2CDisplay mI2CDisplayL; //left display
        private I2CDisplay mI2CDisplayR; // right display
        private I2cBus mI2cBus;
        private I2CRtc mI2CRtc;

        private IMemoryBlock[] mRam = new IMemoryBlock[32];
        private IMemoryBlock[] mRom = new IMemoryBlock[32];
        private bool mBankedMemory;
        private int[] mMemPort = new int[] { 0, 1, 2, 3 };

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

        private MicroBeastboadConfig mMicroBeastboadConfig;
        public PreferencesBase Settings { get { return mMicroBeastboadConfig; } }
        public void SaveSettings(PreferencesBase settings)
        {
            PreferencesBase.Save<MicroBeastboadConfig>(mMicroBeastboadConfig, MicroBeastboadConfig.Key);
        }

        public MicroBeastboard()
        {
            mSystemMemory = new SystemMemory();
            mSystemMemory.WordSize = WordSize.TwoByte;
            mSystemMemory.Endian = Endian.Little;
            mMicroBeastboadConfig = PreferencesBase.Load<MicroBeastboadConfig>(MicroBeastboadConfig.Key);
        }

        public void Init(IBoardHost boardHost)
        {
            mBoardHost = boardHost;
            mBoardHost.Loaded += PluginHost_Loaded;
        }

        private void PluginHost_Loaded(object sender, EventArgs args)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string[] names = assembly.GetManifestResourceNames();
            var resourceName = "MicroBeast.Rom.flash_v1.4.bin";

            byte[] romImage = null;
            romImage = Boards.Boards.loadResource(assembly, resourceName);

            int ii = 0;
            for (; ii < 32; ii++)
            {
                mRam[ii] = mSystemMemory.CreateMemoryBlock(0, null, 0, SystemMemory.SIXTEENK, true);
                mRom[ii] = mSystemMemory.CreateMemoryBlock(0, null, 0, SystemMemory.SIXTEENK, true);
            }
            setMappedMemoryBank(IO_MEM_0, 0);
            setMappedMemoryBank(IO_MEM_1, 1);
            setMappedMemoryBank(IO_MEM_2, 2);
            setMappedMemoryBank(IO_MEM_3, 3);
            mBankedMemory = false;

            var p = mBoardHost.RequestPanel(Name + "Terminal");
            mCmdStateMachine = new CmdStateMachine(executeCommand);
            mMicroBeastTerminalPanel = new MicroBeastTerminalPanel(mBoardHost, mCmdStateMachine);
            p.Controls.Add(mMicroBeastTerminalPanel);
            mMicroBeastTerminalPanel.Dock = DockStyle.Fill;
            mMicroBeastTerminalPanel.OnKeypress += MGWTerminalPanel_OnKeypress;
            p.Controls.Add(mMicroBeastTerminalPanel);

            mI2cBus = new I2cBus(Processor);
            mI2CDisplayL = new I2CDisplay(mI2cBus, 0x50, mMicroBeastTerminalPanel.onSetCharacterDisplayL);
            mI2cBus.AddDevice(mI2CDisplayL);
            mI2CDisplayR = new I2CDisplay(mI2cBus, 0x53, mMicroBeastTerminalPanel.onSetCharacterDisplayR);
            mI2cBus.AddDevice(mI2CDisplayR);

            mI2CRtc = new I2CRtc(mI2cBus, 0x6f);
            mBoardHost.Cycles += mI2CRtc.Cycles;
            mI2cBus.AddDevice(mI2CRtc);
            mZ84C20 = new Z84C20(mI2cBus, Processor);

            //Processor.Breakpoints.Add(0xe842, true);

            // intercept ports for UART
            mBoardHost.RequestPortAccess(0x20, 0x25, portReadurt, portWriteurt);
            // intercept ports for Keyboard
            mBoardHost.RequestPortAccess(0x00, 0x00, portReadkb, null);
            // intercept ports for PIO
            mBoardHost.RequestPortAccess(0x11, 0x13, portReadpio, portWritepio);
            // intercept ports for Memory Map
            mBoardHost.RequestPortAccess(IO_MEM_0, IO_MEM_CTRL, null, portWritemem);

            Processor.Registers.PC = 0;
        }

        private void MGWTerminalPanel_OnKeypress(char ch)
        {
            mKeypress = ch;
        }

        //private uint portReadmem(object sender, PortAccessReadEventArgs args)
        //{
        //    return 0;
        //}
        private void portWritemem(object sender, PortAccessWriteEventArgs args)
        {
            if (args.Port == IO_MEM_CTRL)
            {
                mBankedMemory = (args.Data == 1);
                if (mBankedMemory)
                {
                    setMappedMemoryBank(IO_MEM_0, mMemPort[0]);
                    setMappedMemoryBank(IO_MEM_1, mMemPort[1]);
                    setMappedMemoryBank(IO_MEM_2, mMemPort[2]);
                    setMappedMemoryBank(IO_MEM_3, mMemPort[3]);
                }
                return;
            }

            var port = args.Port - IO_MEM_0;
            mMemPort[port] = args.Data;
            if (!mBankedMemory)
                return;
            setMappedMemoryBank(args.Port, args.Data);
        }//portWritemem

        private void setMappedMemoryBank(int port, int bank)
        {
            uint start = 0;
            switch (port)
            {
                case IO_MEM_0:
                    start = 0x0000;
                    break;
                case IO_MEM_1:
                    start = 0x4000;
                    break;
                case IO_MEM_2:
                    start = 0x8000;
                    break;
                case IO_MEM_3:
                    start = 0xc000;
                    break;
                default:
                    return;
            }
            mMemPort[port - IO_MEM_0] = bank;

            IMemoryBlock memoryBlock = null;
            if (bank >= 0 && bank <= 31)
                memoryBlock = mRom[bank];
            else if (bank >= 32 && bank <= 63)
                memoryBlock = mRam[bank - 32];
            else
                return;

            memoryBlock.StartAddress = start;
            for (int ii = 0; ii < SystemMemory.SIXTEENK; ii++)
                mSystemMemory.MemoryMap[ii + start] = memoryBlock;
        }

        private uint portReadpio(object sender, PortAccessReadEventArgs args)
        {
            switch (args.Port)
            {
                case PIO_A_CTRL:
                    return mZ84C20.PORTA_CTRL;
                case PIO_B_CTRL:
                    return mZ84C20.PORTB_CTRL;
                case PIO_A_DATA:
                    return mZ84C20.PORTA_DATA;
                case PIO_B_DATA:
                    return mZ84C20.PORTB_DATA;
            }
            return 0;
        }
        private void portWritepio(object sender, PortAccessWriteEventArgs args)
        {
            switch (args.Port)
            {
                case PIO_A_CTRL:
                    mZ84C20.PORTA_CTRL = args.Data;
                    break;
                case PIO_B_CTRL:
                    mZ84C20.PORTB_CTRL = args.Data;
                    break;
                case PIO_A_DATA:
                    mZ84C20.PORTA_DATA = args.Data;
                    break;
                case PIO_B_DATA:
                    mZ84C20.PORTB_DATA = args.Data;
                    break;
            }
        }

        private uint portReadkb(object sender, PortAccessReadEventArgs args)
        {
            byte ret = 0x3f;

            var keycode = mMicroBeastTerminalPanel.GetKeycode();
            if (!keycode.HasValue)
                return ret;

            var portcodeBytes = BitConverter.GetBytes(args.Port);
            var keycodeBytes = BitConverter.GetBytes((ushort)keycode);
            if (portcodeBytes[1] != keycodeBytes[1])
                return ret;
            return keycodeBytes[0];
        }

        private uint portReadurt(object sender, PortAccessReadEventArgs args)
        {
            byte ret = 0x20;
            switch(args.Port)
            {
                case UART_LINE_STATUS:
                    if (mKeypress.HasValue)
                        ret |= 0x01;
                    break;
                case UART_TX_RX:
                    ret = (mKeypress.HasValue) ? (byte)mKeypress : (byte)0;
                    mKeypress = null;
                    break;
            }
            return ret;
        }
        private void portWriteurt(object sender, PortAccessWriteEventArgs args)
        {
            if (args.Port == UART_TX_RX)
            {
                if (!mCmdStateMachine.portWrite(args.Data))
                    mMicroBeastTerminalPanel.Feed((char)args.Data);
            }
        }

        private void executeCommand(List<byte> cmd, List<byte> data)
        {
            mMicroBeastTerminalPanel.ClearFromCursor();
        }
    }
}