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
        JoinResult Join(string username, Uri address);

        void SetPlay(Uri address, Play play, int round);

        void Quit(Uri address);
        
        Uri GetLeader();
    }
}
