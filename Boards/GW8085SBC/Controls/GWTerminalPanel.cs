using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using Boards;
using Terminal;
using static Terminal.TerminalControl;
using Parsers;

namespace GW8085SBC.Controls
{
    public partial class GWTerminalPanel : UserControl
    {
        private IBoardHost mIBoardHost;
        private CmdStateMachine mCmdStateMachine;
        private bool mROMEnabled;
        public event KeypressDelegate OnKeypress;

        public GWTerminalPanel(IBoardHost boardHost, CmdStateMachine cmdStateMachine, bool romEnabled)
        {
            mIBoardHost = boardHost;
            mCmdStateMachine = cmdStateMachine;
            mROMEnabled = romEnabled;
            InitializeComponent();
            terminalControl1.OnKeypress += terminalControl1_KeyPress;
        }

        public void terminalControl1_KeyPress(char ch) { OnKeypress?.Invoke(ch); }

        public void Feed(char ch) { terminalControl1.Feed((char)ch); }

        private void setLED(int led, bool value)
        {
            if (InvokeRequired)
                Invoke((Action)delegate { leds1.SetLED(led, value); });
            else
                leds1.SetLED(led, value);
        }

        public void SetROMLED(bool value) { setLED(2, value); }
        public void SetPWRLED(bool value) { setLED(0, value); }
        public void SetUSRLED(bool value) { setLED(1, value); }
        public void SelectedBank(int page)
        {
            if (InvokeRequired)
                Invoke((Action)delegate { label5.Text = page.ToString(); });
            else
                label5.Text = page.ToString();
        }

        //private void loadBINToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    if (openFileDialog1.ShowDialog() == DialogResult.OK)
        //    {
        //        byte[] bytes = File.ReadAllBytes(openFileDialog1.FileName);
        //        var sm = mIBoardHost.Processor.SystemMemory;

        //        uint address = 0x0100;
        //        int index = 0;
        //        for (; index < bytes.Length; address++, index++)
        //        {
        //            sm.SetMemory(address, WordSize.OneByte, bytes[index], false);
        //        }
        //    }
        //}

        public delegate void sendChar(char c);
        public delegate void idleFunction();

        private void sc(char c)
        {
            mCmdStateMachine.SendByte((byte)c);
            //mIBoardHost.FireInterupt("Rst65");
            idle();
        }

        private void idle()
        {
            //Application.DoEvents();
            //Thread.Sleep(10);
            System.Windows.Forms.Application.DoEvents();
        }

        private void sendToDownload(string fileName, byte[] fileBytes, CmdStateMachine cmdStateMachine, sendChar sc, idleFunction idle)
        {
            short len = (short)((fileBytes.Length + 127) / 128);

            byte[] bytes = new byte[len * 128];
            Array.Copy(fileBytes, bytes, fileBytes.Length);

            sc(':');
            string blocks = len.ToString("X4");
            foreach (var c in blocks)
                sc(c);

            string name = Path.GetFileNameWithoutExtension(fileName).ToUpper();
            string ext = Path.GetExtension(fileName);
            byte[] fbytes = System.Text.ASCIIEncoding.ASCII.GetBytes(name);
            if (fbytes.Length > 8)
                return;

            foreach (var b in fbytes)
                sc((char)b);
            for (int ii = 0; ii < (8 - fbytes.Length); ii++)
                sc(' ');

            if (ext.StartsWith("."))
                ext = ext.Substring(1).ToUpper();

            fbytes = System.Text.ASCIIEncoding.ASCII.GetBytes(ext);
            foreach (var b in fbytes)
                sc((char)b);
            for (int ii = 0; ii < (3 - fbytes.Length); ii++)
                sc(' ');

            sc((char)0x0d);

            for (int sector = 0; sector < len; sector++)
            {
                idle();
                while (!cmdStateMachine.IsEscapeOn) { idle(); }
                cmdStateMachine.IsEscapeOn = false;

                ushort cs = 0;
                int addr = sector * 128;
                for (int yy = 0; yy < 128; yy++)
                {
                    byte b = bytes[addr + yy];
                    cs += b;
                    sc((char)b);
                }
                var csBytes = BitConverter.GetBytes(cs);
                sc((char)csBytes[0]);
                sc((char)csBytes[1]);
            }
        }

        private void loadHexFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.Filter = "hex files (*.hex)|*.hex|All files (*.*)|*.*";
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            byte[] bytes = File.ReadAllBytes(dialog.FileName);
            mCmdStateMachine.SendBytes(bytes);
        }

        private void createBootBinToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            var dir = Path.Combine(Application.StartupPath, @"..\..\Boards\GW8085SBC\ROM\Monitor\");
            bool result = CreateBootBin(dir, "boot.asm", "ddt.asm", "CCP.asm", "BDOS.asm");
            if (!result)
            {
                MessageBox.Show("error in assembly");
            }
        }

        private bool CreateBootBin(string baseDir, string boot, string ddt, string ccp, string bdos)
        {
            //string outputDirectory = Path.GetDirectoryName(baseDir);
            byte[] blanks = new byte[(32 * 1024)];

            var bootBytes = handlePartialBootImage(baseDir, boot);
            if (bootBytes == null)
                return false;
            Array.Copy(bootBytes, 0, blanks, 0, bootBytes.Length);

            var ram_bootBytes = handlePartialBootImage(baseDir, "ram_boot.asm");
            if (ram_bootBytes == null)
                return false;
            Array.Copy(ram_bootBytes, 0, blanks, bootBytes.Length, ram_bootBytes.Length);

            var ddtBytes = handlePartialBootImage(baseDir, ddt);
            if (ddtBytes == null)
                return false;
            Array.Copy(ddtBytes, 0, blanks, 4 * 1024, ddtBytes.Length);

            var ccpBytes = handlePartialBootImage(baseDir, ccp);
            if (ccpBytes == null)
                return false;
            Array.Copy(ccpBytes, 0, blanks, 12 * 1024, ccpBytes.Length);

            var bdosBytes = handlePartialBootImage(baseDir, bdos);
            if (bdosBytes == null)
                return false;
            Array.Copy(bdosBytes, 0, blanks, 16 * 1024, bdosBytes.Length);

            string bootImage = Path.Combine(baseDir, "bootImage.bin");
            File.WriteAllBytes(bootImage, blanks);
            return true;
        }

        private byte[] handlePartialBootImage(string dir, string fileName)
        {
            string fname = Path.Combine(dir, fileName);
            var parseSettings = new ParseSettings { Path = fname, Debug = true };
            var parser = Parsers.Parsers.GetParser(mIBoardHost.Processor.ParserName, mIBoardHost.Processor);
            //var parser = mIBoardHost.Processor.CreateParser();
            parser.Parse(fname, parseSettings);
            if (parser.Errors.Count > 0)
                return null;

            var sections = parser.Sections;
            //int length = (int)(sections.EndAddress - sections.StartAddress);
            string outputFile = Path.Combine(dir, Path.GetFileNameWithoutExtension(fileName));
            BinaryFile.Create(Path.ChangeExtension(outputFile, ".bin"), parser.SystemMemory, sections);
            //            PRNFile.Create(Path.ChangeExtension(outputFile, ".prn"), parser.SystemMemory, parser.CodeLines);

            return File.ReadAllBytes(Path.ChangeExtension(outputFile, ".bin"));
        }

        private void uploadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.Filter = "all files |*.*";
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            byte[] bytes = File.ReadAllBytes(dialog.FileName);
            sendToDownload(dialog.FileName, bytes, mCmdStateMachine, sc, idle);
        }
    }
}