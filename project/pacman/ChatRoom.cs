using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pacman
{
    public class Message : EventArgs
    {
        public string Username { get; private set; }
        public string Content { get; private set; }

        public Message(string username, string message)
        {
            Username = username;
            Content = message;
        }
    }

    public class ChatRoom : IChat
    {
        public List<IClient> Peers { get; private set; }
        private List<Message> messagesReceived;
        public event EventHandler OnMessageReceived;
        public delegate void RemovedPeerHandler(IClient e);
        public event RemovedPeerHandler OnPeerRemoved;
        private Session session;

        public ChatRoom(Session session)
        {
            if(session == null)
            {
                throw new Exception("The session must not be null");
            }
            this.session = session;
            Peers = new List<IClient>();
            messagesReceived = new List<Message>();
        }

        //Remoting
        public void SendMessage(string username, string message)
        {
            Message newMessage = new Message(username, message);
            messagesReceived.Add(newMessage);
            OnMessageReceived?.Invoke(this, newMessage);
        }

        //Remoting
        public void SetPeers(Dictionary<string, Uri> peers)
        {
            foreach (string key in peers.Keys)
            {
                Uri address = peers[key];
                IClient client = (IClient)Activator.GetObject(
                    typeof(IClient),
                    address.ToString());

                Peers.Add(client);
            }
        }


        public void PublishMessage(string username, string message)
        {
            Message newMessage = new Message(username, message);
            messagesReceived.Add(newMessage);

            for (int i = Peers.Count; i >= 0; i--)
            {
                try
                {
                    IClient client = Peers[i];
                    client.SendMessage(username, message);
                }
                catch (Exception)
                {
                    IClient removedClient = Peers[i];
                    Peers.RemoveAt(i);
                    OnPeerRemoved?.Invoke(removedClient);
                }
            }
        }

    }
}
