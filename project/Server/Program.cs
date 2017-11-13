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
            ServerManager serverManager = new ServerManager(8086);
            serverManager.createChannel();
            serverManager.server.Run(160);
            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();
        }
    }
}
