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
        public const int NUM_PLAYERS = 1;

        /*
         * Ao registar os clientes ficam numa lista de espera, como clientes
         * quando existem condicoes para começar um jogo entao estes passam daquela lista
         * para uma nosta lista como jogadores
         * 
         */
        public Dictionary<string, IClient> waitingQueue;
        public Dictionary<string, IPlayer> players;
        private Dictionary<IPlayer, Play> playerMoves;



        // to remove
        private List<IClient> clients; 
        private Dictionary<string, IPlayer> playersAddressMap;
        


        private IStage stage;
        /// <summary>
        /// Scalar timestamp to identify each round
        /// </summary>
        private int round = 0;

        private Timer timer;
        private int roundIntervalMsec;



        public ConcreteServer()
        {
            clients = new List<IClient>();
            playerMoves = new Dictionary<IPlayer, Play>();
            playersAddressMap = new Dictionary<string, IPlayer>();
            stage = new Stage();
            Console.WriteLine("Server initiated.");
        }

        public void Run(int roundIntervalMsec)
        {
            this.roundIntervalMsec = roundIntervalMsec;
        }

        private void Tick(Object parameters)
        {
            Console.WriteLine("hehe: " + hasGameEnded());
            if (!hasGameEnded())
            {
                Console.WriteLine("{0}", this.round);
                this.buildStage();
                this.round++;
                this.broadcastStart();  //todo: fazer o broadcast no inicio!!!

                // how to block threads
                // clean players moves

                foreach(IPlayer player in this.stage.GetPlayers()) {
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

        /// <summary>
        /// Determines if the game has ended, by checking if all the pacmans are dead,
        /// or if all the coins have been collected.
        /// </summary>
        /// <returns></returns>
        private bool hasGameEnded()
        {
            //return this.stage.GetPlayers().Count(p => p.Alive) > 0 || this.stage.GetCoins().Count() == 0;
            return false;
        }

        /// <summary>
        /// Returns the player with the most coins collected.
        /// </summary>
        /// <returns>Player that won the game.</returns>
        private IPlayer getWinner()
        {
            int maxScore = this.stage.GetPlayers().Max(p => p.Score);
            // check if this cast doesn't present a problem; at first it should be a problem.
            return (IPlayer) this.stage.GetPlayers().Where(p => p.Score == maxScore);
        }


        // adiciona os jogadores na lista de espera. quando a lista de espera atingir o numero minimo de jogadores entao passa-os para outra lista, limpa a lista de espera
        // e inicia o jogo
        public bool Join(string username, string address)
        {
            if (NUM_PLAYERS == clients.Count)
            {
                //Either queue the client or simply reject it
                return false;
            }
            IClient client = (IClient)Activator.GetObject(
                typeof(IClient),
                address);

            clients.Add(client);
            IPlayer player = new Player();
            player.Address = address;
            player.Username = username;
            playerMoves[player] = Play.NONE;
            playersAddressMap.Add(address, player);
            stage.AddPlayer(player);
            Console.WriteLine(String.Format("User '{0}' was registered, with the address: {1}", username, address));

            if (NUM_PLAYERS == clients.Count)
            {
                // build game stage
                stage.BuildInitStage(NUM_PLAYERS);
                timer = new Timer(new TimerCallback(Tick), null, roundIntervalMsec, Timeout.Infinite);


            }

            return true;
        }

        public void Quit(string address)
        {
            Console.WriteLine(String.Format("Client [name] at {0} is disconnecting.", address));
            //remover player da lista
        }

        private void broadcastStart()
        {
            for (int i = clients.Count - 1; i >= 0; i--)
            {
                try
                {
                    Console.WriteLine("Round number: {0}", this.round);
                    if(round == 1)
                    {
                        clients.ElementAt(i).Start(stage);
                    } else
                    {
                        clients.ElementAt(i).SendRoundStage(stage, round);
                    }

                    // real simple approach now
                }
                catch (Exception)
                {
                    clients.RemoveAt(i);
                }
            }
        }

        public void SetPlay(string address, Play play, int round)
        {
            Console.WriteLine("Round: {0} Play: {1}", play, round);
            IPlayer player = playersAddressMap[address];
            
             playerMoves[player] = play;
           
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
            foreach(IMonster monster in stage.GetMonsters())
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
                player.Move(play);
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
