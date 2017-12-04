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
            context.ReplicaServersURIsList = this.newList;

            if(context.CurrentRole == ServerContext.Role.Leader)
            {
                IEnumerable<IClient> clients = context.sessionClients == null ?
                    context.pendingClients : context.pendingClients.Concat(context.sessionClients);

                //Executes after committing
                foreach (IClient client in clients)
                {
                    List<Uri> servers = new List<Uri>(context.ReplicaServersURIsList);
                    servers.Add(context.Address);
                    client.SetAvailableServers(servers);
                }
            }
        }
    }
}
