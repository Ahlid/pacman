using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using Shared;
using System.Threading;


namespace Server
{
    public class Server : MarshalByRefObject, IServer
    {

        private ServerContext context;
        private ServerStrategy strategy;


        //Common private constructor
        private Server(Uri address, string PID)
        {
            this.context = new ServerContext()
            {
                Channel = new TcpChannel(address.Port),
                PID = PID,
                Address = address
            };

            //Start services
            ChannelServices.RegisterChannel(this.context.Channel, false);
            RemotingServices.Marshal(this, "Server", typeof(Server));

        }

        //Leader constructor
        public Server(Uri address, string PID = "not set", int roundIntervalMsec = 20, int numPlayers = 3) : this(address, PID)
        {
            //This server will start as a Master
            this.context.NumPlayers = numPlayers;
            this.context.RoundIntervalMsec = roundIntervalMsec;
            this.context.CurrentGameSession = new GameSession(numPlayers);
            this.context.GameSessionsTable[this.context.CurrentGameSession.ID] = this.context.CurrentGameSession;
            this.context.RoundIntervalMsec = roundIntervalMsec;
            this.strategy = new LeaderStrategy(this.context);
        }

        //Follower constructor
        public Server(Uri address, Uri masterURL, string PID = "not set", int roundIntervalMsec = 20, int numPlayers = 3) : this(address, PID)
        {
            //This server will start as a Follower
            this.strategy = new FollowerStrategy(this.context, masterURL);
        }

        public JoinResult Join(string username, Uri address)
        {
            return ((IServer)strategy).Join(username, address);
        }

        public void SetPlay(Uri address, Play play, int round)
        {
            ((IServer)strategy).SetPlay(address, play, round);
        }

        public void Quit(Uri address)
        {
            ((IServer)strategy).Quit(address);
        }

        public void RegisterReplica(Uri ReplicaServerURL)
        {
            ((IServer)strategy).RegisterReplica(ReplicaServerURL);
        }

        public Uri GetLeader()
        {
            return ((IServer)strategy).GetLeader();
        }

        public AppendEntriesAck AppendEntries(AppendEntries appendEntries)
        {
            return ((IServer)strategy).AppendEntries(appendEntries);
        }

        public VoteResponse RequestVote(RequestVote requestVote)
        {
            return ((IServer)strategy).RequestVote(requestVote);
        }
    }

}
