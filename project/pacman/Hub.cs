using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using Shared;
using System.Net.Sockets;
using System.Net;

namespace pacman
{
    public class Hub: MarshalByRefObject, IClient
    {
        //Connection Information
        public Uri Address { get; set; }
        private const string resource = "Client";
        private TcpChannel channel;
        private Uri serverURL { get; set; }
        private IServer server { get; set; }

        //Current Session Information
        private Session currentSession;
        private ChatRoom currentChatRoom;

        //Interface complience
        string IClient.Username { get { return currentSession.Username; } }
        int IClient.Round { get { return currentSession.Round; } }
        List<IClient> Peers { get { return currentChatRoom.Peers; }  }

        //Events

        public delegate void StartEventHandler(IStage e);
        public delegate void RoundActionsEventHandler(List<Shared.Action> actions, int score, int round);
        public delegate void GameEndEvent(IPlayer e);
        public delegate string GetStateHandler(int round);

        public event StartEventHandler OnStart;
        public event RoundActionsEventHandler OnRoundReceived;
        public event EventHandler OnDeath;
        public event GameEndEvent OnGameEnd;

        public GetStateHandler getStateHandler { get; set; }

        public Hub(Uri serverURL, Uri address)
        {
            if(serverURL == null)
            {
                throw new Exception("The serverURL must be provided.");
            }

            if(address == null)
            {
                address = new Uri("tcp://localhost:"+ FreeTcpPort().ToString());
            }

            this.serverURL = serverURL;
            Address = address;

            channel = new TcpChannel(address.Port);
            ChannelServices.RegisterChannel(channel, false);

            RemotingServices.Marshal(this, resource,
                typeof(Hub));

            server = (IServer)Activator.GetObject(
                typeof(IServer),
                this.serverURL.ToString() + "Server");
        }

        public Hub(Uri serverURL) : this(serverURL, null) {}

        static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }


        //Attempts to join the server using a username
        public JoinResult Join(string username)
        {
            if(currentSession != null)
            {
                throw new Exception("A session is already open");
            }

            JoinResult result = server.Join(username, Address);
            switch (result)
            {
                case JoinResult.QUEUED:
                    currentSession = new Session(username, Session.Status.QUEUED);
                    currentSession.SessionStatus = Session.Status.QUEUED;
                    currentChatRoom = new ChatRoom(currentSession);
                    break;
            }
            
            return result;
        }

        public void SetPlay(Play play)
        {
            if(currentSession == null)
            {
                throw new Exception("The session hasn't started yet.");
            }
            server.SetPlay(Address, play, currentSession.Round);
        }

        public void Quit()
        {
            if (currentSession == null)
            {
                throw new Exception("The session hasn't started yet.");
            }
            server.Quit(Address);
        }


        //IClient Interface 

        void IClient.Start(IStage stage)
        {
            currentSession.SessionStatus = Session.Status.RUNNING;
            OnStart?.Invoke(stage);
        }

        void IClient.SendRound(List<Shared.Action> actions, int score, int round)
        {
            currentSession.Round = round;
            OnRoundReceived?.Invoke(actions, score, round);
        }

        void IClient.Died()
        {
            currentSession.SessionStatus = Session.Status.DIED;
            OnDeath?.Invoke(this, null);
        }

        void IClient.End(IPlayer winner)
        {
            currentSession.SessionStatus = Session.Status.ENDED;
            OnGameEnd?.Invoke(winner);
        }


        //IChatRoom

        public void SendMessage(string username, string message)
        {
            if (currentChatRoom == null)
            {
                throw new Exception("The session hasn't started.");
            }
            currentChatRoom.SendMessage(username, message);
            //todo tie events to the form
        }

        public void SetPeers(Dictionary<string, Uri> peers)
        {
            if (currentChatRoom == null)
            {
                throw new Exception("The session hasn't started.");
            }
            currentChatRoom.SetPeers(peers);
        }

        public void PublishMessage(string message)
        {
            if (currentChatRoom == null)
            {
                throw new Exception("The session hasn't started.");
            }
            currentChatRoom.PublishMessage(currentSession.Username, message);
        }

        public string GetState(int round)
        {
            return getStateHandler.Invoke(round);
        }
    }
}
