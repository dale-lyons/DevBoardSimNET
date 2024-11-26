using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using System.IO;
//using System.Reflection;
using System.Windows.Forms;

namespace Processors
{
    public class InvalidInstructionException : Exception
    {
        uint Address { get; set; }
        uint Opcode { get; set; }
        public InvalidInstructionException(uint address, uint opcode)
        {
            Address = address;
            Opcode = opcode;
        }
    }

    public static class Processors
    {
        public static IList<ProcessorDef> AvailableProcessors
        {
            get
            {
                var ret = new List<ProcessorDef>();
                var processorsDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                foreach (var file in Directory.GetFiles(processorsDirectory, "*.dll"))
                {
                    var pluginAssembly = Assembly.LoadFile(file);
                    if (!IsSimProcessorExtension(pluginAssembly))
                        continue;

                    foreach (Type type in pluginAssembly.GetTypes())
                    {
                        if (type.GetInterface(typeof(IProcessor).ToString(), false) == null)
                            continue;
                        ret.Add(new ProcessorDef { Name = type.Name, FullName = type.FullName, Path = file });
                    }//foreach
                }//foreach
                return ret;
            }//get
        }//AvailableProcessors

        public static IProcessor GetProcessor(string processorName, ISystemMemory systemMemory)
        {
            var boardsDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            foreach (var file in Directory.GetFiles(boardsDirectory, "*.dll"))
            {
                var pluginAssembly = Assembly.LoadFile(file);
                if (!IsSimProcessorExtension(pluginAssembly))
                    continue;

                foreach (Type type in pluginAssembly.GetTypes())
                {
                    if (type.GetInterface(typeof(IProcessor).ToString(), false) == null)
                        continue;

                    if (string.Compare(processorName, type.FullName) != 0)
                        continue;
                    var processor = Activator.CreateInstance(type, systemMemory) as IProcessor;
                    return processor;
                }
            }
            return null;
        }

        private static bool IsSimProcessorExtension(Assembly pluginAssembly)
        {
            var attributes = pluginAssembly.GetCustomAttributes(typeof(AssemblySimProcessorAttribute), false);
            return (attributes.Length > 0);
        }
    }//class Processors
}