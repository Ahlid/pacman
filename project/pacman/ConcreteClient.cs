using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Server;

namespace pacman
{
    class ConcreteClient : MarshalByRefObject, IClient
    {
        public static FormStage StageForm;
        public string Address { get; set; }
        public int Round { get; set; }
        private bool hasGameStarted;


        public ConcreteClient()
        {
            this.Round = 0;
            this.hasGameStarted = false;
        }

        public void SendRoundStage(IStage stage, int round)
        {
            if (!hasGameStarted)
            {
                MessageBox.Show("Something went wrong.");
                return;
            }
            //Construir o form através dos objetos que estão no stage
            //MessageBox.Show(string.Format("Stage number {0} received from the server.", round));
            buildMonsters(stage);
            buildCoins(stage);
            buildPlayers(stage);
            this.Round = round;
        }

        public void Start(IStage stage)
        {
            if(hasGameStarted)
            {
                MessageBox.Show("Game has already started.");
                return;
            }
            //MessageBox.Show("The game has started(signal received from the server).");
            buildMonsters(stage);
            buildCoins(stage);
            buildPlayers(stage);
            hasGameStarted = true;
            //round = 0;
        }

        private void buildPlayers(IStage stage)
        {
            PictureBox newPlayer;
            foreach (IPlayer player in stage.GetPlayers())
            {
                newPlayer = new PictureBox();
                newPlayer.BackColor = System.Drawing.Color.Transparent;
                newPlayer.Image = global::pacman.Properties.Resources.Left;
                newPlayer.Location = new System.Drawing.Point(player.Position.X - Player.WIDTH / 2, player.Position.Y - Player.HEIGHT / 2); 
                newPlayer.Margin = new System.Windows.Forms.Padding(0);
                newPlayer.Name = "pacman";
                newPlayer.Size = new System.Drawing.Size(Player.WIDTH, Player.HEIGHT);
                newPlayer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
                newPlayer.TabIndex = 4;
                newPlayer.TabStop = false;
            }
            

            throw new NotImplementedException();
        }

        private void buildCoins(IStage stage)
        {
            throw new NotImplementedException();
        }

        private void buildMonsters(IStage stage)
        {
            throw new NotImplementedException();
        }
    }
}
