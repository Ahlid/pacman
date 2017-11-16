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


        public Crash() : base("Crash") { }

        public override void CommandToExecute(string[] parameters, Dictionary<string, IProcessCreationService> processesPCS)
        {
            // vou fazer uma chamada ao pcs asincronamente e depois a chamada e assincrona tambem
            Console.WriteLine("+++Crash command+++");

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
