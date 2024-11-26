using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Boards;
using Preferences;
using Processors;
using Parsers;

namespace DevBoardSim.NET
{
    public delegate void OnSelectionChangedDelegate(TabpagePropertyGridEnum ptype, string newLabel, string mewValue);

    public enum TabpagePropertyGridEnum
    {
        Application,
        Board,
        Processor,
        Parser
    }

    public class PreferencesEditorTabPage : TabPage
    {
        public event OnSelectionChangedDelegate OnSelctionChanged;
        private PropertyGrid propertyGrid1;
        private TabpagePropertyGridEnum mTabpagePropertyGridEnum;
        private object mObjectInfo;

        public PreferencesEditorTabPage(TabpagePropertyGridEnum propertyGridType, AppPreferencesConfig appPreferencesConfig, string objectName)
        {
            InitializeComponent();
            propertyGrid1.PropertyValueChanged += PropertyGrid1_PropertyValueChanged;
            mTabpagePropertyGridEnum = propertyGridType;

            switch (propertyGridType)
            {
                case TabpagePropertyGridEnum.Application:
                    propertyGrid1.SelectedObject = appPreferencesConfig;
                    break;
                case TabpagePropertyGridEnum.Board:
                    UpdateBoard(objectName);
                    break;
                case TabpagePropertyGridEnum.Processor:
                    UpdateProcessor(objectName);
                    break;
                case TabpagePropertyGridEnum.Parser:
                    UpdateParser(objectName);
                    break;
            }//switch
            this.Controls.Add(propertyGrid1);
            this.Text = propertyGridType.ToString();
        }

        private void PropertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if ((string.Compare(e.ChangedItem.Label, "ActiveBoard") == 0) ||
                (string.Compare(e.ChangedItem.Label, "Processor") == 0) ||
                (string.Compare(e.ChangedItem.Label, "ActiveParser") == 0))
                        OnSelctionChanged?.Invoke(mTabpagePropertyGridEnum, e.ChangedItem.Label, e.ChangedItem.Value as string);
        }

        public void UpdateBoard(string value)
        {
            mObjectInfo = Boards.Boards.GetBoard(value);
            propertyGrid1.SelectedObject = (mObjectInfo as IBoard).Settings;
        }
        public void UpdateProcessor(string value)
        {
            mObjectInfo = Processors.Processors.GetProcessor(value, null);
            propertyGrid1.SelectedObject = (mObjectInfo as IProcessor).Settings;
        }
        public void UpdateParser(string value)
        {
            mObjectInfo = Parsers.Parsers.GetParser(value, null);
            if (mObjectInfo == null)
                return;
            propertyGrid1.SelectedObject = (mObjectInfo as IParser).Settings;
        }
        private void InitializeComponent()
        {
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.SuspendLayout();
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(130, 130);
            this.propertyGrid1.TabIndex = 0;
            this.ResumeLayout(false);
        }

        public void Default()
        {
            (propertyGrid1.SelectedObject as PreferencesBase).Default();
            propertyGrid1.Refresh();
        }

        public void SaveSettings()
        {
            if (mObjectInfo is null)
                return;
            switch (mTabpagePropertyGridEnum)
            {
                case TabpagePropertyGridEnum.Application:
                    AppPreferencesConfig.Save<AppPreferencesConfig>(mObjectInfo as AppPreferencesConfig, AppPreferencesConfig.Key);
                    break;
                case TabpagePropertyGridEnum.Board:
                    (mObjectInfo as IBoard).SaveSettings(propertyGrid1.SelectedObject as PreferencesBase);
                    break;
                case TabpagePropertyGridEnum.Processor:
                    (mObjectInfo as IProcessor).SaveSettings(propertyGrid1.SelectedObject as PreferencesBase);
                    break;
                case TabpagePropertyGridEnum.Parser:
                    (mObjectInfo as IParser).SaveSettings(propertyGrid1.SelectedObject as PreferencesBase);
                    break;
            }//switch
        }
    }
}