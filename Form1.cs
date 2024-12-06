using System;
using System.Windows.Forms;
using System.IO;
using System.Drawing;

using Processors;
using Boards;
using GUI;
using WeifenLuo.WinFormsUI.Docking;
using Parsers;

namespace DevBoardSim.NET
{
    public partial class Form1 : Form
    {
        public bool RestartForm { get; set; }

        private long tmrTicksStart;
        private AppPreferencesConfig mPreferencesConfig;
        private DeserializeDockContent mDeserializeDockContent;
        private IView[] mViews = new IView[8];

        public GUI.Code.CodeView CodeView
        {
            get { return mViews[(int)ViewEnums.CodeT] as GUI.Code.CodeView; }
            set { mViews[(int)ViewEnums.CodeT] = value; }
        }

        public GUI.Stack.StackView StackView
        {
            get { return mViews[(int)ViewEnums.StackT] as GUI.Stack.StackView; }
            set { mViews[(int)ViewEnums.StackT] = value; }
        }

        public GUI.Watch.Watch Watch
        {
            get { return mViews[(int)ViewEnums.WatchT] as GUI.Watch.Watch; }
            set { mViews[(int)ViewEnums.WatchT] = value; }
        }

        public GUI.Registers.RegistersView RegistersView
        {
            get { return mViews[(int)ViewEnums.RegistersT] as GUI.Registers.RegistersView; }
            set { mViews[(int)ViewEnums.RegistersT] = value; }
        }
        public GUI.Output.OutputView OutputView
        {
            get { return mViews[(int)ViewEnums.OutputT] as GUI.Output.OutputView; }
            set { mViews[(int)ViewEnums.OutputT] = value; }
        }
        public GUI.Memory.MemoryView MemoryView
        {
            get { return mViews[(int)ViewEnums.MemoryT] as GUI.Memory.MemoryView; }
            set { mViews[(int)ViewEnums.MemoryT] = value; }
        }
        public GUI.Board.BoardView BoardView
        {
            get { return mViews[(int)ViewEnums.BoardT] as GUI.Board.BoardView; }
            set { mViews[(int)ViewEnums.BoardT] = value; }
        }
        public GUI.OpCodeTrace.OpcodeTraceView OpcodeTraceView
        {
            get { return mViews[(int)ViewEnums.OpCodeTraceT] as GUI.OpCodeTrace.OpcodeTraceView; }
            set { mViews[(int)ViewEnums.OpCodeTraceT] = value; }
        }

        public IBoard Board { get; private set; }
        public IBoardHost BoardHost { get; private set; }
        private IProcessor Processor { get { return Board.Processor; } }

        private IView QueryViewF(ViewEnums view)
        {
            switch (view)
            {
                case ViewEnums.WatchT: return Watch;
                case ViewEnums.RegistersT: return RegistersView;
                case ViewEnums.OpCodeTraceT: return OpcodeTraceView;
                case ViewEnums.CodeT: return CodeView;
                case ViewEnums.BoardT: return BoardView;
                case ViewEnums.OutputT: return OutputView;
                case ViewEnums.MemoryT: return MemoryView;
            }
            return null;
        }

        public Form1()
        {
            InitializeComponent();
            AutoScaleMode = AutoScaleMode.Dpi;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var exe = System.Reflection.Assembly.GetExecutingAssembly();
            var names = exe.GetManifestResourceNames();
            var iconStream = exe.GetManifestResourceStream("DevBoardSim.NET.Resources.sound-card_1741713.ico");
            this.Icon = new Icon(iconStream);

            mPreferencesConfig = Preferences.PreferencesBase.Load<AppPreferencesConfig>(AppPreferencesConfig.Key);
            Board = Boards.Boards.GetBoard(mPreferencesConfig.ActiveBoard);
            this.Text = "DevBoardSim Board:" + mPreferencesConfig.ActiveBoard;
            CodeLine.Processor = Board.Processor;

            CodeView = new GUI.Code.CodeView(QueryViewF, Processor);
            Watch = new GUI.Watch.Watch(QueryViewF, Processor, mPreferencesConfig.WatchConfig);
            StackView = new GUI.Stack.StackView(QueryViewF, Processor, mPreferencesConfig.StackConfig);
            RegistersView = new GUI.Registers.RegistersView(QueryViewF, Processor);
            OutputView = new GUI.Output.OutputView(QueryViewF, Processor);
            MemoryView = new GUI.Memory.MemoryView(QueryViewF, Processor, mPreferencesConfig.MemoryConfig);
            OpcodeTraceView = new GUI.OpCodeTrace.OpcodeTraceView(QueryViewF, Processor);
            BoardView = new GUI.Board.BoardView(QueryViewF, Processor);

            BoardHost = new BoardHost(Board, BoardView, this);
            Board.Init(BoardHost);
            Processor.OnCycleCount += BoardHost.FireCycles;

            BoardHost.FireLoaded();
            RegistersView.panel.Controls.Add(Processor.RegistersView.UIPanel);

            mDeserializeDockContent = new DeserializeDockContent(GetContentFromPersistString);
            string configFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "DockPanel.config");
            if (File.Exists(configFile))
                dockPanel.LoadFromXml(configFile, mDeserializeDockContent);

