using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using Processors;

namespace ARM7.Disassembler
{

    // This class controls the steps of the assembling and linking process.
    // The sequence of steps is ...
    //   0. ArmAssembler(.)		-- create an instance, specifying file names
    //   1. PerformPass()		-- to perform pass 1 on all files
    //   2. PlaceCode(.)		-- to determine the memory layout
    //   3. PerformPass(.)		-- to perform pass 2 on all files
    public class ArmAssembler
    {
        private IList<string> fileList;
        private IDictionary<string, SyEntry> globalSymbols =
                    new Dictionary<string, SyEntry>();  // entries = SyEntry or CommEntry
        private IDictionary<string, string> externSymbols =
                    new Dictionary<string, string>();
        private IList<ArmFileInfo> fileInfoTable;
        private int pass = 0;
        private bool[] passComplete = new bool[3];
        public PlaceCodeSections pcs;
        private AssembledProgram ap;


        // general version of constructor which handles several files
        public ArmAssembler(string[] fileList)
        {
            if (fileList == null)
                throw new Exception("no source files provided");
            this.fileList = new List<string>(fileList);
            initialize();
        }

        // another version of constructor which handles several files
        public ArmAssembler(IList<string> fileList)
        {
            if (fileList == null)
                throw new Exception("no source files provided");
            this.fileList = fileList;
            initialize();
        }

        // version of constructor when there is only one file
        public ArmAssembler(string fileName)
        {
            fileList = new List<string>(1) { fileName };
            initialize();
        }

        // accessor methods

        public IList<ArmFileInfo> FileInfoTable { get { return fileInfoTable; } }

        public int LoadPoint { get { return pcs.LoadPoint; } }

        public int Length { get { return pcs.Length; } }

        public bool TraceParse { get; set; }

        // end of accessor methods

        private void initialize()
        {
            ArmInstructionTemplate.ForceInitialization();
            AssemblerErrors.Initialize();
            fileInfoTable = new List<ArmFileInfo>();
            for (int i = 0; i < fileList.Count; i++)
                fileInfoTable.Add(null);
            for (int i = 0; i < passComplete.Length; i++)
                passComplete[i] = false;
        }

        // perform pass 1 over all the input files
        public void PerformPass1()
        {
            pass = 1;
            for (int i = 0; i < fileList.Count; i++)
                processFile(i);
            passComplete[1] = true;
            //Debug.WriteLine(String.Format(
            //    "{0} errors detected in pass 1", AssemblerErrors.ErrorReports.Count));
            // clean up any null entries created by library processing
            for (int j = fileList.Count - 1; j >= 0; j--)
            {
                if (fileList[j] != null) continue;
                fileList.RemoveAt(j);
                fileInfoTable.RemoveAt(j);
            }
        }

        // perform pass 2 over all the source files
        public void PerformPass2(AssembledProgram ap)
        {
            this.ap = ap;
            pass = 2;
            for (int i = 0; i < fileList.Count; i++)
                processFile(i);
            passComplete[2] = true;
            //Debug.WriteLine(String.Format(
            //    "{0} errors detected in pass 2", AssemblerErrors.ErrorReports.Count));
            ap.TextStart = pcs.TextStart;
            ap.DataStart = pcs.DataStart;
            ap.BssStart = pcs.BssStart;
        }

        // define all labels given a specific load point
        public void PlaceCode(int loadPoint, ISystemMemory systemMemory)
        {
            pcs = new PlaceCodeSections(fileInfoTable, globalSymbols, loadPoint, systemMemory);
        }

        // define all labels, assuming default load point
        //public void PlaceCode()
        //{
        //    pcs = new PlaceCodeSections(fileInfoTable, globalSymbols);
        //}


