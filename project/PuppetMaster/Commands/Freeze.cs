using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster.Commands
{
    public class Freeze : Command
    {
        public Freeze() : base("Freeze") { }

        public override void Execute(string[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}
