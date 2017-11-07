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

        //private IServer server;
        private Play play = Play.NONE;
        //private bool isConnected;
        public ClientManager ClientManager { get; set; }
        public Panel PanelGame {
            get
            {
                return panelGame;
            }
        }
        public TextBox TextBoxChatHistory
        {
            get { return textBoxChatHistory; }
        }

        // to remove
        //IClient client;

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

        public FormStage(ClientManager cm) {

            InitializeComponent();
            this.ClientManager = cm;
            this.textBoxMessage.Enabled = false;

            // remove
            label2.Visible = false;
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
                this.textBoxMessage.Enabled = true;
                this.textBoxMessage.Focus();
                this.textBoxMessage.Select();
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

            if(!this.ClientManager.Connected)
            {
                retryConnection();
                return;
            }

            if(play != Play.NONE)
            {
                // este round não vai ser actualizado, porque nao é o do concrete client, devia ser do concrete client!!!!
                this.ClientManager.server.SetPlay(this.ClientManager.client.Address, play, this.ClientManager.client.Round);
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

        private void textBoxMessage_OnKeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                this.ClientManager.client.SendTextMessage(this.ClientManager.Username, this.textBoxMessage.Text);
                this.textBoxMessage.Clear(); // clear text
                this.textBoxMessage.Enabled = false;
                //this.Focus();
                this.Select();
            }
        }
    }
}
