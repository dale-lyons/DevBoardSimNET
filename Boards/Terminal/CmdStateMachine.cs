using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Terminal
{

    public enum WriteState
    {
        None,
        Escape1,
        Escape2,
        Length1,
        Length2,
        Length3
    }

    public class CmdStateMachine
    {
        public delegate void SendByteReadyDelegate();
        public delegate void ExecuteCommand(List<byte> cmdParams, List<byte> cmdData);

        private List<byte> mEscapeParams;
        private List<byte> mData;
        private int mBytesToRead;
        private const byte ESCAPEBYTE = 0x1b;

        private DateTime mLastByteSentTime = DateTime.Now;

        private List<byte> BytesToSend { get; set; }
        public CmdStateMachine CmdSM { get; set; }
        private WriteState mWriteState = WriteState.None;

        public List<byte> ArrivedData { get; set; }
        public bool IsEscapeOn { get; set; }
        public bool AvailableToSendBytes
        {
            get
            {
                lock (BytesToSend)
                {
                    return (BytesToSend.Count > 0);
                }
            }
        }

        private ExecuteCommand mExecuteCommand;
        public CmdStateMachine(ExecuteCommand executeCommand)
        {
            mExecuteCommand = executeCommand;
            BytesToSend = new List<byte>();
        }

        public byte Next()
        {
            byte ret = 0;
            lock (BytesToSend)
            {
                if (BytesToSend.Count <= 0)
                    return ret;

                ret = BytesToSend[0];
                BytesToSend.RemoveAt(0);
                return ret;
            }
        }

        public void SendByte(byte data)
        {
            lock (BytesToSend)
                BytesToSend.Add(data);
        }

        public void SendBytes(byte[] data)
        {
            foreach (byte b in data)
                SendByte(b);
        }
        public void SendString(string data)
        {
            var bytes = ASCIIEncoding.UTF7.GetBytes(data);
            foreach (byte b in bytes)
                SendByte(b);
        }

        public bool portRead(out byte data)
        {
            data = 0;
            if (!AvailableToSendBytes)
                return false;

            lock (BytesToSend)
            {
                data = BytesToSend[0];
                BytesToSend.RemoveAt(0);
            }
            return true;
        }

        public bool portWrite(byte data)
        {
            switch (mWriteState)
            {
                case WriteState.None:
                    if (data == ESCAPEBYTE)
                    {
                        //Console.WriteLine("got first Escape!");
                        mWriteState = WriteState.Escape1;
                        return true;
                    }
                    return false;
                case WriteState.Escape1:
                    if (data == ESCAPEBYTE)
                    {
                        //Console.WriteLine("got second Escape!");
                        mWriteState = WriteState.Escape2;
                        mEscapeParams = null;
                        return true;
                    }
                    else
                    {
                        mWriteState = WriteState.None;
                        mExecuteCommand(new List<byte> { 0x1b, data }, null);
                        return true;
                    }
                case WriteState.Escape2:
                    //Console.WriteLine("got first size:" + data.ToString());
                    mBytesToRead = data;
                    mEscapeParams = new List<byte>();
                    mWriteState = WriteState.Length1;
                    return true;
                case WriteState.Length1:
                    mEscapeParams.Add(data);
                    //Console.WriteLine("got a byte:" + mEscapeParams.Count.ToString() + ":" + data.ToString("X2"));
                    if (mEscapeParams.Count >= mBytesToRead)
                        mWriteState = WriteState.Length2;
                    return true;
                case WriteState.Length2:
                    //Console.WriteLine("got second size:" + data.ToString());
                    mData = new List<byte>();
                    if (data > 0)
                    {
                        mBytesToRead = data;
                        mWriteState = WriteState.Length3;
                    }
                    else
                    {
                        mWriteState = WriteState.None;
                        mExecuteCommand(mEscapeParams, mData);
                    }
                    return true;
                case WriteState.Length3:
                    mData.Add(data);
                    if (mData.Count >= mBytesToRead)
                    {
                        mWriteState = WriteState.None;
                        mExecuteCommand(mEscapeParams, mData);
                    }
                    return true;
            }
            return true;
        }//portWrite
    }//class CmdStateMachine
}