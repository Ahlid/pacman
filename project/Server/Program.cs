using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("***** Server Initilized *****");
            if (args.Length == 4)
            {
                ServerManager serverManager;
                string PID = args[0];
                string serverURL = args[1];
                string msecPerRound = args[2];
                string numPlayer = args[3];
                serverManager = new ServerManager(PID, serverURL);
                serverManager.CreateChannel();
                serverManager.server.Run(int.Parse(msecPerRound), int.Parse(numPlayer));
            }
            else if(args.Length == 0)
            {
                ServerManager serverManager;
                serverManager = new ServerManager();
                serverManager.CreateChannel();
                serverManager.server.Run(160, 1);
            }
            else
            {
                System.Console.WriteLine("Invalid Arguments!!");
            }

            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();
        }
    }
}
