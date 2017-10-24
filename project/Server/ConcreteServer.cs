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

        private Dictionary<IPlayer, Play> playerMoves;
        private Dictionary<string, IPlayer> playersAddressMap;
        private List<IClient> clients;
        private IStage stage;
        private int round = 0;

        private Timer timer;
        

        public ConcreteServer()
        {
            clients = new List<IClient>();
            playerMoves = new Dictionary<IPlayer, Play>();
            playersAddressMap = new Dictionary<string, IPlayer>();
            stage = new Stage();
            System.Console.WriteLine("Constructor done");
        }

        public void Run(int roundIntervalMsec)
        {
            timer = new Timer(new TimerCallback(Tick), null, 0, roundIntervalMsec);
        }

        private void Tick(Object parameters)
        {
            if (!hasGameEnded())
            {
                buildStage();
                round++;
                broadcastStart();
            }
        }

        /// <summary>
        /// Determines if the game has ended, by checking if all the pacmans are dead,
        /// or if all the coins have been collected.
        /// </summary>
        /// <returns></returns>
        private bool hasGameEnded()
        {
            return this.stage.GetPlayers().Count(p => p.Alive) > 0 || this.stage.GetCoins().Count() == 0;
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

        public bool Join(string address)
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
            playersAddressMap.Add(address, player);
            stage.AddPlayer(player);

            if (NUM_PLAYERS == clients.Count)
            {
                // build game stage
                stage.BuildInitStage(NUM_PLAYERS);
                Thread thread = new Thread(delegate ()
                {
                    broadcastStart();
                });
                thread.Start();
            }

            return true;
        }

        private void broadcastStart()
        {
            for (int i = clients.Count - 1; i >= 0; i--)
            {
                try
                {
                    if(round == 0)
                    {
                        clients.ElementAt(i).Start(stage);
                    } else
                    {
                        clients.ElementAt(i).SendRoundStage(stage, round);
                    }

                    // real simple approach now
                    this.round++;
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
            broadcastStart();
        }

        private void buildStage()
        {
            computeGhost();
            computeMovement();

            throw new NotImplementedException();
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
    }
}
