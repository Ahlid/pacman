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

        public FormWelcome()
        {
            InitializeComponent();
            this.MaximizeBox = false; // disable the maximize button 
            this.labelError.Visible = false;
        }

        private void buttonJoin_Click(object sender, EventArgs e)
        {
            try
            {
                int port = Int32.Parse(textBoxClientPort.Text);
                if (!(port > -1 && port < 65536))
                {
                    labelError.Text = "Invalid port number, should be in rage [0, 65536]";
                    labelError.Visible = true;
                }
                else
                {
                    if(this.textBoxUsername.Text == null || this.textBoxUsername.Text == "") {
                        labelError.Text = "Invalid user number";
                        labelError.Visible = true;
                    }
                    else
                    {
                        labelError.Text = port.ToString();
                        string username = textBoxUsername.Text;
                        this.clientManager = new ClientManager(username);
                        this.clientManager.Port = port;
                        ConcreteClient.WelcomeForm = this;
                        ConcreteClient.ClientManager = clientManager; // :l, waiting for a better solution

                        this.clientManager.createConnectionToServer();
                    
                        this.clientManager.server.Join(username, this.clientManager.client.Address);
                    }
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
            textBoxOnFocus(this.textBoxUsername, "Username");
        }

        private void textBoxUsername_OnLostFocus(object sender, EventArgs e)
        {
            textBoxOnLostFocus(this.textBoxUsername, "Username");
        }

        private void buttonQuit_Click(object sender, EventArgs e)
        {
            disconnetFromServer();
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
