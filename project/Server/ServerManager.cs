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

        public ServerManager(string address = "tcp://localhost:8086/")
        {
            uri = new Uri(address);
        }

        public void CreateChannel()
        {
<<<<<<< HEAD
            this.channel = new TcpChannel(this.Port);
            ChannelServices.RegisterChannel(channel, false);
=======
            this.channel = new TcpChannel(uri.Port);
            ChannelServices.RegisterChannel(channel, true);
>>>>>>> 3273d1b074278497d241b8fbca71aaa47569f403

            RemotingConfiguration.RegisterWellKnownServiceType(
               typeof(ConcreteServer),
               "Server",
               WellKnownObjectMode.Singleton);

            this.server = (IServer)Activator.GetObject(
                typeof(IServer),
                uri.AbsolutePath + "Server");

        }
    }
}
