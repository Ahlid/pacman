using System;

namespace Server
{
    [Serializable]
    public class VoteResponse
    {
        public RPCVotes Vote { get; set; }
        public Uri Voter { get; set; }
        public int VoterTerm { get; set; }
    }
}