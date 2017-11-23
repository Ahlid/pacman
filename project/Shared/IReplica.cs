using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public interface IReplica
    {
        void RegisterReplica(Uri ReplicaServerURL);

        void SetReplicaList(List<Uri> replicaServerURL);

        void SendRoundStage(IStage stage);

        Uri GetMaster();

        string Ping();

    }
}
