using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface IServer
    { 

        /*
         * difference between register and join game!!!
         */

        //returns true if the client was accepted
        bool Join(string username, string address);

        void Run(int roundMsec);

        void SetPlay(String address, Play play, int round);

        void Quit(string address);

        int NextAvailablePort(string address);
    }
}
