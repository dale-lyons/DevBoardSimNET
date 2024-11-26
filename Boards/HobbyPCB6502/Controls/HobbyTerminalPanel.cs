using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Processors;
using Boards;
using Terminal;
using System.Threading;
using System.IO;
using static Terminal.TerminalControl;

namespace HobbyPCB6502.Controls
{
    public delegate void FeedDelegate(char ch);
    public delegate char GetKeyDelegate();
    public delegate bool KeyAvailableDelegate();

    public partial class HobbyTerminalPanel : UserControl
    {
        private IBoardHost mIBoardHost;
        private CmdStateMachine mCmdStateMachine;
        private bool mROMEnabled;
        public event KeypressDelegate OnKeypress;

        public HobbyTerminalPanel(IBoardHost boardHost, CmdStateMachine cmdStateMachine, bool romEnabled)
        {
            mIBoardHost = boardHost;
            mCmdStateMachine = cmdStateMachine;
            mROMEnabled = romEnabled;
            InitializeComponent();
            terminalControl1.OnKeypress += terminalControl1_KeyPress;
        }

        public void terminalControl1_KeyPress(char ch) { OnKeypress?.Invoke(ch); }

        public bool KeyAvailable()
        {
            return false;
        }

        public void Feed(byte data)
        {
            terminalControl1.Feed((char)data);
        }
        public char GetKey()
        {
            return ' ';
        }

        private void loadNESFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "NES files (*.nes)|*.nes";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var bytes = File.ReadAllBytes(openFileDialog1.FileName);
                var romSize = bytes[4] * SystemMemory.SIXTEENK;
                var bytes2 = new byte[SystemMemory.THIRTYTWOK];
                Array.Copy(bytes, 16, bytes2, 0, SystemMemory.THIRTYTWOK);

                mIBoardHost.Processor.SystemMemory.Copy((uint)SystemMemory.THIRTYTWOK, bytes2);
                uint addr = mIBoardHost.SystemMemory.GetMemory(0xfffc, WordSize.TwoByte, false);
                mIBoardHost.Processor.Registers.PC = addr;
            }
        }
    }
}