using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public abstract class ServerStrategy : MarshalByRefObject, IServer
    {

        protected ServerContext context;

        //###### RAFT ######//
        //Persistent State on all servers:
        protected enum Role { LEADER, CANDIDATE, FOLLOWER }
        protected Role currentRole;
        protected int currentTerm = 0; //latest term server has seen(initialized to 0 on first boot, increases monotonically)
        protected Uri votedForURL = null;// candidateId that received vote in current term(or null if none)
        //log entries; each entry contains command for state machine, and term when entry was received by leader(first index is 1)
        protected List<LogEntry> logs = new List<LogEntry>();

        //Volatile State on all servers
        protected int commitIndex = 0; //index of highest log entry known to be committed(initialized to 0, increases monotonically)
        protected int lastApplied = 0; //index of highest log entry applied to state machine(initialized to 0, increases monotonically)

        /*
         All Servers:
            • If commitIndex > lastApplied: increment lastApplied, apply log[lastApplied] to state machine (§5.3)
            • If RPC request or response contains term T > currentTerm: set currentTerm = T, convert to follower (§5.1)
         */

        protected ServerStrategy(ServerContext context, Role role)
        {
            this.context = context;
            this.currentRole = role;
        }

        public abstract Uri GetLeader();
        public abstract JoinResult Join(string username, Uri address);
        public abstract void Quit(Uri address);
        public abstract void RegisterReplica(Uri ReplicaServerURL);
        public abstract void SetPlay(Uri address, Play play, int round);

        //Concrete methods:

        public virtual AppendEntriesResult AppendEntries(int term, int leaderID, int prevLogIndex, int prevLogTerm, List<LogEntry> entries, int leaderCommit)
        {
            if (term < currentTerm)
                return new AppendEntriesResult() { Success = false, Term = currentTerm };
            if (logs.Count > prevLogIndex && logs[prevLogIndex].Term != prevLogTerm)
                return new AppendEntriesResult() { Success = false, Term = currentTerm };

            foreach (LogEntry entry in entries)
            {
                if (logs.Count > entry.Index)
                {
                    if (entry.Term != logs[entry.Index].Term)
                    {
                        //Remove all the logs from logs from this point on
                        logs.RemoveRange(entry.Index, logs.Count - entry.Index);
                        break;
                    }

                    continue;
                }
                else
                {
                    //appending the entry
                    logs.Add(entry);
                }

            }

            if (leaderCommit > commitIndex)
            {
                commitIndex = Math.Min(leaderCommit, logs.Count - 1);
            }

            return new AppendEntriesResult()
            {
                Success = true,
                Term = currentTerm
            };
        }

        public RequestVoteResult RequestVote(int term, Uri candidateURL, int lastLogIndex, int lastLogTerm)
        {
            //If I'm in a bigger term than the one requesting votes, I won't vote for him
            if (term < currentTerm)
            {
                return new RequestVoteResult() { Term = currentTerm, VoteGranted = false };
            }

            // TODO: 2.  If votedFor is null or candidateId, and candidate’s log is at least as up - to - date as receiver’s log, grant vote
            //If I haven't vote yet
            if (votedForURL == null)
            {
                votedForURL = candidateURL; // TODO
                return new RequestVoteResult() { Term = currentTerm, VoteGranted = true };
            }

            return new RequestVoteResult() { Term = currentTerm, VoteGranted = false };
        }
    }
}
