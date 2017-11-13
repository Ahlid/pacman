using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    interface IStrategy
    {
        void Execute(string[] parameters);
    }

    // doesn't provide parameters check. -> acts on the best case scenario
    public abstract class Command : IStrategy
    {
        private string name;

        public Command(string name)
        {
            this.name = name;
        }

        public abstract void Execute(string[] parameters);
    }
}
