using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class GlobalStatus : Command
    {
        public GlobalStatus() : base("GlobalStatus") { }

        public override void Execute(string[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
