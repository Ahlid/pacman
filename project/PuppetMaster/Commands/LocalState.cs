using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace PuppetMaster.Commands
{
    public class LocalState : AsyncCommand
    {
        private delegate string localStateDel(string PID, string roundID);
        private localStateDel remoteCallLocalState;
        public Dictionary<string, IProcessCreationService> processesPCS { get; set; }


        public LocalState() : base("LocalState") { }

        public override void CommandToExecute(string[] parameters)
        {
            Console.WriteLine("+++Local State command+++");

            
            // probabily threads are need here. test it
            IAsyncResult asyncResult;
            string result;
            string filename;
            foreach (KeyValuePair<string, IProcessCreationService> entry in processesPCS)
            {
                remoteCallLocalState = new localStateDel((entry.Value).LocalState);
                asyncResult = remoteCallLocalState.BeginInvoke(entry.Key, parameters[1], null, null);
                asyncResult.AsyncWaitHandle.WaitOne();
                result = remoteCallLocalState.EndInvoke(asyncResult);
                Console.WriteLine("LocalState of the process with PID: '{0}': \n" + result + "\n\n");

                filename = String.Format(@"../../output/LocalState-{0}-{1}.txt", entry.Key, parameters[1]);
                System.IO.File.WriteAllText(filename, result.ToString());
            }
        }
    }
}
