using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using Shared;
using System.Security.Policy;

namespace Server
{
    public class ServerManager
    {
        private TcpChannel channel;
        public IServer server { get; private set; }
        private Uri uri;
        private string PID;

        public ServerManager(string PID = "not set", string address = "tcp://localhost:8086/")
        {
            this.PID = PID;
            uri = new Uri(address);
        }

        public void CreateChannel()
        {

            this.channel = new TcpChannel(uri.Port);
            ChannelServices.RegisterChannel(channel, true);

            RemotingConfiguration.RegisterWellKnownServiceType(
               typeof(ConcreteServer),
               "Server",
               WellKnownObjectMode.Singleton);

            this.server = (IServer)Activator.GetObject(
                typeof(IServer),
                uri.AbsoluteUri + "Server");

        }
    }
}
