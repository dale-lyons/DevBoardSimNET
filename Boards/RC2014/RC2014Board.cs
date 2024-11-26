using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Windows.Forms;

using Boards;
using Processors;
using Terminal;
using RC2014.Controls;
using Preferences;

namespace RC2014
{
    public class RC2014Board : IBoard
    {
        public string Name { get { return "RC2014Plugin"; } }
        public string BoardName { get { return "RC2014.RC2014Board"; } }

        public string ProcessorName { get { return "ZilogZ80.ZilogZ80"; } }

        private RC2014BoardConfig mRC2014BoardConfig;
        private IMemoryBlock mRAMMemoryBlock;
        private IMemoryBlock mROMMemoryBlock;
        private Panel mPluginPanel;
        private CmdStateMachine mCmdStateMachine;

        private Controls.RC2014TerminalPanel mRC2014TerminalPanel;
        private IProcessor mProcessor;
        private ISystemMemory mSystemMemory;

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

        public PreferencesBase Settings { get { return mRC2014BoardConfig; } }

        public void SaveSettings(PreferencesBase settings)
        {
            PreferencesBase.Save<RC2014BoardConfig>(mRC2014BoardConfig, RC2014BoardConfig.Key);
        }

        public RC2014Board()
        {
            mSystemMemory = new SystemMemory();
            mSystemMemory.WordSize = WordSize.TwoByte;
            mSystemMemory.Endian = Endian.Little;
            mRC2014BoardConfig = PreferencesBase.Load<RC2014BoardConfig>(RC2014BoardConfig.Key);
        }

        public IBoardHost BoardHost { get; private set; }
        public void Init(IBoardHost boardHost)
        {
            BoardHost = boardHost;
            BoardHost.Loaded += PluginHost_Loaded;
        }

        private void PluginHost_Loaded(object sender, EventArgs args)
        {
            //            var assembly = Assembly.GetExecutingAssembly();
            //            string[] names = assembly.GetManifestResourceNames();
            //            var resourceName = "RC2014.ROM.R0000009.BIN";
            //            var resourceName = "RC2014.ROM.crt0.rom";
            //var resourceName = "RC2014.ROM.Monitor.ddt.bin";
            //            byte[] bytes = loadResource(resourceName);
            byte[] bytes = null;
            if (bytes == null)
                bytes = new byte[SystemMemory.FIVE_TWELVEK];

            ushort baseAddr = (ushort)((mRC2014BoardConfig.ROMA13 == RC2014BoardConfig.JumperSetting.A1 ? 0x2000 : 0) |
                                       (mRC2014BoardConfig.ROMA14 == RC2014BoardConfig.JumperSetting.A1 ? 0x4000 : 0) |
                                       (mRC2014BoardConfig.ROMA15 == RC2014BoardConfig.JumperSetting.A1 ? 0x8000 : 0));
            byte[] Rom = new byte[SystemMemory.THIRTYTWOK];
            //baseAddr = 0;
            Array.Copy(bytes, baseAddr, Rom, 0, SystemMemory.THIRTYTWOK);
            //for(int ii=baseAddr; count < SystemMemory.THIRTYTWOK; ii++, count++)
            //{
            //    Rom[count] = bytes[ii];
            //}
            mROMMemoryBlock = BoardHost.SystemMemory.CreateMemoryBlock(0, Rom, 0, (uint)Rom.Length, false);
            mRAMMemoryBlock = BoardHost.SystemMemory.CreateMemoryBlock(0x8000, null, 0, (uint)SystemMemory.THIRTYTWOK, true);

            mPluginPanel = BoardHost.RequestPanel(Name);
            mCmdStateMachine = new CmdStateMachine(executeCommand);
            mRC2014TerminalPanel = new RC2014TerminalPanel(BoardHost, mCmdStateMachine);
            mRC2014TerminalPanel.Dock = DockStyle.Fill;
            mRC2014TerminalPanel.OnKeypress += MRC2014TerminalPanel_OnKeypress;
            mRC2014TerminalPanel.OnSendByte += MRC2014TerminalPanel_OnSendByte;
            mPluginPanel.Controls.Add(mRC2014TerminalPanel);
            BoardHost.RequestPortAccess(0x80, 0x81, portRead, portWrite);

            //Processor.Breakpoints.Add(0x03a0, false);
            //BoardHost.RequestMemoryAccess(0xe080, 0xe080, null, memWrite);
            //Processor.Breakpoints.Add(0xc193, true);
            //Processor.Breakpoints.Add(0xc1d1, false);
        }

        private void MRC2014TerminalPanel_OnSendByte(char ch)
        {
            mCmdStateMachine.SendByte((byte)ch);
        }

        private void MRC2014TerminalPanel_OnKeypress(char ch)
        {
            mCmdStateMachine.SendByte((byte)ch);
            //BoardHost.FireInterupt("Rst7");
        }

        private void memWrite(object sender, MemoryAccessWriteEventArgs args) { }

        private uint portRead(object sender, PortAccessReadEventArgs args)
        {
            byte ret = 0;
            if(args.Port == 0x80)
            {
                ret = 0x02;
                if (!mCmdStateMachine.AvailableToSendBytes)
                    return ret;
                ret |= 0x01;
            }
            else if (args.Port == 0x81)
            {
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
            if (args.Port == 0x81)
            {
                if (!mCmdStateMachine.portWrite(args.Data))
                    mRC2014TerminalPanel.Feed(args.Data);
            }
        }

        private byte[] loadResource(string resourceName)
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

        private void executeCommand(List<byte> cmd, List<byte> data) { }
    }
}