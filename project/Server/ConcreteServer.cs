using Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class ConcreteServer : MarshalByRefObject, IServer
    {
        public  int numPlayers = 2;
        private int roundIntervalMsec;
        private int lastID = 0;
        private List<IClient> clients;
        private List<IClient> waitingQueue;
        private Dictionary<int, IGameSession> gameSessionsTable;
        private Timer timer;

        public ConcreteServer()
        { } 
        

        public void Run(int roundIntervalMsec, int numPlayers = 2)
        {
            this.clients = new List<IClient>();
            this.waitingQueue = new List<IClient>();
            this.timer = new Timer(new TimerCallback(Tick), null, Timeout.Infinite, Timeout.Infinite);
            this.numPlayers = numPlayers;
            //this is the server's first game session
            this.gameSessionsTable = new Dictionary<int, IGameSession>();
            gameSessionsTable[++lastID] = new GameSession(lastID, numPlayers);
            this.roundIntervalMsec = roundIntervalMsec;
            this.timer.Change(roundIntervalMsec, roundIntervalMsec);
        }

        public void Stop()
        {
            this.timer.Change(Timeout.Infinite, Timeout.Infinite);

        }

        private void Tick(Object parameters)
        {
            this.timer.Change(Timeout.Infinite, Timeout.Infinite);

            IGameSession actualGameSession = this.gameSessionsTable[this.lastID];
            //check if GameSession has started
            Console.WriteLine("TIMR");
            //the game is waiting for players
            if (!actualGameSession.HasGameStarted)
            {
                AddPlayersToGameSession(actualGameSession);

            }
            else //game started
            {
                //vamos ver se acabou
                if (actualGameSession.HasGameEnded())
                {
                    //temos de começar uma nova ronda
                    actualGameSession.EndGame();
                    IGameSession newGameSession = new GameSession(++this.lastID, numPlayers);
                    this.gameSessionsTable[this.lastID] = newGameSession;
                    this.AddPlayersToGameSession(newGameSession);
                }
                else //senão acabou aplicar a ronda
                {
                    actualGameSession.PlayRound();
                }
            }


            this.timer.Change(roundIntervalMsec, Timeout.Infinite);


        }

        // TODO: What happens if a lot of players try to join at the same time? The method probably isn't thread safe.

        // adiciona os jogadores na lista de espera. quando a lista de espera atingir o numero minimo de jogadores entao passa-os para outra lista, limpa a lista de espera
        // e inicia o jogo
        public bool Join(string username, string address)
        {


            //verificar se o username já se encontra em uso
            if (this.clients.Exists(c => c.Username == username) ||
                this.waitingQueue.Exists(c => c.Username == username))
            {
                //todo: exceptions
                // lancar excepcao, nome ja em uso
                return false; // already exists a player with that username
            }

            // ao enviar os dados dos clients para um cliente devem enviar o endereço e o username.
            IClient client = (IClient)Activator.GetObject(
                typeof(IClient),
                address);
            client.Username = username;


            this.waitingQueue.Add(client); // on enqueued, remove it on this list and change it to the clients list
                                           // send waiting signal - for the game to end
            Console.WriteLine(String.Format("Sending to client '{0}' that he has just been queued", client.Username));
            client.LobbyInfo("Queued for the next game...");

            this.clients.Add(client);
            return true;

        }

        public void Quit(string address)
        {
            Console.WriteLine(String.Format("Client [name] at {0} is disconnecting.", address));
            this.clients.RemoveAll(p => p.Address == address);
            this.waitingQueue.RemoveAll(p => p.Address == address);

        }

        public void SetPlay(string address, Play play, int round)
        {
            IGameSession actualGameSession = this.gameSessionsTable[this.lastID];

            if (actualGameSession != null && actualGameSession.HasGameStarted)
            {
                actualGameSession.SetPlay(address, play, round);
                Console.WriteLine("Round: {0} Play: {1}, by: {2}", play, round, address);
            }


        }

        //todo
        public int NextAvailablePort(string address)
        {
            throw new NotImplementedException();
        }

        public void AddPlayersToGameSession(IGameSession actualGameSession)
        {
            int playersWaiting = actualGameSession.Clients.Count;
            int leftPlayers = numPlayers - playersWaiting;

            List<IClient> inQuePlayers = this.waitingQueue.Take(leftPlayers).ToList();

            //vamos obter os jogadores em lista de espera
            foreach (IClient player in inQuePlayers)
            {
                if (player != null)
                {
                    actualGameSession.Clients.Add(player);
                    this.waitingQueue.RemoveAll(p => p.Address == player.Address);
                }
            }

            //vamos ver se podemos começar o jogo
            if (actualGameSession.Clients.Count - numPlayers == 0)
            {
                actualGameSession.StartGame();
            }
        }
    }
}
