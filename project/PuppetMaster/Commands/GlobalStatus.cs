using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppetMaster.Commands;
using Shared;

namespace PuppetMaster
{
    public class GlobalStatus : AsyncCommand
    {
        private delegate void globalStatusDel();
        private globalStatusDel remoteCallGlobalStatus;


        public GlobalStatus() : base("GlobalStatus") { }

        public override void CommandToExecute(string[] parameters, Dictionary<string, IProcessCreationService> processesPCS)
        {
            Console.WriteLine("+++GlobalStatus command+++");

            string pid = parameters[0];
            IAsyncResult result;
            IProcessCreationService pcs = processesPCS[pid];

            remoteCallGlobalStatus = new globalStatusDel(pcs.GlobalStatus);
            result = remoteCallGlobalStatus.BeginInvoke(null, null);
            result.AsyncWaitHandle.WaitOne();
        }
    }
}
