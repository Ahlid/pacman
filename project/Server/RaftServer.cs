using Shared;
using System;
using System.Collections;
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
    /// <summary>
    /// Implementations of the raft algorithm to produce or server
    /// </summary>
    public class RaftServer : MarshalByRefObject, IRaft, IServer
    {

        /// <summary>
        /// If this server has started or not
        /// </summary>
        private bool HasServerStarted = false;

        /// <summary>
        /// The election timeout of raft
        /// </summary>
        public static int ElectionTime = 1000;

        /// <summary>
        /// the lider timeout of raft to send hearthbeat
        /// </summary>
        public static int LeaderTime = 20;

        /// <summary>
        /// The timmer for election
        /// </summary>
        public System.Timers.Timer ElectionTimer;

        /// <summary>
        /// The timmer for leader
        /// </summary>
        private System.Timers.Timer LeaderTimer;

        /// <summary>
        /// The timmer for group mannager
        /// </summary>
        private System.Timers.Timer ManagerTimer;

        /// <summary>
        /// This is the Raft peer that this server has voted for in *this* term(if any)
        /// </summary>
        public Uri VotedFor;


        #region FROM INTERFACE
        public int CurrentTerm { get; set; }
        public List<RaftLog> Log { get; set; }
        public State State { get; set; }
        public int CommitIndex { get; set; }
        public int LastApplied { get; set; }
        public Dictionary<Uri, int> NextIndex { get; set; }
        public Dictionary<Uri, int> MatchIndex { get; set; }
        #endregion

        /// <summary>
        /// This server's address
        /// </summary>
        public Uri Address { get; set; }

        /// <summary>
        /// This server's tcp channel
        /// </summary>
        public TcpChannel Channel { get; set; }

        /// <summary>
        /// List o URI of the others servers
        /// </summary>
        public List<Uri> PeerURIs;

        /// <summary>
        /// Dictionary from the URI for the remote object
        /// </summary>
        public Dictionary<Uri, IRaft> Peers;

        #region Base server

        /// <summary>
        /// The number of players per game
        /// </summary>
        public int NumPlayers { get; set; }

        /// <summary>
        /// The number of the interval per round im milliseconds
        /// </summary>
        public int RoundIntervalMsec { get; set; }

        /// <summary>
        /// The clients in waiting list
        /// </summary>
        public List<IClient> PendingClients;

        /// <summary>
        /// The clients playing the current game
        /// </summary>
        public List<IClient> SessionClients;
        public List<Uri> SessionClientsAddress;

        /// <summary>
        /// The game state machine for this server
        /// </summary>
        public GameStateMachine StateMachine { get; set; }

        /// <summary>
        /// The list of players
        /// </summary>
        public List<IPlayer> PlayerList { get; set; }

        /// <summary>
        /// Used to request a start(requesting a start takes time to be commited, no more StartGame entries should be created)
        /// </summary>
        public bool GameStartRequest { get; set; }

        /// <summary>
        /// The control if the game has started
        /// </summary>
        public bool HasGameStarted { get; set; }
        #endregion

        #region  Leader only

        /// <summary>
        /// The timer for the game round
        /// </summary>
        public System.Timers.Timer RoundTimer { get; set; }

        /// <summary>
        /// The plays of the clients
        /// </summary>
        public Dictionary<Uri, Play> plays;

        /// <summary>
        /// List of all existed server to manager 
        /// </summary>
        private List<Uri> peerURIManager;
        #endregion

        /// <summary>
        /// Constuctor of the raft server
        /// </summary>
        /// <param name="address"> The Address of this server</param>
        /// <param name="NumPlayers">Number of players per round</param>
        /// <param name="RoundIntervalMsec">The round interval in milisseconds</param>
        public RaftServer(Uri address, int NumPlayers, int RoundIntervalMsec)
        {
            //BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();

            this.Address = address;
            this.NumPlayers = NumPlayers;
            this.RoundIntervalMsec = RoundIntervalMsec;

            IDictionary props = new Hashtable();
            props["port"] = address.Port;
            props["timeout"] = 500; // in milliseconds


            this.Channel = new TcpChannel(address.Port);
            //this.Channel = new TcpChannel(address.Port);
            ChannelServices.RegisterChannel(this.Channel, false);
            RemotingServices.Marshal(this, "Server", typeof(RaftServer));
        }

        /// <summary>
        /// Used do mke ping pong with servers
        /// </summary>
        /// <returns>pong</returns>
        public String Test()
        {
            return "BOT";
        }

        /// <summary>
        /// Starts this server
        /// </summary>
        /// <param name="peerURIs">The list with all known servers</param>
        public void Start(List<Uri> peerURIs)
        {

            this.PeerURIs = peerURIs;
            this.SessionClientsAddress = new List<Uri>();
            this.SessionClients = new List<IClient>();
            this.peerURIManager = new List<Uri>();
            this.Peers = new Dictionary<Uri, IRaft>();
            this.CurrentTerm = 0;
            this.VotedFor = null;
            this.Log = new List<RaftLog>();
            this.State = State.FOLLOWER;
            this.CommitIndex = -1;
            this.LastApplied = -1;
            this.MatchIndex = new Dictionary<Uri, int>();
            this.NextIndex = new Dictionary<Uri, int>();
            this.PendingClients = new List<IClient>();
            this.plays = new Dictionary<Uri, Play>();


            foreach (Uri peerUri in this.PeerURIs)
            {
                bool isStable = false;
                while (!isStable)
                {
                    peerURIManager.Add(peerUri);
                    MatchIndex[peerUri] = -1;
                    NextIndex[peerUri] = 0;
                    //Get the remoting object
                    try
                    {
                        IRaft peer = (IRaft)Activator.GetObject(
                        typeof(IRaft),
                        peerUri.ToString() + "Server");
                        Peers[peerUri] = peer;
                        Console.WriteLine(Peers[peerUri].Test());
                        isStable = true;
                    }
                    catch (Exception)
                    {
                        isStable = false;
                    }


                }

            }


            ElectionTimer = new System.Timers.Timer(ElectionTime);
            ElectionTimer.Elapsed += OnElectionTimer;
            ElectionTimer.AutoReset = false;

            LeaderTimer = new System.Timers.Timer(LeaderTime);
            LeaderTimer.Elapsed += OnHeartbeatTimerOrSendTrigger;
            LeaderTimer.AutoReset = true;


            ManagerTimer = new System.Timers.Timer(ElectionTime);
            ManagerTimer.Elapsed += OnManagerCheck;
            ManagerTimer.AutoReset = false;

            RoundTimer = new System.Timers.Timer(this.RoundIntervalMsec);
            RoundTimer.Elapsed += (sender, e) => { this.NextRound(); };
            RoundTimer.AutoReset = false;

            LeaderTimer.Start();
            ElectionTimer.Start();
            ManagerTimer.Start();

            Console.WriteLine("Started Server " + this.Address);
            Console.WriteLine(this.Peers.Count);
            HasServerStarted = true;

        }

        /// <summary>
        /// Event to trigger on Manager timmer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnManagerCheck(object sender, ElapsedEventArgs e)
        {

            foreach (Uri uri in this.peerURIManager)
            {

                await Task.Run(() =>
                {
                    try
                    {
                        this.Peers[uri].Test();

                        if (!this.PeerURIs.Contains(uri))
                        {
                            lock (this)
                            {
                                this.PeerURIs.Add(uri);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        if (this.PeerURIs.Contains(uri))
                        {
                            lock (this)
                            {
                                this.PeerURIs.Remove(uri);
                            }
                        }
                    }
                });

            }
            /*
            if (this.SessionClients.Count > 0) ;
            foreach (var peerUrI in this.SessionClients)
            {
                try
                {
                    Console.WriteLine(peerUrI.ping());
                }
                catch (Exception exx)
                {

                }
            }*/

            this.ManagerTimer.Start();
        }

        /// <summary>
        /// Event to trigger on Leader timmer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnHeartbeatTimerOrSendTrigger(object sender, ElapsedEventArgs e)
        {

            if (this.State != State.LEADER)
                return;



            for (int i = 0; i < this.PeerURIs.Count; i++)
            {
                Uri peer = this.PeerURIs[i];
                if (peer != this.Address && peer != null)
                    await Task.Run(() =>
                     {
                         OnHeartbeatTimerOrSendTrigger(peer);
                     });
            }


        }

        /// <summary>
        /// Event to trigger on Election timmer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnElectionTimer(object sender, ElapsedEventArgs e)
        {

            if (this.State == State.LEADER)
            {
                return;
            }


            this.OnElectionTimer();


        }

        /// <summary>
        /// Raft request vote
        /// </summary>
        /// <param name="term">The term of the requester</param>
        /// <param name="candidateUri">Candidate's address</param>
        /// <param name="lastLogIndex">Candidate's last index in is log</param>
        /// <param name="lastLogTerm">Candidate's last term in hs log</param>
        /// <returns>A tuple with (1) this server's term (2) if this server granted his vote</returns>
        public Tuple<int, bool> RequestVote(int term, Uri candidateUri, int lastLogIndex, int lastLogTerm)
        {
            lock (this)
            {
                /* if (!HasServerStarted)
                 {
                     return new Tuple<int, bool>(this.CurrentTerm, false);
                 }
                 */
                Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON RequestVote of {this.Address} requested by {candidateUri}");
                // step down before handling RPC if need be

                if (term > this.CurrentTerm)
                {
                    Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON RequestVote {this.Address} requested by {candidateUri}: Stepping down to follower");
                    this.CurrentTerm = term;
                    this.State = State.FOLLOWER;
                    this.VotedFor = null;


                    foreach (Uri peer in this.PeerURIs)
                    {
                        NextIndex[peer] = this.Log.Count;
                        MatchIndex[peer] = -1;
                    }
                }
                else if (term < this.CurrentTerm)
                {
                    return new Tuple<int, bool>(this.CurrentTerm, false);
                }


                // don't double vote
                if (this.VotedFor != null && this.VotedFor != candidateUri)
                {
                    Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON RequestVote {this.Address} requested by {candidateUri}: Already voted for another candidate, voted NO");
                    return new Tuple<int, bool>(this.CurrentTerm, false);
                }



                // check how up-to-date our Log is
                int ourLastLogIndex = this.Log.Count - 1;
                int ourLastLogTerm = -1;
                if (this.Log.Count > 0)
                {
                    ourLastLogTerm = this.Log[ourLastLogIndex].Term;
                }

                // reject leaders with old logs
                if (lastLogTerm < ourLastLogTerm)
                {
                    Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON RequestVote {this.Address} requested by {candidateUri}: Candidate has a lower term, voted NO");
                    return new Tuple<int, bool>(this.CurrentTerm, false);
                }



                // reject leaders with short logs
                if (lastLogTerm == ourLastLogTerm && lastLogIndex < ourLastLogIndex)
                {
                    Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON RequestVote {this.Address} requested by {candidateUri}: Candidate has a smaller Log, voted NO");
                    return new Tuple<int, bool>(this.CurrentTerm, false);
                }

                //Stop the timers
                //Maybe stop the leader timer?
                RoundTimer.Stop();
                ElectionTimer.Stop();
                ElectionTimer.Interval = ElectionTime;
                ElectionTimer.Start();

                this.VotedFor = candidateUri;
                // TODO: persist Raft state
                Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON RequestVote {this.Address} requested by {candidateUri}: Candidate is good voted Yes");
                return new Tuple<int, bool>(this.CurrentTerm, true);
            }
        }

        /// <summary>
        /// Raft appendentries RCP
        /// </summary>
        /// <param name="term">The leader's term</param>
        /// <param name="leaderUri">Leader's Address</param>
        /// <param name="prevLogIndex">The prev log's index that leader has registered for this server </param>
        /// <param name="prevLogTerm">The prev log's term that leader has registered for this server </param>
        /// <param name="entries">The entries to add to this server's log</param>
        /// <param name="leaderCommit">The last commited command for the leader</param>
        /// <returns>A tuple with (1) this server's term, (2) conflictIndex (3) conflictTerm (4) if succes </returns>
        public Tuple<int, int, int, bool> AppendEntries(int term, Uri leaderUri, int prevLogIndex, int prevLogTerm, List<RaftLog> entries, int leaderCommit)
        {
            if (!HasServerStarted)
            {
                throw new Exception("Server hasn't started yet.");
            }

            // Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} Received HeartBeat on {this.Address} sent by {leaderUri}");

            // step down before handling RPC if need be
            if (term > this.CurrentTerm)
            {

                this.CurrentTerm = term;
                this.State = State.FOLLOWER;
                this.VotedFor = null;
                RoundTimer.Stop();

                foreach (Uri peer in this.PeerURIs)
                {
                    NextIndex[peer] = this.Log.Count;
                    MatchIndex[peer] = -1;
                }
                // Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON AppendEntries {this.Address} sent by {leaderUri}: Stepping down to follower");

            }

            //outdated term
            if (term < this.CurrentTerm)
            {
                // Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON AppendEntries {this.Address} sent by {leaderUri}: Leader has an outdated term");
                return new Tuple<int, int, int, bool>(this.CurrentTerm, -1, -1, false);
            }

            //  Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON AppendEntries {this.Address} sent by {leaderUri}: Reseting election timer");

            //  reset election timer
            this.ElectionTimer.Stop();
            this.ElectionTimer.Start();

            if (prevLogIndex >= this.Log.Count)
            {
                return new Tuple<int, int, int, bool>(this.CurrentTerm, this.Log.Count, -1, false);
            }

            int ourPrevLogTerm;
            if (prevLogIndex > 0)
            {
                ourPrevLogTerm = Log[prevLogIndex].Term;
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
                    if (Log[i].Term != ourPrevLogTerm)
                    {
                        break;
                    }
                    firstOfTerm = i;
                }

                return new Tuple<int, int, int, bool>(this.CurrentTerm, firstOfTerm, ourPrevLogTerm, false);

            }


            //remove logs with wrong index or term and append entries
            for (int i = 0; i < entries.Count; i++)
            {
                int index = prevLogIndex + i + 1;

                //Found a matching entry
                if (index >= this.Log.Count || Log[index].Term != entries[i].Term)
                {
                    //  Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON AppendEntries {this.Address} sent by {leaderUri}: Removing Entry Log[{index}]");
                    this.Log = this.Log.Take(index).ToList(); //remove the entries

                    //Append the entries
                    while (i < entries.Count)
                    {
                        //   Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON AppendEntries {this.Address} sent by {leaderUri}: Appending Entry Log[{i}]");
                        this.Log.Add(entries[i]);
                        i++;
                    }

                    break;

                }
            }

            // TODO: persist Raft state

            if (leaderCommit > this.CommitIndex)
                this.CommitIndex = Math.Min(this.Log.Count - 1, leaderCommit);


            if (CommitIndex > LastApplied)
            {
                for (int i = this.LastApplied + 1; i <= this.CommitIndex; i++)
                {
                    this.LastApplied = i;
                    string runIn = this.Log[i].AsLeader ? "Leader" : "Follower";
                    //  Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON AppendEntries {this.Address} sent by {leaderUri}: Applying to the State machine Log[{i}] as {runIn}");
                    this.Log[i].Command.Execute(this, this.Log[i].AsLeader);
                    //  Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON AppendEntries {this.Address} sent by {leaderUri}: Successfully Applied Log[{i}] to the State machine");
                }
            }

            return new Tuple<int, int, int, bool>(this.CurrentTerm, -1, -1, true);


        }

        //
        // ============================================================================
        // Raft event handlers
        // ============================================================================
        //
        /// <summary>
        /// Starts a new election
        /// </summary>
        public void OnElectionTimer()
        {
            lock (this)
            {
                // Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnElectionTimer {this.Address} Time to start an election");
                if (this.State == State.LEADER)
                {
                    //   Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnElectionTimer {this.Address} I am leader, not doing anything");
                    return;
                }

                //  Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnElectionTimer {this.Address} Current State: {this.State}");

                int electionTerm = 0;
                int logCount;

                this.CurrentTerm += 1;
                electionTerm = CurrentTerm;
                this.VotedFor = this.Address;
                this.State = State.CANDIDATE;
                logCount = this.Log.Count;


                int votes = 1;
                int nVotes = 1;


                for (int i = 0; i < this.PeerURIs.Count; i++)
                {
                    Uri peerUri = this.PeerURIs[i];
                    if (peerUri == this.Address) continue;
                    if (peerUri == null) continue;

                    // NOTE: me here is this server's identifier
                    // NOTE: if the RPC fails, it counts as granted = false
                    // NOTE: these RPCs should be made in parallel

                    Task.Run(() =>
                    {
                        int term;
                        bool granted;
                        Tuple<int, bool> res;
                        try
                        {
                            //     Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnElectionTimer {this.Address} Requesting vote to {peerUri}");
                            res = Peers[peerUri].RequestVote(electionTerm, this.Address, this.Log.Count - 1,
                                    (logCount > 0 ? this.Log[this.Log.Count - 1].Term : this.CurrentTerm));

                            term = res.Item1;
                            granted = res.Item2;

                        }
                        catch (Exception e)
                        {
                            //      Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnElectionTimer {this.Address} Request vote to {peerUri} failed with message {e.Message}");
                            term = this.CurrentTerm;
                            granted = false;
                        }


                        if (this.State != State.CANDIDATE)
                        {
                            //    Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnElectionTimer {this.Address} not a candidate anymore");
                            //TODO: IF WE ARE NOW CANDIDATE WE MUST STOP EVERY OTHER REQUEST IF THIS IS RUNNING IN PARALLEL
                            return;
                        }

                        nVotes++;

                        Console.WriteLine(term);
                        Console.WriteLine(CurrentTerm);
                        Console.WriteLine(granted);

                        if (term > this.CurrentTerm)
                        {
                            Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnElectionTimer {this.Address} Steping down to Follower");
                            this.CurrentTerm = term;
                            this.State = State.FOLLOWER;
                            this.VotedFor = null;
                            RoundTimer.Stop();

                            foreach (Uri p in this.PeerURIs)
                            {
                                NextIndex[p] = this.Log.Count;
                                MatchIndex[p] = -1;
                            }

                            this.ElectionTimer.Stop();
                            this.ElectionTimer.Start();
                        }

                        if (granted)
                        {
                            this.ElectionTimer.Stop();
                            this.ElectionTimer.Start(); //reset the timer, we don't want to lose this vote
                            votes += 1;
                        }

                        if (this.CurrentTerm != electionTerm)
                        {
                            return;
                        }

                        //There isn't enought votes to decide yet
                        if (nVotes <= this.PeerURIs.Count / 2)
                            return;

                        if (votes <= this.PeerURIs.Count / 2)
                        {
                            this.State = State.FOLLOWER;
                            RoundTimer.Stop();
                            ElectionTimer.Stop();
                            ElectionTimer.Start();
                            return;
                        }

                        //We have enough votes to become a leader
                        this.State = State.LEADER;

                        Console.WriteLine("LEADER");
                        //This removes the knowledge we aquired from the Peers
                        foreach (Uri p in this.PeerURIs)
                        {
                            NextIndex[p] = this.Log.Count;
                            MatchIndex[p] = -1;
                        }

                        for (int j = 0; j < this.PeerURIs.Count; j++)
                        {
                            Uri peer = this.PeerURIs[j];
                            if (peer != this.Address && peer != null)
                                OnHeartbeatTimerOrSendTrigger(peer);
                        }
                        ElectionTimer.Stop();
                        LeaderTimer.Start();
                        if (this.HasGameStarted)
                        {
                            //todo uncomment
                            RoundTimer.Start();
                        }




                    });
                }

                //Randomize election timer and repeat
                ElectionTimer.Stop();
                int time = new Random().Next(ElectionTime / 2, ElectionTime);
                ElectionTimer.Interval = time;
                ElectionTimer.Start();
            }
        }

        /// <summary>
        /// Sends appendEntries our HearthBeats
        /// </summary>
        /// <param name="peerUri">The address to send it</param>
        public void OnHeartbeatTimerOrSendTrigger(Uri peerUri)
        {

            int rfNextIndex;
            List<RaftLog> entries = new List<RaftLog>();
            int prevLogIndex;
            int prevLogTerm;
            int sendTerm;

            lock (this)
            {

                if (peerUri == null) return;

                // NOTE: it may be useful to have separate timers for each peer, so
                // that you can retry AppendEntries to one peer without sending to all
                // Peers.


                if (State != State.LEADER)
                {
                    return;
                }



                rfNextIndex = this.NextIndex[peerUri];

                if (this.NextIndex[peerUri] > this.Log.Count)
                {
                    rfNextIndex = this.Log.Count;
                }

                for (int i = rfNextIndex; i < this.Log.Count; i++)
                {
                    RaftLog entry = new RaftLog()
                    {
                        Command = this.Log[i].Command,
                        Term = this.Log[i].Term,
                        AsLeader = false
                    };
                    entries.Add(entry);
                }


                prevLogIndex = rfNextIndex - 1;
                prevLogTerm = -1;

                if (prevLogIndex >= 0)
                {
                    prevLogTerm = Log[prevLogIndex].Term;
                }
                else
                {
                    prevLogTerm = this.CurrentTerm;
                }

                sendTerm = this.CurrentTerm;
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
                res = Peers[peerUri].AppendEntries(sendTerm, this.Address, prevLogIndex, prevLogTerm, entries, this.CommitIndex);
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


            if (term > this.CurrentTerm)
            {

                this.CurrentTerm = term;
                this.State = State.FOLLOWER;
                this.VotedFor = null;
                RoundTimer.Stop();

                foreach (Uri p in this.PeerURIs)
                {
                    NextIndex[p] = this.Log.Count;
                    MatchIndex[p] = -1;

                }
            }

            if (this.CurrentTerm != sendTerm)
            {
                return;
            }


            if (!success)
            {

                // Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnHeartbeatTimerOrSendTrigger {this.Address} AppendEntries call to {peerUri} was unsuccessfull");
                //Try to resolve inconsistencies in order to be successfull in the next call
                this.NextIndex[peerUri] = conflitIndex;

                if (conflitTerm != -1)
                {
                    int ourLastInConflictTerm = -1;

                    for (int i = prevLogIndex; i >= 0; i--)
                    {
                        if (this.Log[i].Term == conflitTerm)
                        {
                            ourLastInConflictTerm = i;
                            break;
                        }
                        else if (this.Log[i].Term < conflitTerm)
                        {
                            break;
                        }
                    }

                    if (ourLastInConflictTerm != -1)
                    {
                        NextIndex[peerUri] = ourLastInConflictTerm + 1;
                    }

                }

                OnHeartbeatTimerOrSendTrigger(peerUri); //Retry immediatly
                return;
            }

            //We were successfull
            MatchIndex[peerUri] = prevLogIndex + entries.Count;
            NextIndex[peerUri] = MatchIndex[peerUri] + 1;

            for (int n = this.Log.Count - 1; n > this.CommitIndex; n--)
            {
                if (Log[n].Term != this.CurrentTerm)
                {
                    break;
                }

                int replicas = 0;

                foreach (Uri peerUri2 in this.PeerURIs)
                {
                    if (this.MatchIndex[peerUri2] >= n)
                    {
                        replicas += 1;
                    }
                }

                //We replicated to a majority of servers
                lock (this)
                {
                    if (replicas > this.PeerURIs.Count / 2)
                    {
                        CommitIndex = n;
                        // Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnHeartbeatTimerOrSendTrigger {this.Address} Last applied {LastApplied} Commit Index {CommitIndex}");
                        if (CommitIndex > LastApplied)
                        {
                            for (int i = this.LastApplied + 1; i <= Math.Min(this.CommitIndex, this.Log.Count - 1); i++)
                            {
                                this.LastApplied = i;
                                string runIn = this.Log[i].AsLeader ? "Leader" : "Follower";
                                // Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnHeartbeatTimerOrSendTrigger {this.Address} Applying commit Log[{i}] running as {runIn}");
                                RaftLog log = this.Log[i];
                                Task.Run(() => { log.Command.Execute(this, log.AsLeader); });
                            }
                        }
                    }
                }
            }


        }

        /// <summary>
        /// Add new command to raft server
        /// </summary>
        /// <param name="command">The command to add</param>
        /// <param name="accepted">Out param with re result</param>
        /// <param name="willCommitAt">Out param When will it be commitet</param>
        public void OnCommand(RaftCommand command)
        {
            lock (this)
            {
                if (this.State != State.LEADER)
                {
                    return;
                }

                //Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON OnCommand {this.Address}");

                this.Log.Add(new RaftLog() { Command = command, Term = this.CurrentTerm });
                NextIndex[this.Address] = this.Log.Count;
                MatchIndex[this.Address] = this.Log.Count - 1;

                // TODO: persist Raft state
                // trigger sending of AppendEntries
                for (int j = 0; j < this.PeerURIs.Count; j++)
                {
                    Uri peer = this.PeerURIs[j];
                    if (peer != this.Address && peer != null)
                        OnHeartbeatTimerOrSendTrigger(peer);
                }
            }

        }


        //LEADER

        //Remote
        /// <summary>
        /// Get's the leader's address
        /// </summary>
        /// <returns>Returns the addres of the leader</returns>
        public Uri GetLeader()
        {

            if (this.State == State.LEADER)
                return this.Address;  //Send my address
                                      //todo - send the leader

            return null;
        }
        /// <summary>
        /// Transitates to the next round
        /// </summary>
        private void NextRound()
        {
            bool accepted;
            int commitedAt;
            RaftCommand command;

            if (this.State != State.LEADER)
            {
                return;
            }

            if (!this.HasGameStarted)
            {
                return;
            }
            // Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON NextRound {this.Address} in State {this.State}");
            if (this.StateMachine.HasGameEnded())
            {
                this.HasGameStarted = false;
                Console.WriteLine("  GAME HAS ENDED");
                Console.WriteLine(this.StateMachine.Stage.GetPlayers().Count > 0);


                command = new EndGameCommand();

                this.OnCommand(command);
                // TODO: handle clients and server lists
                // TODO: Generate Log Entry and multicast AppendEntries
                return;
            }

            command = new NewRoundCommand()
            {
                Name = "New Round"
            };


            this.OnCommand(command);
        }

        /// <summary>
        /// To join a game
        /// </summary>
        /// <param name="username">Client's usrname</param>
        /// <param name="address">Client's address</param>
        /// <returns>The result QUEUED</returns>
        public JoinResult Join(string username, Uri address)
        {

            if (this.State != State.LEADER)
            {
                throw new Exception("The server is currently not a leader");
            }

            Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")} ON Join {this.Address}");


            if (this.PendingClients.Exists(c => c.Username == username))
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
            this.OnCommand(command);

            //Task.Run(() => { OnHeartbeatTimerOrSendTrigger(); });

            return JoinResult.QUEUED;

        }

        /// <summary>
        /// Set's a clent's play
        /// </summary>
        /// <param name="address">Client's address</param>
        /// <param name="play">Clien't choosen play</param>
        /// <param name="round">The round that client has choosen</param>
        public async void SetPlay(Uri address, Play play, int round)
        {


            if (this.State != State.LEADER)
            {
                return;
            }

            if (this.StateMachine == null || !this.HasGameStarted)
            {
                return;
            }

            this.OnCommand(new SetPlay() { Address = address, Play = play });

            for (int j = 0; j < this.PeerURIs.Count; j++)
            {
                Uri peer = this.PeerURIs[j];
                if (peer != this.Address && peer != null)
                    await Task.Run(() =>
                      {
                          OnHeartbeatTimerOrSendTrigger(peer);
                      });
            }


        }

        /// <summary>
        /// When a client's quit the game
        /// </summary>
        /// <param name="address">Clien'ts address</param>
        public void Quit(Uri address)
        {

            if (this.State != State.LEADER)
            {
                throw new Exception("The server is currently not a leader");
            }
            Console.WriteLine($"  CLIENT '{address.ToString()}' HAS DISCONNETED");
            this.PendingClients.RemoveAll(p => p.Address.ToString() == address.ToString());
            this.SessionClients.RemoveAll(p => p.Address.ToString() == address.ToString());


        }
    }
}
