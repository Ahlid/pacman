using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared;

namespace Server.RaftCommands
{
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
                foreach (IClient client in server.sessionClients)
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            client.End(server.stateMachine.GetTopPlayer());
                        }
                        catch (Exception ee)
                        {

                        }
                        ;
                    });
                }

            lock (server)
            {

                server.sessionClients = new List<IClient>();

                if (AsLeader)
                {
                    if (!server.HasGameStarted && !server.GameStartRequest &&
                        server.pendingClients.Count >= server.NumPlayers)
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