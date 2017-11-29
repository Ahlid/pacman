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

            

            IVetorMessage<string> message1 = clock.Tick("teste1");
            IVetorMessage<string> message2 = clock.Tick("teste2");
            IVetorMessage< string> message3 = clock.Tick("teste3");

            clock2.ReceiveMessage(message1);
            clock1.ReceiveMessage(message1);
            clock2.ReceiveMessage(message2);
            clock2.ReceiveMessage(message3);

            List<IVetorMessage<string>> x = clock2.GetMissingMessages(clock1.vector);

            foreach (IVetorMessage<string> message in x)
            {
                Console.WriteLine(message.Message);
            }

            Console.ReadLine();

        }
    }
}
