using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppetMaster.Commands;
using Shared;

namespace PuppetMaster
{

    // doesn't provide parameters check. -> acts on the best case scenario
    public abstract class AbstractCommand : ICommand
    {
        protected string name;

        public AbstractCommand(string name)
        {
            this.name = name;
        }

        public abstract void Execute(string[] parameters, Dictionary<string, IProcessCreationService> processesPCS);
    }
}
