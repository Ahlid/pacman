using System;

namespace Server
{
    [Serializable]
    public class RaftLog
    {
        public bool AsLeader = true;
        public int Term { get; set; }
        public RaftCommand Command { get; set; }
    }
}