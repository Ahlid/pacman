using pacman;
using Server;
using Shared;
using Shared.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
<<<<<<< HEAD
using pacman;
using Server;
using Shared;
=======
>>>>>>> 37eca0f73650a39167c2304a95cb97874b800c8f

namespace TEST
{
    class Test
    {

 
        static void Main(string[] args)
        {
<<<<<<< HEAD

            Raft r1 = new Raft("R1");
            Raft r2 = new Raft("R2");
            Raft r3 = new Raft("R3");

            List<Raft> list = new List<Raft>();
            list.Add(r1);
            list.Add(r2);
            list.Add(r3);
=======
            Uri r1URI = new Uri("tcp://127.0.0.1:50006");
            Uri r2URI = new Uri("tcp://127.0.0.1:50007");
            Uri r3URI = new Uri("tcp://127.0.0.1:50008");

            RaftServer r1 = new RaftServer(r1URI, 1, 20);
            RaftServer r2 = new RaftServer(r2URI, 1, 20);
            RaftServer r3 = new RaftServer(r3URI, 1, 20);

            List<Uri> list = new List<Uri>();
            list.Add(r1URI);
            list.Add(r2URI);
            list.Add(r3URI);
>>>>>>> 37eca0f73650a39167c2304a95cb97874b800c8f

            r1.Start(list);
            r2.Start(list);
            r3.Start(list);

<<<<<<< HEAD
            Timer tmr = new Timer();

            tmr.Interval = 5000; // 0.1 second
            tmr.Elapsed += (object sender, ElapsedEventArgs e) =>
            {
                bool accepted;
                int commitedAt;
                r1.OnCommand(new RaftCommand(){Name = "TEste"}, out accepted,out commitedAt);
                r2.OnCommand(new RaftCommand(){Name = "TEste"}, out accepted,out commitedAt);
                r3.OnCommand(new RaftCommand(){Name = "TEste"}, out accepted,out commitedAt);
                r1.OnCommand(new RaftCommand(){Name = "TEste"}, out accepted,out commitedAt);
                r2.OnCommand(new RaftCommand(){Name = "TEste"}, out accepted,out commitedAt);
                r3.OnCommand(new RaftCommand(){Name = "TEste"}, out accepted,out commitedAt);
            }; // We'll write it in a bit
            tmr.Start(); // The countdown is launched!
=======
            Task.Run(() => {
                Uri clientURL = new Uri("tcp://127.0.0.1:50009");
                Hub hub = new Hub(list, clientURL, new SimpleGame());

                
                hub.OnStart += (stage) =>
                {
                    global::System.Console.WriteLine("GAME STARTING IN CLIENT");
                };

                try
                {
                    JoinResult result = hub.Join("TESTE");
                  
                }
                catch (InvalidUsernameException exc)
                {
                    
                }

            });
            

            /*
                Timer tmr = new Timer();

                tmr.Interval = 5000; // 0.1 second
                tmr.Elapsed += (object sender, ElapsedEventArgs e) =>
                {
                    bool accepted;
                    int commitedAt;
                    r1.OnCommand(new RaftCommand() { Name = "TEste1" }, out accepted, out commitedAt);
                    r2.OnCommand(new RaftCommand() { Name = "TEste2" }, out accepted, out commitedAt);
                    r3.OnCommand(new RaftCommand() { Name = "TEste3" }, out accepted, out commitedAt);
                    r1.OnCommand(new RaftCommand() { Name = "TEste4" }, out accepted, out commitedAt);
                    r2.OnCommand(new RaftCommand() { Name = "TEste5" }, out accepted, out commitedAt);
                    r3.OnCommand(new RaftCommand() { Name = "TEste6" }, out accepted, out commitedAt);
                }; // We'll write it in a bit

                tmr.Start(); // The countdown is launched!
                */
>>>>>>> 37eca0f73650a39167c2304a95cb97874b800c8f

            /* Uri uri = new Uri();
             Server.Server server = new Server.Server();
             Server.Server server2 = new Server.Server(new Uri("tcp:3002"), new Uri("tcp:3001"));
             Server.Server server3 = new Server.Server(new Uri("tcp:3003"), new Uri("tcp:3001"));
             Console.ReadLine();*/
            Console.ReadLine();
        }

<<<<<<< HEAD
        
=======

>>>>>>> 37eca0f73650a39167c2304a95cb97874b800c8f
    }

}
