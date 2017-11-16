using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace PuppetMaster.Commands
{
    public class Unfreeze : AsyncCommand
    {
        private delegate void unfreezeDel(string PID);
        private unfreezeDel remoteCallUnfreeze;


        public Unfreeze() : base("Unfreeze") { }

        public override void CommandToExecute(string[] parameters, Dictionary<string, IProcessCreationService> processesPCS)
        {
            Console.WriteLine("+++Unfreeze command+++");

            string pid = parameters[0];
            IAsyncResult result;
            IProcessCreationService pcs = processesPCS[pid];

            remoteCallUnfreeze = new unfreezeDel(pcs.Freeze);
            result = remoteCallUnfreeze.BeginInvoke(pid, null, null);
            result.AsyncWaitHandle.WaitOne();
        }
    }
}
