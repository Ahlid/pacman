using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface IServer
    { 
        //returns true if the client was accepted
        bool Join(string address);

        void Run(int roundMsec);

        void SetPlay(String address, Play play, int round);
    }
}
