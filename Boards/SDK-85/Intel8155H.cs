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
    /// <summary>
    /// 256Bytes of RAM
    /// 2x8 bit io Ports(PA, PB)
    /// 1x6 bit IO Port(PC)
    /// 1x14 bit timer
    /// </summary>
    public class Intel8155H
    {
        private IBoardHost mBoardHost;
        private Controls.SDKPanel mSDKPanel;
        private Timer14 mTimer = new Timer14();

        private byte mBasePort;

        private byte CSR = 0;

        private bool portAOutput;
        private bool portBOutput;
        private byte portCMode;
        private bool portAIE;
        private bool portBIE;

        private byte PORTA;
        private byte PORTB;

        public Intel8155H(IBoardHost boardHost, Controls.SDKPanel sdkPanel)
        {
            mBoardHost = boardHost;
            mSDKPanel = sdkPanel;
        }

        public byte GetCSR() { return CSR; }

        public void GetIOPortA(out byte val, out byte conf)
        {
            val = PORTA;
            conf = 0xff;
        }
        public void GetIOPortB(out byte val, out byte conf)
        {
            val = PORTB;
            conf = 0xff;
        }
        public void GetIOPortC(out byte val, out byte conf)
        {
            val = 0;
            conf = 0;
        }

        public void RequestResources(byte basePort)
        {
            mBasePort = basePort;
            mBoardHost.SystemMemory.CreateMemoryBlock(0x2000, new byte[256], 0, 256, true);
            mBoardHost.RequestPortAccess(basePort, (byte)(basePort + 2), readPortEventHandler, writePortEventHandler);
            mBoardHost.Cycles += MBoardHost_Cycles;
        }

        private void MBoardHost_Cycles(object sender, CyclesEventArgs e)
        {
            if (mTimer.Cycles(e.CycleCount))
            {
                mBoardHost.FireInterupt("Trap");
            }
        }

        public uint readPortEventHandler(object sender, PortAccessReadEventArgs args)
        {
            if (args.Port == mBasePort)
            {//read the CSR: note always returns 0
                return 0;
            }
            return 0;
        }
        public void writePortEventHandler(object sender, PortAccessWriteEventArgs args)
        {
            if (args.Port == mBasePort)
            {//write to the CSR
                CSR = args.Data;
                Debug.WriteLine(String.Format("Write to CSR:{0}", CSR.ToString("X2")));

                portAOutput = (CSR & 0x01) == 0x01;
                portBOutput = (CSR & 0x02) == 0x02;
                portCMode = (byte)((CSR & 0x0c) >> 2);
                portAIE = (CSR & 0x10) == 0x10;
                portBIE = (CSR & 0x20) == 0x20;

                byte timerCMD = (byte)((CSR & 0xc0) >> 6);
                mTimer.Command(timerCMD);
            }
            else if (args.Port == mBasePort + 1)
            {//write to PORTA
                if (!portAOutput)
                    return;
                PORTA = args.Data;
                mSDKPanel.PortA.Invalidate();
            }
            else if (args.Port == mBasePort + 2)
            {//write to PORTB
                if (!portBOutput)
                    return;
                PORTB = args.Data;
                mSDKPanel.PortB.Invalidate();

            }
            else if (args.Port == mBasePort + 3)
            {//write to PORTC
            }
            else if (args.Port == mBasePort + 4)
            {//write TIMER


            }
        }
    }
}