using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GUI;
using GUI.Memory;
using GUI.Stack;
using GUI.Watch;
using Preferences;

namespace DevBoardSim.NET
{
    public class AppPreferencesConfig : PreferencesBase
    {
        public static string Key { get { return "AppPreferencesConfig"; } }

        [Editor(typeof(ActiveBoardEditor), typeof(UITypeEditor))]
        [Description("The Active Board loaded at startup")]
        [DisplayName("ActiveBoard")]
        public string ActiveBoard { get; set; } = "GW8085SBC.GW8085SBCBoard";

        [Description("Stop the simulation if an invalid instruction is encountered")]
        [DisplayName("StopOnInvalidInstruction")]
        public bool StopOnInvalidInstruction { get; set; }

        [Description("Stop the simulation if an invalid memory access is encountered")]
        [DisplayName("StopOnInvalidMemoryAccess")]
        public bool StopOnInvalidMemoryAccess { get; set; }

//        [Description("This is the last memory address specified in a MemoryView")]
//        [DisplayName("Last memory Address")]
//        public uint LastMemAddr { get; set; }

        public override void Default()
        {
            ActiveBoard = "GW8085SBC.GW8085SBCBoard";
            StopOnInvalidInstruction = false;
            StopOnInvalidMemoryAccess = false;
        }

        [Browsable(false)]
        public WatchConfig WatchConfig { get; set; } = new WatchConfig();
        [Browsable(false)]
        public MemoryConfig MemoryConfig { get; set; } = new MemoryConfig();
        [Browsable(false)]
        public StackConfig StackConfig { get; set; } = new StackConfig();
    }
}