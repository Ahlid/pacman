using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    [Serializable]
    public class StartGameCommand : ICommand
    {
        public void Execute(ServerContext context)
        {
            context.GameStartRequest = false; //request fullfilled
            Console.WriteLine("Game start commited.");
            //In the leader it might be necessary to lock 

            foreach (IClient client in context.pendingClients)
            {
                Console.WriteLine($"pending {client.Address}");
            }

            context.sessionClients = context.pendingClients.Take(context.NumPlayers).ToList(); //get the first N clients
            context.pendingClients = context.pendingClients.Skip(context.NumPlayers).ToList();

            Console.WriteLine($"Numplayers {context.NumPlayers}");

            context.HasGameStarted = true;
            foreach (IClient client in context.sessionClients)
            {
                Console.WriteLine($"Address session players {client.Address}");
                IPlayer player = new Player();
                player.Address = client.Address;
                player.Alive = true;
                player.Score = 0;
                player.Username = client.Username;
                context.playerList.Add(player);
            }

            context.stateMachine = new GameStateMachine(context.NumPlayers, context.playerList);

            if (context.CurrentRole == ServerContext.Role.Leader)
            {
                
                //Broadcast the start signal to the client
                Dictionary<string, Uri> clientsP2P = new Dictionary<string, Uri>();
                foreach (IClient c in context.sessionClients)
                {
                    clientsP2P[c.Username] = c.Address;
                }
                //Communication with the client must be done with the leader
                for (int i = context.sessionClients.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        IClient client = context.sessionClients.ElementAt(i);
                        client.Start(context.stateMachine.Stage); //Signal the start
                        client.SetPeers(clientsP2P); //Set the peers for the P2P chat
                    }
                    catch (Exception e)
                    {
                        context.sessionClients.RemoveAt(i);
                        //todo : maybe remove from the whole list of clients
                        // todo: try to reach the client again. Uma thread à parte. Verificar se faz sentido.
                    }
                }

                //Start the game timer
                context.Timer.AutoReset = false;
                context.Timer.Start();
            }
        }
   
   
    
    }
}
