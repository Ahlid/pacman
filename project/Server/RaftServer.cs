using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Server.RaftCommands;

namespace Server
{

    public class RaftServer : MarshalByRefObject, IRaft, IServer
    {
        private bool HasServerStarted = false;

        public static int ElectionTime = 800;
        public static int LeaderTime = 100;

        public System.Timers.Timer electionTimer;
        private System.Timers.Timer leaderTimer;
        private System.Timers.Timer managerTimer;

        //
        // ============================================================================
        // The following data needs to be persisted
        // ============================================================================
        //

        // This is the term this Raft server is currently in
        public int currentTerm;

        // This is the Raft peer that this server has voted for in *this* term (if any)
        public Uri votedFor;

        // The log is a list of {term, command} tuples, where the command is an opaque
        // value which only holds meaning to the replicated state machine running on
        // top of Raft.
        public List<RaftLog> log;

        public long lastHeartBeatTime;

        //
        // ============================================================================
        // The following data is ephemeral
        // ============================================================================
        //

        // The state this server is currently in, can be FOLLOWER, CANDIDATE, or LEADER
        public State state;

        // The Raft entries up to and including this index are considered committed by
        // Raft, meaning they will not change, and can safely be applied to the state
        // machine.
        public int commitIndex;

        // The last command in the log to be applied to the state machine.
        public int lastApplied;

        // nextIndex is a guess as to how much of our log (as leader) matches that of
        // each other peer. This is used to determine what entries to send to each peer
        // next.
        public Dictionary<Uri, int> nextIndex;

        // matchIndex is a measurement of how much of our log (as leader) we know to be
        // replicated at each other server. This is used to determine when some prefix
        // of entries in our log from the current term has been replicated to a
        // majority of servers, and is thus safe to apply.
        public Dictionary<Uri, int> matchIndex;

        public Uri Address { get; set; }
        public TcpChannel Channel { get; set; }

        public List<Uri> peerURIs;
        public Dictionary<Uri, IRaft> peers;

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
        private List<Uri> peerURIManager;

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

        public String Test()
        {
            return "BOT";
        }

        public void Start(List<Uri> peerURIs)
        {
            this.lastHeartBeatTime = 0;
            this.peerURIs = peerURIs;
            this.peerURIManager = new List<Uri>();
            this.peers = new Dictionary<Uri, IRaft>();
            this.currentTerm = 0;
            this.votedFor = null;
            this.log = new List<RaftLog>();
            this.state = State.FOLLOWER;
            this.commitIndex = -1;
            this.lastApplied = -1;
            this.matchIndex = new Dictionary<Uri, int>();
            this.nextIndex = new Dictionary<Uri, int>();
            this.pendingClients = new List<IClient>();
            this.plays = new Dictionary<Uri, Play>();


            foreach (Uri peerUri in this.peerURIs)
            {
                bool isStable = false;
                while (!isStable)
                {
                    peerURIManager.Add(peerUri);
                    matchIndex[peerUri] = -1;
                    nextIndex[peerUri] = 0;
                    //Get the remoting object
                    try
                    {
                        IRaft peer = (IRaft)Activator.GetObject(
                        typeof(IRaft),
                        peerUri.ToString() + "Server");
                        peers[peerUri] = peer;
                        Console.WriteLine(peers[peerUri].Test());
                        isStable = true;
                    }
                    catch (Exception)
                    {
                        isStable = false;
                    }


                }

            }


            electionTimer = new System.Timers.Timer(ElectionTime);
            electionTimer.Elapsed += OnElectionTimer;
            electionTimer.AutoReset = false;

            leaderTimer = new System.Timers.Timer(LeaderTime);
            leaderTimer.Elapsed += OnHeartbeatTimerOrSendTrigger;
            leaderTimer.AutoReset = true;


            managerTimer = new System.Timers.Timer(ElectionTime);
            managerTimer.Elapsed += OnManagerCheck;
            managerTimer.AutoReset = false;

            RoundTimer = new System.Timers.Timer(this.RoundIntervalMsec);
            RoundTimer.Elapsed += (sender, e) => { this.NextRound(); };
            RoundTimer.AutoReset = false;

            leaderTimer.Start();
            electionTimer.Start();
            managerTimer.Start();

            Console.WriteLine("Started Server " + this.Address);
            Console.WriteLine(this.peers.Count);
            HasServerStarted = true;

        }

