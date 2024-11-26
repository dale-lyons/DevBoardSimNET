using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Boards;
using Processors;
using System.Diagnostics;

namespace SDK_85
{
    public delegate void GetIOPortDelegate(out byte val, out byte conf);
    public delegate byte GetCSRDelegate();

    public class SDK85Board : IBoard
    {
        private IBoardHost mBoardHost;

        public string Name { get { return "SDK-85 Development Kit"; } }

        public string ProcessorName { get { return "Intel8085.Intel8085"; } }
//        public string ProcessorName { get { return "ZilogZ80.ZilogZ80"; } }


        public IProcessor Processor { get; set; }
        public ISystemMemory SystemMemory { get; private set; }

        private Panel mPanel;

        private byte mLastChar;

        private Controls.SDKPanel mSDKPanel;

        private Intel8155H mIntel8155H0;
        private Intel8155H mIntel8155H1;
        private Intel8279 mIntel8279;
        private Intel8755A mIntel8755A0;
        private Intel8755A mIntel8755A1;

        private SDKConfig mSDKConfig;
        private byte[] mRom;

        public SDK85Board()
        {
            SystemMemory = new SystemMemory();
            SystemMemory.WordSize = WordSize.TwoByte;
            SystemMemory.Endian = Endian.Little;

            mRom = new byte[2 * 1024];
            var assembly = Assembly.GetExecutingAssembly();
            string[] names = assembly.GetManifestResourceNames();

            using (Stream stream = assembly.GetManifestResourceStream("SDK_85.ROM.SDK-85.bin"))
            {
                stream.Read(mRom, 0, mRom.Length);
            }
        }

        public void CycleCount(int count)
        {
//            mIntel8155H0.CycleCount(count);
        }

        public void Init(IBoardHost boardHost)
        {
            mBoardHost = boardHost;
            mBoardHost.Loaded += MBoardHost_Loaded;
            mBoardHost.Unload += MBoardHost_Unload;

            mSDKConfig = mBoardHost.LoadConfig(SDKConfig.Key, typeof(SDKConfig)) as SDKConfig;
            if (mSDKConfig == null)
                mSDKConfig = new SDKConfig();
        }

        private void MBoardHost_Loaded(object sender, EventArgs e)
        {
            mPanel = mBoardHost.RequestPanel("sdk-85");

            mSDKPanel = new Controls.SDKPanel();
            mSDKPanel.OnKeypress += KeyPanel_OnKeypress;
            mSDKPanel.onReset += MSDKPanel_onReset;

            mPanel.Controls.Add(mSDKPanel);
            mIntel8279 = new Intel8279(mBoardHost, mSDKPanel);
            mIntel8279.RequestResources();

            mIntel8155H0 = new Intel8155H(mBoardHost, mSDKPanel);
            mIntel8155H0.RequestResources(0x20);
            mSDKPanel.PortA.GetIOPort += mIntel8155H0.GetIOPortA;
            mSDKPanel.PortB.GetIOPort += mIntel8155H0.GetIOPortB;
            mSDKPanel.PortC.GetIOPort += mIntel8155H0.GetIOPortC;
            mSDKPanel.GetCSR1 += mIntel8155H0.GetCSR;

            if (mSDKConfig.SecondRAM)
            {
                mIntel8155H1 = new Intel8155H(mBoardHost, mSDKPanel);
                mIntel8155H1.RequestResources(0x28);
                mSDKPanel.GetCSR2 += mIntel8155H1.GetCSR;
            }

            mIntel8755A0 = new Intel8755A(mBoardHost, mSDKPanel);
            mIntel8755A0.RequestResources(0x0, mRom, 0x0);

            if (mSDKConfig.UseSecondROM)
            {
                if (!string.IsNullOrEmpty(mSDKConfig.SecondROMImage))
                {
                    byte[] romImage = null;
                    if (File.Exists(mSDKConfig.SecondROMImage))
                    {
                        romImage = File.ReadAllBytes(mSDKConfig.SecondROMImage);
                    }
                    mIntel8755A1 = new Intel8755A(mBoardHost, mSDKPanel);
                    mIntel8755A1.RequestResources(0x0800, romImage, 0x8);
                }
            }
        }

        private void MSDKPanel_onReset()
        {
            mBoardHost.FireInterupt("Rst0");
        }

        private void KeyPanel_OnKeypress(byte c)
        {
            mLastChar = c;
            mBoardHost.FireInterupt("Rst55");
        }

        private void MBoardHost_Unload(object sender, EventArgs e)
        {
        }

    }
}