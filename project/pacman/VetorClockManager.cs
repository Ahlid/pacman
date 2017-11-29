using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Timers;
using Shared;

namespace pacman
{
    public class VetorClockManager : IChat
    {
        private VetorClock<IChatMessage> vetorClock;
        private BackgroundWorker checkerBackgroundWorker;
        public List<IClient> Peers { get; private set; }
        private int index;
        private int lastAskedIndex;
        private int size;
        private string address;

        public VetorClockManager(int size, int index, string address)
        {
            this.lastAskedIndex = -1;
            this.size = size;
            this.address = address;
            this.vetorClock = new VetorClock<IChatMessage>(size, index);
            this.Peers = new List<IClient>();
            this.index = index;
            checkerBackgroundWorker = new BackgroundWorker();
            checkerBackgroundWorker.DoWork += worker_DoWork;
            Timer timer = new Timer(1000);
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!checkerBackgroundWorker.IsBusy)
                checkerBackgroundWorker.RunWorkerAsync();
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            this.lastAskedIndex = lastAskedIndex++ % this.size;

            IClient client = this.Peers[lastAskedIndex];
            client.VectorRecoveryRequest(this.vetorClock.vector, this.address);

        }


        public void ReceiveMessage(string username, IVetorMessage<IChatMessage> message)
        {
            this.vetorClock.ReceiveMessage(message);
        }

        public void SetPeers(Dictionary<string, Uri> peers)
        {
            int index = 0;
            foreach (string key in peers.Keys)
            {
                if (index == this.index)
                {
                    index++;
                    continue;
                }

                Uri address = peers[key];
                IClient client = (IClient)Activator.GetObject(
                    typeof(IClient),
                    address.ToString() + "Client");

                Peers.Add(client);
                index++;
            }
        }

        public void VectorRecoveryRequest(int[] vetor, string address)
        {
            IClient clientRequested = this.Peers.FirstOrDefault(p => p.Address.ToString() == address);

            if (clientRequested != null)
            {
                List<IVetorMessage<IChatMessage>> messages = this.vetorClock.GetMissingMessages(vetor);

                foreach (IVetorMessage<IChatMessage> vetorMessage in messages)
                {
                    clientRequested.ReceiveMessage(vetorMessage.Message.Username,vetorMessage);
                }
            }
        }

        public List<IChatMessage> GetMessages()
        {
            return this.vetorClock.GetMessages();
        }

        public IVetorMessage<IChatMessage> Tick(Message message)
        {
            return this.vetorClock.Tick(message);
        }
    }
}