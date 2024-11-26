using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
    public enum ViewEnums
    {
        CodeT = 0,
        RegistersT = 1,
        OutputT = 2,
        MemoryT = 3,
        BoardT = 4,
        OpCodeTraceT = 5,
        WatchT = 6,
        StackT = 7
    }
    public delegate IView QueryViewFunc(ViewEnums view);

    public interface IView
    {
        void ResetView();
        void RefreshView();
        void Start();
        void Stop();
    }
}
