using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Drawing.Design;

using Preferences;

namespace GW8085SBC
{
    public enum JumperSetting
    {
        One_Two,
        Two_Three
    }

    public enum UARTRates : int
    {
        b300,
        b600,
        b1200,
        b2400,
        b4800,
        b9600,
        b19200,
        b38400
    }

    public class GW8085BoardConfig : PreferencesBase
    {
        public GW8085BoardConfig()
        {
            //BaudRateEditor.mBaudRates = new List<string>();
            //foreach (var baud in Enum.GetValues(typeof(UARTRates)))
            //{
            //    BaudRateEditor.mBaudRates.Add(baud.ToString());
            //}
        }

        public static string Key = "GlitchWorks 8085 SBC V3";

        /// <summary>
        /// SW2
        /// I/O Port Block Base Address
        /// </summary>
        [Editor(typeof(IOSwitchEditor), typeof(UITypeEditor))]
        [Category("Switches")]
        [Description("Switch specifying I/O Port Block Base Address.")]
        [DisplayName("SW2")]
        [Browsable(true)]
        public byte SW2 { get; set; } = 0b00000000;

        /// <summary>
        /// SW3
        /// ROM Base Address, Write Protect and Boot Page Numbr
        /// </summary>
        [Editor(typeof(IOSwitchEditor), typeof(UITypeEditor))]
        [Category("Switches")]
        [Description("Switch specifying I/O ROM Base Address, write protect and Boot page")]
        [DisplayName("SW3")]
        public byte SW3 { get; set; } = 0b11110000;

        [Category("Terminal")]
        [Description("Use builtin terminal application to Commicate with Simulation.")]
        [DisplayName("UseTerminal")]
        public bool UseTerminal { get; set; } = true;

        [Category("Terminal")]
        [Description("Use PTTY application to Commicate with Simulation.")]
        [DisplayName("UsePutty")]
        public bool UsePutty { get; set; }

        [Category("Terminal")]
        [Editor(typeof(ComPortEditor), typeof(UITypeEditor))]
        [Description("COM Port to use for PTTY application")]
        [DisplayName("ComPort")]
        public string ComPort { get; set; } = "<None>";

        [Category("Jumpers")]
        /// <summary>
        /// Jumper J1
        /// ROM Boot, 1-2 disables ROM boot, 2-3 enable ROM boot.
        /// </summary>
        public JumperSetting J1 { get; set; } = JumperSetting.Two_Three;

        /// <summary>
        /// Jumper J2
        /// ROM Enabled, 1-2 disables ROM on reset, 2-3 enables ROM on reset
        /// </summary>
        [Category("Jumpers")]
        public JumperSetting J2 { get; set; } = JumperSetting.Two_Three;

        [Category("Jumpers")]
        /// <summary>
        /// Jump J5
        /// Console USART bitrate
        /// </summary>
        public UARTRates J5 { get; set; } = UARTRates.b19200;


        [Category("ROM Image")]
        public bool UseCustomBootROM { get; set; } = false;

        [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(UITypeEditor))]
        [Category("ROM Image")]
        [Description("Image file to load into ROM.")]
        [DisplayName("Allow the use of an Image file.")]
        public string RomImage { get; set; } = @"C:\Projects\DevBoardSimNET\Boards\GW8085SBC\ROM\originalbin.bin";


        [Browsable(false)]
        public byte BootRomPage { get { return (byte)(SW3 & 0x07); } }
        [Browsable(false)]
        public uint BootRomBase { get { return (ushort)(BootRomPage * 4096 + 0x8000); } }
        [Browsable(false)]
        public bool RomWriteProtect { get { return ((SW3 & 0x10) != 0); } }




        [Browsable(false)]
        public bool BootROMEnabled { get { return (J1 == JumperSetting.Two_Three); } }
        [Browsable(false)]
        public bool ResetROMEnabled { get { return (J2 == JumperSetting.Two_Three); } }

        [Browsable(false)]
        public string IncludeDirectories { get; set; } = string.Empty;


        public override void Default()
        {
            SW2 = 0;
            SW3 = 0b11110000;
            ComPort = "<None>";
            UseCustomBootROM = false;
            RomImage = @"C:\Projects\DevBoardSimNET\Boards\GW8085SBC\ROM\originalbin.bin";
            J5 = UARTRates.b19200;
            UseTerminal = true;
            UsePutty = false;
            UseCustomBootROM = false;
        }
    }
}