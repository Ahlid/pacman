namespace Server
{
    /// <summary>
    /// Represents a raft server's state
    /// </summary>
    public enum State
    {
        FOLLOWER, CANDIDATE, LEADER
    }
}