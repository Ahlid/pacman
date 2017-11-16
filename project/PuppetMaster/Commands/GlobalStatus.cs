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

            IAsyncResult asyncResult;
            foreach (KeyValuePair<string, IProcessCreationService> entry in processesPCS)
            {
                remoteCallGlobalStatus = new globalStatusDel((entry.Value).GlobalStatus);
                asyncResult = remoteCallGlobalStatus.BeginInvoke(null, null);
                asyncResult.AsyncWaitHandle.WaitOne();
            }
        }
    }
}
