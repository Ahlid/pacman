using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class CandidateStrategy : ServerStrategy, IServer
    {
        private List<Uri> receivedVotes;


        /* 
         • On conversion to candidate, start election:
            • Increment currentTerm
            • Vote for self
            • Reset election timer
            • Send RequestVote RPCs to all other servers
        • If votes received from majority of servers: become leader
        • If AppendEntries RPC received from new leader: convert to follower
        • If election timeout elapses: start new election

            */



        public CandidateStrategy(ServerContext context, FollowerStrategy prevFollowerStrategy) : base(context, Role.Candidate)
        {
            this.CurrentTerm = prevFollowerStrategy.CurrentTerm;
            this.CommitIndex = prevFollowerStrategy.CommitIndex;
            this.LastApplied = prevFollowerStrategy.LastApplied;
            this.Logs = prevFollowerStrategy.Logs;

        }

        public override Uri GetLeader()
        {
            return null;
        }

        public override JoinResult Join(string username, Uri address)
        {
            throw new Exception("Candidates cannot receive client join requests.");
        }

        public override void Quit(Uri address)
        {
            throw new Exception("Candidates cannot quit");
        }

        public override void RegisterReplica(Uri ReplicaServerURL)
        {
            throw new Exception("Candidates cannot register replicas");
        }

        public override void SetPlay(Uri address, Play play, int round)
        {
            throw new NotImplementedException();
        }

        //raft

        private void SetFollowerState(long currentTerm, Uri newLeader)
        {

        }

        private void StartNewElection()
        {
            lock (this)
            {
                this.CurrentTerm++;
                this.VotedForUrl = this.context.Address;
                this.receivedVotes = new List<Uri>();
                this.receivedVotes.Add(this.VotedForUrl);


                //todo send request vote
            }
        }

    }
}
