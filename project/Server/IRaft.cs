using System;
using System.Collections.Generic;

namespace Server
{

    public interface IRaft
    {
        /// <summary>
        /// This is the term this Raft server is currently in
        /// </summary>
        int CurrentTerm { get; set; }

        /// <summary>
        ///  The Log is a list of {term, command} tuples, where the command is an opaque
        /// value which only holds meaning to the replicated state machine running on
        /// top of Raft.
        /// </summary>
        List<RaftLog> Log { get; set; }

        /// <summary>
        /// The state this server is currently in, can be FOLLOWER, CANDIDATE, or LEADER
        /// </summary>
        State State { get; set; }

        /// <summary>
        /// The Raft entries up to and including this index are considered committed by
        /// Raft, meaning they will not change, and can safely be applied to the state
        /// machine.
        /// </summary>
        int CommitIndex { get; set; }

        /// <summary>
        ///  The last command in the Log to be applied to the state machine.
        /// </summary>
        int LastApplied { get; set; }

        /// <summary>
        ///  NextIndex is a guess as to how much of our Log (as leader) matches that of
        ///  each other peer. This is used to determine what entries to send to each peer
        ///  next.
        /// </summary>
        Dictionary<Uri, int> NextIndex { get; set; }

        /// <summary>
        /// MatchIndex is a measurement of how much of our Log (as leader) we know to be
        /// replicated at each other server. This is used to determine when some prefix
        /// of entries in our Log from the current term has been replicated to a
        /// majority of servers, and is thus safe to apply.
        /// </summary>
        Dictionary<Uri, int> MatchIndex { get; set; }

        /// <summary>
        /// Raft RPC to handle request votes
        /// </summary>
        /// <param name="term">The term of the requester</param>
        /// <param name="candidateID">Candidate's address</param>
        /// <param name="lastLogIndex">Candidate's last index in is log</param>
        /// <param name="lastLogTerm">Candidate's last term in hs log</param>
        /// <returns>A tuple with (1) this server's term (2) if this server granted his vote</returns>

        Tuple<int, bool> RequestVote(int term, Uri candidateID, int lastLogIndex, int lastLogTerm);

        /// <summary>
        /// Raft RPC to handle append entries
        /// </summary>
        /// <param name="term">The leader's term</param>
        /// <param name="leaderUri">Leader's Address</param>
        /// <param name="prevLogIndex">The prev log's index that leader has registered for this server </param>
        /// <param name="prevLogTerm">The prev log's term that leader has registered for this server </param>
        /// <param name="entries">The entries to add to this server's log</param>
        /// <param name="leaderCommit">The last commited command for the leader</param>
        /// <returns>A tuple with (1) this server's term, (2) conflictIndex (3) conflictTerm (4) if succes </returns>

        Tuple<int, int, int, bool> AppendEntries(int term, Uri leaderID, int prevLogIndex, int prevLogTerm, List<RaftLog> entries, int leaderCommit);
        String Test();
    }
}