using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessCreationService
{
    public interface IProcessCreationService
    {
        void StartClient(string PID, string CLIENT_URL, string MSEC_PER_ROUND, string NUM_PLAYERS);

        void StartServer(string PID, string SERVER_URL, string MSEC_PER_ROUND, string NUM_PLAYERS);
    }
}
