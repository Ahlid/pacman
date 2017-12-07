namespace TEST
{
    public class RaftLog
    {
        public int Term { get; set; }
        public RaftCommand Command { get; set; }
    }

    public class RaftCommand
    {
        public string Name { get; set; }
    }

    public enum State
    {
        FOLLOWER, CANDIDATE, LEADER
    }
}