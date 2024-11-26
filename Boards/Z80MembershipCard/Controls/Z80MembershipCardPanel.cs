using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using Boards;
using Terminal;

namespace Z80MembershipCard.Controls
{
    public partial class Z80MembershipCardPanel : UserControl
    {
        private IBoardHost mIBoardHost;
        private CmdStateMachine mCmdStateMachine;
        private Z80MembershipConfig mZ80MembershipConfig;
        private SerialPortStateMachine mmSerialPortStateMachine;

        //private char mTermKey;
        //public char TermKey
        //{
        //    get
        //    {
        //        return mTermKey;
        //    }
        //}
        //public void ClearTermKey() { mTermKey = (char)0; }
        public Z80MembershipCardPanel(IBoardHost boardHost, CmdStateMachine cmdStateMachine, Z80MembershipConfig z80MembershipConfig, SerialPortStateMachine serialPortStateMachine)
        {
            mIBoardHost = boardHost;
            mCmdStateMachine = cmdStateMachine;
            mZ80MembershipConfig = z80MembershipConfig;
            mmSerialPortStateMachine = serialPortStateMachine;
            InitializeComponent();

            //terminalControl1.OnKeypress += TerminalControl1_OnKeypress;
        }

        public void Feed(byte data)
        {
            terminalControl1.Feed((char)data);
        }

        public TerminalControl Terminal {  get {  return terminalControl1; } }

        private void sendHexFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.Filter = "hex files (*.hex)|*.hex|All files (*.*)|*.*";
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            byte[] bytes = File.ReadAllBytes(dialog.FileName);
            mCmdStateMachine.SendBytes(bytes);
        }

        private void loadHexFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new Preferences(mZ80MembershipConfig);
            if (dialog.ShowDialog() != DialogResult.OK)
                return;




        }

        private void sendSpaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mmSerialPortStateMachine.BytesToSend.Add((byte)' ');
        }
    }
}