using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster.Commands
{
    public class Unfreeze : Command
    {
        public Unfreeze() : base("Unfreeze") { }

        public override void Execute(string[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
