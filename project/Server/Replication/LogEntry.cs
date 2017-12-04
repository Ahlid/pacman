using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    [Serializable]
    public class LogEntry
    {
        public int Index { get; set; }
        public ICommand Command { get; set; }
        public int Term { get; set; }
    }
}
