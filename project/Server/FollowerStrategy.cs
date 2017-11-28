using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Server
{
    public class FollowerStrategy : ServerStrategy, IServer
    {

        /*
        Followers(§5.2):
        • Respond to RPCs from candidates and leaders
        • If election timeout elapses without receiving AppendEntries RPC from current leader or granting vote 
            to candidate convert to candidate
        */

        public Uri MasterAddress { get; set; }
        public IServer Master { get; set; }

        //How much time to wait for the election to start.
        private readonly int timoutMSec = 150;
        private Timer electionTimer;

        public FollowerStrategy(ServerContext context, Uri masterURL) : base(context, Role.FOLLOWER)
        {
            this.Master = (IServer)Activator.GetObject(
                typeof(IServer),
                masterURL.ToString() + "Server");
            this.Master.RegisterReplica(this.context.Address);

            electionTimer = new Timer(this.context.RoundIntervalMsec);
            electionTimer.Elapsed += electionStart;
            electionTimer.Enabled = true;
        }

        private void electionStart(Object source, ElapsedEventArgs e)
        {
            //TODO: Change to CandidateStrategy
        }

        public override Uri GetLeader()
        {
            return this.context.Address;
        }

        public override JoinResult Join(string username, Uri address)
        {
            throw new Exception("Followers cannot receive client join requests.");
        }

        public override void Quit(Uri address)
        {
            throw new Exception("Followers cannot execute quit.");
        }

        public override void RegisterReplica(Uri ReplicaServerURL)
        {
            throw new Exception("Followers cannot register replicas.");
        }

        public override void SetPlay(Uri address, Play play, int round)
        {
            throw new Exception("Followers receive plays.");
        }

        public override AppendEntriesResult AppendEntries(int term, int leaderID, int prevLogIndex, int prevLogTerm, List<LogEntry> entries, int leaderCommit)
        {
            //Received HearthBeat from the leader
            electionTimer.Stop();
            electionTimer.Start(); //Restart election timer
            return base.AppendEntries(term, leaderID, prevLogIndex, prevLogTerm, entries, leaderCommit);
        }

    }
}
