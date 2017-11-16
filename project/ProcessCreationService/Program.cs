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
            Console.WriteLine("***** Process Creation Server initialized *****");

            TcpChannel channel = new TcpChannel(20001);
            ChannelServices.RegisterChannel(channel, false);

            RemotingConfiguration.RegisterWellKnownServiceType(
               typeof(ProcessCreationService),
               "ProcessCreationService",
               WellKnownObjectMode.Singleton);



            Console.ReadLine();
        }
    }
}
