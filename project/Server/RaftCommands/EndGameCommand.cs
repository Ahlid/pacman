using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared;

namespace Server.RaftCommands
{
    /// <summary>
    /// The command to end the game
    /// </summary>
    [Serializable]
    public class EndGameCommand : RaftCommand
    {
        public override async void Execute(RaftServer server, bool AsLeader)
        {
            lock (server)
            {
                server.HasGameStarted = false;
                server.GameStartRequest = false;
                //send end to all clients in game
            }

            //sending info to clients
            if (AsLeader)
                foreach (IClient client in server.SessionClients)
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            client.End(server.StateMachine.GetTopPlayer());
                        }
                        catch (Exception ee)
                        {

                        }
                        ;
                    });
                }

            lock (server)
            {

                server.SessionClients = new List<IClient>();

                if (AsLeader)
                {
                    if (!server.HasGameStarted && !server.GameStartRequest &&
                        server.PendingClients.Count >= server.NumPlayers)
                    {
                        StartCommand startCommand = new StartCommand()
                        {
                            Name = "Start"
                        };

                        bool response;
                        int commitAt;

                        server.OnCommand(startCommand, out response, out commitAt);
                    }

                }

            }
        }
    }
}