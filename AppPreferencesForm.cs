using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Boards;
using Preferences;
using Processors;
using Parsers;

namespace DevBoardSim.NET
{
    public partial class AppPreferencesForm : Form
    {
        private PreferencesEditorTabPage mApplicationTab;
        private PreferencesEditorTabPage mBoardTab;
        private PreferencesEditorTabPage mProcessorTab;
        private PreferencesEditorTabPage mParserTab;
        public bool ResetSimulation { get; set; }
        private AppPreferencesConfig mAppPreferencesConfig;

        public AppPreferencesForm(AppPreferencesConfig appPreferencesConfig)
        {
            mAppPreferencesConfig = appPreferencesConfig;
            InitializeComponent();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            mApplicationTab.SaveSettings();
            mBoardTab.SaveSettings();
            mProcessorTab.SaveSettings();
            mParserTab.SaveSettings();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void DefaultValues_Click(object sender, EventArgs e)
        {
            mApplicationTab.Default();
            mBoardTab.Default();
            mProcessorTab.Default();
            mParserTab.Default();
        }

        private void AppPreferencesForm_Load(object sender, EventArgs e)
        {
            Preferences.ActiveBoardEditor.mBoards = new List<string>();
            foreach (var board in Boards.Boards.AvailableBoards)
                Preferences.ActiveBoardEditor.mBoards.Add(board.FullName);

            Preferences.ActiveParserEditor.mParsers = new List<string>();
            foreach (var parser in Parsers.Parsers.AvailableParsers)
                Preferences.ActiveParserEditor.mParsers.Add(parser);

            mApplicationTab = new PreferencesEditorTabPage(TabpagePropertyGridEnum.Application, mAppPreferencesConfig, null);
            mApplicationTab.OnSelctionChanged += ApplicationTab_OnSelctionChanged;
            this.tabControl1.TabPages.Add(mApplicationTab);
            mBoardTab = new PreferencesEditorTabPage(TabpagePropertyGridEnum.Board, mAppPreferencesConfig, mAppPreferencesConfig.ActiveBoard);
            this.tabControl1.TabPages.Add(mBoardTab);
            var activeBoard = Boards.Boards.GetBoard(mAppPreferencesConfig.ActiveBoard);
            mProcessorTab = new PreferencesEditorTabPage(TabpagePropertyGridEnum.Processor, mAppPreferencesConfig, activeBoard.ProcessorName);
            mProcessorTab.OnSelctionChanged += ApplicationTab_OnSelctionChanged;
            this.tabControl1.TabPages.Add(mProcessorTab);
            var processor = Processors.Processors.GetProcessor(activeBoard.ProcessorName, null);
            mParserTab = new PreferencesEditorTabPage(TabpagePropertyGridEnum.Parser, mAppPreferencesConfig, processor.ParserName);
            this.tabControl1.TabPages.Add(mParserTab);
        }

        private void ApplicationTab_OnSelctionChanged(TabpagePropertyGridEnum ptype, string newLabel, string newValue)
        {
            if (string.Compare(newLabel, "ActiveBoard") == 0)
            {
                mBoardTab.UpdateBoard(newValue);
                var board = Boards.Boards.GetBoard(newValue);
                mProcessorTab.UpdateProcessor(board.ProcessorName);
                var processor = Processors.Processors.GetProcessor(board.ProcessorName, null);
                mParserTab.UpdateParser(processor.ParserName);
            }
            else if (string.Compare(newLabel, "ActiveParser") == 0)
            {

            }
        }
    }
}