using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static Server.ServerContext;

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
        private System.Timers.Timer electionTimer;

        public FollowerStrategy(ServerContext context, LeaderStrategy prevLeaderStrategy) : base(context, Role.Follower)
        {
            this.context = context;
            //this.Logs = prevLeaderStrategy.Logs;
            //this.CommitIndex = prevLeaderStrategy.CommitIndex;
            //this.CurrentTerm = prevLeaderStrategy.CurrentTerm;
            //this.LastApplied = prevLeaderStrategy.LastApplied;
            //this.VotedForUrl = prevLeaderStrategy.VotedForUrl;
        }

        public FollowerStrategy(ServerContext context, Uri masterURL) : base(context, Role.Follower)
        {
            this.MasterAddress = masterURL;

            electionTimer = new System.Timers.Timer(ServerStrategy.ElectionTimeout);
            electionTimer.Elapsed += electionStart;
            electionTimer.Enabled = true;

            this.Master = (IServer)Activator.GetObject(
                typeof(IServer),
                masterURL.ToString() + "Server");

            ((IReplica)this.Master).RegisterReplica(this.context.Address);
        }

        private void electionStart(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Election has started");
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
            lock (this.context)
            {

                Console.WriteLine("Refresh timer");
                //Received HearthBeat from the leader
                electionTimer.Stop();
                electionTimer.Start(); //Restart election timer

                

                if (appendEntries.LeaderTerm < this.context.CurrentTerm)
                    return new AppendEntriesAck() { Success = false, Term = this.context.CurrentTerm, Node = this.context.Address };
                if (appendEntries.LogEntries.Count - 1 > appendEntries.PrevLogIndex && appendEntries.LogEntries[appendEntries.PrevLogIndex].Term != appendEntries.PrevLogTerm)
                    return new AppendEntriesAck() { Success = false, Term = this.context.CurrentTerm, Node = this.context.Address };

                Console.WriteLine("Follower number of entries:  " + appendEntries.LogEntries.Count);
                foreach (LogEntry entry in appendEntries.LogEntries)
                {
                    if (this.context.Logs.Count > entry.Index)
                    {
                        if (entry.Term != this.context.Logs[entry.Index].Term)
                        {
                            //Remove all the logs from logs from this point on
                            this.context.Logs.RemoveRange(entry.Index, this.context.Logs.Count - entry.Index);
                            break;
                        }

                        continue;
                    }
                    else
                    {
                        //appending the entry
                        Console.WriteLine($"Follower: Appending the entry index {entry.Index}");
                        //
                        this.context.Logs.Add(entry);
                    }

                }

                if(appendEntries.LeaderCommitIndex > this.context.CommitIndex)
                {
                    this.context.CommitIndex = Math.Min(appendEntries.LeaderCommitIndex, this.context.Logs.Last().Index);
                }

                Task.Run(() => { base.CheckLogs(); });

                return new AppendEntriesAck()
                {
                    Success = true,
                    Term = this.context.CurrentTerm,
                    Node = this.context.Address,
                    LastIndex = this.context.Logs.Count - 1
                };

            }
        }

    }
}
