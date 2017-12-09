using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public enum JoinResult { QUEUED, REJECTED_USERNAME }

    public interface IServer
    {

        Uri Address { get; set; }
        /// <summary>
        /// To join a game
        /// </summary>
        /// <param name="username">Client's usrname</param>
        /// <param name="address">Client's address</param>
        /// <returns>The result QUEUED</returns>
        JoinResult Join(string username, Uri address);

        /// <summary>
        /// Set's a clent's play
        /// </summary>
        /// <param name="address">Client's address</param>
        /// <param name="play">Clien't choosen play</param>
        /// <param name="round">The round that client has choosen</param>
        void SetPlay(Uri address, Play play, int round);

        /// <summary>
        /// When a client's quit the game
        /// </summary>
        /// <param name="address">Clien'ts address</param>
        void Quit(Uri address);

        /// <summary>
        /// Get's the leader's address
        /// </summary>
        /// <returns>Returns the addres of the leader</returns>
        Uri GetLeader();

        string Test();
    }
}
