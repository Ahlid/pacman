using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pacman;

namespace TEST
{
    class Program
    {
        static void Main(string[] args)
        {

            VetorClock<string> clock = new VetorClock<string>(3, 0);
            VetorClock<string> clock1 = new VetorClock<string>(3, 1);
            VetorClock<string> clock2 = new VetorClock<string>(3, 2);

            /*

            IVetorMessage message1 = clock.Tick("teste1");
            VectorMessage<string> message2 = clock.Tick("teste2");
            VectorMessage<string> message3 = clock.Tick("teste3");

            clock2.ReceiveMessage(message1);
            clock2.ReceiveMessage(message2);
            clock2.ReceiveMessage(message3);

            VectorMessage<string> messageClock2 = clock2.Tick("Olaaaaa");

            clock1.ReceiveMessage(messageClock2);


            Console.WriteLine(string.Join(",", clock.vector));
            Console.WriteLine(string.Join(",", clock1.vector));
            Console.WriteLine(string.Join(",", clock2.vector));


            clock1.ReceiveMessage(message1);
            Console.WriteLine(string.Join(",", clock1.vector));

            clock1.ReceiveMessage(message3);
            Console.WriteLine(string.Join(",", clock1.vector));

            clock1.ReceiveMessage(message2);
            Console.WriteLine(string.Join(",", clock1.vector));



            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine(string.Join(",", clock.vector));
            Console.WriteLine(string.Join(",", clock1.vector));
            Console.WriteLine(string.Join(",", clock2.vector));

          

            VectorMessage<string> message5 = clock1.Tick("Ola tudo bem?");

            clock.ReceiveMessage(message5);
            clock2.ReceiveMessage(message5);

            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine(string.Join(",", clock.vector));
            Console.WriteLine(string.Join(",", clock1.vector));
            Console.WriteLine(string.Join(",", clock2.vector));

            clock.ReceiveMessage(messageClock2);

            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine(string.Join(",", clock.vector));
            Console.WriteLine(string.Join(",", clock1.vector));
            Console.WriteLine(string.Join(",", clock2.vector));

            Console.ReadLine();
            */
        }
    }
}
