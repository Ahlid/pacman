using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using Shared;
using System.Threading;
using Shared.Exceptions;

namespace Server
{
    public class Server : MarshalByRefObject, IServer
    {
        private TcpChannel channel;
        private Uri address; //Server address
        private string PID; //Assigned Process ID

        //For replica purposes
        private bool isMaster;
        private Uri masterAddress;
        private IServer master;
        private List<Uri> replicaServersURIsList;
        private Dictionary<Uri, IServer> replicas;
        public static volatile Mutex replicaMutex = new Mutex(false);


        public int numPlayers;
        private int roundIntervalMsec;

        private List<IClient> clients;
        private List<IClient> waitingQueue;
        private Dictionary<int, IGameSession> gameSessionsTable;
        private IGameSession currentGameSession;
        private Timer timer;
        private bool stop;

        public static volatile Mutex mutex = new Mutex(false);


        //Commun private constructor
        private Server(Uri address, string PID)
        {
            this.stop = false;
            this.PID = PID;
            this.address = address;
            this.clients = new List<IClient>();
            this.waitingQueue = new List<IClient>();
            this.gameSessionsTable = new Dictionary<int, IGameSession>();
            this.replicaServersURIsList = new List<Uri>();
            this.replicas = new Dictionary<Uri, IServer>();

            //Start services
            channel = new TcpChannel(address.Port);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(this, "Server", typeof(Server));
        }
        //Master constructor
        public Server(Uri address, string PID = "not set", int roundIntervalMsec = 20, int numPlayers = 3) : this(address, PID)
        {
            //This server will start as a Master

            this.isMaster = true;
            this.numPlayers = numPlayers;
            this.roundIntervalMsec = roundIntervalMsec;
            this.currentGameSession = new GameSession(numPlayers);
            this.gameSessionsTable[currentGameSession.ID] = currentGameSession;
            this.roundIntervalMsec = roundIntervalMsec;

        }
        //Replica constructor
        public Server(Uri address, Uri masterURL, string PID = "not set") : this(address, PID)
        {
            //This server will start as a Replica
            this.isMaster = false;

            this.master = (IServer)Activator.GetObject(
                typeof(IServer),
                masterURL.ToString() + "Server");

            this.master.RegisterReplica(address);

        }

        private void Tick(Object parameters)
        {
            Console.WriteLine("tick");
            if (this.currentGameSession.HasGameEnded())
            {
                Console.WriteLine("Game has ended!");
                this.stop = true;
                //temos de começar uma nova ronda
                this.currentGameSession.EndGame();
                IGameSession newGameSession = new GameSession(numPlayers);
                gameSessionsTable[newGameSession.ID] = newGameSession;
                this.currentGameSession = newGameSession;

                //
                // handle clients and server lists to start next game
                //
            }
            else
            {
                currentGameSession.PlayRound();
            }

            if(!this.stop)
            {
                timer = new Timer(new TimerCallback(Tick), null, roundIntervalMsec, Timeout.Infinite);
            }
            
        }

        public void Stop()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void addPlayersToCurrentGameSession()
        {
            int playersWaiting = currentGameSession.Clients.Count;
            int leftPlayers = numPlayers - playersWaiting;

            List<IClient> inQuePlayers = this.waitingQueue.Take(leftPlayers).ToList();

            //vamos obter os jogadores em lista de espera
            foreach (IClient player in inQuePlayers)
            {
                if (player != null)
                {
                    currentGameSession.Clients.Add(player);
                    this.waitingQueue.RemoveAll(p => p.Address == player.Address);
                }
            }
        }

        //#############################
        //##   Interface SERVICES    ##
        //#############################

        // TODO: What happens if a lot of players try to join at the same time? The method probably isn't thread safe.
        // adiciona os jogadores na lista de espera. quando a lista de espera atingir o numero minimo de jogadores entao passa-os para outra lista, limpa a lista de espera
        // e inicia o jogo
        public JoinResult Join(string username, Uri address)
        {
            if (clients.Exists(c => c.Username == username) ||
                waitingQueue.Exists(c => c.Username == username))
            {
                // TODO: Lauch exception to the client (username already exists)
                Console.WriteLine($"Client at {address.ToString()} tried to join with username {username}, but is already in use.");
                throw new InvalidUsernameException("Username already in use");
                //return JoinResult.REJECTED_USERNAME;
            }

            // ao enviar os dados dos clients para um cliente devem enviar o endereço e o username.
            IClient client = (IClient)Activator.GetObject(
                typeof(IClient),
                address.ToString() + "Client");
            //client.Username = username;

            Console.WriteLine(address.ToString());

            waitingQueue.Add(client);

            Console.WriteLine($"Client with username {username} and at {address} joined successfully");
            
            clients.Add(client);

            // check if there are enough players in the waiting queue to start the game
            if (waitingQueue.Count >= numPlayers)
            {
                Thread thread = new Thread(new ThreadStart(()=>
                {
                    addPlayersToCurrentGameSession();
                    currentGameSession.StartGame();
                    timer = new Timer(new TimerCallback(Tick), null, roundIntervalMsec, Timeout.Infinite);
                }));
                thread.Start();
            }

            return JoinResult.QUEUED;
        }

