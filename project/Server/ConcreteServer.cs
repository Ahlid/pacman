using Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class ConcreteServer : MarshalByRefObject, IServer
    {
        private List<IClient> clients;
        private IStage stage;
        public const int PLAYER_SPEED = 5;

        //ghost speed for the one direction ghosts
        public const int GHOST1 = 5;
        public const int GHOST2 = 5;

        //x and y directions for the bi-direccional pink ghost
        public const int GHOST3X = 5;
        public const int GHOST3Y = 5;

        public const int NUM_PLAYERS = 1;


        public int round = 0;
        public Timer timer;

        public ConcreteServer()
        {
            clients = new List<IClient>();
            stage = new Stage();
            ICoin coin = new Coin();
            coin.Position = new Point(2, 4);
            stage.addCoin(coin);
            System.Console.WriteLine("Constructor done");
        }

        public void run(int roundIntervalMsec)
        {
            timer = new Timer(new TimerCallback(Tick), null, 0, roundIntervalMsec);
        }

        private void Tick(Object parameters)
        {
            round++;
            //BuildStage();
            broadcastStart();
        }

        public bool join(string address)
        {
            if (NUM_PLAYERS == clients.Count)
            {
                //Either queue the client or simply reject it
                return false;
            }
            IClient client = (IClient)Activator.GetObject(
                typeof(IClient),
                address);

            clients.Add(client);
            IPlayer player = new Player();
            player.Position = new Point(4, 4);
            stage.addPlayer(player);

            if (NUM_PLAYERS == clients.Count)
            {
                Thread thread = new Thread(delegate ()
                {
                    broadcastStart();
                });
                thread.Start();
            }

            return true;
        }

        private void broadcastStart()
        {
            for (int i = clients.Count - 1; i >= 0; i--)
            {
                try
                {
                    if(round == 0)
                    {
                        clients.ElementAt(i).start(stage);
                    } else
                    {
                        clients.ElementAt(i).sendRoundStage(stage, round);
                    }
                    
                }
                catch (Exception)
                {
                    clients.RemoveAt(i);
                }
            }
        }

        public void setPlay(Play play, int round)
        {
            Console.WriteLine("Round: {0} Play: {1}", play, round);
        }
    }
}
