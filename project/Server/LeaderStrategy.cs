using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;


namespace Server
{
    public class LeaderStrategy : ServerStrategy, IServer
    {

        public LeaderStrategy(ServerContext context) : base(context, ServerContext.Role.Leader)
        {

            Console.WriteLine("\n****LEADER*****\n");
            //start hearthbeat
            this.context.LeaderTimer = new System.Timers.Timer(ServerStrategy.LeaderTimeout);
            this.context.LeaderTimer.Elapsed += (sender, e) => { this.SendHearthBeat(); };
            this.context.LeaderTimer.AutoReset = false;
            this.context.LeaderTimer.Enabled = true;

            //Configure but not start
            this.context.Timer = new System.Timers.Timer(this.context.RoundIntervalMsec);
            this.context.Timer.Elapsed += (sender, e) => { this.NextRound(this.context.sessionClients); };
            this.context.Timer.AutoReset = false;

            this.context.nextIndex.Clear();
            this.context.matchIndex.Clear();
            foreach (Uri uri in this.context.OtherServersUrls)
            {
                this.context.nextIndex[uri] = 0;//Next log entry to send to the server is the first
                this.context.matchIndex[uri] = 0;
            }

            if (this.context.HasGameStarted)
            {
                //Start the game timer

                context.Timer.Start();
            }

            //Set hearthbeat on start
            this.SendHearthBeat();
        }

        //Transitates to the next round
        private void NextRound(List<IClient> sessionClients)
        {
            if (this.context.stateMachine.HasGameEnded())
            {
                Console.WriteLine("  GAME HAS ENDED");
                //TODO: Create a new game if the pending clients are enough

                // TODO: handle clients and server lists
                // TODO: Generate Log Entry and multicast AppendEntries
                return;
            }

            ICommand command;
            command = new RoundCommand(this.context.plays);
            this.context.plays = new Dictionary<Uri, Play>();
            LogEntry entry = new LogEntry()
            {
                Command = command,
                Index = this.context.Logs.Count,
                Term = this.context.CurrentTerm
            };
            this.context.Logs.Add(entry);
            Task.Run(() => SendAppendEntries());
        }

        private void broadcastEndGame(List<IClient> sessionClients)
        {
            IClient client;
            for (int i = 0; i < sessionClients.Count; i++)
            {
                try
                {
                    client = sessionClients.ElementAt(i);
                    client.End(this.context.stateMachine.GetTopPlayer());
                }
                catch (Exception)
                {
                    //sessionClient.RemoveAt(i);
                    // todo: try to reach the client again. Uma thread à parte. Verificar se faz sentido.
                }
            }
        }

        //Remote
        public override Uri GetLeader()
        {
            return this.context.Address;  //Send my address
        }

        //Remote
        public override JoinResult Join(string username, Uri address)
        {
            lock (this.context)
            {
                if (this.context.pendingClients.Exists(c => c.Username == username))
                {
                    // TODO: Lauch exception to the client (username already exists)
                    return JoinResult.REJECTED_USERNAME;
                }

                Console.WriteLine($"  CLIENT '{username} - {address.ToString()}' HAS BEEN QUEUED");

                ICommand command = new JoinCommand(address);
                LogEntry entry = new LogEntry()
                {
                    Command = command,
                    Index = this.context.Logs.Count,
                    Term = this.context.CurrentTerm
                };
                this.context.Logs.Add(entry);

                Task.Run(() => SendAppendEntries());

                return JoinResult.QUEUED;
            }
        }

        public override void SetPlay(Uri address, Play play, int round)
        {
            //lock (this.context) //This lock is blocking the game for some reason, maybe its a flood of SetPlay's that try to acquire the lock
            //{
            if (this.context.stateMachine == null || !this.context.HasGameStarted)
            {
                throw new Exception("Game hasn't started yet");
            }

            this.context.plays[address] = play;
            //}
        }

        public override void Quit(Uri address)
        {
            Console.WriteLine($"  CLIENT '{address.ToString()}' HAS DISCONNETED");
            this.context.pendingClients.RemoveAll(p => p.Address.ToString() == address.ToString());
            this.context.sessionClients.RemoveAll(p => p.Address.ToString() == address.ToString());
        }

        /// REPLICATION

        public override void RegisterReplica(Uri replicaServerURL)
        {
            lock (this.context)
            {
                this.context.OtherServersUrls.Add(replicaServerURL);
                this.context.nextIndex[replicaServerURL] = 0;//Next log entry to send to the server is the first
                this.context.matchIndex[replicaServerURL] = 0;//Last known log entry index is none

                IServer replica = (IServer)Activator.GetObject(
                    typeof(IServer),
                    replicaServerURL.ToString() + "Server");

                this.context.OtherServers[replicaServerURL] = replica;

                List<Uri> serverList = new List<Uri>(this.context.OtherServersUrls);
                serverList.Add(this.context.Address);

                //Generate the log entry
                ICommand command = new ServerListChangedCommand(serverList);
                LogEntry entry = new LogEntry()
                {
                    Command = command,
                    Index = this.context.Logs.Count,
                    Term = this.context.CurrentTerm
                };
                this.context.Logs.Add(entry);

                Task.Run(() => SendAppendEntries());
            }
        }

