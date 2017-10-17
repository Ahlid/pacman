using Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace pacman {
    public partial class FormStage : Form {

        private IServer server;

        // direction player is moving in. Only one will be 
        bool goup;
        bool godown;
        bool goleft;
        bool goright;

        Play play = Play.NONE;

        int boardRight = 320;
        int boardBottom = 320;
        int boardLeft = 0;
        int boardTop = 40;
        //player speed
        int speed = 5;

        int score = 0; int total_coins = 61;

        //ghost speed for the one direction ghosts
        int ghost1 = 5;
        int ghost2 = 5;
        
        //x and y directions for the bi-direccional pink ghost
        int ghost3x = 5;
        int ghost3y = 5;            

        public FormStage() {
            InitializeComponent();
            label2.Visible = false;

            //todo: case of two clients in the same port
            int port = 8081;
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ConcreteClient),
                "Client",
                WellKnownObjectMode.Singleton);

            IClient client = (IClient)Activator.GetObject(
                typeof(IClient),
                "tcp://localhost:" + port + "/Client");

            client.Address = "tcp://localhost:" + port + "/Client";

            server = (IServer)Activator.GetObject(
                typeof(IServer),
                "tcp://localhost:8086/Server");

            server.join(client.Address);
            ///MessageBox.Show("Server has joined");

        }

        private void keyisdown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Left) {
                play = Play.LEFT;
                goleft = true;
                pacman.Image = Properties.Resources.Left; //Change image to pacman looking left 
            }
            if (e.KeyCode == Keys.Right) {
                goright = true;
                play = Play.RIGHT;
                pacman.Image = Properties.Resources.Right;
            }
            if (e.KeyCode == Keys.Up) {
                goup = true;
                play = Play.UP;
                pacman.Image = Properties.Resources.Up;
            }
            if (e.KeyCode == Keys.Down) {
                godown = true;
                play = Play.DOWN;
                pacman.Image = Properties.Resources.down;
            }
            if (e.KeyCode == Keys.Enter) {
                    tbMsg.Enabled = true; tbMsg.Focus();
               }
        }

        private void keyisup(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Left) {
                goleft = false;
                play = Play.NONE;
            }
            if (e.KeyCode == Keys.Right) {
                goright = false;
                play = Play.NONE;
            }
            if (e.KeyCode == Keys.Up) {
                goup = false;
                play = Play.NONE;
            }
            if (e.KeyCode == Keys.Down) {
                godown = false;
                play = Play.NONE;
            }
        }

        //Running every 20 ms
        private void timer1_Tick(object sender, EventArgs e) {
            label1.Text = "Score: " + score;

            if(play != Play.NONE)
            {
                server.setPlay(play, 4);
            }


            /*
             * 
             * 
             * 
            //move player
            if (goleft) {
                if (pacman.Left > (boardLeft))
                    pacman.Left -= speed;
            }
            if (goright) {
                if (pacman.Left < (boardRight))
                pacman.Left += speed;
            }
            if (goup) {
                if (pacman.Top > (boardTop))
                    pacman.Top -= speed;
            }
            if (godown) {
                if (pacman.Top < (boardBottom))
                    pacman.Top += speed;
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

        private void tbMsg_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                tbChat.Text += "\r\n" + tbMsg.Text; tbMsg.Clear(); tbMsg.Enabled = false; this.Focus();
            }
        }
    }
}
