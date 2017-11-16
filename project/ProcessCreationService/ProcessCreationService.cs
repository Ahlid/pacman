using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessCreationService
{
    class ProcessCreationService : MarshalByRefObject, IProcessCreationService
    {
        IDictionary<string, Process> processes = new Dictionary<string, Process>();

        public void Crash(string PID)
        {
            throw new NotImplementedException();
        }

        public void Freeze(string PID)
        {
            throw new NotImplementedException();
        }

        public void GlobalStatus()
        {
            throw new NotImplementedException();
        }

        public void InjectDelay(string sourcePID, string destinationPID)
        {
            throw new NotImplementedException();
        }

        public string LocalState(string PID, string roundID)
        {
            throw new NotImplementedException();
        }

        public void Something()
        {
            Console.WriteLine("Something happened");
        }

        public void StartClient(string PID, string clientURL, string msecPerRound, string numPlayers)
        {
            Process clientProcess = new Process();
            clientProcess.StartInfo.FileName = @"..\..\..\Server\bin\Release\Client.exe";
            clientProcess.StartInfo.Arguments = $"{PID} {clientURL} {msecPerRound} {numPlayers}";
            clientProcess.Start();
            processes.Add(PID, clientProcess);
        }

        public void StartClient(string PID, string clientURL, string msecPerRound, string numPlayers, string instructions)
        {
            //todo
        }

        public void StartServer(string PID, string serverURL, string msecPerRound, string numPlayers)
        {
            Process serverProcess = new Process();
            serverProcess.StartInfo.FileName = @"..\..\..\Server\bin\Release\Server.exe";
            serverProcess.StartInfo.Arguments = $"{PID} {serverURL} {msecPerRound} {numPlayers}";
            serverProcess.Start();
            processes.Add(PID, serverProcess);
        }

        public void Unfreeze(string PID)
        {
            throw new NotImplementedException();
        }
    }
}
