using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace PuppetMaster.Commands
{
    public class InjectDelay : AsyncCommand
    {
        private delegate void injectDealyDel(string sourcePID, string destinationPID);
        private injectDealyDel remoteCallInjectDelay;


        public InjectDelay() : base("InjectDelay") { }

        public override void CommandToExecute(string[] parameters, Dictionary<string, IProcessCreationService> processesPCS)
        {
            Console.WriteLine("+++Inject Delay command+++");

            string pid = parameters[0];
            IAsyncResult asyncResult;
            IProcessCreationService pcs = processesPCS[pid];

            remoteCallInjectDelay = new injectDealyDel(pcs.InjectDelay);
            asyncResult = remoteCallInjectDelay.BeginInvoke(pid, parameters[1], null, null);
            asyncResult.AsyncWaitHandle.WaitOne();
            remoteCallInjectDelay.EndInvoke(asyncResult);
        }
    }
}
