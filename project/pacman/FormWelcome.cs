using Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Shared.Exceptions;

namespace pacman
{
    /// <summary>
    /// Description of the window responsible to handle the client joining to the server 
    /// </summary>
    public partial class FormWelcome : Form
    {
        private Hub hub;

        public FormWelcome(Hub hub)
        {
            InitializeComponent();
            this.MaximizeBox = false; // disable the maximize button 
            this.labelError.Visible = false;
            this.textBoxUsername.Select();
            this.hub = hub;

            hub.OnStart += (stage) =>
            {
                Invoke(new System.Action(() =>
                {
                    // TODO: remove this handler of the OnStart event
                    Hide();
                    FormStage formStage = new FormStage(hub, stage);
                    formStage.Show();
                }));
            };

        }

        private void joinClick(object sender, EventArgs e)
        {
            if (this.textBoxUsername.Text == null || this.textBoxUsername.Text.Trim() == "")
            {
                labelError.Text = "Invalid user number";
                labelError.Visible = true;
                return;
            }

            this.labelError.Visible = false;
            string username = textBoxUsername.Text.Trim();

            // todo: should this call be async?
            //     should if we want to do something with the gui, like move it around, 
            //     otherwise there is not much to do at the same time
            try
            {
                JoinResult result = hub.Join(username);
                switch (result)
                {
                    case JoinResult.QUEUED:
                        this.labelError.Visible = true;
                        this.labelError.Text = "Waiting for the server to start the game session..";
                        this.buttonJoin.Enabled = false;
                        this.textBoxUsername.Enabled = false;   // can't change username after joining 
                        break;
                }
            }
            catch (InvalidUsernameException exc)
            {
                this.labelError.Visible = true;
                this.labelError.Text = exc.Message;
                this.textBoxUsername.Clear();   // clear text
            }
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
            this.Close();   // this will trigger the event form on close
            Application.Exit();
        }

        private void formWelcome_OnClose(object sender, FormClosedEventArgs e)
        {
            disconnectFromServer();
            Application.Exit();
        }
       
        private void disconnectFromServer()
        {
            hub.Quit();
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
