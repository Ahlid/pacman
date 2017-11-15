    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace pacman {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {

            if (args.Length > 0)
            {
                string PID = args[0];
                string clientURL = args[1];
                string msecPerRound = args[2];
                string numPlayer = args[3];
                FormWelcome form = new FormWelcome(clientURL, int.Parse(msecPerRound), int.Parse(numPlayer));
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(form);
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FormWelcome());
            }
            
        }
    }
}
