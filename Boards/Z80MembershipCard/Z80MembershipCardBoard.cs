using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

using Boards;
using Processors;
using Terminal;
using System.Windows.Forms;
using Preferences;

namespace Z80MembershipCard
{
    public class Z80MembershipCardBoard : IBoard
    {
        private Panel mPanel;
        private CmdStateMachine mCmdStateMachine;
        private Controls.Z80MembershipCardPanel mTerminal;
        public string Name { get { return "Z80 Membership Card"; } }
        public string BoardName { get { return "Z80MembershipCard.Z80MembershipCardBoard"; } }

        public string ProcessorName { get { return "ZilogZ80.ZilogZ80"; } }

        public ISystemMemory SystemMemory { get; set; }

        private IBoardHost mBoardHost;
        private SerialPortStateMachine mSerialPortStateMachine = new SerialPortStateMachine();

        private IProcessor mProcessor;
        public IProcessor Processor
        {
            get
            {
                if (mProcessor == null)
                {
                    mProcessor = Processors.Processors.GetProcessor(ProcessorName, SystemMemory);
                }
                return mProcessor;
            }
        }

        private Z80MembershipConfig mZ80MembershipConfig;
        public PreferencesBase Settings { get { return mZ80MembershipConfig; } }
        public void SaveSettings(PreferencesBase settings)
        {
            PreferencesBase.Save<Z80MembershipConfig>(settings, Z80MembershipConfig.Key);
        }

        public Z80MembershipCardBoard()
        {
            SystemMemory = new SystemMemory();
            SystemMemory.WordSize = WordSize.TwoByte;
            SystemMemory.Endian = Endian.Little;
            mZ80MembershipConfig = PreferencesBase.Load<Z80MembershipConfig>(Z80MembershipConfig.Key);
        }

        public void Init(IBoardHost boardHost)
        {
            mBoardHost = boardHost;
            mBoardHost.Loaded += PluginHost_Loaded;
        }

        private void PluginHost_Loaded(object sender, EventArgs args)
        {
            byte[] romImage = null;
            if (mZ80MembershipConfig.UseCustomBootROM)
            {
                romImage = File.ReadAllBytes(mZ80MembershipConfig.RomImage);
            }
            else
            {
                var assembly = Assembly.GetExecutingAssembly();
                string[] names = assembly.GetManifestResourceNames();
                var resourceName = "Z80MembershipCard.ROM.zmcv15.bin";
                romImage = loadResource(resourceName);
            }
            mBoardHost.SystemMemory.CreateMemoryBlock(0, romImage, 0, (uint)Processors.SystemMemory.THIRTYTWOK, false);
            mBoardHost.SystemMemory.CreateMemoryBlock(0x8000, null, 0, 0x8000, true);

            mCmdStateMachine = new CmdStateMachine(null);
            mPanel = mBoardHost.RequestPanel("Name");
            mTerminal = new Controls.Z80MembershipCardPanel(mBoardHost, mCmdStateMachine, mZ80MembershipConfig, mSerialPortStateMachine);
            mTerminal.Dock = DockStyle.Fill;
            mPanel.Controls.Add(mTerminal);

            //Processor.Breakpoints.Add(0x0e4f, false);
//            Processor.Breakpoints.Add(0x0ba9, false);
//            mTerminal.Terminal.onKeyPress += Terminal_onKeyPress;
            mBoardHost.RequestCodeAccess(0x13e8, putChr);
            mBoardHost.RequestCodeAccess(0x1513, inChr);
            //mBoardHost.RequestPortAccess(0xc8, 0xc8, readP, null);
            //mBoardHost.RequestPortAccess(0x40, 0x40, readP, writeP);

        }

        //private uint readP(object sender, PortAccessReadEventArgs args)
        //{
        //    var lineState = mSerialPortStateMachine.Readbit();
        //    if (lineState == SerialPortStateMachine.LineState.None)
        //        return 0;

        //    return (uint)lineState;
        //}
        //private void writeP(object sender, PortAccessWriteEventArgs args)
        //{
        //}

        //private void Terminal_onKeyPress(char data)
        //{
        //    KeyChar = (byte)data;
        //}

        private void putChr(object sender, CodeAccessReadEventArgs args)
        {
            uint data = Processor.Registers.GetSingleRegister("A");
            mTerminal.Feed((byte)data);

            uint sp = Processor.Registers.GetDoubleRegister("SP");

            byte[] buff = new byte[2];
            buff[0] = (byte)SystemMemory.GetMemory(sp++, WordSize.OneByte, true);
            buff[1] = (byte)SystemMemory.GetMemory(sp, WordSize.OneByte, true);
            ushort pc = BitConverter.ToUInt16(buff, 0);
            Processor.Registers.PC = 0x13f7;
        }
        private void inChr(object sender, CodeAccessReadEventArgs args)
        {
            //if (!mTerminal.Terminal.KeyAvailable)
            //{
            //    Processor.Registers.SetFlag("C", true);
            //}
            //else
            //{
            //    char ch = mTerminal.Terminal.GetKey();
            //    Processor.Registers.SetSingleRegister("A", ch);
            //    Processor.Registers.SetFlag("C", false);
            //}
            //Processor.Registers.PC = 0x1533;
        }

        //private uint readM(object sender, MemoryAccessReadEventArgs args)
        //{
        //    uint data = Processor.Registers.FetchRegister("A");
        //    Processor.Registers.PC = 0x1636;
        //    return 0;
        //}

        //private uint readP(object sender, PortAccessReadEventArgs args)
        //{
        //    return 0x80;
        //}
        //private void writeM(object sender, MemoryAccessWriteEventArgs args)
        //{
        //    uint pc = mBoardHost.Processor.Registers.PC;
        //}

        private static byte[] loadResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string[] names = assembly.GetManifestResourceNames();

            byte[] bytes;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    return null;
                bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
            }//using
            return bytes;
        }
    }
}