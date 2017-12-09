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
                        List<Uri> serverURLs = new List<Uri>();
                        foreach(string serverURL in args.Skip(4))
                        {
                            serverURLs.Add(new Uri(serverURL));
                        }

                        var base64EncodedBytes = System.Convert.FromBase64String(args[3]);
                        string instructions = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);

                        instructedClient(PID, clientURL, serverURLs, instructions);
                    }
                    else if (args[0] == NOT_INSTRUCTED)
                    {
                        string PID = args[1];
                        Uri clientURL = new Uri(args[2]);
                        List<Uri> serverURLs = new List<Uri>();
                        foreach (string serverURL in args.Skip(3))
                        {
                            serverURLs.Add(new Uri(serverURL));
                        }

                        notInstructedClient(PID, clientURL, serverURLs);
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
                MessageBox.Show(ex.ToString());
            }
        }

        private static void instructedClient(string PID, Uri clientURL, List<Uri> serverURLs, string instructions)
        {
            Hub hub = new Hub(serverURLs, clientURL, new AutomatedGame(instructions));
            Form form = new AutomaticStartForm(hub, PID);
            FormStage formStage = null;
            hub.OnStart += (stage) =>
            {
                form.Invoke(new System.Action(() => {
                    form.Hide();
                    formStage = new FormStage(hub, stage);
                    formStage.Show();
                    hub.CurrentSession.game.Play(0);
                }));
            };

            hub.OnDeath += () =>
            {
                form.Invoke(new System.Action(() =>
                {
                    FormDead f = new FormDead();
                    f.Show();
                }));
            };
            hub.OnGameEnd += (winner) =>
            {
                hub.UnregisterChannel(); //unrigester current channel before creating a new one
                form.Invoke(new System.Action(() =>
                {
                    formStage.Hide();
                    FormEndGame endGame = new FormEndGame(winner);
                    endGame.PID = PID;
                    endGame.clientURL = clientURL;
                    endGame.serverURLs = serverURLs;
                    endGame.instructions = instructions;
                    endGame.Show();
                    //quando um botao for clicado este método tem de ser executado.
                    endGame.OnPlayAgainInstructable += (_PID, _clientURL, _serverURLs, _instructions) =>
                    {
                        instructedClient(_PID, _clientURL, _serverURLs, _instructions);
                    };
                }));
            };
            try
            {
  
                Application.Run(form);
            }
            catch (InvalidUsernameException exc)
            {
                MessageBox.Show(exc.Message);
                Application.Exit();
            }
        }

        private static void notInstructedClient(string PID, Uri clientURL, List<Uri> serverURLs)
        {
            Hub hub = new Hub(serverURLs, clientURL, new SimpleGame());
            Form form = new AutomaticStartForm(hub, PID);
            FormStage formStage = null;
            hub.OnStart += (stage) =>
            {
                form.Invoke(new System.Action(() => {
                    form.Hide();
                    formStage = new FormStage(hub, stage);
                    formStage.Show();
                }));
            };

            hub.OnDeath += () =>
            {
                form.Invoke(new System.Action(() =>
                {
                    FormDead f = new FormDead();
                    f.Show();
                }));
            };

            hub.OnGameEnd += (winner) =>
            {
                hub.UnregisterChannel(); //unrigester current channel before creating a new one
                form.Invoke(new System.Action(() =>
                {
                    //MessageBox.Show("END");
                    formStage.Hide();
                    FormEndGame endGame = new FormEndGame(winner);
                    endGame.PID = PID;
                    endGame.clientURL = clientURL;
                    endGame.serverURLs = serverURLs;
                    endGame.Show();
                    //quando um botao for clicado este método tem de ser executado.
                    endGame.OnPlayAgain += (_PID, _clientURL, _serverURLs) =>
                    {
                        notInstructedClient(_PID, _clientURL, _serverURLs);
                    };
                }));
            };

            try
            {
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
            List<Uri> serverURIs = new List<Uri>();
            serverURIs.Add(new Uri("tcp://127.0.0.1:30001"));
            Hub hub = new Hub(serverURIs);
            FormWelcome form = new FormWelcome(hub);
            Application.Run(form);
        }

    }
}
