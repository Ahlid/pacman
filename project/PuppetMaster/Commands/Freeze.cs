using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace PuppetMaster.Commands
{
    public class Freeze : AsyncCommand
    {
        private delegate void freezeDel(string PID);
        private freezeDel remoteCallFreeze;
        public Dictionary<string, IProcessCreationService> processesPCS { get; set; }

        public Freeze() : base("Freeze") { }

        public override void CommandToExecute(string[] parameters)
        {
            Console.WriteLine("+++Freeze command+++");

            string pid = parameters[0];
            IAsyncResult asyncResult;
            IProcessCreationService pcs = processesPCS[pid];

            remoteCallFreeze = new freezeDel(pcs.Freeze);
            asyncResult = remoteCallFreeze.BeginInvoke(pid, null, null);
            asyncResult.AsyncWaitHandle.WaitOne();
            remoteCallFreeze.EndInvoke(asyncResult);
        }
    }
}
