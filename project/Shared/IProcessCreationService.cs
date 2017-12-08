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

        /// <summary>
        /// Process to start a client
        /// </summary>
        /// <param name="PID">Client's PID</param>
        /// <param name="clientURL">Clients url</param>
        /// <param name="instructions">Instructions</param>
        /// <param name="serverURLs">The current servers list</param>
        void StartClient(string PID, string clientURL, string instructions, List<string> serverURLs);

        /// <summary>
        /// Process to start a client
        /// </summary>
        /// <param name="PID">Client's PID</param>
        /// <param name="clientURL">Clients url</param>
        /// <param name="serverURLs">The current servers list</param>
        void StartClient(string PID, string clientURL, List<string> serverURLs);

        /// <summary>
        /// Process to create a server
        /// </summary>
        /// <param name="PID">Server's PID</param>
        /// <param name="serverURL">Server's URL</param>
        /// <param name="msecPerRound">Round interval</param>
        /// <param name="numPlayers">Number of players per game</param>
        /// <param name="serverURLs">The current servers list</param>
        void StartServer(string PID, string serverURL, string msecPerRound, string numPlayers, List<string> serverURLs);

        /// <summary>
        /// Freezes a process
        /// </summary>
        /// <param name="PID">Process PID</param>
        void Freeze(string PID);

        /// <summary>
        /// Unfreezes a process
        /// </summary>
        /// <param name="PID">Process PID</param>
        void Unfreeze(string PID);

        /// <summary>
        /// Add a delay
        /// </summary>
        /// <param name="sourcePID"></param>
        /// <param name="destinationPID"></param>
        void InjectDelay(string sourcePID, string destinationPID);

        /// <summary>
        /// Get's the state
        /// </summary>
        /// <param name="PID">Process PID</param>
        /// <param name="roundID">Round</param>
        /// <returns></returns>
        string LocalState(string PID, string roundID);

        void GlobalStatus();

        /// <summary>
        /// Crashs a process
        /// </summary>
        /// <param name="PID">Process PID</param>
        void Crash(string PID);
    }
}
