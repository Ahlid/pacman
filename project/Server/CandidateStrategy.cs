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
        


        public CandidateStrategy(ServerContext context) : base(context, Role.CANDIDATE)
        {
            
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

    }
}
