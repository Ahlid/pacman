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
        public const int NUM_PLAYERS = 3;
        private int roundIntervalMsec;

        private int lastID = 1;

        /// <summary>
        /// Contains information of all clients registered at the moment.
        /// </summary>
        private List<IClient> clients;
        /// <summary>
        /// Contain information of the clients that are waiting.
        /// </summary>
        private List<IClient> waitingQueue;
        /// <summary>
        /// Players on the game or ready to enter the game.
        /// </summary>
        private List<IPlayer> playersInGame;
        /// <summary>
        /// Map between players in the game and theirs current move.
        /// </summary>
        private Dictionary<IPlayer, Play> playerMoves;

        private bool hasGameStarted;

        private IStage stage;
        /// <summary>
        /// Scalar timestamp to identify each round
        /// </summary>
        private int round = 0;

        private Timer timer;

        List<Shared.Action> actions;



        // to remove
        //private List<IClient> clients;
        //private Dictionary<string, IPlayer> playersAddressMap;

        public ConcreteServer()
        {
            this.clients = new List<IClient>();
            this.waitingQueue = new List<IClient>();
            this.playersInGame = new List<IPlayer>();
            this.playerMoves = new Dictionary<IPlayer, Play>();
            stage = new Stage();
            Console.WriteLine("Server initiated.");
            actions = new List<Shared.Action>();

            // to remove
            //clients = new List<IClient>();
            //playersAddressMap = new Dictionary<string, IPlayer>();
        }

        public void Run(int roundIntervalMsec)
        {
            this.roundIntervalMsec = roundIntervalMsec;
        }

        private void Tick(Object parameters)
        {
            Console.WriteLine("Has game ended: " + this.hasGameEnded());
            if (!hasGameEnded())
            {
                Console.WriteLine("TICK: Round {0}", this.round);
                this.buildStage();
                this.round++;
                //this.broadcastStart();  //todo: fazer o broadcast no inicio!!!
                this.broadcastRound();

                // how to block threads
                // clean players moves

                foreach (IPlayer player in this.stage.GetPlayers())
                {
                    playerMoves[player] = Play.NONE;
                }

                timer = new Timer(new TimerCallback(Tick), null, roundIntervalMsec, Timeout.Infinite);
            }
            else
            {
                // clear variables
                // get ready for a next game
                // clear timer
            }
        }


        private bool hasGameEnded()
        {
            return false;
            /*
            if(this.stage.GetPlayers().Count > 0) // if players exist on the game 
            {
                // check if there are any player alive or if the all the coins have been captured.
                return !(this.stage.GetPlayers().Count(p => p.Alive) > 0) || !(this.stage.GetCoins().Count > 0);
            }else
            {
                Console.Write("no players in the stage?");
                return true; // there are not players on the game, they all quit or crashed. 
            }*/
        }

        /// <summary>
        /// Returns the player with the most coins collected.
        /// </summary>
        /// <returns>Player that won the game.</returns>
        private IPlayer getWinner()
        {
            int maxScore = this.stage.GetPlayers().Max(p => p.Score);
            // check if this cast doesn't present a problem; at first it should be a problem.
            return (IPlayer)this.stage.GetPlayers().Where(p => p.Score == maxScore);
        }


        // TODO: What happens if a lot of players try to join at the same time? The method probably isn't thread safe.

        // adiciona os jogadores na lista de espera. quando a lista de espera atingir o numero minimo de jogadores entao passa-os para outra lista, limpa a lista de espera
        // e inicia o jogo
        public bool Join(string username, string address)
        {
            if (this.playersInGame.Exists(c => c.Username == username) || 
                this.waitingQueue.Exists(c => c.Username == username))
            {
                // lancar excepcao, nome ja em uso
                return false; // already exists a player with that username
            }

            // ao enviar os dados dos clients para um cliente devem enviar o endereço e o username.
            IClient client = (IClient)Activator.GetObject(
                typeof(IClient),
                address);
            client.Username = username;
                
            if (hasGameStarted)
            {
                this.waitingQueue.Add(client); // on enqueued, remove it on this list and change it to the clients list
                // send waiting signal - for the game to end
                Console.WriteLine(String.Format("Sending to client '{0}' that he has just been queued", client.Username));
                client.LobbyInfo("Queued for the next game...");
                return true;
            }

            this.clients.Add(client);
            IPlayer player = new Player();
            player.Address = address;
            player.Username = username;
            this.playersInGame.Add(player);
            playerMoves[player] = Play.NONE;

            stage.AddPlayer(player);
            Console.WriteLine(String.Format("User '{0}' was registered, with the address: {1}", username, address));

            if (NUM_PLAYERS == this.playersInGame.Count)
            {
                // minimum numbers of players required to start the game has been reached. Simple strategy
                // build game stage
                stage.BuildInitStage(NUM_PLAYERS);
                timer = new Timer(new TimerCallback(Tick), null, roundIntervalMsec, Timeout.Infinite);
                this.broadcastStartSignal();
                this.hasGameStarted = true;
            }
            else
            {
                // send waiting signal - for the game to start 
                Console.WriteLine(String.Format("Sending to client '{0}' that he is just waiting for other to join", client.Username));
                client.LobbyInfo("Waiting for other players to join...");
                //sendClientsReadyToPlay(client);
                //sendNewClientReadyToPlay(client);
            }

            return true;

        }

        public void Quit(string address)
        {
            Console.WriteLine(String.Format("Client [name] at {0} is disconnecting.", address));
            int indexOfWaitingClient = this.waitingQueue.FindIndex(c => c.Address == address);

            if (indexOfWaitingClient == -1)
            {
                int indexPlayer = this.playersInGame.FindIndex(c => c.Address == address);
                if (indexPlayer != -1)
                {
                    this.playersInGame.RemoveAt(indexPlayer);
                    // remove the player from the stage 
                    IPlayer p = this.stage.GetPlayers().FirstOrDefault(c => c.Address == address);
                    this.stage.RemovePlayer(p);
                }
            }
            else
            {
                this.waitingQueue.RemoveAt(indexOfWaitingClient);
            }
        }




        private void sendClientsReadyToPlay(IClient client)
        {
            Dictionary<string, string> _clients = new Dictionary<string, string>();
            IPlayer player;
            
            for (int i = this.playersInGame.Count - 1; i >= 0; i--)
            {
                player = this.playersInGame[i];
                if(player.Username != client.Username)
                {
                    _clients.Add(player.Username, player.Address);
                }
            }
            for (int k = this.clients.Count - 1; k >= 0; k--)
            {
                try
                {
                    if(this.clients[k].Username != client.Username)
                    {
                        this.clients.ElementAt(k).sendPlayersOnGame(_clients);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("error: \n" + e.StackTrace);
                    this.clients.RemoveAt(k);
                    // todo: try to reach the client again. Uma thread à parte. Verificar se faz sentido.
                }
            }
            Console.WriteLine("done");
        }

        public void sendNewClientReadyToPlay(IClient client)
        {
            //broadcast to all besides client
            IPlayer player;
            for (int k = this.playersInGame.Count - 1; k >= 0; k--)
            {
                try
                {
                    player = this.playersInGame[k];
                    if (player.Username != client.Username)
                    {
                        this.clients.ElementAt(k).sendNewPlayer(player.Username, player.Address);
                    }
                }
                catch (Exception)
                {
                    this.clients.RemoveAt(k);
                    // todo: try to reach the client again. Uma thread à parte. Verificar se faz sentido.
                }
            }
        }




        private void broadcastStartSignal()
        {
            IClient client;
            for (int i = this.clients.Count - 1; i >= 0; i--)
            {
                try
                {
                    client = this.clients.ElementAt(i);
                    Console.WriteLine(String.Format("Sending start signal to client: {0}, at: {1}", client.Username, client.Address));
                    client.Start(stage);
                }
                catch (Exception)
                {
                    this.clients.RemoveAt(i);
                    // todo: try to reach the client again. Uma thread à parte. Verificar se faz sentido.
                }
            }
        }

        private void broadcastRound()
        {
            IClient client;
            for (int i = this.clients.Count - 1; i >= 0; i--)
            {
                try
                {
                    client = this.clients.ElementAt(i);
                    Console.WriteLine(String.Format("Sending stage to client: {0}, at: {1}", client.Username, client.Address));
                    Console.WriteLine(String.Format("Round Nº{0}", this.round));
                    //todo change score
                    client.SendRoundStage(this.actions, -9999, this.round);
                }
                catch (Exception)
                {
                    this.clients.RemoveAt(i);
                    // todo: try to reach the client again. Uma thread à parte. Verificar se faz sentido.
                }
            }
            this.actions = new List<Shared.Action>();
        }

        public void SetPlay(string address, Play play, int round)
        {
            IPlayer player = this.playersInGame.FirstOrDefault(p => p.Address == address);
            Console.WriteLine("Round: {0} Play: {1}, by: {2}", play, round, player.Username);
            playerMoves[player] = play;

            //todo:
            //compute position of players, monsters, scores, 
            // should have a timer controlling this
            // send to all the new stage
        }

        private void buildStage()
        {
            computeGhost();
            computeMovement();

        }

        private void computeGhost()
        {
            foreach (IMonster monster in stage.GetMonsters())
            {
                //monster.Step();
            }


            /*
            //move ghosts
            redGhost.Left += ghost1;
            yellowGhost.Left += ghost2;


            // if the red ghost hits the picture box 4 then wereverse the speed
            if (redGhost.Bounds.IntersectsWith(pictureBox1.Bounds))
                ghost1 = -ghost1;
            // if the red ghost hits the picture box 3 we reverse the speed
            else if (redGhost.Bounds.IntersectsWith(pictureBox2.Bounds))
                ghost1 = -ghost1;
            // if the yellow ghost hits the picture box 1 then wereverse the speed
            if (yellowGhost.Bounds.IntersectsWith(pictureBox3.Bounds))
                ghost2 = -ghost2;
            // if the yellow chost hits the picture box 2 then wereverse the speed
            else if (yellowGhost.Bounds.IntersectsWith(pictureBox4.Bounds))
                ghost2 = -ghost2;
            //moving ghosts and bumping with the walls end
*/
        }

        private void computeMovement()
        {
            //computeGhost();

            //Assume that the playerMoves has a play for every play, with default Play.NONE
            foreach (Player player in stage.GetPlayers())
            {
                Play play = playerMoves[player];
                Shared.Action action = player.Move(play);
                if(action != null)
                    actions.Add(action);
                Console.WriteLine("Position player: {0}", player.Position);
            }

            /*
             * 
             * 
             * 
                 

            //for loop to check walls, ghosts and points
            foreach (Control x in this.Controls) {
                // checking if the player hits the wall or the ghost, then game is over
                if (x is PictureBox && x.Tag == "wall" || x.Tag == "ghost") {
                    if (((PictureBox)x).Bounds.IntersectsWith(pacman.Bounds)) {
                        pacman.Left = 0;
                        pacman.Top = 25;
                        label2.Text = "GAME OVER";
                        label2.Visible = true;
                        timer1.Stop();
                    }
                }
                if (x is PictureBox && x.Tag == "coin") {
                    if (((PictureBox)x).Bounds.IntersectsWith(pacman.Bounds)) {
                        this.Controls.Remove(x);
                        score++;
                        //TODO check if all coins where "eaten"
                        if (score == total_coins) {
                            //pacman.Left = 0;
                            //pacman.Top = 25;
                            label2.Text = "GAME WON!";
                            label2.Visible = true;
                            timer1.Stop();
                            }
                    }
                }
            }
                pinkGhost.Left += ghost3x;
                pinkGhost.Top += ghost3y;

                if (pinkGhost.Left < boardLeft ||
                    pinkGhost.Left > boardRight ||
                    (pinkGhost.Bounds.IntersectsWith(pictureBox1.Bounds)) ||
                    (pinkGhost.Bounds.IntersectsWith(pictureBox2.Bounds)) ||
                    (pinkGhost.Bounds.IntersectsWith(pictureBox3.Bounds)) ||
                    (pinkGhost.Bounds.IntersectsWith(pictureBox4.Bounds))) {
                    ghost3x = -ghost3x;
                }
                if (pinkGhost.Top < boardTop || pinkGhost.Top + pinkGhost.Height > boardBottom - 2) {
                    ghost3y = -ghost3y;
                }
                */
        }

        private void computeCollisionsPlayerCoin()
        {

        }

        private void computeCollisionsPlayerWall()
        {

        }

        private void computeCollisionsPlayerMonster()
        {

        }

        public int NextAvailablePort(string address)
        {
            throw new NotImplementedException();
        }
    }
}
