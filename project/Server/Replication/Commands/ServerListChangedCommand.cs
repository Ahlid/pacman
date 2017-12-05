using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    [Serializable]
    class ServerListChangedCommand : ICommand
    {
        private List<Uri> newList;
        public ServerListChangedCommand(List<Uri> newList)
        {
            this.newList = newList;
        }
        public void Execute(ServerContext context)
        {
            context.OtherServersUrls = this.newList;
            context.OtherServersUrls.Remove(context.Address);
            Console.WriteLine($"SETTING SERVER LIST size {context.OtherServersUrls.Count}");

            if (context.CurrentRole == ServerContext.Role.Leader)
            {
                IEnumerable<IClient> clients = context.sessionClients == null ?
                    context.pendingClients : context.pendingClients.Concat(context.sessionClients);

                foreach (IClient client in clients)
                {
                    List<Uri> servers = new List<Uri>(context.OtherServersUrls);
                    servers.Add(context.Address);
                    client.SetAvailableServers(servers);
                }
            }
            else
            {
                context.OtherServers.Clear();
                foreach (Uri uri in context.OtherServersUrls)
                {
                    IServer replica = (IServer)Activator.GetObject(
                        typeof(IServer), uri.ToString() + "Server");
                    context.OtherServers.Add(uri, replica);
                }
                    
            }
        }
    }
}
