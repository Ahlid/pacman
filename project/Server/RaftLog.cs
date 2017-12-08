using System;

namespace Server
{
    /// <summary>
    /// This class represents a raft log entry
    /// </summary>
    [Serializable]
    public class RaftLog
    {

        /// <summary>
        /// If was created by the leader
        /// </summary>
        public bool AsLeader = true;

        /// <summary>
        /// This log's term
        /// </summary>
        public int Term { get; set; }

        /// <summary>
        /// This log's command
        /// </summary>
        public RaftCommand Command { get; set; }
    }
}