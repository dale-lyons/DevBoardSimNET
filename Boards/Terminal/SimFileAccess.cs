using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using System.IO;

namespace Terminal
{
    public static class SimFileAccess
    {
        private const int SECTORS_PER_TRACK = 64;
        private static Stream[] mDiskFiles = new Stream[4];
        public static byte[] ccp;

        public static byte[] readDiskSector(byte[] diskParams)
        {
            int disk = diskParams[1];
            int track = diskParams[2];
            int sector = diskParams[3];

            int offset = ((track * SECTORS_PER_TRACK) + sector) * 128;
            //Console.WriteLine(string.Format("Reading Disk:{0} Track:{1} Sector:{2}", disk, track, sector));

            if (mDiskFiles[disk] == null)
            {
                string fileName = createFilename(disk);
                if (!File.Exists(fileName))
                    createBlankDiskFile(fileName);
                mDiskFiles[disk] = File.Open(fileName, FileMode.Open);
            }

            var data = new byte[128 + 2];
            mDiskFiles[disk].Seek(offset, SeekOrigin.Begin);
            mDiskFiles[disk].Read(data, 0, 128);
            ushort cs = 0;
            for (int ii = 0; ii < 128; ii++)
                cs += data[ii];
            byte[] bytes = BitConverter.GetBytes(cs);
            data[128] = bytes[0];
            data[129] = bytes[1];
            return data;
        }

        public static void writeDiskSector(byte[] diskParams, byte[] diskData)
        {
            int disk = diskParams[1];
            int track = diskParams[2];
            int sector = diskParams[3];

            int offset = ((track * SECTORS_PER_TRACK) + sector) * 128;
            //Console.WriteLine(string.Format("Writing Disk:{0} Track:{1} Sector:{2} Offset:{3}", disk, track, sector, offset));

            if (mDiskFiles[disk] == null)
            {
                string fileName = createFilename(disk);
                if (!File.Exists(fileName))
                    createBlankDiskFile(fileName);

                mDiskFiles[disk] = File.Open(fileName, FileMode.Open);
            }
            mDiskFiles[disk].Seek(offset, SeekOrigin.Begin);
            mDiskFiles[disk].Write(diskData, 0, 128);
            mDiskFiles[disk].Flush();
        }

        private static string createFilename(int diskno)
        {
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(directory, string.Format(@"CPMDisks\cpmDisk{0}.img", diskno));
        }

        public static void CreateBootDisk()
        {
            string fileName = createFilename(0);
            if (File.Exists(fileName))
                File.Delete(fileName);
            createBlankDiskFile(fileName);
        }


        private static void createBlankDiskFile(string fileName)
        {
            string directory = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using (var stream = File.Create(fileName))
            {
                //2MB
                byte[] bytes = new byte[2*1024*1024];
                for (int ii = 0; ii < bytes.Length; ii++)
                    bytes[ii] = 0xe5;

                Array.Copy(ccp, bytes, ccp.Length);

                stream.Write(bytes, 0, bytes.Length);
                stream.Close();
            }
        }
    }
}