        private async void OnManagerCheck(object sender, ElapsedEventArgs e)
        {

            foreach (Uri uri in this.peerURIManager)
            {

                await Task.Run(() =>
                {
                    try
                    {
                        this.peers[uri].Test();

                        if (!this.peerURIs.Contains(uri))
                        {
                            lock (this)
                            {
                                this.peerURIs.Add(uri);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        if (this.peerURIs.Contains(uri))
                        {
                            lock (this)
                            {
                                this.peerURIs.Remove(uri);
                            }
                        }
                    }
                });

            }


            foreach (var peerUrI in this.peerURIs)
            {
             //   Console.WriteLine(peerUrI);
            }

            this.managerTimer.Start();
        }

        private async void OnHeartbeatTimerOrSendTrigger(object sender, ElapsedEventArgs e)
        {

            if (this.state != State.LEADER)
                return;



            for (int i = 0; i < this.peerURIs.Count; i++)
            {
                Uri peer = this.peerURIs[i];
                if (peer != this.Address && peer != null)
                    this.OnHeartbeatTimerOrSendTrigger(peer);
            }


        }

        private void OnElectionTimer(object sender, ElapsedEventArgs e)
        {

            if (this.state == State.LEADER)
            {
                return;
            }


            this.OnElectionTimer();


        }

        public Tuple<int, bool> RequestVote(int term, Uri candidateUri, int lastLogIndex, int lastLogTerm)
        {
            if (!HasServerStarted)
            {
                return new Tuple<int, bool>(this.currentTerm, false);
            }

            Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON RequestVote of {this.Address} requested by {candidateUri}");
            // step down before handling RPC if need be
            lock (this)
            {
                if (term > this.currentTerm)
                {
                    Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON RequestVote {this.Address} requested by {candidateUri}: Stepping down to follower");
                    this.currentTerm = term;
                    this.state = State.FOLLOWER;
                    this.votedFor = null;

                    //Stop the timers
                    //Maybe stop the leader timer?
                    RoundTimer.Stop();
                    electionTimer.Stop();
                    electionTimer.Interval = ElectionTime;
                    electionTimer.Start();

                    foreach (Uri peer in this.peerURIs)
                    {
                        nextIndex[peer] = this.log.Count;
                        matchIndex[peer] = -1;
                    }
                }
                else if (term < this.currentTerm)
                {
                    return new Tuple<int, bool>(this.currentTerm, false);
                }


                // don't double vote
                if (this.votedFor != null && this.votedFor != candidateUri)
                {
                    Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON RequestVote {this.Address} requested by {candidateUri}: Already voted for another candidate, voted NO");
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
                    Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON RequestVote {this.Address} requested by {candidateUri}: Candidate has a lower term, voted NO");
                    return new Tuple<int, bool>(this.currentTerm, false);
                }

                // reject leaders with short logs
                if (lastLogTerm == ourLastLogTerm && lastLogIndex < ourLastLogIndex)
                {
                    Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON RequestVote {this.Address} requested by {candidateUri}: Candidate has a smaller log, voted NO");
                    return new Tuple<int, bool>(this.currentTerm, false);
                }

                this.votedFor = candidateUri;
                this.lastHeartBeatTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                this.electionTimer.Stop();
                this.electionTimer.Start();
                // TODO: persist Raft state
                Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON RequestVote {this.Address} requested by {candidateUri}: Candidate is good voted Yes");
                return new Tuple<int, bool>(this.currentTerm, true);
            }
        }

        public Tuple<int, int, int, bool> AppendEntries(int term, Uri leaderUri, int prevLogIndex, int prevLogTerm, List<RaftLog> entries, int leaderCommit)
        {
            if (!HasServerStarted)
            {
                throw new Exception("Server hasn't started yet.");
            }
            lock (this)
            {
               // Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} Received HeartBeat on {this.Address} sent by {leaderUri}");

                // step down before handling RPC if need be
                if (term > this.currentTerm)
                {
                   // Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON AppendEntries {this.Address} sent by {leaderUri}: Stepping down to follower");
                    this.currentTerm = term;
                    this.state = State.FOLLOWER;
                    this.votedFor = null;
                    RoundTimer.Stop();

                    foreach (Uri peer in this.peerURIs)
                    {
                        nextIndex[peer] = this.log.Count;
                        matchIndex[peer] = -1;
                    }
                }

                //outdated term
                if (term < this.currentTerm)
                {
                   // Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON AppendEntries {this.Address} sent by {leaderUri}: Leader has an outdated term");
                    return new Tuple<int, int, int, bool>(this.currentTerm, -1, -1, false);
                }

              //  Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON AppendEntries {this.Address} sent by {leaderUri}: Reseting election timer");

                //  reset election timer
                this.lastHeartBeatTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
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


                //remove logs with wrong index or term and append entries
                for (int i = 0; i < entries.Count; i++)
                {
                    int index = prevLogIndex + i + 1;

                    //Found a matching entry
                    if (index >= this.log.Count || log[index].Term != entries[i].Term)
                    {
                      //  Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON AppendEntries {this.Address} sent by {leaderUri}: Removing Entry log[{index}]");
                        this.log = this.log.Take(index).ToList(); //remove the entries

                        //Append the entries
                        while (i < entries.Count)
                        {
                         //   Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON AppendEntries {this.Address} sent by {leaderUri}: Appending Entry log[{i}]");
                            this.log.Add(entries[i]);
                            i++;
                        }

                        break;

                    }
                }

                // TODO: persist Raft state

                if (leaderCommit > this.commitIndex)
                    this.commitIndex = Math.Min(this.log.Count - 1, leaderCommit);


                if (commitIndex > lastApplied)
                {
                    for (int i = this.lastApplied + 1; i <= this.commitIndex; i++)
                    {
                        this.lastApplied = i;
                        string runIn = this.log[i].AsLeader ? "Leader" : "Follower";
                      //  Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON AppendEntries {this.Address} sent by {leaderUri}: Applying to the state machine log[{i}] as {runIn}");
                        this.log[i].Command.Execute(this, this.log[i].AsLeader);
                      //  Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON AppendEntries {this.Address} sent by {leaderUri}: Successfully Applied log[{i}] to the state machine");
                    }
                }

                return new Tuple<int, int, int, bool>(this.currentTerm, -1, -1, true);
            }

        }

        //
        // ============================================================================
        // Raft event handlers
        // ============================================================================
        //

        public async void OnElectionTimer()
        {
           // Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnElectionTimer {this.Address} Time to start an election");
            if (this.state == State.LEADER)
            {
             //   Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnElectionTimer {this.Address} I am leader, not doing anything");
                return;
            }

          //  Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnElectionTimer {this.Address} Current state: {this.state}");

            int electionTerm = 0;
            int logCount;
            lock (this)
            {
                this.currentTerm += 1;
                electionTerm = currentTerm;
                this.votedFor = this.Address;
                this.state = State.CANDIDATE;
                logCount = this.log.Count;
            }

            int votes = 1;
            int nVotes = 1;


            for (int i = 0; i < this.peerURIs.Count; i++)
            {
                Uri peerUri = this.peerURIs[i];
                if (peerUri == this.Address) continue;
                if (peerUri == null) continue;

                // NOTE: me here is this server's identifier
                // NOTE: if the RPC fails, it counts as granted = false
                // NOTE: these RPCs should be made in parallel

                await Task.Run(() =>
                 {
                     int term;
                     bool granted;
                     Tuple<int, bool> res;
                     try
                     {
                    //     Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnElectionTimer {this.Address} Requesting vote to {peerUri}");
                         res = peers[peerUri].RequestVote(electionTerm, this.Address, this.log.Count - 1,
                             (logCount > 0 ? this.log[this.log.Count - 1].Term : this.currentTerm));

                         term = res.Item1;
                         granted = res.Item2;

                     }
                     catch (Exception e)
                     {
                   //      Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnElectionTimer {this.Address} Request vote to {peerUri} failed with message {e.Message}");
                         term = this.currentTerm;
                         granted = false;
                     }


                     if (this.state != State.CANDIDATE)
                     {
                     //    Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnElectionTimer {this.Address} not a candidate anymore");
                         //TODO: IF WE ARE NOW CANDIDATE WE MUST STOP EVERY OTHER REQUEST IF THIS IS RUNNING IN PARALLEL
                         return;
                     }

                     nVotes++;

                     if (term > this.currentTerm)
                     {
                         Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnElectionTimer {this.Address} Steping down to Follower");
                         this.currentTerm = term;
                         this.state = State.FOLLOWER;
                         this.votedFor = null;
                         RoundTimer.Stop();

                         foreach (Uri p in this.peerURIs)
                         {
                             nextIndex[p] = this.log.Count;
                             matchIndex[p] = -1;
                         }

                         this.electionTimer.Stop();
                         this.electionTimer.Start();
                     }

                     if (granted)
                     {
                         this.electionTimer.Stop();
                         this.electionTimer.Start(); //reset the timer, we don't want to lose this vote
                         votes += 1;
                     }

                     if (this.currentTerm != electionTerm)
                     {
                         return;
                     }

                     //There isn't enought votes to decide yet
                     if (nVotes <= this.peers.Count / 2)
                         return;

                     if (votes <= this.peers.Count / 2)
                     {
                         this.state = State.FOLLOWER;
                         RoundTimer.Stop();
                         electionTimer.Stop();
                         electionTimer.Interval = new Random().Next(ElectionTime / 2, ElectionTime * 2); ;
                         electionTimer.Start();
                         return;
                     }

                     //We have enough votes to become a leader
                     this.state = State.LEADER;
                     electionTimer.Stop();
                     leaderTimer.Start();
                     if (this.HasGameStarted)
                     {
                         //todo uncomment
                         RoundTimer.Start();
                     }


                     //This removes the knowledge we aquired from the peers
                     foreach (Uri p in this.peerURIs)
                     {
                         nextIndex[p] = this.log.Count;
                         matchIndex[p] = -1;
                     }

                     for (int j = 0; j < this.peerURIs.Count; j++)
                     {
                         Uri peer = this.peerURIs[j];
                         if (peer != this.Address && peer != null)
                             OnHeartbeatTimerOrSendTrigger(peer);
                     }

                 });
            }

            //Randomize election timer and repeat
            electionTimer.Stop();
            int time = new Random().Next(ElectionTime / 2, ElectionTime * 2);
            electionTimer.Interval = time;
            electionTimer.Start();

        }

        public async void OnHeartbeatTimerOrSendTrigger(Uri peerUri)
        {


            // NOTE: it may be useful to have separate timers for each peer, so
            // that you can retry AppendEntries to one peer without sending to all
            // peers.


            if (state != State.LEADER)
            {
                return;
            }

            int rfNextIndex;
            List<RaftLog> entries = new List<RaftLog>();
            int prevLogIndex;
            int prevLogTerm;
            int sendTerm;
            lock (this)
            {
                rfNextIndex = this.nextIndex[peerUri];

                if (this.nextIndex[peerUri] > this.log.Count)
                {
                    rfNextIndex = this.log.Count;
                }

                for (int i = rfNextIndex; i < this.log.Count; i++)
                {
                    RaftLog entry = new RaftLog()
                    {
                        Command = this.log[i].Command,
                        Term = this.log[i].Term,
                        AsLeader = false
                    };
                    entries.Add(entry);
                }


                prevLogIndex = rfNextIndex - 1;
                prevLogTerm = -1;

                if (prevLogIndex >= 0)
                {
                    prevLogTerm = log[prevLogIndex].Term;
                }
                else
                {
                    prevLogTerm = this.currentTerm;
                }

                sendTerm = this.currentTerm;
                // NOTE: if length(entries) == 0, you may want to check that we
                // haven't sent this peer an AppendEntries recently. If we
                // have, just return.

                // NOTE: if the RPC fails, stop processing for this peer, but
                // trigger sending AppendEntries again immediately.
            }

            Tuple<int, int, int, bool> res;
            try
            {
               // Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnHeartbeatTimerOrSendTrigger {this.Address} Calling appendEntries in {peerUri}");
                res = peers[peerUri].AppendEntries(sendTerm, this.Address, prevLogIndex, prevLogTerm, entries, this.commitIndex);
               // Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnHeartbeatTimerOrSendTrigger {this.Address} Received response from append entries in {peerUri}");
            }
            catch (Exception ex)
            {
              //  Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnHeartbeatTimerOrSendTrigger {this.Address} AppendEntries call to {peerUri} failed with {ex.Message}");
                return;
            }

            int term = res.Item1;
            int conflitIndex = res.Item2;
            int conflitTerm = res.Item3;
            bool success = res.Item4;

            lock (this)
            {
                if (term > this.currentTerm)
                {

                    this.currentTerm = term;
                    this.state = State.FOLLOWER;
                    this.votedFor = null;
                    RoundTimer.Stop();

                    foreach (Uri p in this.peerURIs)
                    {
                        nextIndex[p] = this.log.Count;
                        matchIndex[p] = -1;

                    }
                }

                if (this.currentTerm != sendTerm)
                {
                    return;
                }


                if (!success)
                {

                   // Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnHeartbeatTimerOrSendTrigger {this.Address} AppendEntries call to {peerUri} was unsuccessfull");
                    //Try to resolve inconsistencies in order to be successfull in the next call
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

                    OnHeartbeatTimerOrSendTrigger(peerUri); //Retry immediatly
                    return;
                }

                //We were successfull
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

                    //We replicated to a majority of servers
                    lock (this)
                    {
                        if (replicas > this.peers.Count / 2)
                        {
                            commitIndex = n;
                           // Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnHeartbeatTimerOrSendTrigger {this.Address} Last applied {lastApplied} Commit Index {commitIndex}");
                            if (commitIndex > lastApplied)
                            {
                                for (int i = this.lastApplied + 1; i <= Math.Min(this.commitIndex, this.log.Count - 1); i++)
                                {
                                    this.lastApplied = i;
                                    string runIn = this.log[i].AsLeader ? "Leader" : "Follower";
                                   // Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnHeartbeatTimerOrSendTrigger {this.Address} Applying commit log[{i}] running as {runIn}");
                                    RaftLog log = this.log[i];
                                    Task.Run(() => { log.Command.Execute(this, log.AsLeader); });
                                }
                            }
                        }
                    }
                }
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

            //Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnCommand {this.Address}");

            this.log.Add(new RaftLog() { Command = command, Term = this.currentTerm });
            nextIndex[this.Address] = this.log.Count;
            matchIndex[this.Address] = this.log.Count - 1;

            // TODO: persist Raft state
            // trigger sending of AppendEntries
            for (int j = 0; j < this.peerURIs.Count; j++)
            {
                Uri peer = this.peerURIs[j];
                if (peer != this.Address && peer != null)
                    OnHeartbeatTimerOrSendTrigger(peer);
            }

            accepted = true;
            willCommitAt = this.log.Count - 1;

        }


        //LEADER

        //Remote
        public Uri GetLeader()
        {

            if (this.state == State.LEADER)
                return this.Address;  //Send my address
                                      //todo - send the leader

            return null;
        }

        //Transitates to the next round
        private void NextRound()
        {
            bool accepted;
            int commitedAt;
            RaftCommand command;

            if (!this.HasGameStarted)
            {
                return;
            }
           // Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON NextRound {this.Address} in state {this.state}");
            if (this.stateMachine.HasGameEnded())
            {
                Console.WriteLine("  GAME HAS ENDED");
                Console.WriteLine(this.stateMachine.Stage.GetPlayers().Count > 0);


                command = new EndGameCommand();

                this.OnCommand(command, out accepted, out commitedAt);
                // TODO: handle clients and server lists
                // TODO: Generate Log Entry and multicast AppendEntries
                return;
            }

            command = new NewRoundCommand()
            {
                Name = "New Round"
            };


            this.OnCommand(command, out accepted, out commitedAt);
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

            Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON Join {this.Address}");


            if (this.pendingClients.Exists(c => c.Username == username))
            {
                // TODO: Lauch exception to the client (username already exists)
                return JoinResult.REJECTED_USERNAME;
            }

            Console.WriteLine($"  CLIENT '{username} - {address.ToString()}' HAS BEEN QUEUED");

            JoinCommand command = new JoinCommand(address)
            {
                Name = "Join"
            };

            bool accepted;
            int commitedAt;
            this.OnCommand(command, out accepted, out commitedAt);

            //Task.Run(() => { OnHeartbeatTimerOrSendTrigger(); });

            return JoinResult.QUEUED;

        }

        public void SetPlay(Uri address, Play play, int round)
        {


            if (this.state != State.LEADER)
            {
                throw new Exception("The server is currently not a leader");
            }

            if (this.stateMachine == null || !this.HasGameStarted)
            {
                throw new Exception("Game hasn't started yet");
            }

            this.plays[address] = play;


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
