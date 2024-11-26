using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Windows.Forms;
using Processors;

namespace Parsers
{
	public static class Parsers
	{
        public static List<string> AvailableParsers
        {
            get
            {
                var ret = new List<string>();
                var processorsDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                foreach (var file in Directory.GetFiles(processorsDirectory, "*.dll"))
                {
                    var pluginAssembly = Assembly.LoadFile(file);
                    if (!IsSimParsersExtension(pluginAssembly))
                        continue;

                    foreach (Type type in pluginAssembly.GetTypes())
                    {
                        if (type.GetInterface(typeof(IParser).ToString(), false) == null)
                            continue;
                        ret.Add(type.FullName);
                    }//foreach
                }//foreach
                return ret;
            }//get
        }//AvailableParsers

        public static IParser GetParser(string parsername, IProcessor processor)
        {
            var parserssDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            foreach (var file in Directory.GetFiles(parserssDirectory, "*.dll"))
            {
                var pluginAssembly = Assembly.LoadFile(file);
                if (!IsSimParsersExtension(pluginAssembly))
                    continue;

                foreach (Type type in pluginAssembly.GetTypes())
                {
                    if (type.GetInterface(typeof(IParser).ToString(), false) == null)
                        continue;

                    if (string.Compare(parsername, type.FullName) != 0)
                        continue;
                    return Activator.CreateInstance(type, processor) as IParser;
                }
            }
            return null;
        }

        private static bool IsSimParsersExtension(Assembly pluginAssembly)
        {
            var attributes = pluginAssembly.GetCustomAttributes(typeof(AssemblySimParserAttribute), false);
            return (attributes.Length > 0);
        }
    }
}