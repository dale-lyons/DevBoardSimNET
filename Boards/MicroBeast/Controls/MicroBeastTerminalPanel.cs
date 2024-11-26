using System;
using System.Windows.Forms;

using Boards;
using Terminal;
using static Terminal.TerminalControl;

namespace MicroBeast.Controls
{
    public partial class MicroBeastTerminalPanel : UserControl
    {
        private IBoardHost mIBoardHost;
        private CmdStateMachine mCmdStateMachine;
        public event KeypressDelegate OnKeypress;

        private ushort? mKeyCode;

        public MicroBeastTerminalPanel(IBoardHost boardHost, CmdStateMachine cmdStateMachine)
        {
            mIBoardHost = boardHost;
            mCmdStateMachine = cmdStateMachine;
            InitializeComponent();
            terminalControl1.OnKeypress += terminalControl1_KeyPress;
            keyboard1.OnKeypress += Keyboard1_OnKeypress;
        }

        private void Keyboard1_OnKeypress(ushort code)
        {
            //if (code == 0x0000)
            //{
            //    mKeyCode = null;
            //    return;
            //}
            mKeyCode = code;
        }

        public ushort? GetKeycode()
        {
            return mKeyCode;
        }

        public void terminalControl1_KeyPress(char ch)
        {
            OnKeypress?.Invoke(ch);
        }

        public void Feed(char ch)
        {
            terminalControl1.Feed(ch);
        }

        public void ClearFromCursor()
        {
            terminalControl1.ClearFromCursor();
        }

        public void onSetCharacterDisplayL(byte col, ushort pattern)
        {
            display1.SetCharacterDisplayL(col, pattern);
        }

        public void onSetCharacterDisplayR(byte col, ushort pattern)
        {
            display1.SetCharacterDisplayR(col, pattern);
        }

        private void loadSourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                mIBoardHost.LoadSourceCode(openFileDialog1.FileName);
        }
    }
}