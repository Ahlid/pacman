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

        public FollowerStrategy(ServerContext context, Uri masterURL) : base(context, Role.Follower)
        {
            Console.WriteLine("\n****FOLLOWER*****\n");

            this.MasterAddress = masterURL;

            this.context.electionTimer = new System.Timers.Timer(ServerStrategy.ElectionTimeout);
            this.context.electionTimer.Elapsed += electionStart;
            this.context.electionTimer.AutoReset = false;

            Console.WriteLine($"{this.context.hasRegistered}");
            this.Master = (IServer)Activator.GetObject(
                typeof(IServer),
                masterURL.ToString() + "Server");

            if (!this.context.hasRegistered)
            {
                do
                {
                    try
                    {
                        ((IReplica)this.Master).RegisterReplica(this.context.Address);
                        this.context.hasRegistered = true;
                    }
                    catch (Exception) { }

                } while (!this.context.hasRegistered);
                Console.WriteLine("  FOLLOWER REGISTER");
            }
            else
            {
                this.context.electionTimer.Enabled = true;
            }

        }

        private void electionStart(Object source, ElapsedEventArgs e)
        {
            this.context.electionTimer.Stop();

            //todo - make sure every follower function stops
            if (this.context.CurrentRole == Role.Follower)
                this.context.SwitchStrategy(this, new CandidateStrategy(this.context));
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

        public override VoteResponse RequestVote(RequestVote requestVote)
        {

            //If RPC request or response contains term T > currentTerm:
            //set currentTerm = T, convert to follower (§5.1)
            if (requestVote.Term > this.context.CurrentTerm)
            {
                this.context.CurrentTerm = requestVote.Term;
            }

            VoteResponse response = new VoteResponse { Voter = this.context.Address, VoterTerm = this.context.CurrentTerm };

            //Reply false if term < currentTerm (§5.1)
            if (requestVote.Term < this.context.CurrentTerm)
            {
                response.Vote = RPCVotes.VoteNo;
                return response;
            }
            //f votedFor is null or candidateId, and candidate’s log is at
            //least as up - to - date as receiver’s log, grant vote(§5.2, §5.4)
            if (this.context.VotedForUrl == null && requestVote.LastLogIndex >= this.context.Logs.Count - 1)
            {
                response.Vote = RPCVotes.VoteYes;
                this.context.VotedForUrl = requestVote.Candidate;
                /*
                 If election timeout elapses without receiving AppendEntries
                    RPC from current leader or granting vote to candidate:
                    convert to candidate
                 * */
                //granting vote means restart election timer
                this.context.electionTimer.Stop();
                this.context.electionTimer.Start(); //Restart election timer

                return response;
            }

            response.Vote = RPCVotes.VoteNo;
            return response;

            /*
            lock(this.context)
            {
              
                VoteResponse response = base.RequestVote(requestVote);

                if (requestVote.Term > this.context.CurrentTerm)
                {
                    this.context.CurrentTerm = requestVote.Term;
                }

                return response;
            }
            */
        }


        public override AppendEntriesAck AppendEntries(AppendEntries appendEntries)
        {
            lock (this.context)
            {

                //If RPC request or response contains term T > currentTerm:
                //set currentTerm = T, convert to follower (§5.1)
                if (appendEntries.LeaderTerm > this.context.CurrentTerm)
                {
                    this.context.CurrentTerm = appendEntries.LeaderTerm;
                }

                //Received HearthBeat from the leader
                this.context.electionTimer.Stop();
                this.context.electionTimer.Start(); //Restart election timer

                //Console.WriteLine("A: " + (appendEntries.LeaderTerm < this.context.CurrentTerm));
                //Console.WriteLine("B: " + (appendEntries.LogEntries.Count - 1 > appendEntries.PrevLogIndex && appendEntries.LogEntries[appendEntries.PrevLogIndex].Term != appendEntries.PrevLogTerm));

                if (appendEntries.LeaderTerm < this.context.CurrentTerm)
                    return new AppendEntriesAck() { Success = false, Term = this.context.CurrentTerm, Node = this.context.Address };
                if (appendEntries.LogEntries.Count - 1 > appendEntries.PrevLogIndex &&
                    appendEntries.LogEntries[appendEntries.PrevLogIndex].Term != appendEntries.PrevLogTerm)
                    return new AppendEntriesAck() { Success = false, Term = this.context.CurrentTerm, Node = this.context.Address };

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
                        Console.WriteLine($" APPENDING ENTRY WiTH INDEX {entry.Index}");
                        this.context.Logs.Add(entry);
                    }

                }

                if (appendEntries.LeaderCommitIndex > this.context.CommitIndex)
                {
                    this.context.CommitIndex = Math.Min(appendEntries.LeaderCommitIndex, this.context.Logs.Last().Index);
                    Console.WriteLine($" SETTING COMMIT INDEX TO {this.context.CommitIndex}");
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

        public override int ReceiveHearthBeath(Uri from, int term)
        {

            if (term > this.context.CurrentTerm)
            {
                this.context.CurrentTerm = term;
                this.Master = (IServer)Activator.GetObject(
                   typeof(IServer),
                   from.ToString() + "Server");
            }

            this.context.electionTimer.Stop();
            this.context.electionTimer.Start(); //Restart election timer

            return this.context.CurrentTerm;
        }
    }
}