        // perform next pass over the i-th file in the list
        private void processFile(int i)
        {
            string fileName = fileList[i];
            if (fileName == null)
            {
                Debug.WriteLine("error? -- missing file name");
                return;
            }
            int dotPos = fileName.LastIndexOf('.');
            if (dotPos < 0) dotPos = 0;
            string fileNameSuffix = fileName.Substring(dotPos).ToLower();
            ArmFileInfo fileInfo = fileInfoTable[i];
            switch (fileNameSuffix)
            {
                case ".s":
                    //// process ARM assembly language file
                    //bool parseOK;
                    //Scanner sc = new Scanner();
                    //if (fileInfo == null)
                    //{
                    //    fileInfo = new AsmFileInfo(fileName, globalSymbols, externSymbols);
                    //    fileInfoTable[i] = fileInfo;
                    //}
                    //sc.SetSource(fileInfo.SourceLine);
                    //sc.FileName = fileName;

                    //if (pass == 1)
                    //{
                    //    Parser1 parser = new Parser1();
                    //    parser.scanner = sc;
                    //    parser.FileInfo = (AsmFileInfo)fileInfo;
                    //    parser.Trace = traceParse;
                    //    parseOK = parser.Parse();
                    //}
                    //else
                    //{
                    //    fileInfo.ProgramSpace = ap;
                    //    Parser2 parser = new Parser2();
                    //    parser.scanner = sc;
                    //    parser.FileInfo = (AsmFileInfo)fileInfo;
                    //    parser.Trace = traceParse;
                    //    parseOK = parser.Parse();
                    //}

                    //if (parseOK)
                    //{
                    //    // truncate list of source lines where .END was seen
                    //    // Note: we cannot truncate when Parse returns false because
                    //    // the parser may have skipped input up to the end of file
                    //    int lastLineProcessed = sc.LineNumber;
                    //    int len = fileInfo.SourceLine.Count;
                    //    while (len >= lastLineProcessed)
                    //        fileInfo.SourceLine.RemoveAt(--len);
                    //}
                    break;
                case ".o":   // object code file
                case ".o)":  // member of a library archive
                             // process ELF format object code file
                    if (fileInfo == null)
                    {
                        fileInfo = new ObjFileInfo(fileName, globalSymbols, externSymbols);
                        fileInfoTable[i] = fileInfo;
                    }
                    if (pass == 2)
                    {
                        fileInfo.ProgramSpace = ap;
                    }
                    if (fileInfo.Pass < pass)
                    {
                        fileInfo.Pass = pass;
                        fileInfo.StartPass();
                    }
                    break;
                case ".a":
                    //// accept a Gnu format archive file containing ELF object code members
                    //if (fileInfo == null)
                    //{
                    //    ArmElfLibReader archive = new ArmElfLibReader(fileName);
                    //    IList<ArmFileInfo> newLibMembers = new List<ArmFileInfo>();
                    //    bool progress = true;
                    //    while (progress)
                    //    {
                    //        progress = false;
                    //        IList<string> defined = new List<string>();
                    //        // copy the list of externals so that we can modify it inside the loop
                    //        string[] externs = new string[externSymbols.Keys.Count];
                    //        externSymbols.Keys.CopyTo(externs, 0);
                    //        foreach (string sy in externs)
                    //        {
                    //            if (globalSymbols.ContainsKey(sy))
                    //            {
                    //                defined.Add(sy);
                    //                continue;
                    //            }
                    //            string fn;
                    //            FileStream member = archive.GetLibraryFile(sy, out fn);
                    //            if (member == null) continue;
                    //            defined.Add(sy);
                    //            fileInfo = new ObjFileInfo(member, fileName, fn, globalSymbols, externSymbols);
                    //            newLibMembers.Add(fileInfo);
                    //            fileInfo.Pass = pass;
                    //            fileInfo.StartPass();  // this call might change externSymbols
                    //            progress = true;
                    //        }
                    //        foreach (string sy in defined)
                    //        {
                    //            externSymbols.Remove(sy);
                    //        }
                    //    }
                    //    fileInfoTable[i] = null;
                    //    fileList[i] = null;
                    //    foreach (ArmFileInfo af in newLibMembers)
                    //    {
                    //        fileInfoTable.Add(af);
                    //        fileList.Add(af.FileName);
                    //    }
                    //}
                    //else
                    //    throw new AsmException(
                    //        "unexpected library file in pass 2: {0}", fileName);
                    break;
                default:
                    throw new AsmException("unsupported file type ({0})", fileName);
            }

        }

        public SyEntry LookupGlobalSymbol(string name)
        {
            return globalSymbols[name.ToLower()];
        }

        public int LookupGlobalLabelValue(string name)
        {
            SyEntry entry = LookupGlobalSymbol(name);
            if (entry == null || entry.Kind != SymbolKind.Label)
                return -1;
            return entry.Value;
        }

        public int EntryPointAddress
        {
            get
            {
                if (pass < 2) return -1;
                int ep = LookupGlobalLabelValue("_start");
                if (ep > 0) return ep;
                ep = LookupGlobalLabelValue("main");
                return (ep >= 0) ? ep : pcs.LoadPoint;
            }
        }

        public void DumpInfo()
        {
            if (pass == 0)
            {
                Console.WriteLine("No assembly or object files processed yet");
                return;
            }
            if (passComplete[2])
                Console.WriteLine("Pass 2 completed");
            else if (passComplete[1])
                Console.WriteLine("Pass 1 completed");
//            foreach (AsmFileInfo fi in fileInfoTable)
            foreach (var fi in fileInfoTable)
                    fi.DumpInfo();
        }

#if STANDALONETEST
	static void Main( string[] args ) {
		bool tf = false;
		AssembledProgram ap;
		List<string> fl = new List<string>();
		foreach(string s in args) {
			if (s == "/t" || s == "/trace")
				tf = true;
			else
				fl.Add(s);
		}
		ArmAssembler arms = new ArmAssembler(fl);
		arms.TraceParse = tf;
		arms.PerformPass();
		if (AssemblerErrors.ErrorReports.Count == 0) {
			arms.DumpInfo();
			arms.PlaceCode();
			ap = new AssembledProgram(arms.pcs.LoadPoint, arms.pcs.Length);
			arms.PerformPass(ap);
		}
		arms.DumpInfo();
	}
#endif
    }




} // end namespace

