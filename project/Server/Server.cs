using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using Shared;
using System.Threading;


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

            //Start services
            channel = new TcpChannel(address.Port);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(this, "Server", typeof(Server));
        }
        //Master constructor
        public Server(Uri address, string PID = "not set", int roundIntervalMsec = 20, int numPlayers = 2) : this(address, PID)
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

            // TODO: communicate with the main server and request a stage and the missing information (numPlayers, roundIntervalMsec)
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
                // handle clients and server lists
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
            Console.WriteLine($"Username {username} Address {address.ToString()}");
            if (clients.Exists(c => c.Username == username) ||
                waitingQueue.Exists(c => c.Username == username))
            {
                // TODO: Lauch exception to the client (username already exists)
                return JoinResult.REJECTED_USERNAME;
            }

            // ao enviar os dados dos clients para um cliente devem enviar o endereço e o username.
            IClient client = (IClient)Activator.GetObject(
                typeof(IClient),
                address.ToString() + "Client");
            //client.Username = username;

            Console.WriteLine(address.ToString());

            waitingQueue.Add(client);

            Console.WriteLine(string.Format("Sending to client '{0}' that he has just been queued", username));
            
            clients.Add(client);

            //vamos ver se podemos começar o jogo
            if (waitingQueue.Count >= numPlayers)
            {
                
                //mutex.WaitOne();
                Thread thread = new Thread(new ThreadStart(()=>
                {
                    // todo: this block has a problem
                    addPlayersToCurrentGameSession();
                    currentGameSession.StartGame();
                    timer = new Timer(new TimerCallback(Tick), null, roundIntervalMsec, Timeout.Infinite);
                }));
                thread.Start();
                //mutex.ReleaseMutex();
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
            Console.WriteLine(String.Format("Client at {0} is disconnecting.", address));
            this.clients.RemoveAll(p => p.Address.ToString() == address.ToString());
            this.waitingQueue.RemoveAll(p => p.ToString() == address.ToString());
        }

    }

}
