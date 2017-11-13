using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace PuppetMaster.Commands
{
    public class RemoteAsyncCommand : Command
    {
        public delegate IServer RemoteAsynServerDelegate(IServer s);
        public delegate IClient RemoteAsynIClientDelegate(IClient s);

        // test
        public RemoteAsynServerDelegate del { get; set; }

        public RemoteAsyncCommand (string name) : base(name) { }

        // encapsulate here async
        // nem que tenha que receber uma funcao 

        public override void Execute(string[] parameters)
        {   

            throw new NotImplementedException();
        }
    }
}
