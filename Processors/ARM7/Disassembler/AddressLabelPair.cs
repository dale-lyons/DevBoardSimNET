using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARM7.Disassembler
{
    public class AddressLabelPair
    {
        public uint Address { get; set; }
        public string Label { get; set; }

        public AddressLabelPair(uint address, string label)
        {
            this.Address = address;
            this.Label = label;
        }
    }
}