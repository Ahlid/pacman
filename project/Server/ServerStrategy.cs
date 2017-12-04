using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Server.ServerContext;

namespace Server
{
    public abstract class ServerStrategy : MarshalByRefObject, IServer
    {

        protected ServerContext context;

        //###### RAFT ######//
        //Persistent State on all servers:

        //timeout de eleição
        protected static readonly long ElectionTimeout = 2000;//1s
        protected static readonly long LeaderTimeout =  ElectionTimeout/ 3;//1s

        /*
         All Servers:
            • If commitIndex > lastApplied: increment lastApplied, apply log[lastApplied] to state machine (§5.3)
            • If RPC request or response contains term T > currentTerm: set currentTerm = T, convert to follower (§5.1)
         */

        protected ServerStrategy(ServerContext context, Role role)
        {
            this.context = context;
            this.context.CurrentRole = role;
        }

        public void CheckLogs()
        {
            lock(this.context)
            {
                while (this.context.CommitIndex > this.context.LastApplied)
                {
                    Console.WriteLine($"Commiting index {this.context.LastApplied + 1}");
                    this.context.Logs[++this.context.LastApplied].Command.Execute(this.context);
                }
            }
        }

        public abstract Uri GetLeader();

        public virtual VoteResponse RequestVote(RequestVote requestVote)
        {
            //If I'm in a bigger term than the one requesting votes, I won't vote for him
            if (requestVote.Term < this.context.CurrentTerm)
            {
                return new VoteResponse() { Voter = this.context.Address, VoterTerm = this.context.CurrentTerm, Vote = RPCVotes.VoteNo };
            }

            //If I haven't voted yet
            if (this.context.VotedForUrl == null && this.context.CommitIndex <= requestVote.LastLogIndex)
            {
                this.context.VotedForUrl = requestVote.Candidate;
                return new VoteResponse()
                {
                    Vote = RPCVotes.VoteYes,
                    Voter = this.context.Address,
                    VoterTerm = this.context.CurrentTerm
                };
            }

            return new VoteResponse()
            {
                Vote = RPCVotes.VoteNo,
                Voter = this.context.Address,
                VoterTerm = this.context.CurrentTerm
            };
        }

        public abstract AppendEntriesAck AppendEntries(AppendEntries appendEntries);

        public abstract void RegisterReplica(Uri ReplicaServerURL);

        //Concrete methods:
        public abstract JoinResult Join(string username, Uri address);

        public abstract void SetPlay(Uri address, Play play, int round);

        public abstract void Quit(Uri address);
    }
}