            CodeView.Show(dockPanel);
            updateCPUStatus(Color.Red, "Stopped");

            if (mPreferencesConfig.ActiveBoard == "GW8085SBC.GW8085SBCBoard")
            {
                var filename = @"C:\Projects\DevBoardSimNET\Boards\GW8085SBC\ROM\Monitor\ddt.asm";
                var parser = Parsers.Parsers.GetParser(Processor.ParserName, Processor);
                if (parser == null)
                    return;
                var parseSettings = new ParseSettings { Listing = true, Bin = true, Path = Path.GetDirectoryName(filename) };
                parser.Parse(filename, parseSettings);
                if (parser.Errors.Count > 0)
                {
                    var ov = OutputView;
                    foreach (var error in parser.Errors)
                        ov.WriteLine(string.Format("Compile errror Line:{0} Text:{1}", error.Line, error.Text));
                    MessageBox.Show("Error on assembly!");
                }
                CodeView.AddCompiledCode(parser, true);
            }

            //DEBUG
            else if (mPreferencesConfig.ActiveBoard == "MicroBeast.MicroBeastboard")
            {
                var filename = @"C:\Projects\DevBoardSimNET\Boards\MicroBeast\Firmware\firmware.asm";
                var parser = Parsers.Parsers.GetParser(Processor.ParserName, Processor);
                var parseSettings = new ParseSettings { Listing = true, Bin = true, Path = Path.GetDirectoryName(filename) };
                parser.Parse(filename, parseSettings);
                if (parser.Errors.Count > 0)
                {
                    var ov = OutputView;
                    foreach (var error in parser.Errors)
                        ov.WriteLine(string.Format("Compile errror Line:{0} Text:{1}", error.Line, error.Text));
                    MessageBox.Show("Error on assembly!");
                }
                CodeView.AddCompiledCode(parser, true);

                filename = @"C:\Projects\DevBoardSimNET\Boards\MicroBeast\Firmware\build\monitor.asm";
                parser = Parsers.Parsers.GetParser(Processor.ParserName, Processor);
                parseSettings = new ParseSettings { Listing = true, Bin = true, Path = Path.GetDirectoryName(filename) };
                parser.Parse(filename, parseSettings);
                if (parser.Errors.Count > 0)
                {
                    var ov = OutputView;
                    foreach (var error in parser.Errors)
                        ov.WriteLine(string.Format("Compile errror Line:{0} Text:{1}", error.Line, error.Text));
                    MessageBox.Show("Error on assembly!");
                }
                CodeView.AddCompiledCode(parser, true);
            }

            //string filename = @"C:\Projects\6502\nes-test-roms-master\instr_test-v5\source\dale.nes";
            //var bytes = File.ReadAllBytes(filename);
            //var bytes2 = new byte[SystemMemory.THIRTYTWOK];
            //Array.Copy(bytes, 16, bytes2, 0, SystemMemory.THIRTYTWOK);
            //mIBoardHost.LoadBinaryCode(0x8000, bytes2);
            //ushort pc = (ushort)Processor.SystemMemory.GetMemory(0xfffc, WordSize.TwoByte, false);
            //Processor.Registers.PC = pc;

            //var filename = @"C:\Projects\DevBoardSimNET\Boards\Z80MembershipCard\ROM\zmcv15.z";
            ////            var filename = @"C:\Projects\DevBoardSimNET\Boards\HobbyPCB6502\Docs\uchess.asm";
            //var parser = Processor.CreateParser();
            //var parseSettings = new ParseSettings { Listing = true, Bin = true, Path = Path.GetDirectoryName(filename) };
            //parser.Parse(filename, parseSettings);

