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
        private Timer electionTimer;

        public FollowerStrategy(ServerContext context, LeaderStrategy prevLeaderStrategy) : base(context, Role.Follower)
        {
            this.context = context;
            this.Logs = prevLeaderStrategy.Logs;
            this.CommitIndex = prevLeaderStrategy.CommitIndex;
            this.CurrentTerm = prevLeaderStrategy.CurrentTerm;
            this.LastApplied = prevLeaderStrategy.LastApplied;
            this.VotedForUrl = prevLeaderStrategy.VotedForUrl;
        }

        public FollowerStrategy(ServerContext context, Uri masterURL) : base(context, Role.Follower)
        {
            this.Master = (IServer)Activator.GetObject(
                typeof(IServer),
                masterURL.ToString() + "Server");
            this.Master.RegisterReplica(this.context.Address);

            this.MasterAddress = masterURL;

            electionTimer = new Timer(ServerStrategy.EletionTimeout);
            electionTimer.Elapsed += electionStart;
            electionTimer.Enabled = true;
        }

        private void electionStart(Object source, ElapsedEventArgs e)
        {
            electionTimer.Stop();

            new CandidateStrategy(this.context, this);
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

        public override AppendEntriesAck AppendEntries(AppendEntries appendEntries)
        {
            lock (this)
            {
                //Received HearthBeat from the leader
                electionTimer.Stop();
                electionTimer.Start(); //Restart election timer
                return base.AppendEntries(appendEntries);
            }
        }

    }
}
