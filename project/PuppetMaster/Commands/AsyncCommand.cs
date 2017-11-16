using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shared;

namespace PuppetMaster.Commands
{
    public abstract class AsyncCommand : AbstractCommand
    {
        public AsyncCommand(string name) : base(name) { }

        public override void Execute(string[] parameters, Dictionary<string, IProcessCreationService> processesPCS)
        {
            Thread t = new Thread(delegate ()
            {
                Console.WriteLine(String.Format("Async Execution: '{0}' started", this.name));
                CommandToExecute(parameters, processesPCS);
            });
            t.Start();
        }

        public abstract void CommandToExecute(string[] parameters, Dictionary<string, IProcessCreationService> processesPCS);
    }
}
