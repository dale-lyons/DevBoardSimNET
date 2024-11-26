using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Forms;

using Processors;
using Boards;
using System.Security.AccessControl;
using System.Runtime.CompilerServices;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.Net.NetworkInformation;

namespace Assembler
{
    // Assembler -cpu Intel8085.Intel8085 -file download.asm
    internal class Program
    {
        private static string mCpu;
        private static string mFileName;
        private static ISystemMemory SystemMemory;
        private static string[] IncludeDirs;

        private static bool assemble;
        private static uint address;

        static void Main(string[] args)
        {
            for(int ii=0;ii<args.Length; ii++)
            {
                switch(args[ii])
                {
                    case "-unassemble":
                        assemble = false;
                        break;
                    case "-address":
                        address = uint.Parse(args[ii+1], NumberStyles.AllowHexSpecifier);
                        break;
                    case "-assemble":
                        assemble = true;
                        break;
                    case "-cpu":
                        mCpu = args[ii + 1]; ii++;
                        break;
                    case "-file":
                        mFileName = args[ii + 1]; ii++;
                        break;
                    case "-directory":
                        IncludeDirs = args[ii + 1].Split(';');
                        ii++;
                        break;
                    default:
                        break;

                }
            }//for

            if(string.IsNullOrEmpty(mCpu))
            {
                Console.WriteLine("no cpu specified");
                return;
            }
            if (string.IsNullOrEmpty(mFileName))
            {
                Console.WriteLine("no file specified");
                return;
            }

            string boardsDirectory = Path.GetDirectoryName(Application.ExecutablePath);

            SystemMemory = new SystemMemory();
            SystemMemory.Default(Processors.SystemMemory.SIXTY_FOURK);
            var processor = Processors.Processors.GetProcessor(mCpu, SystemMemory);
            if (processor == null)
            {
                Console.WriteLine(String.Format("Processor {0} could not be loaded", mCpu));
                return;
            }
            //CodeLine.Processor = processor;

            if (assemble)
            {
                var parser = processor.CreateParser();
                string cpath = Directory.GetCurrentDirectory();
                var parseSettings = new ParseSettings { Hex = true, Path = cpath, Bin = true, Listing = true, Debug = true };
                parseSettings.IncludeDirs = IncludeDirs;

                parser.Parse(mFileName, parseSettings);
                if (parser.Errors.Count > 0)
                {
                    Console.WriteLine("Errors!");
                    foreach (var error in parser.Errors)
                        Console.WriteLine(error.Text);
                    Console.ReadLine();
                    return;
                }
                Console.WriteLine("All OK)");
            }
            else
            {
                var ret = new List<CodeLine>();

                var dissasm = processor.CreateDisassembler((int)address);

                string cpath = Path.Combine(Directory.GetCurrentDirectory(), mFileName);
                byte[] bytes = File.ReadAllBytes(cpath);
                uint addr = address;
                for(int ii= 0; ii< bytes.Length && addr <= 0xffff; ii++, addr++)
                    SystemMemory.SetMemory(addr, WordSize.OneByte, bytes[ii], false);

                addr = address;
                while (addr < 0xffff)
                {
                    var cl = dissasm.ProcessInstruction(addr, SystemMemory, out addr);
                    ret.Add(cl);
                }

                Dictionary<ushort,string> symbols = null;
                string symfilename = Path.ChangeExtension(cpath, "sym");
                if (File.Exists(symfilename))
                {
                    symbols = new Dictionary<ushort, string>();
                    var lines = File.ReadAllLines(symfilename);
                    foreach(var line in lines)
                    {
                        var syms = line.Split(new char[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
                        symbols[ushort.Parse(syms[1], NumberStyles.AllowHexSpecifier)] = syms[0];
                    }
                }

                string ofilename = Path.ChangeExtension(cpath, "asm");
                string outpath = Path.Combine(Directory.GetCurrentDirectory(), ofilename);
                using (var dale = File.Create(outpath))
                {
                    using (var d2 = new StreamWriter(dale))
                    {
                        foreach (var line in ret)
                        {
                            ushort laddr = (ushort)line.Address;
                            if (symbols == null)
                            {
                                d2.WriteLine(line.ToString());
                                continue;
                            }
                            if (symbols.ContainsKey(laddr))
                            {
                                d2.WriteLine(symbols[laddr] + ":");
                            }
                            var b0 = (byte)SystemMemory.GetMemory(laddr, WordSize.OneByte, false);
                            if (b0 == 0xc3)
                            {
                                var w0 = (ushort)SystemMemory.GetMemory((uint)(laddr + 1), WordSize.TwoByte, false);
                                if (symbols.ContainsKey(w0))
                                {
                                    d2.WriteLine("\t\tjmp   " + symbols[w0]);
                                }
                            }
                            d2.WriteLine(line.ToString());
                        }
                    }
                }
            }
            Console.ReadLine();
        }
    }
}