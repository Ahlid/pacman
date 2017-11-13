using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{

    public interface IChat
    {

        List<IClient> Clients { get; set; }

        void SendTextMessage(string username, string message);
        void MessageToAnotherPeer(string message);
        void SendClients(Dictionary<string, string> clients);
    }
}
