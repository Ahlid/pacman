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
        void StartClient(string PID, string clientURL, IList<string> serverURLList, string msecPerRound, string numPlayers, string instructions);

        void StartClient(string PID, string clientURL, IList<string> serverURLList, string msecPerRound, string numPlayers);

        void StartServer(string PID, string serverURL, string msecPerRound, string numPlayers);

        void Freeze(string PID);

        void Unfreeze(string PID);

        void InjectDelay(string sourcePID, string destinationPID);

        string LocalState(string PID, string roundID);

        void GlobalStatus();

        void Crash(string PID);

    }
}
