using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pacman;
using Shared;

namespace TEST
{
    class Program
    {
        static void Main(string[] args)
        {

            VetorClock<string> clock = new VetorClock<string>(3, 0);
            VetorClock<string> clock1 = new VetorClock<string>(3, 1);
            VetorClock<string> clock2 = new VetorClock<string>(3, 2);

            

            IVetorMessage<string> message1 = clock.Tick("1");
            clock1.ReceiveMessage(message1);
            clock2.ReceiveMessage(message1);

            IVetorMessage<string> message2 = clock1.Tick("2");
            clock2.ReceiveMessage(message2);

            IVetorMessage<string> message3 = clock2.Tick("3");
            clock1.ReceiveMessage(message3);

            IVetorMessage<string> message4 = clock1.Tick("4");
            clock2.ReceiveMessage(message4);

            IVetorMessage<string> message5 = clock2.Tick("5");
            clock.ReceiveMessage(message5);

            IVetorMessage<string> message6 = clock.Tick("6");
            clock1.ReceiveMessage(message6);
            clock2.ReceiveMessage(message6);




            Console.ReadLine();

        }
    }
}
