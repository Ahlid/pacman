using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Shared;

namespace pacman
{
    public partial class FormEndGame : Form
    {
        public delegate void PlayAgainHandler(string PID, Uri clientURL, List<Uri> serverURLs);
        public delegate void PlayAgainInstructableHandler(string PID, Uri clientURL, List<Uri> serverURLs, string instructions);

        public event PlayAgainHandler OnPlayAgain;
        public event PlayAgainInstructableHandler OnPlayAgainInstructable;


        public string PID { get; set; }
        public Uri clientURL { get; set; }
        public List<Uri> serverURLs { get; set; }
        public string instructions { get; set; }

        public FormEndGame(IPlayer winner)
        {
            InitializeComponent();
            this.textBoxWinner.Text = winner.Username;
            this.textBoxScore.Text = winner.Score.ToString();
        }

        private void buttonLeave_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /* Not working because channels aren't being unregistered successfully
        private void buttonYes_Click(object sender, EventArgs e)
        {
            if (OnPlayAgain != null)
            {
                OnPlayAgain(PID, clientURL, serverURLs);
            }
            if (OnPlayAgainInstructable != null)
            {
                OnPlayAgainInstructable(PID, clientURL, serverURLs, instructions);
            }
        }
        */
    }
}
