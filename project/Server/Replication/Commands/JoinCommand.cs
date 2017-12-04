using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    [Serializable]
    public class JoinCommand : ICommand
    {
        private Uri address;

        public JoinCommand(Uri address)
        {
            this.address = address;
        }

        public void Execute(ServerContext context)
        {
            Console.WriteLine("Join Commited.");
            IClient client = (IClient)Activator.GetObject(
                typeof(IClient),
                address.ToString() + "Client");

            context.pendingClients.Add(client);
            Console.WriteLine("ADDDED NOW PENDING: " + context.pendingClients.Count);

            if (context.CurrentRole == ServerContext.Role.Leader)
            {
                //Have enough players
                if (!context.HasGameStarted && !context.GameStartRequest &&
                    context.pendingClients.Count >= context.NumPlayers)
                {
                    context.GameStartRequest = true; // this is used to make sure no more startgame logs are created before the startgame entry is commited
                
                    ICommand startCommand = new StartGameCommand();
                    LogEntry startEntry = new LogEntry()
                    {
                        Command = startCommand,
                        Index = context.Logs.Count,
                        Term = context.CurrentTerm
                    };
                    context.Logs.Add(startEntry);
                }
            }
        }
    }
}
