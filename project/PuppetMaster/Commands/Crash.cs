using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace PuppetMaster.Commands
{
    public class Crash : AsyncCommand
    {
        private delegate void crashDel(string PID);
        private crashDel remoteCallCrash;
        public Dictionary<string, IProcessCreationService> processesPCS { get; set; }


        public Crash() : base("Crash") { }

        public override void CommandToExecute(string[] parameters)
        {
            string pid = parameters[0];
            IAsyncResult asyncResult;
            IProcessCreationService pcs = processesPCS[pid];

            remoteCallCrash = new crashDel(pcs.Crash);
            asyncResult = remoteCallCrash.BeginInvoke(pid, null, null);
            asyncResult.AsyncWaitHandle.WaitOne();
            remoteCallCrash.EndInvoke(asyncResult);
        }
    }
}
