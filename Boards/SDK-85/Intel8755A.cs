using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Boards;
using Processors;

namespace SDK_85
{
    /// <summary>
    /// 2K Bytes ROM
    /// 2x8bit IO Ports (PA, PB)
    /// </summary>
    public class Intel8755A
    {
        private IBoardHost mBoardHost;
        private Controls.SDKPanel mSDKPanel;
        private byte mBasePort;

        private byte CSR = 0;
        private byte PORTA;
        private byte PORTB;

        public Intel8755A(IBoardHost boardHost, Controls.SDKPanel sdkPanel)
        {
            mBoardHost = boardHost;
            mSDKPanel = sdkPanel;

        }
        public void RequestResources(uint baseAddr, byte[] Rom, uint basePort)
        {
            if (Rom != null)
                mBoardHost.SystemMemory.CreateMemoryBlock(baseAddr, Rom, 0, (uint)Rom.Length, false);

            mBasePort = (byte)basePort;
            mBoardHost.RequestPortAccess(mBasePort, mBasePort, readPortEventHandler, writePortEventHandler);
        }

        private uint readPortEventHandler(object sender, PortAccessReadEventArgs args)
        {
            return 0;
        }
        private void writePortEventHandler(object sender, PortAccessWriteEventArgs args)
        {
        }
    }
}