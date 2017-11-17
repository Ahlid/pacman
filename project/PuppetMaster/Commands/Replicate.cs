using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace PuppetMaster.Commands
{
    public class Replicate : AsyncCommand
    {
        private delegate void StartReplicaDel(string PID, string serverURL, string replicaURL);
        private StartReplicaDel remoteCallStartReplica;
        public Dictionary<string, IProcessCreationService> processesPCS { get; set; }
        public string masterServerUrl;


        public Replicate() : base("Replica") { }

        public override void CommandToExecute(string[] parameters)
        {
            Console.WriteLine("+++Start Server command+++");

            string pid = parameters[0];
            IAsyncResult asyncResult;
            IProcessCreationService pcs = processesPCS[pid];

            remoteCallStartReplica = new StartReplicaDel(pcs.StartReplica);
            asyncResult = remoteCallStartReplica.BeginInvoke(parameters[0], masterServerUrl, parameters[3], null, null);
            asyncResult.AsyncWaitHandle.WaitOne();
        }
    }
}
