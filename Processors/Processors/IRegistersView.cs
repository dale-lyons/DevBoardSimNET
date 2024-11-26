using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Processors
{
    public interface IRegistersView
    {
        Panel UIPanel { get; }
        void Start();
        void Stop();

    }
}
