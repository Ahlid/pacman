using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shared;

namespace PuppetMaster.Commands
{

    // local, acts on the puppet master itself
    public class Wait : AbstractCommand
    {
        public Wait() : base("Wait") { }

        public override void Execute(string[] parameters)
        {
            Console.WriteLine(String.Format("Sync Execution of command: '{0}'", this.name));
            // stop current thread for the miliseconds received as parameter
            Thread.Sleep(Int32.Parse(parameters[0]));
        }
    }
}
