using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface IChat
    {
        void SendMessage(string username,string message);

        void SetPeers(Dictionary<string, Uri> peers);
    }
}
