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
        private delegate void startServerDel(string PID, string clientURL, string msecPerRound, string numPlayers);
        private startServerDel remoteCallStartServer;


        public StartServer() : base("StartServer") { }

        public override void CommandToExecute(string[] parameters, Dictionary<string, IProcessCreationService> processesPCS)
        {
            Console.WriteLine("+++Start Server command+++");

            string pid = parameters[0];
            IAsyncResult asyncResult;
            IProcessCreationService pcs = processesPCS[pid];

            remoteCallStartServer = new startServerDel(pcs.StartServer);
            asyncResult = remoteCallStartServer.BeginInvoke(pid, parameters[1], parameters[2], parameters[3], null, null);
            asyncResult.AsyncWaitHandle.WaitOne();
        }
    }
}
