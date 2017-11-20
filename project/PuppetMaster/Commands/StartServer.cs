using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppetMaster.Commands;
using Shared;

namespace PuppetMaster
{
    public class StartServer : AsyncCommand
    {
        private delegate void startServerDel(string PID, string serverURL, string msecPerRound, string numPlayers);
        private startServerDel remoteCallStartServer;
        public Dictionary<string, IProcessCreationService> processesPCS { get; set; }


        public StartServer() : base("StartServer") { }

        public override void CommandToExecute(string[] parameters)
        {
            string pid = parameters[0];
            IAsyncResult asyncResult;
            IProcessCreationService pcs = processesPCS[pid];

            remoteCallStartServer = new startServerDel(pcs.StartServer);
            asyncResult = remoteCallStartServer.BeginInvoke(pid, parameters[2], parameters[3], parameters[4], null, null);
            asyncResult.AsyncWaitHandle.WaitOne();
        }
    }
}
