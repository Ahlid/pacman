using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster.Commands
{
    public class Crash : RemoteAsyncCommand
    {
        public Crash() : base("Crash") { }

        public override void Execute(string[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