        public void SetPlay(Uri address, Play play, int round)
        {
            if (currentGameSession != null && currentGameSession.HasGameStarted)
            {
                currentGameSession.SetPlay(address, play, round);
                Console.WriteLine("Round: {0} Play: {1}, by: {2}", play, round, address);
            }
            else
            {
                // What?
            }
        }

        public void Quit(Uri address)
        {
            //Get username associated to the given address
            //string username = this.clients.Where(s => s.Address == address).Select(s => s.Username).FirstOrDefault();

            Console.WriteLine(String.Format("Client at {0} is disconnecting.", address));
            this.clients.RemoveAll(p => p.Address.ToString() == address.ToString());
            this.waitingQueue.RemoveAll(p => p.Address.ToString() == address.ToString());
            //todo: se player está vivo na game session então deve passar a morto
            // remove players from the current game session

            //
            //foreach(IPlayer player in this.currentGameSession.Stage.GetPlayers())
            //{
            //    if()
            //}
        }

        //REPLICATION

        //Remote
        public void RegisterReplica(Uri replicaServerURL)
        {
            if(!this.isMaster)
            {
                throw new Exception("This server is a replica and cannot register replicas");
            }

            //Todo: Test the communication with the replica

            replicaMutex.WaitOne();
            replicaServersURIsList.Add(replicaServerURL);

            IServer replica = (IServer)Activator.GetObject(
                typeof(IServer),
                replicaServerURL.ToString() + "Server");

            replicas.Add(replicaServerURL, replica);
            broadcastReplicaList();

            replicaMutex.ReleaseMutex();
        }

        private void broadcastReplicaList()
        {
            foreach (IClient client in clients)
            {
                client.SetReplicaList(replicaServersURIsList);
            }

            foreach (Uri uri in replicaServersURIsList)
            {
                IServer server = replicas[uri];
                server.SetReplicaList(replicaServersURIsList);
            }
        }

        //Remote
        public void SetReplicaList(List<Uri> replicasURLs)
        {
            if (this.isMaster)
            {
                throw new Exception("This server has to be a replica");
            }

            replicaMutex.WaitOne();
            replicas = new Dictionary<Uri, IServer>();
            foreach (Uri replicaURL in replicasURLs)
            {
                IServer replica = (IServer)Activator.GetObject(
                                typeof(IServer),
                                replicaURL.ToString() + "Server");
                replicas.Add(replicaURL, replica);
            }
            replicaMutex.ReleaseMutex();

        }

        public Uri GetMaster()
        {
            if(this.isMaster)
            {
                return this.address;
            }

            if(this.masterAddress == null)
            {
                return null;
            }

            try
            {
                //Check if it is still on
                this.master.Ping();
                return this.masterAddress;
            }
            catch(Exception)
            {
                int index = replicaServersURIsList.IndexOf(this.address);
                //todo: get the next in the chain.  
                if (index == 0)
                {
                    Thread thread = new Thread(new ThreadStart(() =>
                    {
                        makeMaster();
                    }));
                }
                else
                {
                    Thread thread = new Thread(new ThreadStart(() =>
                    {
                        resolveNewMaster();
                        master = null;
                        masterAddress = null;
                    }));
                }

                return null;
            }

        }

        private void makeMaster()
        {
            //Assume that we are the new master.
            this.isMaster = true;
            replicaServersURIsList.Remove(this.address); //I'm no longer a replica
            broadcastReplicaList();
            master = null;
            masterAddress = null;
        }

        private void resolveNewMaster()
        {
            int index = replicaServersURIsList.IndexOf(this.address);
            
            if (index == 0)
            {
                //Assume that we are the new master.
                makeMaster();
                return;
            }

            Uri previousReplicaURL = getPreviousReplica();
            if(previousReplicaURL == null) {
                //Assume that we are the new master.
                makeMaster();
                return;
            }

            //Ask the index-1 replica who is the server.
            masterAddress = replicas[previousReplicaURL].GetMaster();
            if(masterAddress == null)
            {
                //Assume that we are the new master.
                makeMaster();
                return;
            }

            master = (IServer)Activator.GetObject(
                    typeof(IServer),
                    masterAddress.ToString() + "Server");

        }


        private Uri getPreviousReplica()
        {
            if (replicaServersURIsList.Count == 1)
            {
                return null;
            }

            int index = replicaServersURIsList.IndexOf(this.address);
            if (index - 1 < 0)
            {
                return replicaServersURIsList[replicaServersURIsList.Count - 1];
            }

            return replicaServersURIsList[index - 1];
        }

        private Uri getNextReplica()
        {
            if(replicaServersURIsList.Count == 1)
            {
                return null;
            }

            int index = replicaServersURIsList.IndexOf(this.address);
            if (index + 1 >= replicaServersURIsList.Count) {
                return replicaServersURIsList[0];
            }

            return replicaServersURIsList[index + 1];
        }



        public void SendRoundStage(IStage stage)
        {
            if(isMaster)
            {
                throw new Exception("Master can't receive Rounds Stages");
            }

            //Assume a session already exists
            //TODO: may be necessary see if the session has arived

            currentGameSession.Stage = stage;
            Thread thread = new Thread(new ThreadStart(() =>
            {
                
            }));
            
        }

        public string Ping()
        {
            return "Here";
        }
    }

}
