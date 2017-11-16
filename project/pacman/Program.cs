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
            Application.SetCompatibleTextRenderingDefault(false);
            Application.EnableVisualStyles();
            try
            {
                if (args.Length == 4)
                {
                    string PID = args[0];
                    string clientURL = args[1];
                    string msecPerRound = args[2];
                    string numPlayer = args[3];
                    var uri = new Uri(clientURL);

                    FormWelcome form = new FormWelcome(clientURL, int.Parse(msecPerRound), int.Parse(numPlayer));
                    
                    Application.Run(form);

                    // init connection with the server automatically
                    // form.textBoxUsername.Text = PID;
                    // form.textBoxClientPort.Text = uri.Port.ToString(); 
                    // form.buttonJoin.PerformClick();

                }
                else if (args.Length == 5)
                {
                    string PID = args[0];
                    string clientURL = args[1];
                    string msecPerRound = args[2];
                    string numPlayer = args[3];
                    var uri = new Uri(clientURL);
                    var base64EncodedBytes = System.Convert.FromBase64String(args[4]);
                    string instructions = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                    FormWelcome form = new FormWelcome(clientURL, int.Parse(msecPerRound), int.Parse(numPlayer), instructions);
                    Application.EnableVisualStyles();
                    
                    Application.Run(form);

                    // init connection with the server automatically
                    //form.textBoxUsername.Text = PID;
                    //form.textBoxClientPort.Text = uri.Port.ToString();
                    //form.buttonJoin.PerformClick();
                }
                else if (args.Length == 0)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new FormWelcome());
                }
                else
                {
                    throw new Exception("Invalid Arguments.");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }

    }
}
