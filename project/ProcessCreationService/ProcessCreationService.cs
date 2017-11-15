using Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ProcessCreationService
{
    class ProcessCreationService : IProcessCreationService
    {
        IDictionary<string, Process> processes = new Dictionary<string, Process>();


        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);

        public void Crash(string PID)
        {
            processes[PID].Close();
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
            Process clientProcess = new Process();
            clientProcess.StartInfo.FileName = @"..\..\..\Server\bin\Release\Client.exe";
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(instructions);
            string base64Instructions = System.Convert.ToBase64String(plainTextBytes);
            clientProcess.StartInfo.Arguments = $"{PID} {clientURL} {msecPerRound} {numPlayers} {base64Instructions}";
            clientProcess.Start();
            processes.Add(PID, clientProcess);
        }

        public void StartServer(string PID, string serverURL, string msecPerRound, string numPlayers)
        {
            Process serverProcess = new Process();
            serverProcess.StartInfo.FileName = @"..\..\..\Server\bin\Release\Server.exe";
            serverProcess.StartInfo.Arguments = $"{PID} {serverURL} {msecPerRound} {numPlayers}";
            serverProcess.Start();
            processes.Add(PID, serverProcess);
        }

        public void Freeze(string PID)
        {
            var process = processes[PID];

            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                SuspendThread(pOpenThread);

                CloseHandle(pOpenThread);
            }
        }

        public void Unfreeze(string PID)
        {
            Process process = processes[PID];

            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                var suspendCount = 0;
                do
                {
                    suspendCount = ResumeThread(pOpenThread);
                } while (suspendCount > 0);

                CloseHandle(pOpenThread);
            }
        }
    }
}
