using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface IServer
    {
        bool Join(string username, string address);

        void SetPlay(String address, Play play, int round);

        void Quit(string address);

    }
}
