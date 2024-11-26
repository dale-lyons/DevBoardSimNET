using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using System.Xml;
using System.IO;
using Processors;
using System.ComponentModel;
using System.Drawing.Design;

using Preferences;

namespace Motorola6502
{
    public class Motorola6502ProcessorConfig : PreferencesBase
    {
        public static string Key = "Motorola 6502 Settings";
        public static string DefaultParser = "ParserCa65.ParserCa65";

        [Editor(typeof(ActiveParserEditor), typeof(UITypeEditor))]
        [Description("The Parser to use for this processor.")]
        [DisplayName("ActiveParser")]
        public string ActiveParser { get; set; } = DefaultParser;

        [Description("Allow the use of the 65c02 processor instructions.")]
        [DisplayName("Allow the use of the 65c02")]
        public bool Use65c02 { get; set; }

        public override void Default()
        {
            Use65c02 = false;
            ActiveParser = DefaultParser;
        }
    }
}