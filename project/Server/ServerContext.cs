using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class ServerContext
    {
        public delegate void SwitchStrategyDelegate(ServerStrategy previous, ServerStrategy next);
        public SwitchStrategyDelegate SwitchStrategy;

        public TcpChannel Channel { get; set; }
        public Uri Address { get; set; } //Server address
        public string PID { get; set; } //Assigned Process ID

        //For replica purposes
        public enum Role { Leader, Candidate, Follower }
        public Role CurrentRole { get; set; }

        public bool hasRegistered;
        public List<Uri> OtherServersUrls { get; set; }
        public Dictionary<Uri, IServer> OtherServers { get; set; }

        private int _CurrentTerm = 0;
        //latest term server has seen(initialized to 0 on first boot, increases monotonically)
        public int CurrentTerm {
            get { return _CurrentTerm; }
            set {
                if(_CurrentTerm != value)
                {
                    //Changed terms
                    this.VotedForUrl = null;
                }
                _CurrentTerm = value;
            }
        } 

        public Uri VotedForUrl { get; set; }// candidateId that received vote in current term(or null if none)
        //log entries; each entry contains command for state machine, and term when entry was received by leader(first index is 1)
        public List<LogEntry> Logs { get; set; }

        //Volatile State on all servers
        public int CommitIndex { get; set; } //index of highest log entry known to be committed(initialized to 0, increases monotonically)
        public int LastApplied { get; set; } //index of highest log entry applied to state machine(initialized to 0, increases monotonically)


        public int NumPlayers { get; set; }
        public int RoundIntervalMsec { get; set; }


        public Dictionary<Uri, int> nextIndex;   //for each server, index of the next log entry to send to that server(initialized to leader last log index + 1)
        public Dictionary<Uri, int> matchIndex;  //for each server, index of highest log known to be replicated on server(initialized to 0, increases monotonically)


        //Session
        public List<IClient> pendingClients;
        public List<IClient> sessionClients;
        public GameStateMachine stateMachine { get; set; }
        public List<IPlayer> playerList { get; set; }
        public bool GameStartRequest { get; set; } //Used to request a start(requesting a start takes time to be commited, no more StartGame entries should be created)
        public bool HasGameStarted { get; set; }


        //Leader Only
        public System.Timers.Timer Timer { get; set; }
        public System.Timers.Timer LeaderTimer { get; set; } //timer do leader
        public Dictionary<Uri, Play> plays;

        public static volatile Mutex Mutex = new Mutex(false);

        public ServerContext()
        {
            Console.WriteLine("Creating context");
            OtherServersUrls = new List<Uri>();
            OtherServers = new Dictionary<Uri, IServer>();

            //start dictionaries
            this.nextIndex = new Dictionary<Uri, int>();
            this.matchIndex = new Dictionary<Uri, int>();

            foreach (Uri uri in this.OtherServersUrls)
            {
                this.nextIndex[uri] = this.Logs.Count;
                this.matchIndex[uri] = 0;
            }

            //Session
            this.pendingClients = new List<IClient>();
            this.playerList = new List<IPlayer>();
            this.GameStartRequest = false;
            this.HasGameStarted = false;

            this.plays = new Dictionary<Uri, Play>();
            this.CurrentTerm = 1;
            this.VotedForUrl = null;
            this.Logs = new List<LogEntry>();
            this.CommitIndex = -1;
            this.LastApplied = -1;
        }
    }
}
