using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class StartClient : Command
    {
        public StartClient () : base("StartClient") { }

        public override void Execute(string[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
