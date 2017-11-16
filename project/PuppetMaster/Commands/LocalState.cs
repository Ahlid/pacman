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

            string pid = parameters[0];
            IAsyncResult result;
            IProcessCreationService pcs = processesPCS[pid];

            remoteCallLocalState = new localStateDel(pcs.LocalState);
            result = remoteCallLocalState.BeginInvoke(pid, parameters[1], null, null);
            result.AsyncWaitHandle.WaitOne();

            Console.WriteLine("LocalState of the process with PID: '{0}': \n" + result + "\n\n");
        }
    }
}
