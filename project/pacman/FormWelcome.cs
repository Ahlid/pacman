using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pacman
{
    /// <summary>
    /// Description of the window responsible to handle the client joining to the server 
    /// </summary>
    public partial class FormWelcome : Form
    {
        private ClientManager clientManager;
        public Label LabelError { get { return this.labelError; } set { this.labelError = value; } }

        public FormWelcome(string clientURL, int msecPerRound, int numPlayer)
        {
            InitializeComponent();
            this.MaximizeBox = false; // disable the maximize button 
            this.labelError.Visible = false;
            this.textBoxUsername.Select();

            Uri uri = new Uri(clientURL);
            textBoxClientPort.Text = uri.Port.ToString();
            textBoxClientPort.ReadOnly = true;
            this.clientManager = new ClientManager();
        }

        public FormWelcome()
        {
            InitializeComponent();
            this.MaximizeBox = false; // disable the maximize button 
            this.labelError.Visible = false;
            this.textBoxUsername.Select();
            this.clientManager = new ClientManager();
        }

        private void createManager()
        {

        }


        private bool validateForm()
        {
            int port = Int32.Parse(textBoxClientPort.Text);
            if (!(port > -1 && port < 65536))
            {
                labelError.Text = "Invalid port number, should be in rage [0, 65536]";
                labelError.Visible = true;
                return false;
            }

            if (this.textBoxUsername.Text == null || this.textBoxUsername.Text == "")
            {
                labelError.Text = "Invalid user number";
                labelError.Visible = true;
                return false;
            }

            return true;
        }

        private void buttonJoin_Click(object sender, EventArgs e)
        {
            
            try
            {
                if(!validateForm())
                {
                    return;
                }

                int port = int.Parse(textBoxClientPort.Text);
                this.labelError.Visible = false;
                string username = textBoxUsername.Text.Trim();
                this.clientManager.Port = port;
                ConcreteClient.WelcomeForm = this;
                ConcreteClient.ClientManager = clientManager; // :l, waiting for a better solution
                
                //Connect to the server
                this.clientManager.createConnectionToServer();

                //If it connected
                if (this.clientManager.Connected)
                {
                    this.textBoxClientPort.Enabled = false; // user no longer can update the port 
                }
                

                clientManager.Username = username;
                //this.clientManager.Port = port;

                // todo: should this call be async?
                bool result = this.clientManager.server.Join(username, this.clientManager.client.Address);
                if(result)
                {
                    this.clientManager.Joined = true;
                }
                else
                {
                    this.labelError.Visible = true;
                    this.labelError.Text = "Username already in use";
                }
                
                
            }
            catch (FormatException)
            {
                labelError.Visible = true;
                labelError.Text = "Port value should contain only numbers.";
            }
        }

        private void textBoxClientPort_OnFocus(object sender, EventArgs e)
        {
            this.labelError.Visible = false;
            textBoxOnFocus(this.textBoxClientPort, "Client Port");
        }

        private void textBoxClientPort_OnLostFocus(object sender, EventArgs e)
        {
            textBoxOnLostFocus(this.textBoxClientPort, "Client Port");
        }

        private void textBoxUsername_OnFocus(object sender, EventArgs e)
        {
            this.labelError.Visible = false;
            textBoxOnFocus(this.textBoxUsername, "Username");
        }

        private void textBoxUsername_OnLostFocus(object sender, EventArgs e)
        {
            textBoxOnLostFocus(this.textBoxUsername, "Username");
        }

        private void buttonQuit_Click(object sender, EventArgs e)
        {
            disconnetFromServer();
            this.Close();
        }

        private void formWelcome_OnClosing(object sender, FormClosingEventArgs e)
        {
            disconnetFromServer();
        }

        // todo: Adicionar este código a um delegate e depois chamar quando fechar a janela do jogo directamente.
        private void disconnetFromServer()
        {
            if (this.clientManager != null && this.clientManager.Connected)
            {
                // if already exists a connection to a server
                clientManager.server.Quit(this.clientManager.client.Address);
            }
        }

        private void textBoxOnFocus(TextBox tb, string defaultText)
        {
            if (tb.Text == defaultText)
            {
                tb.Text = "";
                tb.ForeColor = Color.Black;
            }
        }

        private void textBoxOnLostFocus(TextBox tb, string defaultText)
        {
            if (tb.Text == "")
            {
                tb.Text = defaultText;
                tb.ForeColor = SystemColors.InactiveCaption;
            }
        }

    }
}
