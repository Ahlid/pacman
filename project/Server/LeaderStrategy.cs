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

        //criação normal de um lider
        public LeaderStrategy(ServerContext context) : base(context, ServerContext.Role.Leader)
        {
            //start hearthbeat
            this.context.LeaderTimer = new System.Timers.Timer(ServerStrategy.LeaderTimeout);
            this.context.LeaderTimer.Elapsed += (sender, e) =>  { this.SendHearthBeat(); };
            this.context.LeaderTimer.AutoReset = false;
            this.context.LeaderTimer.Enabled = true;

            //Configure but not start
            this.context.Timer = new System.Timers.Timer(this.context.RoundIntervalMsec);
            this.context.Timer.Elapsed += (sender, e) => { this.NextRound(this.context.sessionClients); };

            //sempre que ele emerge como lider manda logo um hearthbeat
            this.SendHearthBeat();   
        }

        // aqui é quando ele era um candidate e emerge como lider
        public LeaderStrategy(ServerContext context, CandidateStrategy prevCandidateStrategy) : this(context)
        {
            //aqui é copiar os valorer porque eles não se perdem ok
            //this.context = context;
            //this.context.Logs = prevCandidateStrategy.Logs;
            //this.context.CommitIndex = prevCandidateStrategy.CommitIndex;
            //this.CurrentTerm = prevCandidateStrategy.CurrentTerm;
            //this.LastApplied = prevCandidateStrategy.LastApplied;
            //this.VotedForUrl = prevCandidateStrategy.VotedForUrl;

            //todo - Copy the rest of the game session information
        }


        //Transitates to the next round
        private void NextRound(List<IClient> sessionClients)
        {
            if (this.context.stateMachine.HasGameEnded())
            {
                Console.WriteLine("Game has ended!");
                //TODO: Create a new game if the pending clients are enough

                // TODO: handle clients and server lists
                // TODO: Generate Log Entry and multicast AppendEntries
                return;
            }
            Console.WriteLine($"TIme {this.context.RoundIntervalMsec}");
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
            lock(this.context)
            {
                Console.WriteLine("lock");
                Console.WriteLine($"Username {username} Address {address.ToString()}");
                if (this.context.pendingClients.Exists(c => c.Username == username))
                {
                    // TODO: Lauch exception to the client (username already exists)
                    return JoinResult.REJECTED_USERNAME;
                }

                Console.WriteLine(address.ToString());
                Console.WriteLine(string.Format("Sending to client '{0}' that he has just been queued", username));

                ICommand command = new JoinCommand(address);
                LogEntry entry = new LogEntry()
                {
                    Command = command,
                    Index = this.context.Logs.Count,
                    Term = this.context.CurrentTerm
                };
                this.context.Logs.Add(entry);

                Task.Run(() => SendAppendEntries());

                Console.WriteLine("unlock");
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
            Console.WriteLine(String.Format("Client at {0} is disconnecting.", address));
            this.context.pendingClients.RemoveAll(p => p.Address.ToString() == address.ToString());
            this.context.sessionClients.RemoveAll(p => p.Address.ToString() == address.ToString());
        }

        /// REPLICATION

        public override void RegisterReplica(Uri replicaServerURL)
        {
            lock(this.context)
            {
                Console.WriteLine("lock");
                this.context.ReplicaServersURIsList.Add(replicaServerURL);
                this.context.nextIndex[replicaServerURL] = 0;//Next log entry to send to the server is the first
                this.context.matchIndex[replicaServerURL] = 0;//Last known log entry index is none

                IServer replica = (IServer)Activator.GetObject(
                    typeof(IServer),
                    replicaServerURL.ToString() + "Server");

                this.context.Replicas[replicaServerURL] = replica;

                //Generate the log entry
                ICommand command = new ServerListChangedCommand(this.context.ReplicaServersURIsList);
                LogEntry entry = new LogEntry()
                {
                    Command = command,
                    Index = this.context.Logs.Count,
                    Term = this.context.CurrentTerm
                };
                this.context.Logs.Add(entry);

                Task.Run(() => SendAppendEntries());


                Console.WriteLine("unlock");
            }
        }

        private void SendHearthBeat()
        {
            SendAppendEntries();
            this.context.LeaderTimer.Start(); //Restart leader timer
        }




        //send entries to one peer
        private void SendAppendEntries() //Can throw exception, in that case the append must be retried later
        {
            lock (this.context)
            {
                if (this.context.matchIndex.Count == 0)
                {
                    this.context.CommitIndex = this.context.Logs.Count;
                }

                Console.WriteLine("lock");
                foreach (Uri peer in this.context.ReplicaServersURIsList)
                {
                    do
                    {
                        int prevIndex = Math.Max(this.context.nextIndex[peer] - 1, 0); // agrupar com o último index

                        if (this.context.matchIndex[peer] >= this.context.nextIndex[peer])
                        {
                            List<LogEntry> copyEntries = new List<LogEntry>(); //copiar as entries que faltam
                            Console.WriteLine("this.context.nextIndex[peer] " + this.context.nextIndex[peer]);
                            for (int i = this.context.nextIndex[peer]; i < this.context.Logs.Count; i++)
                            {
                                copyEntries.Add(this.context.Logs[i]);
                            }

                            try
                            {
                                AppendEntriesAck ack = ((IReplica)this.context.Replicas[peer]).AppendEntries(new AppendEntries()
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
                                    this.StepDown(ack.Term);
                                    break;
                                }
                                else if (this.context.CurrentTerm == ack.Term && ack.Success) //Success
                                {
                                    Console.WriteLine();
                                    //atualizar os valores de match
                                    this.context.matchIndex[peer] = this.context.Logs.Count - 1;
                                    this.context.nextIndex[peer] = this.context.Logs.Count - 1;
                                    break;
                                }
                                else if (this.context.CurrentTerm == ack.Term && !ack.Success)
                                {
                                    //aqui diziam pra fazer isto no algoritmo mas nao entendi o porque
                                    this.context.nextIndex[peer]--; //isto aqui é porque o gajo vai ter que agarrar mais um log, e vai aumentando a cada vez que falha
                                }

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                                break;
                            }
                        }
                        
                    } while (true);

                    List<int> sortedMatches = this.context.matchIndex.Values.ToList();
                    sortedMatches.OrderByDescending(i => i);
                    int index = (int)sortedMatches.Count / 2;
                    this.context.CommitIndex = sortedMatches.ElementAt(index);

                    Task.Run(() => { base.CheckLogs(); });

                    Console.WriteLine("unlock");

                }
                
            } //lock
        }

        private void StepDown(int term)
        {
            lock (this)//prender este objeto 
            {
                this.context.CurrentTerm = term; //atualizar o termo

                //parar os timers
                this.context.LeaderTimer.Stop();
                this.context.Timer.Stop();
                //aqui baixar a follower mas esta mandeira pareceu-me estranha, criaste o follower e deixaste ai?
                //sim a partir daqui deixas de ser lider e passar a ser follower,  sima mas nao guardaste 
                //fiquei confuso onde tinha de guardar... 
                //como acedes ai a partir daqui?? Temos que arranjar maneira.
                //esta foi uma das cenas que me deixou confuso, Podes passar um delegate para a estrategia.
                new FollowerStrategy(this.context, this);
            }
        }

        public void ReceiveCommand(ICommand command)
        {
            this.context.Logs.Add(new LogEntry()
            {
                Command = command,
                Term = this.context.CurrentTerm,
                Index = this.context.Logs.Count
            });
        }

        public override AppendEntriesAck AppendEntries(AppendEntries appendEntries)
        {
            throw new NotImplementedException();
        }
    }
}
