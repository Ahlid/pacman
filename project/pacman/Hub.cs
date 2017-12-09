using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using Shared;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace pacman
{
    public class Hub : MarshalByRefObject, IClient, IChat
    {
        //Connection Information
        public Uri Address { get; set; }
        private const string resource = "Client";
        private TcpChannel channel;
        private Uri lastKnownLeaderUri { get; set; }
        private List<Uri> serverURIList;
        private Dictionary<Uri, IServer> servers;

        //Current Session Information
        public Session CurrentSession { get; set; } // changed to property
        public ChatRoom CurrentChatRoom { get; set; }

        // # refactor this variables..
        private int msecPerRound;
        private IGame game;


        //Interface complience
        public string Username { get { return CurrentSession.Username; } }
        public int Round { get { return CurrentSession.Round; } }
        public List<IClient> Peers { get { return CurrentChatRoom.Peers; } }
        public List<Uri> peerURIManager;
        public IServer leader = null;

        //Events

        public delegate void StartEventHandler(IStage e);
        public delegate void RoundActionsEventHandler(List<Shared.Action> actions, List<IPlayer> players, int round);
        public delegate void GameEndEvent(IPlayer e);
        public delegate void PlayerDiedEvent();
        public delegate string GetStateHandler(int round);

        public event StartEventHandler OnStart;
        public event RoundActionsEventHandler OnRoundReceived;
        public event PlayerDiedEvent OnDeath;
        public event GameEndEvent OnGameEnd;

        /// <summary>
        /// The timmer for group mannager
        /// </summary>
        private System.Timers.Timer ManagerTimer;

        public GetStateHandler getStateHandler { get; set; }

        public Hub(List<Uri> serverURIList, Uri address, IGame game)
        {
            if (serverURIList == null || serverURIList.Count == 0)
            {
                throw new Exception("The list of servers must be provided and be not empty.");
            }

            if (address == null)
            {
                address = new Uri("tcp://127.0.0.1:" + FreeTcpPort().ToString());
            }
            this.peerURIManager = new List<Uri>();
            this.game = game;
            this.lastKnownLeaderUri = null;
            this.Address = address;
            this.serverURIList = serverURIList;
            this.servers = new Dictionary<Uri, IServer>();

            CurrentSession = new Session(game, msecPerRound);

            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();

            IDictionary props = new Hashtable();
            props["port"] = address.Port;
            props["timeout"] = 500; // in milliseconds

            channel = new TcpChannel(props, null, provider);
            // channel = new TcpChannel(address.Port);
            ChannelServices.RegisterChannel(channel, false);

            RemotingServices.Marshal(this, resource,
                typeof(Hub));

            foreach (Uri peer in this.serverURIList)
            {
                IServer server = (IServer)Activator.GetObject(
                    typeof(IServer),
                    peer.ToString() + "Server");

                this.servers.Add(peer, server);
            }


            ManagerTimer = new System.Timers.Timer(2000);
            ManagerTimer.Elapsed += OnManagerCheck;
            ManagerTimer.AutoReset = false;
            ManagerTimer.Start();


        }

        public Hub(List<Uri> serverURIList) : this(serverURIList, null, new SimpleGame()) { }


        /// <summary>
        /// Event to trigger on Manager timmer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnManagerCheck(object sender, ElapsedEventArgs e)
        {

            for (int x = 0; x < this.serverURIList.Count; x++)
            {
                Uri uri = this.serverURIList[x];


                try
                {
                    this.servers[uri].Test();

                    if (!this.serverURIList.Contains(uri))
                    {

                        this.serverURIList.Add(uri);

                    }

                }
                catch (Exception ex)
                {
                    if (this.serverURIList.Contains(uri))
                    {

                        this.serverURIList.Remove(uri);

                    }
                }


            }




            this.ManagerTimer.Start();
        }

        static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        //Asks who is the leader and if there is one available return its URI
        public Uri AskWhoIsLeader(Uri serverUri)
        {
            try
            {
                IServer server = this.servers[serverUri];
                Uri leaderUri = server.GetLeader();
                if (leaderUri == null)
                {
                    //We don't know who is leader
                    return null;
                }

                if (leaderUri == serverUri)
                {
                    //The server is the leader since I asked the leader himself
                    return serverUri;
                }
                else
                {
                    //I asked another server, I need to make sure the leader is available
                    return this.AskWhoIsLeader(leaderUri); //Recursively ask if that who is the leader
                }
            }
            catch (Exception)
            {
                //We don't know who is leader
                return null;
            }
        }


        public IServer FindLeader()
        {

            if (this.leader != null)
            {
                return this.leader;
            }
            for (int x = 0; x < this.serverURIList.Count; x++)
            {
                Uri serverUri = this.serverURIList[x];

                if (serverUri == null) continue; ;
                try
                {
                    this.lastKnownLeaderUri = AskWhoIsLeader(serverUri);
                    if (this.lastKnownLeaderUri != null)
                    {
                        this.leader = this.servers[this.lastKnownLeaderUri];
                        return this.servers[this.lastKnownLeaderUri];
                    }

                }
                catch (Exception)
                {
                    //We need to continue looking for an available leader
                    continue;
                }
            }

            return null;


        }

        //Attempts to join the server using a username
        public JoinResult Join(string username)
        {
            if (CurrentSession.SessionStatus != Session.Status.PENDING)
            {
                throw new Exception("A session is already open");
            }

            CurrentSession.Username = username;

            IServer server = FindLeader(); //Block until a leader was found
            CurrentChatRoom = new ChatRoom(CurrentSession);
            JoinResult result = server.Join(username, Address); //This might still fail
            switch (result)
            {
                case JoinResult.QUEUED:
                    CurrentSession.SessionStatus = Session.Status.QUEUED;


                    this.CurrentSession.game.OnPlayHandler += () =>
                    {
                        this.SetPlay(this.CurrentSession.game.Move);
                    };

                    break;
            }

            return result;
        }

        public void SetPlay(Play play)
        {
            if (CurrentSession == null)
            {
                throw new Exception("The session hasn't started yet.");
            }

            try
            {
                IServer server = FindLeader(); //This will block until we have a leader
                server.SetPlay(Address, play, CurrentSession.Round); //This might still fail
            }
            catch (Exception ex)
            {
                this.leader = null;
                //We ignore this play since the player will have another oportunity and we have no leader available.                
            }
        }

        public void Quit()
        {
            try
            {
                if (CurrentSession != null)
                {
                    IServer server = FindLeader();
                    server.Quit(Address);
                }
            }
            catch (Exception e)
            {
                //We tried to quit, the server was not available we just ignore it since we can't do anything about it
            }
        }

        public void UnregisterChannel()
        {
            ChannelServices.UnregisterChannel(this.channel);
        }
        
        //IClient Interface 

        void IClient.Start(IStage stage)
        {
            CurrentSession.SessionStatus = Session.Status.RUNNING;
            OnStart?.Invoke(stage);
        }

        /// client receive from the server the result from the previous round and clients needs to update the game
        void IClient.SendRound(List<Shared.Action> actions, List<IPlayer> players, int round, string leader)
        {
           
                this.leader = (IServer)Activator.GetObject(
                    typeof(IServer),
                    leader + "Server");
           

            CurrentSession.Round = round;
            OnRoundReceived?.Invoke(actions, players, round);
            CurrentSession.game.Play(round);    // force player to play when is in mode auto
        }

        void IClient.Died()
        {
            MessageBox.Show("DIED");
            CurrentSession.SessionStatus = Session.Status.DIED;
            OnDeath?.Invoke();
        }

        void IClient.End(IPlayer winner)
        {
            //MessageBox.Show("END");
            CurrentSession.SessionStatus = Session.Status.ENDED;
            OnGameEnd?.Invoke(winner);
        }

        //IChatRoom

        public void ReceiveMessage(string username, IVectorMessage<IMessage> message)
        {
            if (CurrentChatRoom == null)
            {
                throw new Exception("The session hasn't started.");
            }
            CurrentChatRoom.ReceiveMessage(username, message);
        }

        public void SetPeers(Dictionary<string, Uri> peers)
        {
            if (CurrentChatRoom == null)
            {
                throw new Exception("The session hasn't started.");
            }
            CurrentChatRoom.SetPeers(peers);
        }

        public void VectorRecoveryRequest(int[] vetor, string adress)
        {
            CurrentChatRoom.VectorRecoveryRequest(vetor, adress);
        }

        public void PublishMessage(string message)
        {
            if (CurrentChatRoom == null)
            {
                throw new Exception("The session hasn't started.");
            }
            CurrentChatRoom.PublishMessage(CurrentSession.Username, message);
        }

        public string GetState(int round)
        {
            return getStateHandler.Invoke(round);
        }

        public void SetAvailableServers(List<Uri> serverURIList)
        {
            this.serverURIList = serverURIList;



            foreach (Uri peer in this.serverURIList)
            {
                this.peerURIManager.Add(peer);
                IServer server = (IServer)Activator.GetObject(
                    typeof(IServer),
                    this.lastKnownLeaderUri.ToString() + "Server");

                server.Address = peer;
                this.servers[peer] = server;
            }
        }

        public string ping()
        {
            return "pong";
        }
    }
}
