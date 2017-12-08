using System;
using System.Collections.Generic;
using System.Linq;
using Shared;

namespace Server.RaftCommands
{

    [Serializable]
    public class StartCommand : RaftCommand
    {
        public override void Execute(RaftServer server, bool AsLeader)
        {
            server.GameStartRequest = false; //request fullfilled
            Console.WriteLine("Game start commited. AsLeader: " + AsLeader);

            //In the leader it might be necessary to lock 
            try
            {
                foreach (IClient client2 in server.pendingClients)
                {
                    Console.WriteLine($"pending {client2.Address}");
                }

                server.sessionClients = server.pendingClients.Take(server.NumPlayers).ToList(); //get the first N clients
                server.pendingClients = server.pendingClients.Skip(server.NumPlayers).ToList();
                server.playerList = new List<IPlayer>();
                Console.WriteLine($"Numplayers {server.NumPlayers}");
                //Console.WriteLine(System.Environment.StackTrace);
                server.HasGameStarted = true;
                foreach (IClient client2 in server.sessionClients)
                {
                    Console.WriteLine($"Address session players {client2.Address}");
                    IPlayer player = new Player();
                    player.Address = client2.Address;
                    player.Alive = true;
                    player.Score = 0;
                    player.Username = client2.Username;
                    server.playerList.Add(player);
                }
                Console.WriteLine("Creating State Machine");
                server.stateMachine = new GameStateMachine(server.NumPlayers, server.playerList);

                if (AsLeader)
                {
                    Console.WriteLine("Contacting Clients");
                    //Broadcast the start signal to the client
                    Dictionary<string, Uri> clientsP2P = new Dictionary<string, Uri>();
                    foreach (IClient c in server.sessionClients)
                    {
                        clientsP2P[c.Username] = c.Address;
                    }


                    //Communication with the client must be done with the leader
                    for (int i = server.sessionClients.Count - 1; i >= 0; i--)
                    {
                        try
                        {
                            IClient client2 = server.sessionClients.ElementAt(i);
                            client2.Start(server.stateMachine.Stage); //Signal the start
                            client2.SetPeers(clientsP2P); //Set the peers for the P2P chat
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            //server.sessionClients.RemoveAt(i);
                            //todo : maybe remove from the whole list of clients
                            // todo: try to reach the client again. Uma thread à parte. Verificar se faz sentido.
                        }
                    }

                    //Start the game timer
                    server.RoundTimer.AutoReset = false;
                    server.RoundTimer.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}