using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using Processors;

namespace Parsers
{
    public interface IParser
    {
        void Parse(string filename, ParseSettings parseSettings);
        IList<CodeLine> ParseListingFile(string filename);
        string ParserName { get; }
        IList<ASMError> Errors { get; }

        OrgSections Sections { get; }
        IList<CodeLine> CodeLines { get; }

        uint EntryPoint { get; }
        IDictionary<string, ushort> Labels { get; }

        ISystemMemory SystemMemory { get; }

        string[] Lines { get; }

        object Settings { get; }
        void SaveSettings(object settings);

    }
}
