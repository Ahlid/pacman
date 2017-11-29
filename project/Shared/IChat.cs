using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface IChat
    {
        void ReceiveMessage(string username,IVetorMessage<IChatMessage> message);

        void SetPeers(Dictionary<string, Uri> peers);

        void VectorRecoveryRequest(int[] vetor, string adress);
    }
}
