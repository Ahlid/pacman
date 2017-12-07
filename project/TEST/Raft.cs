using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace TEST
{
    class Raft
    {
        public static int ElectionTime = 1000;
        public static int LeaderTime = ElectionTime / 3;

        private System.Timers.Timer electionTimer;
        private System.Timers.Timer leaderTimer;


        public string Id { get; private set; }
        //
        // ============================================================================
        // The following data needs to be persisted
        // ============================================================================
        //

        // This is the term this Raft server is currently in
        private int currentTerm;

        // This is the Raft peer that this server has voted for in *this* term (if any)
        private string votedFor;

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
        private Dictionary<String, int> nextIndex;

        // matchIndex is a measurement of how much of our log (as leader) we know to be
        // replicated at each other server. This is used to determine when some prefix
        // of entries in our log from the current term has been replicated to a
        // majority of servers, and is thus safe to apply.
        private Dictionary<String, int> matchIndex;

        private List<Raft> peers;

        public Raft(String id)
        {
            this.Id = id;
        }

        public void Start(List<Raft> peers)
        {
            this.peers = peers;
            this.currentTerm = 0;
            this.votedFor = null;
            this.log = new List<RaftLog>();
            this.state = State.FOLLOWER;
            this.commitIndex = -1;
            this.lastApplied = -1;
            this.matchIndex = new Dictionary<string, int>();
            this.nextIndex = new Dictionary<string, int>();


            foreach (Raft peer in this.peers)
            {
                matchIndex[peer.Id] = -1;
                nextIndex[peer.Id] = 0;
            }

            electionTimer = new System.Timers.Timer(ElectionTime);
            electionTimer.Elapsed += OnElectionTimer;
            electionTimer.AutoReset = false;


            leaderTimer = new System.Timers.Timer(LeaderTime);
            leaderTimer.Elapsed += OnHeartbeatTimerOrSendTrigger;
            leaderTimer.AutoReset = false;

            leaderTimer.Start();
            electionTimer.Start();

            Console.WriteLine("Started Server " + this.Id);

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

        public Tuple<int, bool> RequestVote(int term, string candidateID, int lastLogIndex, int lastLogTerm)
        {
            Console.WriteLine("HUHUHUHUHUHUH");
            // step down before handling RPC if need be
            if (term > this.currentTerm)
            {
                this.currentTerm = term;
                this.state = State.FOLLOWER;
                this.votedFor = null;

                foreach (Raft peer in this.peers)
                {
                    nextIndex[peer.Id] = this.log.Count;
                    matchIndex[peer.Id] = -1;

                }

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

        public Tuple<int, int, int, bool> AppendEntries(int term, string leaderID, int prevLogIndex, int prevLogTerm, List<RaftLog> entries, int leaderCommit)
        {

            Console.WriteLine("RECEIVED <3");
            // step down before handling RPC if need be
            if (term > this.currentTerm)
            {
                this.currentTerm = term;
                this.state = State.FOLLOWER;
                this.votedFor = null;

                foreach (Raft peer in this.peers)
                {
                    nextIndex[peer.Id] = this.log.Count;
                    matchIndex[peer.Id] = -1;

                }

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

        //
        // ============================================================================
        // Raft event handlers
        // ============================================================================
        //

        public void OnElectionTimer()
        {

            if (this.state == State.LEADER)
            {
                electionTimer.Start();
                return;
            }

            Console.WriteLine("Server " + this.Id + " Got Election Call and Emerged as Candidate");

            int electionTerm = 0;

            this.currentTerm += 1;
            electionTerm = currentTerm;
            this.votedFor = null;
            this.state = State.CANDIDATE;

            int votes = 0;
            int nVotes = 0;

            foreach (Raft peer in this.peers)
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
                        res = peer.RequestVote(electionTerm, this.Id, this.log.Count - 1,
                             this.log[this.log.Count - 1].Term);

                    }
                    else
                    {
                        res = peer.RequestVote(electionTerm, this.Id, this.log.Count - 1,
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
                    Console.WriteLine(this.Id);
                    Console.WriteLine(term);
                    Console.WriteLine(granted);
                    Console.WriteLine(peer.Id);
                    Console.WriteLine("####### RECEIVED VOTE ######");

                    if (term > this.currentTerm)
                    {

                        this.currentTerm = term;
                        this.state = State.FOLLOWER;
                        this.votedFor = null;

                        foreach (Raft p in this.peers)
                        {
                            nextIndex[p.Id] = this.log.Count;
                            matchIndex[p.Id] = -1;

                        }

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
                        Console.WriteLine("Server " + this.Id + " Got Elected Emerged as Leader");

                        this.state = State.LEADER;
                        foreach (Raft p in this.peers)
                        {
                            nextIndex[p.Id] = this.log.Count;
                            matchIndex[p.Id] = -1;

                        }
                        // reset election timer
                        this.electionTimer.Stop();
                        this.electionTimer.Start();
                        // trigger sending of AppendEntries
                        this.OnHeartbeatTimerOrSendTrigger();
                    }


                });
            }

            int time = new Random().Next(ElectionTime, (int)1.5 * ElectionTime);
            Console.WriteLine("Server " + this.Id + " new Time " + time);
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
                this.leaderTimer.Start();
                return;
            }
            Console.WriteLine("Server " + this.Id + " Send HeartBeat");

            foreach (Raft peer in this.peers)
            {

                if(peer.Id == this.Id)continue;
                // NOTE: do this in parallel for each peer
                Task.Run(() =>
                {
                    int rfNextIndex;
                    List<RaftLog> entries = new List<RaftLog>();
                    lock (this)
                    {
                        rfNextIndex = this.nextIndex[peer.Id];

                        if (this.nextIndex[peer.Id] > this.log.Count)
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



                    Tuple<int, int, int, bool> res = peer.AppendEntries(sendTerm, this.Id, prevLogIndex, prevLogTerm, entries, this.commitIndex);

                 
                    int term = res.Item1;
                    int conflitIndex = res.Item2;
                    int conflitTerm = res.Item3;
                    bool success = res.Item4;

                    if (term > this.currentTerm)
                    {
                        this.currentTerm = term;
                        this.state = State.FOLLOWER;
                        this.votedFor = null;

                        foreach (Raft p in this.peers)
                        {
                            nextIndex[p.Id] = this.log.Count;
                            matchIndex[p.Id] = -1;

                        }
                    }

                    if (currentTerm != sendTerm)
                    {
                        return;
                    }

                    if (!success)
                    {
                        this.nextIndex[peer.Id] = conflitIndex;

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
                                nextIndex[peer.Id] = ourLastInConflictTerm + 1;
                            }

                        }
                        // Trigger sending AppendEntries again immediately
                        this.leaderTimer.Stop();
                        this.leaderTimer.Start();
                        this.OnHeartbeatTimerOrSendTrigger();

                        return;
                    }

                    matchIndex[peer.Id] = prevLogIndex + entries.Count;
                    nextIndex[peer.Id] = matchIndex[peer.Id] + 1;

                    for (int n = this.log.Count - 1; n > this.commitIndex; n--)
                    {
                        if (log[n].Term != this.currentTerm)
                        {
                            break;
                        }

                        int replicas = 0;

                        foreach (Raft pRaft in this.peers)
                        {
                            if (this.matchIndex[pRaft.Id] >= n)
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

            this.leaderTimer.Start();

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
            nextIndex[this.Id] = this.log.Count;
            matchIndex[this.Id] = this.log.Count - 1;

            // TODO: persist Raft state
            // trigger sending of AppendEntries
            this.OnHeartbeatTimerOrSendTrigger();
            this.leaderTimer.Stop();
            this.leaderTimer.Start();

            accepted = true;
            willCommitAt = this.log.Count - 1;

            Console.WriteLine("Accepted command at commit " + willCommitAt);
        }

    }
}
