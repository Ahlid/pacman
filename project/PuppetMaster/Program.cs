using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class Program : MarshalByRefObject
    {
        private static PlataformOrchestration master;
        private static Uri puppetMasterURL;
        private static string pathConfigFile;

        static void Main(string[] args)
        {
            Console.WriteLine("***** Puppet Master initialized *****");
            string commandWithParameters;
            pathConfigFile = @"../../scripts/config.txt";

            master = new PlataformOrchestration();
            // load configurations
            loadConfig();
            // init remoting channel
            createServer();

            // check if was submitted any scrit file
            if (args.Length > 0 && args[0] != null && args[0].Trim() != "")
            {
                executeScriptFile(args[0]);
            }


            /*
             * TODO:
             * Usar uma pool de threads e quando existir um ficheiro esperar que tudo seja
             * executado é só depois é que possibilito a leitura de comandos a partir da consola
             * 
             */ 

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

        private static void createServer()
        {
            string[] uri_splited = puppetMasterURL.AbsolutePath.Split('/');
            string resource = uri_splited[uri_splited.Length - 1];
            TcpChannel channel = new TcpChannel(puppetMasterURL.Port);
            ChannelServices.RegisterChannel(channel, false);

            RemotingConfiguration.RegisterWellKnownServiceType(
               typeof(Program),
               resource,
               WellKnownObjectMode.Singleton);
        }

        private static void loadConfig()
        {
            if (File.Exists(pathConfigFile))
            {
                Console.WriteLine("*** Loading configs ***");
                string line = File.ReadAllLines(pathConfigFile)[0];
                puppetMasterURL = new Uri(line);
            }else
            {
                // Exit application, no configuration file!
                Environment.Exit(0);
            }
        }
    }
}
