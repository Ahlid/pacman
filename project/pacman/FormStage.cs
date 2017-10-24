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

        Play play = Play.NONE;

        private bool isConnected;

        public ClientManager clientManager { get; set; }


        // to remove
        IClient client;

        #region TO REMOVE

        // direction player is moving in. Only one will be 
        bool goup;
        bool godown;
        bool goleft;
        bool goright;

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

        #endregion


        

        public FormStage() {
            InitializeComponent();

            // to remove
            label2.Visible = false;





            // to remove

            //todo: case of two clients in the same port
            int port = 8081;
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ConcreteClient),
                "Client",
                WellKnownObjectMode.Singleton);

            client = (IClient)Activator.GetObject(
                typeof(IClient),
                "tcp://localhost:" + port + "/Client");

            client.Address = "tcp://localhost:" + port + "/Client";

            server = (IServer)Activator.GetObject(
                typeof(IServer),
                "tcp://localhost:8086/Server");

            server.Join(client.Address);
            ///MessageBox.Show("Server has joined");
            ///
            this.isConnected = true;
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
                    //tbMsg.Enabled = true; tbMsg.Focus();
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

            if(!isConnected)
            {
                retryConnection();
                return;
            }

            if(play != Play.NONE)
            {
                // este round não vai ser actualizado, porque nao é o do concrete client, devia ser do concrete client!!!!
                server.SetPlay(this.client.Address, play, this.client.Round);
            }




        }

        private void retryConnection()
        {
            throw new NotImplementedException();
        }

        private void tbMsg_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                //tbChat.Text += "\r\n" + tbMsg.Text; tbMsg.Clear(); tbMsg.Enabled = false; this.Focus();
            }
        }
    }
}
