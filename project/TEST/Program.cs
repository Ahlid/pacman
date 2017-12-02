using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pacman;
using Server;
using Shared;

namespace TEST
{
    class Program
    {
        static void Main(string[] args)
        {
            Server.Server server = new Server.Server(new Uri("tcp:3001"));
            Server.Server server2 = new Server.Server(new Uri("tcp:3002"), new Uri("tcp:3001"));
            Server.Server server3 = new Server.Server(new Uri("tcp:3003"), new Uri("tcp:3001"));
            Console.ReadLine();
        }
    }
}
