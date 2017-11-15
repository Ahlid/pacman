using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace ProcessCreationService
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpChannel channel = new TcpChannel(11000);
            ChannelServices.RegisterChannel(channel, true);

            RemotingConfiguration.RegisterWellKnownServiceType(
               typeof(ProcessCreationService),
               "ProcessCreationService",
               WellKnownObjectMode.Singleton);

        }
    }
}
