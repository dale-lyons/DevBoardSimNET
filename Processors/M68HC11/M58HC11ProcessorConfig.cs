using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Preferences;

namespace M68HC11
{
    public class M68HC11ProcessorConfig : PreferencesBase
    {
        public static string Key = "M68HC11 Processor Settings";
        public static string DefaultParser = "ParserA85.ParserA85";

        [Description("Stop simulation on invalid memory access")]
        [DisplayName("StoponInvalid")]
        public bool StoponInvalid { get; set; } = false;

        [Editor(typeof(ActiveParserEditor), typeof(UITypeEditor))]
        [Description("The Parser to use for this processor.")]
        [DisplayName("ActiveParser")]
        public string ActiveParser { get; set; } = DefaultParser;

        public override void Default()
        {
            StoponInvalid = false;
        }
    }
}