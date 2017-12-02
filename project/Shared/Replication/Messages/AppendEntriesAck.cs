using System;

namespace Server
{
    [Serializable]
    public class AppendEntriesAck
    {
        public Uri Node { get; set; }
        public int Term { get; set; }
        public bool Success { get; set; }
        public int LastIndex { get; set; }
    }
}