        private void SendHearthBeat()
        {
            Console.WriteLine("  SENDING HEARTHBEAT");
            SendAppendEntries();
            this.context.LeaderTimer.Start(); //Restart leader timer
        }

        //send entries to one peer
        private void SendAppendEntries() //Can throw exception, in that case the append must be retried later
        {
            lock (this.context)
            {
                //if (this.context.matchIndex.Count == 0)
                //{
                //    this.context.CommitIndex = this.context.Logs.Count;
                //}

                Console.WriteLine($"  NUMBER OF SERVERS KNOWN: {this.context.OtherServersUrls.Count}");
                foreach (Uri peer in this.context.OtherServersUrls)
                {
                    Task.Run(() =>
                    {

                        do
                        {
                            int lastLogIndex = this.context.Logs.Count - 1;
                            int prevIndex = Math.Max(this.context.nextIndex[peer] -1, 0); // agrupar com o último index
                            if (lastLogIndex >= prevIndex)
                            {
                                List<LogEntry> copyEntries = new List<LogEntry>(); //copiar as entries que faltam

                                for (int i = this.context.nextIndex[peer]; i < this.context.Logs.Count; i++)
                                {
                                    copyEntries.Add(this.context.Logs[i]);
                                }

                                try
                                {
                                    Console.WriteLine($"  $$$ SENDING APPEND ENTRIES to {peer} $$$");
                                    AppendEntriesAck ack = ((IReplica)this.context.OtherServers[peer]).AppendEntries(new AppendEntries()
                                    {
                                        Leader = this.context.Address,
                                        LeaderTerm = this.context.CurrentTerm,
                                        PrevLogIndex = prevIndex,
                                        PrevLogTerm = this.context.Logs[prevIndex].Term,
                                        LeaderCommitIndex = this.context.CommitIndex,
                                        LogEntries = copyEntries
                                    });

                                    //se tem um maior termo temos de deixar de ser lider
                                    if (ack.Term > this.context.CurrentTerm)
                                    {
                                        //baixar para follower
                                        this.stepDown(ack.Term);
                                        break;
                                    }
                                    else if (this.context.CurrentTerm == ack.Term && ack.Success) //Success
                                    {
                                        Console.WriteLine("  APPEND ENTRIES WAS SUCESSFULL");
                                        //atualizar os valores de match
                                        this.context.matchIndex[peer] = this.context.Logs.Count - 1;
                                        this.context.nextIndex[peer] = this.context.Logs.Count ;
                                        break;
                                    }
                                    else if (this.context.CurrentTerm == ack.Term && !ack.Success)
                                    {
                                        Console.WriteLine("  APPEND ENTRIES WAS UNSUCCESSFULL");
                                        this.context.nextIndex[peer]--;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"  SERVER {peer} IS UNREACHABLE");
                                    break;
                                }
                            }
                            else
                            {
                                Console.WriteLine("***********INFINITE LOOP***********");
                                AppendEntriesAck ack = ((IReplica)this.context.OtherServers[peer]).AppendEntries(new AppendEntries()
                                {
                                    Leader = this.context.Address,
                                    LeaderTerm = this.context.CurrentTerm,
                                    PrevLogIndex = prevIndex,
                                    PrevLogTerm = this.context.Logs[prevIndex].Term,
                                    LeaderCommitIndex = this.context.CommitIndex,
                                    LogEntries = new List<LogEntry>()
                                });
                                break;
                            }

                        } while (true);

                        List<int> sortedMatches = this.context.matchIndex.Values.ToList();
                        sortedMatches.OrderByDescending(i => i);
                        int index = (int)sortedMatches.Count / 2;
                        this.context.CommitIndex = sortedMatches.ElementAt(index);
                        base.CheckLogs();
                    });

                   // Task.Run(() => { });
                }
            } //lock
        }

        public override VoteResponse RequestVote(RequestVote requestVote)
        {
            lock (this.context)
            {
                if (this.context.CurrentTerm < requestVote.Term)
                {
                    //this.context.CurrentTerm = requestVote.Term;
                    stepDown(requestVote.Term);
                }

            }

            return new VoteResponse()
            {
                Vote = RPCVotes.VoteNo,
                Voter = this.context.Address,
                VoterTerm = this.context.CurrentTerm
            };
        }

        private void stepDown(int term)
        {
            lock (this)//prender este objeto 
            {

                Console.WriteLine("STEP DOWN LEADER");
                Console.WriteLine("Term " + this.context.CurrentTerm);
                Console.WriteLine("TO " + term);

                this.context.CurrentTerm = term; //atualizar o termo

                //parar os timers
                this.context.LeaderTimer.Stop();
                this.context.Timer.Stop();

                this.context.SwitchStrategy(this, new FollowerStrategy(this.context, this.context.Address));
            }
        }

        public void ReceiveCommand(ICommand command)
        {
            lock (this.context)
            {
                this.context.Logs.Add(new LogEntry()
                {
                    Command = command,
                    Term = this.context.CurrentTerm,
                    Index = this.context.Logs.Count
                });


            }

            this.SendAppendEntries();

        }

        public override AppendEntriesAck AppendEntries(AppendEntries appendEntries)
        {
            throw new NotImplementedException();
        }
    }
}
