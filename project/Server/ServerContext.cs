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
        public TcpChannel Channel { get; set; }
        public Uri Address { get; set; } //Server address
        public string PID { get; set; } //Assigned Process ID

        //For replica purposes

        public List<Uri> ReplicaServersURIsList { get; set; }
        public Dictionary<Uri, IServer> Replicas { get; set; }
        public static volatile Mutex ReplicaMutex = new Mutex(false);

        public int NumPlayers { get; set; }
        public int RoundIntervalMsec { get; set; }

        public List<IClient> Clients { get; set; }
        public List<IClient> WaitingQueue { get; set; }
        public Dictionary<int, IGameSession> GameSessionsTable { get; set; }
        public IGameSession CurrentGameSession { get; set; }
        

        public static volatile Mutex Mutex = new Mutex(false);

        public ServerContext()
        {
            Clients = new List<IClient>();
            WaitingQueue = new List<IClient>();
            GameSessionsTable = new Dictionary<int, IGameSession>();
            ReplicaServersURIsList = new List<Uri>();
            Replicas = new Dictionary<Uri, IServer>();
        }
    }
}
