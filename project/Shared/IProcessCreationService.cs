using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface IProcessCreationService
    {
        void StartClient(string PID, string clientURL, string instructions, List<string> serverURLs);

        void StartClient(string PID, string clientURL, List<string> serverURLs);

        void StartServer(string PID, string serverURL, string msecPerRound, string numPlayers, List<string> serverURLs);

        void Freeze(string PID);

        void Unfreeze(string PID);

        void InjectDelay(string sourcePID, string destinationPID);

        string LocalState(string PID, string roundID);

        void GlobalStatus();

        void Crash(string PID);
    }
}
