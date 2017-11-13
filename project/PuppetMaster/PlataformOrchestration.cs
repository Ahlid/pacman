using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class PlataformOrchestration
    {
        private string a;

        public PlataformOrchestration()
        {

        }

        public void executeScript(string filepath)
        {
            string[] lines = File.ReadAllLines(filepath);
            foreach(string line in lines)
            {
                // interperter each command and its parameters from each line
            }
        }
    }
}
