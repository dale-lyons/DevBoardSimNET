using System;

namespace Parsers
{
	public static class Parsers
	{
        public static IParser GetParser(string parsername)
        {
            var parserssDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            foreach (var file in Directory.GetFiles(parserssDirectory, "*.dll"))
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


    }
}