using System;

namespace Server
{
    [Serializable]
    public class RequestVote
    {
        public int Term { get; set; }
        public Uri Candidate { get; set; }
        public int LastLogIndex { get; set; }
        public int LastLogTerm { get; set; }
    }
}