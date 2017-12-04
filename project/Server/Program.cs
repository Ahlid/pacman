using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Server
{
    class Program
    {
        const string MASTER_MODE = "master";
        const string REPLICA_MODE = "replica";

        static void Main(string[] args)
        {
            Console.WriteLine("***** Server Initilized *****");

            //if (!Debugger.IsAttached)
//                Debugger.Launch();
            //Debugger.Break();

            try
            { 

                if(args.Length > 0 && args[0] == MASTER_MODE)
                {
                    string PID = args[1];
                    Uri serverURL = new Uri(args[2]);
                    int msecPerRound = int.Parse(args[3]);
                    int numPlayers = int.Parse(args[4]);

                    StartMasterMode(PID, serverURL, msecPerRound, numPlayers);
                }
                else if(args.Length > 0 && args[0] == REPLICA_MODE)
                {
                    string PID = args[1];
                    Uri serverURL = new Uri(args[2]);
                    Uri replicaURL = new Uri(args[3]);
                    int msecPerRound = int.Parse(args[4]);
                    int numPlayers = int.Parse(args[5]);

                    StartReplicaMode(PID, serverURL, replicaURL, msecPerRound, numPlayers);
                }
                else if(args.Length == 0)
                {
                    StartMasterModeDefault();
                }
                else
                {
                    System.Console.WriteLine("Invalid Arguments!!");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

            System.Console.ReadLine();
        }

        private static void StartMasterModeDefault()
        {
            Server server = new Server(new Uri("tcp://127.0.0.1:30001"));
        }

        private static void StartMasterMode(string PID, Uri serverURL, int msecPerRound, int numPlayers)
        {
            Console.WriteLine($"Master Mode {PID} {serverURL} {msecPerRound} {numPlayers}");
            Server server = new Server(serverURL, PID, msecPerRound, numPlayers);
        }

        private static void StartReplicaMode(string PID, Uri serverURL, Uri replicaURL, int msecPerRound, int numPlayers)
        {
            Console.WriteLine($"Replica mode {PID} {serverURL} {replicaURL} {msecPerRound} {numPlayers}");
            Server server = new Server(replicaURL, serverURL, PID, msecPerRound, numPlayers);
        }

    }
}
