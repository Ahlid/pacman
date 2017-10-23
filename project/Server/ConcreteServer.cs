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

        private Dictionary<IPlayer, Play> playerMoves;
        private Dictionary<string, IPlayer> playersAddressMap;

        private List<IClient> clients;
        private IStage stage

        public const int NUM_PLAYERS = 1;




        public int round = 0;
        public Timer timer;
        

        public ConcreteServer()
        {
            clients = new List<IClient>();
            playerMoves = new Dictionary<IPlayer, Play>();
            playersAddressMap = new Dictionary<string, IPlayer>();
            stage = new Stage();
            ICoin coin = new Coin();
            coin.Position = new Point(2, 4);
            stage.addCoin(coin);
            System.Console.WriteLine("Constructor done");
        }

        public void run(int roundIntervalMsec)
        {
            timer = new Timer(new TimerCallback(Tick), null, 0, roundIntervalMsec);
        }

        private void Tick(Object parameters)
        {
            buildStage();
            round++;
            broadcastStart();
        }



        public bool join(string address)
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
            player.Position = new Point(4, 4);
            playersAddressMap.Add(address, player);
            stage.addPlayer(player);

            if (NUM_PLAYERS == clients.Count)
            {
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
                        clients.ElementAt(i).start(stage);
                    } else
                    {
                        clients.ElementAt(i).sendRoundStage(stage, round);
                    }
                    
                }
                catch (Exception)
                {
                    clients.RemoveAt(i);
                }
            }
        }

        public void setPlay(string address, Play play, int round)
        {
            Console.WriteLine("Round: {0} Play: {1}", play, round);
            IPlayer player = playersAddressMap[address];
            playerMoves[player] = play;
        }

        private void buildStage()
        {
            computeGhost();
            computeMovement();

            throw new NotImplementedException();
        }

        private void computeGhost()
        {
            foreach(IMonster monster in stage.getMonsters())
            {
                monster.step();
            }


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



        }
            int xA1 = centerA.X - widthA / 2;

        private bool isColliding(Point centerA, int widthA, int heightA, 
                                 Point centerB, int widthB, int heightB)
        {
            int xA2 = centerA.X + widthA / 2;

            int xB1 = centerB.X - widthB / 2;
            int xB2 = centerB.X + widthB / 2;

            int yA1 = centerA.Y - heightA / 2;
            int yA2 = centerA.Y + heightA / 2;

            int yB1 = centerB.Y - heightB / 2;
            int yB2 = centerB.Y + heightB / 2;

            //Check if there is a gap between the two AABB's in the X axis
            if (xA2 < xB1 || xB2 < xA1) {
                return false;
            }

            if (xA2 < xB1 || xB2 < xA1)
            {
                return false;
            }

            //Check if there is a gap between the two AABB's in the Y axis
            if (yA2 < yB1 || yB2 < yA1)
            {
                return false;
            }

            if (yA2 < yB1 || yB2 < yA1)
            {
                return false;
            }


            // We have an overlap
            return true;
        }

        private void computeMovement()
        {
            computeGhost();

            //Assume that the playerMoves has a play for every play, with default Play.NONE
            foreach (Player player in stage.getPlayers())
            {
                Play play = playerMoves[player];
                player.move(play);
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
