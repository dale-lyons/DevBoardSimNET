using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Boards;
using Processors;

namespace SDK_85
{
    public class Intel8279
    {
        private IBoardHost mBoardHost;
        private Controls.SDKPanel mSDKPanel;
        private byte mLastChar;

        public Intel8279(IBoardHost boardHost, Controls.SDKPanel sdkPanel)
        {
            mBoardHost = boardHost;
            mSDKPanel = sdkPanel;
    }

        public void RequestResources()
        {
            mBoardHost.RequestMemoryAccess(0x1800, 0x1801, readEventHandler18, writeEventHandler18);
            mBoardHost.RequestMemoryAccess(0x1900, 0x1901, readEventHandler19, writeEventHandler19);
        }

        public uint readEventHandler18(object sender, MemoryAccessReadEventArgs args)
        {
            uint ret = mLastChar;
            mLastChar = (byte)0x80;
            return ret;
        }
        public void writeEventHandler18(object sender, MemoryAccessWriteEventArgs args)
        {
            byte val = (byte)args.Value;
            Debug.WriteLine(string.Format("write to 1800:{0}", val.ToString("X2")));
            mSDKPanel.DisplayWriteSegment(val);
        }
        public uint readEventHandler19(object sender, MemoryAccessReadEventArgs args)
        {
            return 0;
        }
        public void writeEventHandler19(object sender, MemoryAccessWriteEventArgs args)
        {
            byte val = (byte)args.Value;
            Debug.WriteLine(string.Format("wtite to 1900:{0}", val.ToString("X2")));
            if (val == 0x00)
            {
                mSDKPanel.SetIndex(0);
            }
            else if ((val & 0xe0) == 0x80)
            {
                int index = (val & 0x0f);
                mSDKPanel.SetIndex(index);
            }
        }
    }
}