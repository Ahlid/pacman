using System;
using Shared;

namespace Server.RaftCommands
{
    [Serializable]
    public class JoinCommand : RaftCommand
    {

        private Uri address;
        public JoinCommand(Uri address)
        {
            this.address = address;
        }

        public override void Execute(RaftServer server, bool AsLeader)
        {
            Console.WriteLine("Join Commited.");
            IClient client = (IClient)Activator.GetObject(
                typeof(IClient),
                address.ToString() + "Client");

            server.pendingClients.Add(client);
            Console.WriteLine("ADDDED NOW PENDING: " + server.pendingClients.Count);

            if (AsLeader)
            {
                Console.WriteLine("AS LEADER");

                //Have enough players
                if (!server.HasGameStarted && !server.GameStartRequest &&
                    server.pendingClients.Count >= server.NumPlayers)
                {
                    server.GameStartRequest = true; // this is used to make sure no more startgame logs are created before the startgame entry is commited

                    //Make start log

                    StartCommand startCommand = new StartCommand()
                    {
                        Name = "Start"
                    };

                    bool accepted;
                    int commitedAt;
                    server.OnCommand(startCommand, out accepted, out commitedAt);

                    /*
                    if (server.peerURIs.Count == 1)
                    {
                        //I'm the only one, I can commit everything
                        server.commitIndex = server.log.Count - 1;
                        Task.Run(() => server.StateMachine(server.log[server.commitIndex].Command));
                    }*/

                    //Task.Run(() => server.OnHeartbeatTimerOrSendTrigger());

                }
            }
        }
    }

}