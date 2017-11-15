using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace pacman
{
    public class ClientManager
    {
        public string Username { get; set; }
        public int Port { get; set; }
        public string Server { get; set; }
        public bool Connected { get; private set; }
        public bool Joined { get; set; }
        public IServer server { get; private set; }
        public IClient client { get; private set; }
        private TcpChannel channel;
        private string resource;
        private Uri uri;
        private Uri serverURL = new Uri("tcp://localhost:8086/Server");

        public ClientManager()
        {
            this.resource = "Client";
            this.Connected = false;
        }

        //todo: catch exceptions
        public void createConnectionToServer()
        {
            if (!this.Connected)
            {
                try
                {
                    //this.Port = this.server.NextAvailablePort(this.Address); // when this function is working on the server side is just to uncommment in this side.
<<<<<<< HEAD
                    channel = new TcpChannel(this.Port);
                    ChannelServices.RegisterChannel(channel, false);
=======
                    channel = new TcpChannel(uri.Port);
                    ChannelServices.RegisterChannel(channel, true);
>>>>>>> 3273d1b074278497d241b8fbca71aaa47569f403

                    RemotingConfiguration.RegisterWellKnownServiceType(
                        typeof(ConcreteClient),
                        this.resource,
                    WellKnownObjectMode.Singleton);

                    string address = string.Format("{0}/{1}", this.uri.AbsolutePath, this.resource);
                    client = (IClient)Activator.GetObject(
                        typeof(IClient),
                        address);

                    this.client.Address = address;
                    //what happens on replication
                    server = (IServer)Activator.GetObject(
                        typeof(IServer),
                        this.serverURL.AbsolutePath);

                    this.Connected = true;
                }
                catch (Exception ex)
                {
                    this.Connected = false;
                }
            }
        }
    }
}