            RefreshAllViews();
        }

        private void SystemMemory_OnInvalidMemoryAccess(uint address)
        {
            if (mPreferencesConfig.StopOnInvalidMemoryAccess)
            {
                Processor.Stop();
                OutputView.WriteLine(string.Format("Invalid memory accessed at {0:X4} PPC {1}", address, Processor.Registers.PC));
                OutputView.WriteLine("simulation stopped.");
            }
        }

        private void Processor_InvalidInstruction(byte opcode, uint pc)
        {
            if (mPreferencesConfig.StopOnInvalidInstruction)
            {
                Processor.Stop();
                OutputView.WriteLine(string.Format("Invalid instruction {0} found at {1:X4}", opcode, pc));
                OutputView.WriteLine("simulation stopped.");
            }
        }

        private IDockContent GetContentFromPersistString(string persistString)
        {
            if (persistString == typeof(GUI.Registers.RegistersView).ToString())
                return RegistersView;
            else if (persistString == typeof(GUI.Watch.Watch).ToString())
                return Watch;
            else if (persistString == typeof(GUI.Stack.StackView).ToString())
                return StackView;
            else if (persistString == typeof(GUI.Code.CodeView).ToString())
                return CodeView;
            else if (persistString == typeof(GUI.Output.OutputView).ToString())
                return OutputView;
            else if (persistString == typeof(GUI.Memory.MemoryView).ToString())
                return MemoryView;
            else if (persistString == typeof(GUI.Board.BoardView).ToString())
                return BoardView;
            else if (persistString == typeof(GUI.OpCodeTrace.OpcodeTraceView).ToString())
                return OpcodeTraceView;
            else
                return null;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Processor.Stop();
            Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Processor.Stop();

            DateTime now = DateTime.Now;
            while ((DateTime.Now - now).Milliseconds < 500)
                Application.DoEvents();

            string configFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "DockPanel.config");
            dockPanel.SaveAsXml(configFile);

