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
        private static PlataformOrchestration master;

        static void Main(string[] args)
        {
            Console.WriteLine("***** Puppet Master initialized *****");
            string commandWithParameters;
            master = new PlataformOrchestration();

            // check if was submitted any scrit file
            if (args.Length > 0 && args[0] != null && args[0].Trim() != "")
            {
                executeScriptFile(args[0]);
            }



            // todo: Program should wait for all threads to be finished before allowing 
            //to accept commands from the command line

            // read commands through the command line in real time 
            Console.WriteLine();
            Console.WriteLine("** Ready to read commands from console **");
            string[] resolveCommandWithParameters;
            string[] parameters;
            do
            {
                Console.WriteLine("> Writer your command: ");
                Console.Write("> ");
                commandWithParameters = Console.ReadLine();
                resolveCommandWithParameters = commandWithParameters.Split(' ');
                parameters = new List<String>(resolveCommandWithParameters).GetRange(1, resolveCommandWithParameters.Length - 1).ToArray();
                master.ExecuteCommand(resolveCommandWithParameters[0], parameters);

            } while (commandWithParameters != "QUIT");
            Console.WriteLine("***** Pupper Master shutting down! *****");
            System.Threading.Thread.Sleep(1000); // wait thread execution, to see message
        }

        private static void executeScriptFile(string filename)
        {
            string uniformFilepath = @"../../scripts/" + filename;
            // check if the file exists
            if (File.Exists(uniformFilepath))
            {
                Console.WriteLine("*** Start execution of the script file ***");
                master.ExecuteScript(uniformFilepath);
                Console.WriteLine("*** Finished executing script file ***");
            }
            else
            {
                Console.WriteLine("** Script file doesn't exist! **");
            }
        }
    }
}
