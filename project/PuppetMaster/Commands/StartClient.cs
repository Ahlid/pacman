using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppetMaster.Commands;
using Shared;
using System.IO;

namespace PuppetMaster
{
    public class StartClient : AsyncCommand
    {
        private delegate void startClientDel(string PID, string clientURL, string masterServerUrl, string msecPerRound, string numPlayers);
        private delegate void startClientWithInstructionsDel(string PID, string clientURL, string masterServerUrl, string msecPerRound, string numPlayers, string instructions);

        private startClientDel remoteCallStartClient;
        private startClientWithInstructionsDel remoteCallStartClientWithInstructions;

        public Dictionary<string, IProcessCreationService> processesPCS { get; set; }
        public string masterServerUrl;


        public StartClient () : base("StartClient") { }

        public override void CommandToExecute(string[] parameters)
        {
            Console.WriteLine("+++Start Client command+++");

            string pid = parameters[0];
            IAsyncResult asyncResult;
            IProcessCreationService pcs = processesPCS[pid];

            if (parameters.Length < 6) // there is not instructions
            {
                remoteCallStartClient = new startClientDel(pcs.StartClient);
                asyncResult = remoteCallStartClient.BeginInvoke(pid, parameters[3], masterServerUrl, parameters[4], parameters[5], null, null);
                // wait for result
                asyncResult.AsyncWaitHandle.WaitOne();
                return;
            }
            // else -> client will be played automatically, following a moves trace file 
            
            string instructions = readInstructions(parameters[6]); // pass filename
            if(instructions != "")
            {
                remoteCallStartClientWithInstructions = new startClientWithInstructionsDel(pcs.StartClient);
                asyncResult = remoteCallStartClientWithInstructions.BeginInvoke(pid, parameters[3], masterServerUrl, parameters[4], parameters[5], instructions, null, null);
                asyncResult.AsyncWaitHandle.WaitOne();
                return;
            }
            else
            {
                Console.WriteLine("No file or no instructions");
                return;
            }
        }

        private string readInstructions(string filename)
        {
            string instructions = "";
            string uniformFilepath = @"../../scripts/" + filename;
            // check if the file exists
            if (File.Exists(uniformFilepath))
            {
                Console.WriteLine("*** Reading client moves trace file ***");
                string[] lines = File.ReadAllLines(uniformFilepath);
                foreach (string line in lines)
                {
                    instructions += line + "\n";
                }
                Console.WriteLine("*** Finished reading client moves trace file ***");
            }
            return instructions;
        }
    }
}
