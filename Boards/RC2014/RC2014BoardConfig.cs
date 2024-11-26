using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Preferences;

namespace RC2014
{
    public class RC2014BoardConfig : PreferencesBase
    {
        public enum JumperSetting
        {
            A0,
            A1
        }

        public static string Key = "RC2014 v1.3";

        /// <summary>
        /// ROM board jumper settings
        /// </summary>
        public JumperSetting ROMA13 { get; set; } = JumperSetting.A1;
        public JumperSetting ROMA14 { get; set; } = JumperSetting.A1;
        public JumperSetting ROMA15 { get; set; } = JumperSetting.A1;

        public byte SW1 { get
            {
                byte ret = (ROMA13 == JumperSetting.A1) ? (byte)0x01 : (byte)0;
                ret |= (ROMA14 == JumperSetting.A1) ? (byte)0x02 : (byte)0;
                ret |= (ROMA15 == JumperSetting.A1) ? (byte)0x04 : (byte)0;
                return ret;
            } }

        public override void Default()
        {
        }
    }
}