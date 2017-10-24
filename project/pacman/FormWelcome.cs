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
    public partial class FormWelcome : Form
    {
        public ClientManager clientManager;

        public FormWelcome()
        {
            InitializeComponent();
        }

        private void buttonJoin_Click(object sender, EventArgs e)
        {
            this.Hide();
            var formStage = new FormStage();
            formStage.Closed += (s, args) => this.Close();
            formStage.Show();
        }
    }
}
