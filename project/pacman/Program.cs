using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Shared;
using Shared.Exceptions;

namespace pacman
{

    class MyContext : ApplicationContext
    {

    }

    static class Program
    {

        private const string INSTRUCTED = "instructed";
        private const string NOT_INSTRUCTED = "not-instructed";

        [STAThread]
        static void Main(string[] args)
        {
            Application.SetCompatibleTextRenderingDefault(false);
            Application.EnableVisualStyles();
            /*

                        if (!Debugger.IsAttached)
                            Debugger.Launch();
                        Debugger.Break();
                   */
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

                        instructedClient(PID, clientURL, serverURL, msecPerRound, numPlayers, instructions);
                    }
                    else if (args[0] == NOT_INSTRUCTED)
                    {
                        string PID = args[1];
                        Uri clientURL = new Uri(args[2]);
                        Uri serverURL = new Uri(args[3]);
                        int msecPerRound = int.Parse(args[4]);
                        int numPlayers = int.Parse(args[5]);

                        notInstructedClient(PID, clientURL, serverURL, msecPerRound, numPlayers);
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

        private static void instructedClient(string PID, Uri clientURL, Uri serverURL, int msecPerRound, int numPlayers, string instructions)
        {
            Hub hub = new Hub(serverURL, clientURL, msecPerRound, new AutomatedGame(instructions));
            Form form = new AutomaticStartForm();
            hub.OnStart += (stage) =>
            {
                form.Invoke(new System.Action(() =>
                {
                    form.Hide();
                    FormStage formStage = new FormStage(hub, stage);
                    formStage.Show();
                    hub.CurrentSession.game.Play(0); // force player to play when in auto mode
                }));
            };
            try
            {
                JoinResult result = hub.Join(PID);
                Application.Run(form);
            }
            catch (InvalidUsernameException exc)
            {
                MessageBox.Show(exc.Message);
                Application.Exit();
            }
        }

        private static void notInstructedClient(string PID, Uri clientURL, Uri serverURL, int msecPerRound, int numPlayers)
        {
            Hub hub = new Hub(serverURL, clientURL, msecPerRound, new SimpleGame());

            Form form = new AutomaticStartForm();
            hub.OnStart += (stage) =>
            {
                form.Invoke(new System.Action(() =>
                {
                    form.Hide();
                    FormStage formStage = new FormStage(hub, stage);
                    formStage.Show();
                }));
            };

            try
            {
                JoinResult result = hub.Join(PID);
                Application.Run(form);
            }
            catch (InvalidUsernameException exc)
            {
                MessageBox.Show(exc.Message);
                Application.Exit();
            }
        }

        private static void defaultClient()
        {
            Hub hub = new Hub(new Uri("tcp://127.0.0.1:30001"), 20);
            FormWelcome form = new FormWelcome(hub);
            Application.Run(form);
        }

    }
}
