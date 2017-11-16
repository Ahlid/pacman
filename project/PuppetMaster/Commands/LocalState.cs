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


        public LocalState() : base("LocalState") { }

        public override void CommandToExecute(string[] parameters, Dictionary<string, IProcessCreationService> processesPCS)
        {
            Console.WriteLine("+++Local State command+++");

            
            // probabily threads are need here. test it
            IAsyncResult result;
            foreach (KeyValuePair<string, IProcessCreationService> entry in processesPCS)
            {
                remoteCallLocalState = new localStateDel((entry.Value).LocalState);
                result = remoteCallLocalState.BeginInvoke(entry.Key, parameters[1], null, null);
                result.AsyncWaitHandle.WaitOne();
                Console.WriteLine("LocalState of the process with PID: '{0}': \n" + result + "\n\n");
            }
        }
    }
}
