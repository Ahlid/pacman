using Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pacman
{
    public partial class AutomaticStartForm : Form
    {
        public AutomaticStartForm(Hub hub, string PID)
        {
            InitializeComponent();

            hub.Join(PID);
        }
    }
}
