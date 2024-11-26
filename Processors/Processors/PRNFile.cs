using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace Processors
{
    public static class PRNFile
    {
        public static void Create(string prnFilename, ISystemMemory sm, IList<CodeLine> lines)
        {
            var outputLines = new List<string>();
            foreach(var line in lines)
                outputLines.Add(line.ToString());
            File.WriteAllLines(prnFilename, outputLines.ToArray());
        }
    }
}