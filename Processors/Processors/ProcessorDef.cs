using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Processors
{
    public class ProcessorDef
    {
        public string FullName { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }

        public override string ToString()
        {
            return Name;
        }

    }
}