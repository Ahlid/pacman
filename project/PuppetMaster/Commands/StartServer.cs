using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppetMaster.Commands;

namespace PuppetMaster
{
    public class StartServer : RemoteAsyncCommand // : Command
    {
        public StartServer() : base("StartServer") { }

        public override void Execute(string[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
