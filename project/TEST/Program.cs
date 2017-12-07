using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using pacman;
using Server;
using Shared;

namespace TEST
{
    class Program
    {
        static void Main(string[] args)
        {

            Raft r1 = new Raft("R1");
            Raft r2 = new Raft("R2");
            Raft r3 = new Raft("R3");

            List<Raft> list = new List<Raft>();
            list.Add(r1);
            list.Add(r2);
            list.Add(r3);

            r1.Start(list);
            r2.Start(list);
            r3.Start(list);

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

            /* Uri uri = new Uri();
             Server.Server server = new Server.Server();
             Server.Server server2 = new Server.Server(new Uri("tcp:3002"), new Uri("tcp:3001"));
             Server.Server server3 = new Server.Server(new Uri("tcp:3003"), new Uri("tcp:3001"));
             Console.ReadLine();*/
            Console.ReadLine();
        }

        
    }
}
