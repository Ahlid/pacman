using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pacman
{
    [Serializable]
    public class Message : EventArgs, IMessage
    {

        public string Username { get; set; }
        public string Content { get; set; }

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
        public event MessageReceivedHandler OnMessageReceived;
        public delegate void RemovedPeerHandler(IClient e);
        public event RemovedPeerHandler OnPeerRemoved;
        private Session session;
        private VetorClockManager vetorClockManager;

        public ChatRoom(Session session)
        {
            if (session == null)
            {
                throw new Exception("The session must not be null");
            }
            this.session = session;
            Peers = new List<IClient>();
            messagesReceived = new List<Message>();
        }



        //Remoting
        public void ReceiveMessage(string username, IVectorMessage<IMessage> message)
        {

            this.vetorClockManager.ReceiveMessage(username, message);//quando recebe manda para o vetorclock para ele ver
            //se existe dependencias
            //pede a lista e mostra no jogo
            OnMessageReceived?.Invoke(this.vetorClockManager.GetMessages());
        }


        public void SetPeers(Dictionary<string, Uri> peers)
        {

            int index = 0;
            foreach (string key in peers.Keys)
            {

                Uri address = peers[key];
                IClient client = (IClient)Activator.GetObject(
                    typeof(IClient),
                    address.ToString() + "Client");

                Peers.Add(client);

                if (client.Username == this.session.Username)
                {
                    //cria o vetor clock com a lista de clientes recebida
                    this.vetorClockManager = new VetorClockManager(peers.Keys.Count, index, address.ToString());
                    this.vetorClockManager.SetPeers(peers);
                }
                index++;
            }


        }

        public void VectorRecoveryRequest(int[] vetor, string adress)
        {
            this.vetorClockManager.VectorRecoveryRequest(vetor, adress);
        }


        public void PublishMessage(string username, string message)
        {

            //cria a mensagem, dá um tick o clock
            Message m = new Message(username, message);
            IVectorMessage<IMessage> vetorMessage = this.vetorClockManager.Tick(m);

            //todo upadate
            OnMessageReceived?.Invoke(this.vetorClockManager.GetMessages());

            foreach (IClient client in this.Peers)
            {
                Task.Run(() => {
                   
                    try
                    {
                        if (client.Username == username) return;
                        //envia para cada cliente

                        ((IChat)client).ReceiveMessage(username, vetorMessage);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                });
            }
        }

    }

    public delegate void MessageReceivedHandler(List<IMessage> messages);
}
