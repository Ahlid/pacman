using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace Server
{
    public class ServerManager
    {

        public int Port { get; set; }
        public string Address { get; set; }
        private TcpChannel channel;
        private string resource;
        public IServer server { get; private set; }

        public ServerManager(int port)
        {
            this.Port = port;
            this.resource = "Server";
            this.Address = "tcp://localhost:" + port + "/" + this.resource;
        }

        public void CreateChannel()
        {
            this.channel = new TcpChannel(this.Port);
            ChannelServices.RegisterChannel(channel, false);

            RemotingConfiguration.RegisterWellKnownServiceType(
               typeof(ConcreteServer),
               this.resource,
               WellKnownObjectMode.Singleton);

            this.server = (IServer)Activator.GetObject(
                typeof(IServer),
                this.Address);
        }
    }
}
