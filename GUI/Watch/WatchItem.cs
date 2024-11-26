using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Processors;
using System.Text;
using GUI.Arithmetic;

namespace GUI.Watch
{
    public class WatchItem
    {
        public string Expression { get; set; }
        public string Name { get; set; }
        public WordSize WordSize { get; set; }

        [XmlIgnoreAttribute]
        public IProcessor Processor { get; set; }
        [XmlIgnoreAttribute]
        public Dictionary<string, bool> Registers;

        public string EvaluateExpression()
        {
            var sb = new StringBuilder();
            ushort addr = (ushort)AddressExpression.eval(Processor, Expression);
            sb.Append("(");
            sb.Append(Processor.SystemMemory.FormatAddress(addr));
            sb.Append(") ");

            uint ret = Processor.SystemMemory.GetMemory(addr, WordSize, false);
            switch (WordSize)
            {
                case WordSize.OneByte:
                    sb.Append(((byte)ret).ToString("X2"));
                    break;
                case WordSize.TwoByte:
                    sb.Append(((ushort)ret).ToString("X4"));
                    break;
                default:
                    sb.Append(ret.ToString("X8"));
                    break;
            }
            return sb.ToString();
        }
    }
}