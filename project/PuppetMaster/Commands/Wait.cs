using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster.Commands
{

    // local, acts on the puppet master itself
    public class Wait : Command
    {
        public Wait() : base("Wait") { }

        public override void Execute(string[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
