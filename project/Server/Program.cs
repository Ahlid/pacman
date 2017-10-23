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

            int port = 8086;
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);

            RemotingConfiguration.RegisterWellKnownServiceType(
               typeof(ConcreteServer),
               "Server",
               WellKnownObjectMode.Singleton);

            IServer server = (IServer)Activator.GetObject(
                typeof(IServer),
                "tcp://localhost:8086/Server");

            server.Run(2000);

            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();
        }
    }
}
