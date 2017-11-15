using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PupetMaster
{
    class Program
    {
        static void Main(string[] args)
        {
            string PID = args[1];
            string PCS_URL = args[2];
            string MSEC_PER_ROUND = args[4];
            string NUM_PLAYERS = args[5];
            Shared.IProcessCreationService pcs;
            switch (args[0])
            {
                case "StartClient":
                    PID = args[1];
                    PCS_URL = args[2];
                    string CLIENT_URL = args[3];
                    MSEC_PER_ROUND = args[4];
                    NUM_PLAYERS = args[5];
                    string FILENAME = args[6];

                    pcs = (Shared.IProcessCreationService)Activator.GetObject(
                        typeof(Shared.IProcessCreationService),
                        PCS_URL);

                    pcs.StartClient(PID, CLIENT_URL, MSEC_PER_ROUND, NUM_PLAYERS);

                    break;

                case "StartServer":
                    PID = args[1];
                    PCS_URL = args[2];
                    string SERVER_URL = args[3];
                    MSEC_PER_ROUND = args[4];
                    NUM_PLAYERS = args[5];


                    pcs = (Shared.IProcessCreationService)Activator.GetObject(
                        typeof(Shared.IProcessCreationService),
                        PCS_URL);

                    pcs.StartServer(PID, SERVER_URL, MSEC_PER_ROUND, NUM_PLAYERS);

                    break;
            }

            Console.WriteLine(args[1]);

        }
    }
}
