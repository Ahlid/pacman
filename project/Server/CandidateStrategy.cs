using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Server.ServerContext;

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

        private System.Timers.Timer timer;
        private bool stopElecting;


        public CandidateStrategy(ServerContext context) : base(context, Role.Candidate)
        {
            Console.WriteLine("\n****CANDIDATE*****\n");

            /*if(!System.Diagnostics.Debugger.IsAttached)
            System.Diagnostics.Debugger.Launch();
            */

            timer = new System.Timers.Timer();
            timer.Interval = new Random().Next(150, 300);
            timer.Elapsed += (sender, e) => { StartNewElection(); };
            timer.AutoReset = false;
            Task.Run(() => { StartNewElection(); });
            stopElecting = false;
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


        private void StartNewElection() {

            if (this.context.CurrentRole != Role.Candidate)
            {
                return;
            }

                bool done;
            
            lock (this.context)
            {
                Console.WriteLine("\n  STARTING ELECTION");
                this.context.CurrentTerm++;
                this.context.VotedForUrl = this.context.Address;
                this.receivedVotes = new List<Uri>();
                this.receivedVotes.Add(this.context.VotedForUrl); //self vote
                 done = false;
            }

            int electionTerm = this.context.CurrentTerm;
                Console.WriteLine($"  COMMIT INDEX: {this.context.CommitIndex}");
                Console.WriteLine($"  TERM: {this.context.CurrentTerm}");

                Console.WriteLine($"  NUMBER OF SERVERS KNOWN: {this.context.OtherServers.Count}");
                foreach (Uri uri in this.context.OtherServersUrls)
                {
                    Console.WriteLine($"  KNOWS: {uri}");
                }

                foreach (IServer server in this.context.OtherServers.Values)
                {
                    Task.Run(() => {
                        Console.WriteLine($"  SENDING ELECTION REQUEST");
                        try
                        {
                            RequestVote requestVote;
                            lock (this.context)
                            {
                                requestVote = new RequestVote()
                                {
                                    Candidate = this.context.Address,
                                    LastLogIndex = this.context.Logs.Count - 1,
                                    LastLogTerm = this.context.Logs.Count == 0 ? 1 : this.context.Logs.Last().Term,
                                    Term = this.context.CurrentTerm
                                };
                            }

                                VoteResponse response = ((IReplica)server).RequestVote(requestVote);

                            

                            if (done || this.stopElecting || electionTerm != this.context.CurrentTerm)
                            {
                                return;
                            }

                            if (response.Vote == RPCVotes.VoteNo)
                            {
                                if(response.VoterTerm > this.context.CurrentTerm)
                                {
                                done = true;
                                this.stepDown(response.Voter, response.VoterTerm);
                                }
                                Console.WriteLine("  VOTE: NO");
                             return;
                            }

                            Console.WriteLine("  VOTE: YES");

                            if (!this.receivedVotes.Contains(response.Voter))
                            {
                                this.receivedVotes.Add(response.Voter);
                            }


                            int majority = this.context.OtherServersUrls.Count / 2 + 1;
                            Console.WriteLine($"  MAJORITY {majority}\n  VOTES RECEIVED {this.receivedVotes.Count}");
                            if (majority >= this.receivedVotes.Count)
                            {
                                done = true;
                                this.stopElecting = true;
                                timer.Stop();

                                //We have a majority
                                Console.WriteLine("EMERGING AS LEADER");
                                Console.WriteLine("With term "+this.context.CurrentTerm);
                                this.context.SwitchStrategy(this, new LeaderStrategy(this.context));
                                return;
                            }
                        }
                        catch (Exception)
                        {
                            return;
                        }
                    });
                    //{
                      
                //    });
                }
                if (!stopElecting)
                {
                    timer.Interval = new Random().Next(ServerStrategy.LeaderTimeout, ServerStrategy.ElectionTimeout);
                    timer.Start();
                }

            
        }
        public override VoteResponse RequestVote(RequestVote requestVote)
        {
            lock(this.context)
            {
               

                if (requestVote.Term > this.context.CurrentTerm)
                {

                    Console.WriteLine("#################### Received vote request from someone with higher term");
                   // this.timer.Stop();
                    //this.timer.Start();
                   // this.context.CurrentTerm = requestVote.Term;
                    stepDown(requestVote.Candidate,requestVote.Term);
                    
                }

                return new VoteResponse()
                {
                    Vote = RPCVotes.VoteNo,
                    Voter = this.context.Address,
                    VoterTerm = this.context.CurrentTerm
                };
            }    
        }

        private FollowerStrategy stepDown(Uri leader, int term) {

            lock (this.context) { 
                timer.Stop();
                this.stopElecting = true;
                //go back to follower
                this.context.CurrentTerm = term;
                FollowerStrategy follower = new FollowerStrategy(this.context, leader);
                this.context.SwitchStrategy(this, follower);
                return follower;
            }

        }

        public override AppendEntriesAck AppendEntries(AppendEntries appendEntries)
        {
           
                FollowerStrategy follower = stepDown(appendEntries.Leader, appendEntries.LeaderTerm);
                return follower.AppendEntries(appendEntries);
            
        }

        public override int ReceiveHearthBeath(Uri from, int term)
        {

            if (term > this.context.CurrentTerm)
            {
                this.stepDown(from, term);
            }
            
            return this.context.CurrentTerm;
        }
    }
}
