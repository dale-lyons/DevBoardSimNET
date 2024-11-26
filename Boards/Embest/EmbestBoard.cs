using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Boards;
using Preferences;
using Processors;

namespace Embest
{
    public class EmbestBoard : IBoard
    {
        public void CycleCount(int count) { }
        private IBoardHost mBoardHost;

        public string BoardName { get { return "Embest.EmbestBoard"; } }

        public string ProcessorName { get { return "ARM7.ARM7"; } }

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

        public ISystemMemory SystemMemory { get; private set; }

        public Preferences.PreferencesBase Settings
        {
            get
            {
                return mEmbestBoardConfig;
            }
            set
            {
                mEmbestBoardConfig = value as EmbestBoardConfig;
            }
        }
        public void SaveSettings(PreferencesBase settings) { }

        private EmbestBoardConfig mEmbestBoardConfig;
        public EmbestBoard()
        {
            mEmbestBoardConfig = Preferences.PreferencesBase.Load<EmbestBoardConfig>(EmbestBoardConfig.Key);
            SystemMemory = new SystemMemory();
            SystemMemory.Default(Processors.SystemMemory.FIVE_TWELVEK);
            SystemMemory.WordSize = WordSize.FourByte;
            SystemMemory.Endian = Endian.Big;
        }

        public void Init(IBoardHost boardHost)
        {
            mBoardHost = boardHost;
            mBoardHost.Loaded += MBoardHost_Loaded;
        }

        private void MBoardHost_Loaded(object sender, EventArgs e)
        {
            //Processor.AddOpCodeOverride(0x0f000000, 0x0f000000, swiFunc);
        }

        private uint swiFunc(object sender, EventArgs args)
        {
            return 1;
        }
    }
}
