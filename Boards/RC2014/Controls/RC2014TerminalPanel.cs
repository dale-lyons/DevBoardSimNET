using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using Processors;
using Boards;
using Terminal;
using static Terminal.TerminalControl;

namespace RC2014.Controls
{
    public partial class RC2014TerminalPanel : UserControl
    {
        private IBoardHost mBoardHost;
        private CmdStateMachine mCmdStateMachine;
        public event KeypressDelegate OnKeypress;
        public event KeypressDelegate OnSendByte;

        public RC2014TerminalPanel(IBoardHost boardHost, CmdStateMachine cmdStateMachine)
        {
            mBoardHost = boardHost;
            mCmdStateMachine = cmdStateMachine;
            InitializeComponent();
            terminalControl1.OnKeypress += TerminalControl1_OnKeypress;
            terminalControl1.OnSendByte += TerminalControl1_OnSendByte;
        }

        private void TerminalControl1_OnSendByte(char ch)
        {
            OnSendByte?.Invoke(ch);
        }

        private void TerminalControl1_OnKeypress(char ch)
        {
            OnKeypress?.Invoke(ch);
        }

        public void Feed (byte c)
        {
            terminalControl1.Feed((char)c);
        }
    }
}