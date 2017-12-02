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
        public enum Role { Leader, Candidate, Follower }
        public Role CurrentRole { get; protected set; }
        public int CurrentTerm { get; protected set; } //latest term server has seen(initialized to 0 on first boot, increases monotonically)
        public Uri VotedForUrl { get; protected set; }// candidateId that received vote in current term(or null if none)
        //log entries; each entry contains command for state machine, and term when entry was received by leader(first index is 1)
        public List<LogEntry> Logs { get; protected set; }

        //Volatile State on all servers
        public int CommitIndex { get; protected set; } //index of highest log entry known to be committed(initialized to 0, increases monotonically)
        public int LastApplied { get; protected set; } //index of highest log entry applied to state machine(initialized to 0, increases monotonically)

        protected static readonly long EletionTimeout = 1000;//1s
        protected static readonly long LeaderTimeout = EletionTimeout / 3;//1s

        /*
         All Servers:
            • If commitIndex > lastApplied: increment lastApplied, apply log[lastApplied] to state machine (§5.3)
            • If RPC request or response contains term T > currentTerm: set currentTerm = T, convert to follower (§5.1)
         */

        protected ServerStrategy(ServerContext context, Role role)
        {
            this.context = context;
            this.CurrentRole = role;
            this.CurrentTerm = 1;
            this.VotedForUrl = null;
            this.Logs = new List<LogEntry>();
            this.CommitIndex = 0;
            this.LastApplied = 0;
        }

        public abstract Uri GetLeader();
        public virtual VoteResponse RequestVote(RequestVote requestVote)
        {

            //If I'm in a bigger term than the one requesting votes, I won't vote for him
            if (requestVote.Term < CurrentTerm)
            {
                return new VoteResponse() { Voter = this.context.Address, VoterTerm = CurrentTerm, Vote = RPCVotes.VoteNo };
            }


            //If I haven't vote yet
            if (this.VotedForUrl == null && this.CommitIndex <= requestVote.LastLogIndex)
            {
                this.VotedForUrl = requestVote.Candidate;
                return new VoteResponse() { Vote = RPCVotes.VoteYes, Voter = this.context.Address, VoterTerm = CurrentTerm };
            }
            return new VoteResponse() { Vote = RPCVotes.VoteNo, Voter = this.context.Address, VoterTerm = CurrentTerm };


        }

        public virtual AppendEntriesAck AppendEntries(AppendEntries appendEntries)
        {

            if (appendEntries.LeaderTerm < CurrentTerm)
                return new AppendEntriesAck() { Success = false, Term = CurrentTerm, Node = this.context.Address };
            if (appendEntries.LogEntries.Count-1 > appendEntries.PrevLogIndex && appendEntries.LogEntries[appendEntries.PrevLogIndex].Term != appendEntries.PrevLogTerm)
                return new AppendEntriesAck() { Success = false, Term = CurrentTerm, Node = this.context.Address };

            foreach (LogEntry entry in appendEntries.LogEntries)
            {
                if (Logs.Count > entry.Index)
                {
                    if (entry.Term != Logs[entry.Index].Term)
                    {
                        //Remove all the logs from logs from this point on
                        Logs.RemoveRange(entry.Index, Logs.Count - entry.Index);
                        break;
                    }

                    continue;
                }
                else
                {
                    //appending the entry
                    Logs.Add(entry);
                }

            }

            if (appendEntries.LeaderCommitIndex > CommitIndex)
            {
                
                CommitIndex = Math.Min(appendEntries.LeaderCommitIndex, Logs.Count - 1);
            }

            return new AppendEntriesAck()
            {
                Success = true,
                Term = CurrentTerm,
                Node = this.context.Address,
                LastIndex = this.Logs.Count-1
            };
        }

        public abstract void RegisterReplica(Uri ReplicaServerURL);

        //Concrete methods:
        public abstract JoinResult Join(string username, Uri address);

        public abstract void SetPlay(Uri address, Play play, int round);

        public abstract void Quit(Uri address);
    }
}
