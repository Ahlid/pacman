using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class Program
    {
        private static string commandWithParameters;
        private static PlataformOrchestration master;
        static void Main(string[] args)
        {
            Console.WriteLine("***** Puppet Master initialized *****");
            master = new PlataformOrchestration();
//#if DEBUG  // release env receives the arguments directly from the command line
            //args = new[] { "scripts/script20.txt" };
            args = new[] { "" };
//#endif
            executeScriptFile(args[0]);

            // read commands through the command line in real time 
            Console.WriteLine("** Ready to read commands from console **");
            do
            {
                Console.WriteLine("> Writer your command: ");
                Console.Write("> ");
                commandWithParameters = Console.ReadLine();

            } while (commandWithParameters != "QUIT");
            Console.WriteLine("***** Pupper Master shutting down! *****");
            System.Threading.Thread.Sleep(1000); // wait thread execution
        }

        private static void executeScriptFile(string filepath)
        {
            // check if filepath was provided
            if (filepath == null || filepath.Trim() == "")
            {
                return;
            }
            string uniformFilepath = @"../../" + filepath;
            // check if the file exists
            if (File.Exists(uniformFilepath))
            {
                Console.WriteLine("*** Initiating executing of script file ***");
                master.executeScript(uniformFilepath);
                Console.WriteLine("*** Finished Executing Script file ***");
            }
            else
            {
                Console.WriteLine("** Script file doesn't exist! **");
            }
        }
    }
}
