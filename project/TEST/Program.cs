using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pacman;
using Server;
using Shared;

namespace TEST
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerContext context = new ServerContext();
            ServerContext context2 = new ServerContext();
            ServerContext context3 = new ServerContext();
            context.hasRegistered = true;
            context2.hasRegistered = true;
            context3.hasRegistered = true;


            Uri uri1 = new Uri("tcp://127.0.0.1:50006");
            Uri uri2 = new Uri("tcp://127.0.0.1:50007");
            Uri uri3 = new Uri("tcp://127.0.0.1:50008");

 

            Server.Server serverA = new Server.Server(uri1,new FollowerStrategy(context, new Uri("tcp://127.0.0.1:50000")),context);
            Server.Server serverB = new Server.Server(uri2, new FollowerStrategy(context2, new Uri("tcp://127.0.0.1:50000")),context2);
            Server.Server serverC = new Server.Server(uri3, new FollowerStrategy(context3, new Uri("tcp://127.0.0.1:50000")), context3);


            IServer server1 = (IServer)Activator.GetObject(
             typeof(IServer), uri1.ToString() + "Server");


            IServer server2 = (IServer)Activator.GetObject(
                        typeof(IServer), uri2.ToString() + "Server");


            IServer server3 = (IServer)Activator.GetObject(
                        typeof(IServer), uri3.ToString() + "Server");

            context.OtherServersUrls.Add(uri2);
            context.OtherServersUrls.Add(uri3);

            context2.OtherServersUrls.Add(uri1);
            context2.OtherServersUrls.Add(uri3);

            context3.OtherServersUrls.Add(uri1);
            context3.OtherServersUrls.Add(uri2);

            context.OtherServers.Add(uri2, server2);
            context.OtherServers.Add(uri3, server3);

            context2.OtherServers.Add(uri1, server1);
            context2.OtherServers.Add(uri3, server3);

            context3.OtherServers.Add(uri1, server1);
            context3.OtherServers.Add(uri2, server2);

            Console.ReadLine();
        }
    }
}
