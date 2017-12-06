﻿using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Server
{

    public class RaftLog
    {
        public int Term { get; set; }
        public RaftCommand Command { get; set; }
    }

    public class RaftCommand
    {
        public string Name { get; set; }
        public delegate void Command();
        public Command Execute { get; set; }
    }

    public enum State
    {
        FOLLOWER, CANDIDATE, LEADER
    }

    public interface IRaft
    {
        Tuple<int, bool> RequestVote(int term, Uri candidateID, int lastLogIndex, int lastLogTerm);
        Tuple<int, int, int, bool> AppendEntries(int term, Uri leaderID, int prevLogIndex, int prevLogTerm, List<RaftLog> entries, int leaderCommit);
    }

    public class RaftServer : MarshalByRefObject, IRaft, IServer
    {
        public static int ElectionTime = 250;
        public static int LeaderTime = ElectionTime / 4;

        private System.Timers.Timer electionTimer;
        private System.Timers.Timer leaderTimer;

        //
        // ============================================================================
        // The following data needs to be persisted
        // ============================================================================
        //

        // This is the term this Raft server is currently in
        private int currentTerm;

        // This is the Raft peer that this server has voted for in *this* term (if any)
        private Uri votedFor;

        // The log is a list of {term, command} tuples, where the command is an opaque
        // value which only holds meaning to the replicated state machine running on
        // top of Raft.
        private List<RaftLog> log;

        //
        // ============================================================================
        // The following data is ephemeral
        // ============================================================================
        //

        // The state this server is currently in, can be FOLLOWER, CANDIDATE, or LEADER
        private State state;

        // The Raft entries up to and including this index are considered committed by
        // Raft, meaning they will not change, and can safely be applied to the state
        // machine.
        private int commitIndex;

        // The last command in the log to be applied to the state machine.
        private int lastApplied;

        // nextIndex is a guess as to how much of our log (as leader) matches that of
        // each other peer. This is used to determine what entries to send to each peer
        // next.
        private Dictionary<Uri, int> nextIndex;

        // matchIndex is a measurement of how much of our log (as leader) we know to be
        // replicated at each other server. This is used to determine when some prefix
        // of entries in our log from the current term has been replicated to a
        // majority of servers, and is thus safe to apply.
        private Dictionary<Uri, int> matchIndex;

        private Uri Address { get; set; }
        private TcpChannel Channel { get; set; }

        private List<Uri> peerURIs;
        private Dictionary<Uri, IRaft> peers;


        //Base server
        public int NumPlayers { get; set; }
        public int RoundIntervalMsec { get; set; }
        public List<IClient> pendingClients;
        public List<IClient> sessionClients;
        public GameStateMachine stateMachine { get; set; }
        public List<IPlayer> playerList { get; set; }
        public bool GameStartRequest { get; set; } //Used to request a start(requesting a start takes time to be commited, no more StartGame entries should be created)
        public bool HasGameStarted { get; set; }

        //Leader only
        public System.Timers.Timer RoundTimer { get; set; }
        public Dictionary<Uri, Play> plays;

        //If mode is set to true it works in test Mode
        public RaftServer(Uri address, int NumPlayers, int RoundIntervalMsec)
        {
            this.Address = address;
            this.NumPlayers = NumPlayers;
            this.RoundIntervalMsec = RoundIntervalMsec;

            this.Channel = new TcpChannel(address.Port);
            ChannelServices.RegisterChannel(this.Channel, false);
            RemotingServices.Marshal(this, "Server", typeof(RaftServer));
        }

        public void Start(List<Uri> peerURIs)
        {
            this.peerURIs = peerURIs;
            this.peers = new Dictionary<Uri, IRaft>();
            this.currentTerm = 0;
            this.votedFor = null;
            this.log = new List<RaftLog>();
            this.state = State.FOLLOWER;
            this.commitIndex = -1;
            this.lastApplied = -1;
            this.matchIndex = new Dictionary<Uri, int>();
            this.nextIndex = new Dictionary<Uri, int>();

            foreach (Uri peerUri in this.peerURIs)
            {
                matchIndex[peerUri] = -1;
                nextIndex[peerUri] = 0;
                //Get the remoting object
                IRaft peer = (IRaft)Activator.GetObject(
                    typeof(IRaft),
                    peerUri.ToString() + "Server");
                peers.Add(peerUri, peer);
            }

            electionTimer = new System.Timers.Timer(ElectionTime);
            electionTimer.Elapsed += OnElectionTimer;
            electionTimer.AutoReset = false;

            leaderTimer = new System.Timers.Timer(LeaderTime);
            leaderTimer.Elapsed += OnHeartbeatTimerOrSendTrigger;
            leaderTimer.AutoReset = true;

            //leaderTimer.Start();
            electionTimer.Start();

            RoundTimer = new System.Timers.Timer(this.RoundIntervalMsec);
            RoundTimer.Elapsed += (sender, e) => { this.NextRound(this.sessionClients); };
            RoundTimer.AutoReset = false;

            Console.WriteLine("Started Server " + this.Address);

        }

        private void OnHeartbeatTimerOrSendTrigger(object sender, ElapsedEventArgs e)
        {
            this.OnHeartbeatTimerOrSendTrigger();
        }

        private void OnElectionTimer(object sender, ElapsedEventArgs e)
        {
            this.OnElectionTimer();
        }

        // This function updates the state machine as a result of the command we pass
        // it. In order to build a replicated state machine, we need to call
        // stateMachine with the same commands, in the same order, on all servers.
        void StateMachine(RaftCommand command)
        {
            Console.WriteLine("################# COMMAND #############");
            Console.WriteLine(command.Name);
            Console.WriteLine("################# COMMAND #############");
            command.Execute();
        }

        //
        // ============================================================================
        // Raft RPC handlers
        // ============================================================================
        //

        /*
            RequestVote(term, candidateID, lastLogIndex, lastLogTerm)
    -> (term, voteGranted)
            */

        public void RegisterPeer(Uri serverUri)
        {
            // lock (this.context)
            // {
            this.peerURIs.Add(serverUri);
            this.nextIndex[serverUri] = 0;//Next log entry to send to the server is the first
            this.matchIndex[serverUri] = 0;//Last known log entry index is none

            IRaft peer = (IRaft)Activator.GetObject(
                typeof(IRaft),
                serverUri.ToString() + "Server");

            this.peers[serverUri] = peer;

            List<Uri> serverList = new List<Uri>(this.peerURIs);
            serverList.Add(this.Address);

            //Does it work? it serializes the closure?
            RaftCommand.Command command = () =>
            {
                this.peerURIs = serverList;
                this.peerURIs.Remove(this.Address);
                Console.WriteLine($"SETTING SERVER LIST size {this.peerURIs.Count}");

                if (this.state == State.LEADER)
                {
                    IEnumerable<IClient> clients = this.sessionClients == null ?
                        this.pendingClients : this.pendingClients.Concat(this.sessionClients);

                    foreach (IClient client in clients)
                    {
                        List<Uri> servers = new List<Uri>(this.peerURIs);
                        servers.Add(this.Address);
                        client.SetAvailableServers(servers);
                    }
                }
                else
                {
                    this.peers.Clear();
                    foreach (Uri uri in this.peerURIs)
                    {
                        IRaft server = (IRaft)Activator.GetObject(
                            typeof(IRaft), uri.ToString() + "Server");
                        this.peers.Add(uri, server);
                    }
                }

                if (this.state == State.FOLLOWER)
                {
                    this.electionTimer.Start();
                }
            };

            RaftLog log = new RaftLog()
            {
                Term = this.currentTerm,
                Command = new RaftCommand() { Name = "Register Peer", Execute = command }
            };

            this.log.Add(log);

            Task.Run(() => OnHeartbeatTimerOrSendTrigger());


            // }
        }

        public Tuple<int, bool> RequestVote(int term, Uri candidateID, int lastLogIndex, int lastLogTerm)
        {
            Console.WriteLine("HUHUHUHUHUHUH");
            // step down before handling RPC if need be
            if (term > this.currentTerm)
            {
                ToFollower(term);
            }

            // don't vote for out-of-date candidates
            if (term < this.currentTerm)
            {
                return new Tuple<int, bool>(this.currentTerm, false);
            }

            // don't double vote
            if (this.votedFor != null && this.votedFor != candidateID)
            {
                return new Tuple<int, bool>(this.currentTerm, false);
            }

            // check how up-to-date our log is
            int ourLastLogIndex = this.log.Count - 1;
            int ourLastLogTerm = -1;

            if (this.log.Count > 0)
            {
                ourLastLogTerm = this.log[ourLastLogIndex].Term;
            }

            // reject leaders with old logs
            if (lastLogTerm < ourLastLogTerm)
            {
                return new Tuple<int, bool>(this.currentTerm, false);
            }

            // reject leaders with short logs
            if (lastLogTerm == ourLastLogTerm && lastLogIndex < ourLastLogIndex)
            {
                return new Tuple<int, bool>(this.currentTerm, false);
            }

            this.votedFor = candidateID;
            this.electionTimer.Stop();
            this.electionTimer.Start();
            // TODO: persist Raft state
            return new Tuple<int, bool>(this.currentTerm, true);

        }

        /*
        AppendEntries(term, leaderID, prevLogIndex, prevLogTerm, entries[], leaderCommit)
            -> (term, conflictIndex, conflictTerm, success)
        */

        public Tuple<int, int, int, bool> AppendEntries(int term, Uri leaderID, int prevLogIndex, int prevLogTerm, List<RaftLog> entries, int leaderCommit)
        {

            Console.WriteLine("RECEIVED <3");
            // step down before handling RPC if need be
            if (term > this.currentTerm)
            {
                ToFollower(term);
            }

            //outdated term
            if (term < this.currentTerm)
            {
                return new Tuple<int, int, int, bool>(this.currentTerm, -1, -1, false);
            }

            //  reset election timer
            this.electionTimer.Stop();
            this.electionTimer.Start();

            if (prevLogIndex >= this.log.Count)
            {
                return new Tuple<int, int, int, bool>(this.currentTerm, this.log.Count, -1, false);
            }

            int ourPrevLogTerm;
            if (prevLogIndex > 0)
            {
                ourPrevLogTerm = log[prevLogIndex].Term;
            }
            else
            {
                ourPrevLogTerm = -1;
            }


            if (prevLogIndex >= 0 && ourPrevLogTerm != prevLogTerm)
            {
                int firstOfTerm = prevLogIndex;

                for (int i = prevLogIndex; i >= 0; i--)
                {
                    if (log[i].Term != ourPrevLogTerm)
                    {
                        break;
                    }
                    firstOfTerm = i;
                }

                return new Tuple<int, int, int, bool>(this.currentTerm, firstOfTerm, ourPrevLogTerm, false);

            }


            /*
                for i from 0 to length(entries) {
                index = prevLogIndex + i + 1
                    if index >= length(log) or log[index].term != entries[i].term {
                    log = log[:index] ++ entries[i:]
                    break
                }
            }
                */

            //remove logs with wrong index or term and append entries
            for (int i = 0; i < entries.Count; i++)
            {
                int index = prevLogIndex + i + 1;
                if (index >= this.log.Count || log[index].Term != entries[i].Term)
                {

                    this.log = this.log.Take(index).ToList();

                    while (i < entries.Count)
                    {
                        this.log.Add(entries[i]);
                        i++;
                    }

                    break;

                }
            }

            // TODO: persist Raft state

            if (leaderCommit > this.commitIndex)
            {
                this.commitIndex = this.log.Count - 1;

                if (this.commitIndex > leaderCommit)
                {
                    this.commitIndex = leaderCommit;

                }
            }

            if (commitIndex > lastApplied)
            {

                for (int i = this.lastApplied + 1; i <= this.commitIndex; i++)
                {
                    this.StateMachine(this.log[i].Command);
                    this.lastApplied = i;
                }
            }

            return new Tuple<int, int, int, bool>(this.currentTerm, -1, -1, true);


        }

        public void ToFollower(int term)
        {
            this.currentTerm = term;
            this.state = State.FOLLOWER;
            this.votedFor = null;
            this.leaderTimer.Stop();

            foreach (Uri peerUri2 in this.peerURIs)
            {
                nextIndex[peerUri2] = this.log.Count;
                matchIndex[peerUri2] = -1;
            }
        }


        public void ToLeader()
        {
            Console.WriteLine("Server " + this.Address + " Got Elected Emerged as Leader");

            this.state = State.LEADER;
            foreach (Uri peerUri2 in this.peerURIs)
            {
                nextIndex[peerUri2] = this.log.Count;
                matchIndex[peerUri2] = -1;
            }

            RoundTimer.Start();
            // reset election timer
            this.electionTimer.Start();
            // trigger sending of AppendEntries
            this.OnHeartbeatTimerOrSendTrigger();
        }

        //
        // ============================================================================
        // Raft event handlers
        // ============================================================================
        //

        public void OnElectionTimer()
        {

            if (this.state == State.LEADER)
            {
                return;
            }

            Console.WriteLine("Server " + this.Address + " Got Election Call and Emerged as Candidate");

            int electionTerm = 0;

            this.currentTerm += 1;
            electionTerm = currentTerm;
            this.votedFor = null;
            this.state = State.CANDIDATE;

            int votes = 0;
            int nVotes = 0;

            foreach (Uri peerUri in this.peerURIs)
            {


                // NOTE: me here is this server's identifier
                // NOTE: if the RPC fails, it counts as granted = false
                // NOTE: these RPCs should be made in parallel
                Task.Run(() =>
                {

                    Console.WriteLine("SOME VOTE");
                    Tuple<int, bool> res;
                    if (this.log.Count > 0)
                    {
                        res = peers[peerUri].RequestVote(electionTerm, this.Address, this.log.Count - 1,
                            this.log[this.log.Count - 1].Term);

                    }
                    else
                    {
                        res = peers[peerUri].RequestVote(electionTerm, this.Address, this.log.Count - 1,
                            this.currentTerm);
                    }

                    int term = res.Item1;
                    bool granted = res.Item2;

                    Console.WriteLine(this.state);
                    if (this.state != State.CANDIDATE)
                    {
                        return;
                    }

                    nVotes++;

                    Console.WriteLine("####### RECEIVED VOTE ######");
                    Console.WriteLine(this.Address);
                    Console.WriteLine(term);
                    Console.WriteLine(granted);
                    Console.WriteLine(peerUri);
                    Console.WriteLine("####### RECEIVED VOTE ######");

                    if (term > this.currentTerm)
                    {
                        ToFollower(term);
                    }
                    if (granted)
                    {
                        // trigger sending of AppendEntries
                        this.electionTimer.Stop();
                        this.electionTimer.Start();
                        votes += 1;
                    }

                    if (this.currentTerm != electionTerm)
                    {
                        return;
                    }

                    Console.WriteLine(votes);
                    if (nVotes == this.peers.Count && votes <= this.peers.Count / 2)
                    {
                        this.state = State.FOLLOWER;
                        return;
                    }

                    if (votes > this.peers.Count / 2)
                    {
                        ToLeader();
                    }


                });
            }

            //Randomize election timer and repeat
            int time = new Random().Next(ElectionTime, (int)1.5 * ElectionTime);
            Console.WriteLine("Server " + this.Address + " new Time " + time);
            electionTimer.Interval = time;
            electionTimer.Start();

        }

        public void OnHeartbeatTimerOrSendTrigger()
        {
            // NOTE: it may be useful to have separate timers for each peer, so
            // that you can retry AppendEntries to one peer without sending to all
            // peers.

            
            if (state != State.LEADER)
            {
                //Não percebi a necessidade disto.
                //this.leaderTimer.Start();
                return;
            }

            Console.WriteLine("Server " + this.Address + " Send HeartBeat");

            foreach (Uri peerUri in this.peerURIs)
            {

                if (peerUri == this.Address) continue;
                // NOTE: do this in parallel for each peer
                Task.Run(() =>
                {
                    int rfNextIndex;
                    List<RaftLog> entries = new List<RaftLog>();
                    lock (this)
                    {
                        rfNextIndex = this.nextIndex[peerUri];

                        if (this.nextIndex[peerUri] > this.log.Count)
                        {
                            rfNextIndex = this.log.Count;
                        }

                        for (int i = rfNextIndex; i < this.log.Count; i++)
                        {
                            entries.Add(this.log[i]);
                        }
                    }

                    int prevLogIndex = rfNextIndex - 1;
                    int prevLogTerm = -1;

                    if (prevLogIndex >= 0)
                    {
                        prevLogTerm = log[prevLogIndex].Term;
                    }

                    int sendTerm = this.currentTerm;

                    // NOTE: if length(entries) == 0, you may want to check that we
                    // haven't sent this peer an AppendEntries recently. If we
                    // have, just return.

                    // NOTE: if the RPC fails, stop processing for this peer, but
                    // trigger sending AppendEntries again immediately.
                    if (this.log.Count > 0)
                    {
                        Console.WriteLine();
                    }

                    Tuple<int, int, int, bool> res = peers[peerUri].AppendEntries(sendTerm, this.Address, prevLogIndex, prevLogTerm, entries, this.commitIndex);


                    int term = res.Item1;
                    int conflitIndex = res.Item2;
                    int conflitTerm = res.Item3;
                    bool success = res.Item4;

                    if (term > this.currentTerm)
                    {
                        ToFollower(term);
                    }

                    if (currentTerm != sendTerm)
                    {
                        return;
                    }

                    if (!success)
                    {
                        this.nextIndex[peerUri] = conflitIndex;

                        if (conflitTerm != -1)
                        {
                            int ourLastInConflictTerm = -1;

                            for (int i = prevLogIndex; i >= 0; i--)
                            {
                                if (this.log[i].Term == conflitTerm)
                                {
                                    ourLastInConflictTerm = i;
                                    break;
                                }
                                else if (this.log[i].Term < conflitTerm)
                                {
                                    break;
                                }
                            }

                            if (ourLastInConflictTerm != -1)
                            {
                                nextIndex[peerUri] = ourLastInConflictTerm + 1;
                            }

                        }
                        // Trigger sending AppendEntries again immediately
                        //this.leaderTimer.Stop();
                        //this.leaderTimer.Start();
                        this.OnHeartbeatTimerOrSendTrigger();

                        return;
                    }

                    matchIndex[peerUri] = prevLogIndex + entries.Count;
                    nextIndex[peerUri] = matchIndex[peerUri] + 1;

                    for (int n = this.log.Count - 1; n > this.commitIndex; n--)
                    {
                        if (log[n].Term != this.currentTerm)
                        {
                            break;
                        }

                        int replicas = 0;

                        foreach (Uri peerUri2 in this.peerURIs)
                        {
                            if (this.matchIndex[peerUri2] >= n)
                            {
                                replicas += 1;
                            }
                        }

                        if (replicas > this.peers.Count / 2)
                        {
                            commitIndex = n;
                            //not sure about this
                            this.StateMachine(this.log[commitIndex].Command);
                            break;
                        }

                    }

                });
            }
        }

        public void OnCommand(RaftCommand command, out bool accepted, out int willCommitAt)
        {
            if (this.state != State.LEADER)
            {
                accepted = false;
                willCommitAt = -1;
                return;
            }

            this.log.Add(new RaftLog() { Command = command, Term = this.currentTerm });
            nextIndex[this.Address] = this.log.Count;
            matchIndex[this.Address] = this.log.Count - 1;

            // TODO: persist Raft state
            // trigger sending of AppendEntries
            this.OnHeartbeatTimerOrSendTrigger();
            //this.leaderTimer.Stop();
            //this.leaderTimer.Start();

            accepted = true;
            willCommitAt = this.log.Count - 1;

            Console.WriteLine("Accepted command at commit " + willCommitAt);
        }



        //LEADER

        //Remote
        public Uri GetLeader()
        {
            if(this.state == State.LEADER)
                return this.Address;  //Send my address
            //todo - send the leader
            return null;
        }

        //Transitates to the next round
        private void NextRound(List<IClient> sessionClients)
        {
            if (this.stateMachine.HasGameEnded())
            {
                Console.WriteLine("  GAME HAS ENDED");
                //TODO: Create a new game if the pending clients are enough

                // TODO: handle clients and server lists
                // TODO: Generate Log Entry and multicast AppendEntries
                return;
            }


            RaftCommand.Command command = () =>
            {
                Console.WriteLine("number players:" + this.stateMachine.Stage.GetPlayers().Count);

                foreach (Uri address in plays.Keys)
                {
                    IPlayer player = this.stateMachine
                        .Stage.GetPlayers().First(p => p.Address.ToString() == address.ToString());
                    this.stateMachine.SetPlay(player, plays[address]);
                }

                if (this.state == State.LEADER)
                {
                    List<Shared.Action> actionList = this.stateMachine.NextRound();

                    IClient client;
                    for (int i = this.sessionClients.Count - 1; i >= 0; i--)
                    {
                        try
                        {
                            client = this.sessionClients.ElementAt(i);
                            client.SendRound(actionList, this.playerList, this.stateMachine.Round);
                            Console.WriteLine(String.Format("Sending stage to client: {0}, at: {1}", client.Username, client.Address));
                            Console.WriteLine(String.Format("Round Nº{0}", this.stateMachine.Round));
                        }
                        catch (Exception)
                        {
                            this.sessionClients.RemoveAt(i);


                            // todo: try to reach the client again. Uma thread à parte. Verificar se faz sentido.

                            /*todo:
                             * qual a estrategia a adoptar aqui para tentar reconectar com o cliente?
                             * 
                             * Dectar falhas de clientes, lidar com falsos positivos.
                             * 
                             * Caso não seja pssível contactar o cliente, na próxima ronda deve de ir uma acção em que o player 
                             * está morto, e deve ser removido do jogo.
                             * E deve ser apresentado no chat UMA MENSAGEM no chat a indicar que o jogador saiu do jogo
                             * 
                             * garantimos a possibilidade de um cliente voltar a entrar no jogo?
                             * 
                             */
                        }
                    }

                    this.RoundTimer.Start(); //Start the timer

                };


            };

            RaftLog log = new RaftLog()
            {
                Term = this.currentTerm,
                Command = new RaftCommand() { Name = "New Round", Execute = command }
            };

            this.log.Add(log);

            Task.Run(() => OnHeartbeatTimerOrSendTrigger());
        }

        //TODO - Revisit
        private void broadcastEndGame(List<IClient> sessionClients)
        {
            IClient client;
            for (int i = 0; i < sessionClients.Count; i++)
            {
                try
                {
                    client = sessionClients.ElementAt(i);
                    client.End(this.stateMachine.GetTopPlayer());
                }
                catch (Exception)
                {
                    //sessionClient.RemoveAt(i);
                    // todo: try to reach the client again. Uma thread à parte. Verificar se faz sentido.
                }
            }
        }


        //Remote
        public JoinResult Join(string username, Uri address)
        {
            if (this.state != State.LEADER)
            {
                throw new Exception("The server is currently not a leader");
            }

            lock (this)
            {
                if (this.pendingClients.Exists(c => c.Username == username))
                {
                    // TODO: Lauch exception to the client (username already exists)
                    return JoinResult.REJECTED_USERNAME;
                }

                Console.WriteLine($"  CLIENT '{username} - {address.ToString()}' HAS BEEN QUEUED");

                RaftCommand.Command command = () =>
                {
                    Console.WriteLine("Join Commited.");
                    IClient client = (IClient)Activator.GetObject(
                        typeof(IClient),
                        address.ToString() + "Client");

                    this.pendingClients.Add(client);
                    Console.WriteLine("ADDDED NOW PENDING: " + this.pendingClients.Count);

                    if (this.state == State.LEADER)
                    {
                        //Have enough players
                        if (!this.HasGameStarted && !this.GameStartRequest &&
                            this.pendingClients.Count >= this.NumPlayers)
                        {
                            this.GameStartRequest = true; // this is used to make sure no more startgame logs are created before the startgame entry is commited

                            //Make start log

                            RaftCommand.Command startCommand = () =>
                            {
                                this.GameStartRequest = false; //request fullfilled
                                Console.WriteLine("Game start commited.");
                                //In the leader it might be necessary to lock 

                                foreach (IClient client2 in this.pendingClients)
                                {
                                    Console.WriteLine($"pending {client2.Address}");
                                }


                                this.sessionClients = this.pendingClients.Take(this.NumPlayers).ToList(); //get the first N clients
                                this.pendingClients = this.pendingClients.Skip(this.NumPlayers).ToList();

                                Console.WriteLine($"Numplayers {this.NumPlayers}");

                                this.HasGameStarted = true;
                                foreach (IClient client2 in this.sessionClients)
                                {
                                    Console.WriteLine($"Address session players {client2.Address}");
                                    IPlayer player = new Player();
                                    player.Address = client2.Address;
                                    player.Alive = true;
                                    player.Score = 0;
                                    player.Username = client2.Username;
                                    this.playerList.Add(player);
                                }

                                this.stateMachine = new GameStateMachine(this.NumPlayers, this.playerList);

                                if (this.state == State.LEADER)
                                {

                                    //Broadcast the start signal to the client
                                    Dictionary<string, Uri> clientsP2P = new Dictionary<string, Uri>();
                                    foreach (IClient c in this.sessionClients)
                                    {
                                        clientsP2P[c.Username] = c.Address;
                                    }
                                    //Communication with the client must be done with the leader
                                    for (int i = this.sessionClients.Count - 1; i >= 0; i--)
                                    {
                                        try
                                        {
                                            IClient client2 = this.sessionClients.ElementAt(i);
                                            client2.Start(this.stateMachine.Stage); //Signal the start
                                            client2.SetPeers(clientsP2P); //Set the peers for the P2P chat
                                        }
                                        catch (Exception e)
                                        {
                                            this.sessionClients.RemoveAt(i);
                                            //todo : maybe remove from the whole list of clients
                                            // todo: try to reach the client again. Uma thread à parte. Verificar se faz sentido.
                                        }
                                    }

                                    //Start the game timer
                                    this.RoundTimer.AutoReset = false;
                                    this.RoundTimer.Start();
                                }
                            };

                            RaftLog startLog = new RaftLog()
                            {
                                Term = this.currentTerm,
                                Command = new RaftCommand() { Name = "Start Game", Execute = startCommand }
                            };

                            this.log.Add(startLog);

                            Task.Run(() => OnHeartbeatTimerOrSendTrigger());


                        }
                    }
                };

                RaftLog log = new RaftLog()
                {
                    Term = this.currentTerm,
                    Command = new RaftCommand() { Name = "Join", Execute = command }
                };

                this.log.Add(log);

                Task.Run(() => OnHeartbeatTimerOrSendTrigger());

                return JoinResult.QUEUED;
            }
        }

        public void SetPlay(Uri address, Play play, int round)
        {
            if (this.state != State.LEADER)
            {
                throw new Exception("The server is currently not a leader");
            }
            //lock (this.context) //This lock is blocking the game for some reason, maybe its a flood of SetPlay's that try to acquire the lock
            //{
            if (this.stateMachine == null || !this.HasGameStarted)
            {
                throw new Exception("Game hasn't started yet");
            }

            this.plays[address] = play;
            //}
        }

        public void Quit(Uri address)
        {
            if (this.state != State.LEADER)
            {
                throw new Exception("The server is currently not a leader");
            }
            Console.WriteLine($"  CLIENT '{address.ToString()}' HAS DISCONNETED");
            this.pendingClients.RemoveAll(p => p.Address.ToString() == address.ToString());
            this.sessionClients.RemoveAll(p => p.Address.ToString() == address.ToString());
        }
    } 
}
