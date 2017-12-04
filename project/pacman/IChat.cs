using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pacman
{
    public interface IChat
    {
        void ReceiveMessage(string username, IVectorMessage<IMessage> message);

        void VectorRecoveryRequest(int[] vetor, string adress);
    }
}
