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


        static void Main(string[] args)
        {
            Console.WriteLine("***** Server Initilized *****");

            //if (!Debugger.IsAttached)
//                Debugger.Launch();
            //Debugger.Break();

            try
            {

                if (args.Length > 0)
                {
                    for (int i = 0; i < args.Count(); i++) {
                        Console.WriteLine($"i {i} arg {args[i]}");
                    }


                    string PID = args[0];
                    Uri serverURI = new Uri(args[1]);
                    int msecPerRound = int.Parse(args[2]);
                    int numPlayers = int.Parse(args[3]);

                    List<Uri> serverURLs = new List<Uri>();
                    foreach (string serverURL in args.Skip(4))
                    {
                        serverURLs.Add(new Uri(serverURL));
                    }

                    Console.WriteLine($"Starting {PID} {msecPerRound} {numPlayers}");
                    RaftServer server = new RaftServer(serverURI, numPlayers, msecPerRound);
                    server.Start(serverURLs);
                }
                else if(args.Length == 0)
                {
                    RaftServer server = new RaftServer(new Uri("tcp://127.0.0.1:30001"), 2, 20);
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
    }
}
