using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Processors;

namespace Boards
{
    public static class Boards
    {
        public static IList<BoardDef> AvailableBoards
        {
            get
            {
                var boardsDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                var ret = new List<BoardDef>();
                foreach (var file in Directory.GetFiles(boardsDirectory, "*.dll"))
                {
                    var pluginAssembly = Assembly.LoadFile(file);
                    if (!IsSimBoardExtension(pluginAssembly))
                        continue;

                    foreach (Type type in pluginAssembly.GetTypes())
                    {
                        if (type.GetInterface(typeof(IBoard).ToString(), false) == null)
                            continue;
                        ret.Add(new BoardDef { type = type, Name = type.Name, FullName = type.FullName, Path = file });
                    }
                }
                return ret;
            }
        }

        public static IBoard GetBoard(string boardName)
        {
            foreach (var board in AvailableBoards)
            {
                if (string.Compare(boardName, board.FullName) != 0)
                    continue;
                var boardPlugin = Activator.CreateInstance(board.type) as IBoard;
                return boardPlugin;
            }
            return null;
        }

        private static bool IsSimBoardExtension(Assembly pluginAssembly)
        {
            var attributes = pluginAssembly.GetCustomAttributes(typeof(AssemblySimBoardAttribute), false);
            return (attributes.Length > 0);
        }

        public static byte[] loadResource(Assembly assembly, string resourceName)
        {
            string[] names = assembly.GetManifestResourceNames();

            byte[] bytes;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    return null;
                bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
            }//using
            return bytes;
        }
    }
}