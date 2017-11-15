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
<<<<<<< HEAD
            ServerManager serverManager = new ServerManager(8086);
            serverManager.LoadConfigs(@"../../config.cfg");
            serverManager.CreateChannel();
            serverManager.server.Run(160);
=======
            
            if (args.Length > 0)
            {
                ServerManager serverManager;
                string PID = args[0];
                string serverURL = args[1];
                string msecPerRound = args[2];
                string numPlayer = args[3];
                serverManager = new ServerManager(serverURL);
                serverManager.createChannel();
                serverManager.server.Run(int.Parse(msecPerRound), int.Parse(numPlayer));
            }
            else
            {
                ServerManager serverManager;
                serverManager = new ServerManager();
                serverManager.createChannel();
                serverManager.server.Run(160);
            }

>>>>>>> 3273d1b074278497d241b8fbca71aaa47569f403
            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();
        }
    }
}