            mPreferencesConfig.MemoryConfig = MemoryView.Config;
            mPreferencesConfig.WatchConfig = Watch.mWatchConfig;
            AppPreferencesConfig.Save<AppPreferencesConfig>(mPreferencesConfig, AppPreferencesConfig.Key);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Processor.IsRunning)
                return;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var parser = Parsers.Parsers.GetParser(Processor.ParserName, Processor);
                var parseSettings = new ParseSettings { Hex = true, Listing = true, Bin = true, Path = openFileDialog1.FileName };
                parser.Parse(openFileDialog1.FileName, parseSettings);
                if (parser.Errors.Count > 0)
                {
                    var dialog = new ErrorDialog(parser.Errors);
                    dialog.ShowDialog();
                    return;
                }
                CodeView.AddCompiledCode(parser, true);
                RefreshAllViews();
            }
        }
        public void StepStartAllViews()
        {
            foreach (var view in mViews)
                view.Start();
        }
        public void StepStopAllViews()
        {
            foreach (var view in mViews)
                view.Stop();
        }

        public void RefreshAllViews()
        {
            foreach (var view in mViews)
                view.RefreshView();
        }

        private void HideShowView(ToolWindows tw)
        {
            if (tw.IsHidden)
                tw.Show(dockPanel);
            else
                tw.Hide();
        }
        private void toolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Processor.IsRunning)
                return;

            var tm = sender as ToolStripMenuItem;
            switch (tm.Tag as string)
            {
                case "Board":
                    HideShowView(BoardView);
                    return;
                case "OpCodeTrace":
                    HideShowView(OpcodeTraceView);
                    return;
                case "Memory":
                    HideShowView(MemoryView);
                    return;
                case "Registers":
                    HideShowView(RegistersView);
                    return;
                case "Watch":
                    HideShowView(Watch);
                    return;
                case "Stack":
                    HideShowView(StackView);
                    return;
                case "Output":
                    HideShowView(OutputView);
                    return;
                default:
                    break;
            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Processor.Stop();
            this.RefreshAllViews();
        }

        private void Preferences_Click(object sender, EventArgs e)
        {
            if (Processor.IsRunning)
                return;

            var dialog = new AppPreferencesForm(mPreferencesConfig);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                AppPreferencesConfig.Save<AppPreferencesConfig>(mPreferencesConfig, AppPreferencesConfig.Key);
                if (dialog.ResetSimulation)
                    RestartForm = true;
                this.Close();
            }
        }

        private void updateCPUStatus(Color color, string text)
        {
            CPUStatus.Text = text;
            CPUStatus.BackColor = color;
        }

        private void loadHexFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.Filter = "hix files (*.ihx)|*.ihx|hex files (*.hex)|*.hex|All files (*.*)|*.*";
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            var hf = HexFile.Load(dialog.FileName);
            if (hf == null)
                return;
            //CodeView.AddBinaryCode(hf.Bytes, hf.StartAddress, hf.EndAddress);
            RefreshAllViews();
        }

        public void LoadBinaryCode(uint addr, byte[] bytes)
        {
            uint start = Processor.SystemMemory.GetMemory(0xfffc, WordSize.TwoByte, false);
            //CodeView.AddBinaryCode(bytes, start);
            RefreshAllViews();
        }

        private void loadSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "S19 files (*.s19)|*.s19";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var s19 = new S19File();
                s19.Load(openFileDialog1.FileName);
                //CodeView.AddBinaryCode(s19.Bytes, 0xe000);
                RefreshAllViews();
            }
        }

        private void loadBinaryFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "binary files|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var bytes = File.ReadAllBytes(openFileDialog1.FileName);
                //CodeView.AddBinaryCode(bytes, 0x8000);
                RefreshAllViews();
            }
        }

        private void hexToBinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.Filter = "hex files (*.hex)|*.hex|All files (*.*)|*.*";
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            var hf = HexFile.Load(dialog.FileName);
            if (hf == null)
                return;

            string binName = Path.ChangeExtension(dialog.FileName, "bin");
            using (var stream = File.Create(binName))
                stream.Write(hf.Bytes, 0, hf.Length);
        }

        private void loadLSTIHXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.Filter = "hex files (*.hex)|*.ihx|Lst files (*.lst)|*.lst";
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            var hf = HexFile.Load(Path.ChangeExtension(dialog.FileName, ".ihx"));
            if (hf == null)
                return;

            var parser = Parsers.Parsers.GetParser(Processor.ParserName, Processor);
            var codeLines = parser.ParseListingFile(Path.ChangeExtension(dialog.FileName, ".lst"));
            //            CodeView.AddCompiledCode(Processor, codeLines, hf);
            RefreshAllViews();
        }

        private void stepOverToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Processor.IsHalted || Processor.IsRunning)
                return;

            StepStartAllViews();
            Processor.StepOver();
            StepStopAllViews();
            RefreshAllViews();
        }

        private void stepIntoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Processor.IsHalted || Processor.IsRunning)
                return;
            StepStartAllViews();
            Processor.StepInto();
            StepStopAllViews();
            RefreshAllViews();
        }

        private void goToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Processor.IsHalted || Processor.IsRunning)
                return;

            tmrTicksStart = DateTime.Now.Ticks;
            Processor.CycleCount = 0;
            timer1.Enabled = true;
            updateCPUStatus(Color.Green, "Running");
            Processor.ExecuteUntilBreakpoint();
            updateCPUStatus(Color.Red, "Stopped");
            timer1.Enabled = false;
            RefreshAllViews();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            var elapsedSpan = new TimeSpan(now.Ticks - tmrTicksStart);
            //tmrTicksStart = now.Ticks;
            ulong cycles = Processor.CycleCount;
            var speed = cycles / elapsedSpan.TotalSeconds / 1000000;
            CPUSpeed.Text = string.Format("Cycles Per Second {0:0.00}MHz", speed);
        }

        private void debugToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            bool enabled = (Processor != null && !Processor.IsRunning && !Processor.IsHalted);

            var tsm = (sender as ToolStripMenuItem);
            var items = tsm.DropDownItems;
            foreach (var item in items)
            {
                ToolStripMenuItem tsmi = item as ToolStripMenuItem;
                if (tsmi == null)
                    continue;
                switch (tsmi.Text)
                {
                    case "Step Into":
                    case "&Step Over":
                    case "&Go":
                        tsmi.Enabled = enabled;
                        break;
                    case "Stop":
                        tsmi.Enabled = !enabled;
                        break;
                    default:
                        break;
                }
            }//foreach
        }
    }
}