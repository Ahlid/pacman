using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Server;
using System.Drawing;
using System.Threading;

namespace pacman
{
    class ConcreteClient : MarshalByRefObject, IClient
    {
        public static ClientManager ClientManager;
        public static FormWelcome WelcomeForm;
        public FormStage StageForm;

        public Dictionary<string, IClient> Clients { get; set; }
        public string Address { get; set; }
        public int Round { get; set; }

        private bool hasGameStarted;

        // check if is 1st round

        public ConcreteClient()
        {
            this.Round = 0;
            this.hasGameStarted = false;
        }

        public void SendRoundStage(IStage stage, int round)
        {
            // check if game has already started
            if (!hasGameStarted)
            {
                return; 
            }
            //Construir o form através dos objetos que estão no stage
            //MessageBox.Show(string.Format("Stage number {0} received from the server.", round));
            
            // REMOVE THIS INSTRUCTIONS, WRONG APPROACH
            StageForm.Invoke(new Action(() => StageForm.Controls.Clear()));

            buildMonsters(stage);
            buildCoins(stage);
            buildPlayers(stage);
            Round = round;
        }

        /// <summary>
        /// Builds the initial stage of the game. This method initiates the game on the client side.
        /// </summary>
        /// <param name="stage">stage</param>
        public void Start(IStage stage)
        {
            // this check prevents some server to restart the game
            if (hasGameStarted)
            {
                return;
            }else // create the inital stage of the game
            {
                // asking the thread creator of the welcome form to hide it
                WelcomeForm.Invoke(new Action(() => WelcomeForm.Hide()));   // With invoke -> app waits until the action is done


                // create the form responsible for the game, Welcome form has the main thread has to be responsible to create the new form
                WelcomeForm.Invoke((Action)delegate    // with begin invoke 
                {
                    
                    this.StageForm = new FormStage(ClientManager);
                    //StageForm.Closed += (s, args) => WelcomeForm.Show(); // show the welcome windown on closing the stage form window.  //.Close();
                    this.StageForm.Show();
                }); 
                
                //buildWalls(stage);
                //buildCoins(stage);
                //buildMonsters(stage);
                //buildPlayers(stage);

                hasGameStarted = true;
                Round = 0;
            }
            //MessageBox.Show("The game has started(signal received from the server).");
        }

        public void End(IPlayer player)
        {

        }

        private PictureBox createControl(string name, Point position, int width, int height)
        {
            PictureBox pictureBox = new PictureBox();
            pictureBox.BackColor = System.Drawing.Color.Transparent;
            pictureBox.Location = new System.Drawing.Point(position.X - width / 2, position.Y - height / 2);
            pictureBox.Margin = new System.Windows.Forms.Padding(0);
            pictureBox.Name = name;
            pictureBox.Size = new System.Drawing.Size(width, height);
            pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox.TabIndex = 4;
            pictureBox.TabStop = false;
            return pictureBox;
        }

        private void buildWalls(IStage stage)
        {
            PictureBox newWall;
            int i = 1;
            foreach (IWall wall in stage.GetWalls())
            {
                newWall = createControl("wall" + i++, wall.Position, Wall.WIDTH, Wall.HEIGHT);
                newWall.BackColor = Color.Blue;
                StageForm.Invoke(new Action(() => StageForm.Controls.Add(newWall)));
            }

        }


        // pacmans need 
        private void buildPlayers(IStage stage)
        {
            PictureBox newPlayer;
            int i = 1;
            foreach (IPlayer player in stage.GetPlayers())
            { 
                newPlayer = createControl("pacman"+i++, player.Position, Player.WIDTH, Player.HEIGHT);
                newPlayer.Image = global::pacman.Properties.Resources.Left;
                StageForm.Invoke(new Action(() => StageForm.Controls.Add(newPlayer)));
                //MessageBox.Show(String.Format("Point({0}, {1})", player.Position.X, player.Position.Y));
            }

        }

        private void buildCoins(IStage stage)
        {
            PictureBox newCoin;
            int i = 1;
            foreach (ICoin coin in stage.GetCoins())
            {
                newCoin = createControl("coin" + i++, coin.Position, Coin.WIDTH, Coin.HEIGHT);
                newCoin.Image = global::pacman.Properties.Resources.coin;
                StageForm.Invoke(new Action(() => StageForm.Controls.Add(newCoin)));
            }
        }

        private void buildMonsters(IStage stage)
        {
            PictureBox newMonster;
            int i = 1;
            foreach (IMonster monster in stage.GetMonsters())
            {
                newMonster = createControl("monster" + i++, monster.Position, MonsterAware.WIDTH, MonsterAware.HEIGHT);
                if(i % 3 == 0)
                {
                    newMonster.Image = global::pacman.Properties.Resources.pink_guy;
                } 
                else if (i % 3 == 1)
                {
                    newMonster.Image = global::pacman.Properties.Resources.red_guy;
                }
                else if (i % 3 == 2)
                {
                    newMonster.Image = global::pacman.Properties.Resources.yellow_guy;
                }

                StageForm.Invoke(new Action(() => StageForm.Controls.Add(newMonster)));
                
            }
        }


    }
}
