using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    [Serializable]
    public abstract class RaftCommand
    {
        public string Name { get; set; }
        public abstract void Execute(RaftServer server, bool AsLeader);
    }
}
