using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    [Serializable]
    public class LogEntry
    {
        public int Index { get; set; }
        public Command Command { get; set; }
        public int Term { get; set; }
    }
}
