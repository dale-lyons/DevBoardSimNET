using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Boards;
using Processors;
using Terminal;
//using ZX81.Controls;
using Preferences;

using System.Windows.Forms;

namespace ZX81
{
    public class ZX81Board : IBoard
    {
        public string Name { get { return "ZX81Plugin"; } }
        public string ProcessorName { get { return "ZilogZ80.ZilogZ80"; } }

        private ZX81BoardConfig mZX81BoardConfig;
        //private IMemoryBlock mRAMMemoryBlock;
        //private IMemoryBlock mROMMemoryBlock;
        //private Panel mPluginPanel;
        //private CmdStateMachine mCmdStateMachine;
        //private Controls.ZX81TerminalPanel mZX81TerminalPanel;
        private IProcessor mProcessor;
        private ISystemMemory mSystemMemory;
        public string BoardName { get { return "ZX81.ZX81Board"; } }

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

        public PreferencesBase Settings { get { return mZX81BoardConfig; } }

        public void SaveSettings(PreferencesBase settings)
        {
            PreferencesBase.Save<ZX81BoardConfig>(mZX81BoardConfig, ZX81BoardConfig.Key);
        }

        public ZX81Board()
        {
            mSystemMemory = new SystemMemory();
            mSystemMemory.WordSize = WordSize.TwoByte;
            mSystemMemory.Endian = Endian.Little;
            mSystemMemory.Default(SystemMemory.SIXTY_FOURK);
            mZX81BoardConfig = PreferencesBase.Load<ZX81BoardConfig>(ZX81BoardConfig.Key);
        }

        public IBoardHost BoardHost { get; private set; }
        public void Init(IBoardHost boardHost)
        {
            BoardHost = boardHost;
            BoardHost.Loaded += PluginHost_Loaded;
        }

        private void PluginHost_Loaded(object sender, EventArgs args)
        {
            Processor.Breakpoints.Add(0x04cc, false);

        }
    }
}