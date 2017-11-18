using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace pacman {
    static class Program {

        private const string INSTRUCTED = "instructed";
        private const string NOT_INSTRUCTED = "not-instructed";

        [STAThread]
        static void Main(string[] args) {
            Application.SetCompatibleTextRenderingDefault(false);
            Application.EnableVisualStyles();
            try
            {
                if (args.Length > 0)
                {
                    if (args[0] == INSTRUCTED)
                    {
                        string PID = args[1];
                        Uri clientURL = new Uri(args[2]);
                        Uri serverURL = new Uri(args[3]);
                        int msecPerRound = int.Parse(args[4]);
                        int numPlayers = int.Parse(args[5]);

                        var base64EncodedBytes = System.Convert.FromBase64String(args[6]);
                        string instructions = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);

                        instructedClient(clientURL, serverURL, msecPerRound, numPlayers, instructions);
                    }
                    else if(args[0] == NOT_INSTRUCTED)
                    {
                        string PID = args[1];
                        Uri clientURL = new Uri(args[2]);
                        Uri serverURL = new Uri(args[3]);
                        int msecPerRound = int.Parse(args[4]);
                        int numPlayers = int.Parse(args[5]);

                        notInstructedClient(clientURL, serverURL, msecPerRound, numPlayers);
                    }

                }
                else if (args.Length == 0)
                {
                    defaultClient();
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

        private static void instructedClient(Uri clientURL, Uri serverURL, int msecPerRound, int numPlayers, string instructions)
        {
            Hub hub = new Hub(serverURL, clientURL);
            FormWelcome form = new FormWelcome(hub);
            Application.Run(form);

            // init connection with the server automatically
            // form.textBoxUsername.Text = PID;
            // form.textBoxClientPort.Text = uri.Port.ToString(); 
            // form.buttonJoin.PerformClick();
        }

        private static void notInstructedClient(Uri clientURL, Uri serverURL, int msecPerRound, int numPlayers)
        {
            Hub hub = new Hub(serverURL, clientURL);
            FormWelcome form = new FormWelcome(hub);
            Application.Run(form);
        }

        private static void defaultClient()
        {
            Hub hub = new Hub(new Uri("tcp://localhost:8086"), null);
            FormWelcome form = new FormWelcome(hub);
            Application.Run(form);
        }

    }
}
