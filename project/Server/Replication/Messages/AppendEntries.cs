using System;
using System.Collections.Generic;
using Shared;

namespace Server
{
    [Serializable]
    public class AppendEntries
    {
        public int LeaderTerm { get; set; }
        public Uri Leader { get; set; }
        public int PrevLogTerm { get; set; }
        public int PrevLogIndex { get; set; }
        public int LeaderCommitIndex { get; set; }
        public List<LogEntry> LogEntries { get; set; }
    }
}