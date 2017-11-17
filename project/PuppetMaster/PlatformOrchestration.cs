using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppetMaster.Commands;
using Shared;

namespace PuppetMaster
{
    public class PlatformOrchestration
    {
        // pid - remote service of server and replicas
        private Dictionary<String, IProcessCreationService> processesPCS;
        
        // pid - url of master servers
        private Dictionary<string, string> masterServersUrl;


        public PlatformOrchestration()
        {
            this.processesPCS = new Dictionary<string, IProcessCreationService>();
            this.masterServersUrl = new Dictionary<string, string>();
        }


        public void ExecuteScript(string filepath)
        {
            string[] lines = File.ReadAllLines(filepath);
            string[] resolveCommandWithParameters;
            string commandName;
            string[] parameters;
            ICommand command;
            foreach (string line in lines)
            {
                Console.WriteLine("*****************************");
                resolveCommandWithParameters = line.Split(' ');
                commandName = resolveCommandWithParameters[0];
                if (commandName != null && commandName.Trim() != "")
                {
                    parameters = new List<String>(resolveCommandWithParameters).GetRange(1, resolveCommandWithParameters.Length - 1).ToArray();
                    Console.WriteLine("Read Command: " + resolveCommandWithParameters[0]);
                    Console.WriteLine("    With Parameters: " + string.Join(" ", parameters));
                    Console.WriteLine();
                    try
                    {
                        command = createCommand(commandName, parameters);
                        command.Execute(parameters);
                    }
                    catch (ArgumentException e)
                    {
                        Console.WriteLine("***ERROR***");
                        Console.WriteLine("*" + e.Message + "*");
                        Console.WriteLine();
                    }
                }
            }
        }

        public void ExecuteCommand(string commandName, string[] parameters)
        {
            Console.WriteLine("*****************************");
            if (commandName != null && commandName.Trim() != "")
            {
                Console.WriteLine("Read Command: " + commandName);
                Console.WriteLine("    With Parameters: " + string.Join(" ", parameters));
                Console.WriteLine();
                try
                {
                    ICommand command = createCommand(commandName, parameters);
                    command.Execute(parameters);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine("***ERROR***");
                    Console.WriteLine("*" + e.Message + "*");
                    Console.WriteLine();
                }

            }
        }

        private ICommand createCommand(string commandName, string[] parameters)
        {
            ICommand command;
            string masterServerUrl;

            switch (commandName)
            {
                case "StartClient":
                    // find the server url to which im going to connect
                    masterServerUrl = findServerToConnect(parameters[2]);
                    if (masterServerUrl != "")
                    {
                        StartClient cli = new StartClient();
                        cli.masterServerUrl = masterServerUrl;
                        cli.processesPCS = processesPCS;
                        command = cli;
                        saveProcessPCS(parameters[0], parameters[1]);
                    } else
                    {
                        throw new ArgumentException(String.Format("Couldn't start client because server with PID: '{0}' doesn't exist!", parameters[2]));
                    }
                    break;
                case "StartServer":
                    StartServer sv = new StartServer();
                    sv.processesPCS = processesPCS;
                    command = sv;
                    saveProcessPCS(parameters[0], parameters[1]);
                    this.masterServersUrl.Add(parameters[0], parameters[2]); // save pid - server url
                    break;
                case "Replicate":
                    // find the server url to which im going to connect
                    masterServerUrl = findServerToConnect(parameters[2]);
                    if (masterServerUrl != "")
                    {
                        Replicate rep = new Replicate();
                        rep.masterServerUrl = masterServerUrl;
                        command = rep;
                        saveProcessPCS(parameters[0], parameters[1]);
                    }
                    else
                    {
                        throw new ArgumentException(String.Format("Couldn't start replica because server with PID: '{0}' doesn't exist!", parameters[2]));
                    }
                    break;
                case "GlobalStatus":
                    GlobalStatus gs = new GlobalStatus();
                    gs.processesPCS = processesPCS;
                    command = gs;
                    break;
                case "Crash":
                    Crash c = new Crash();
                    c.processesPCS = processesPCS;
                    command = c;
                    break;
                case "Freeze":
                    Freeze f = new Freeze();
                    f.processesPCS = processesPCS;
                    command = f;
                    break;
                case "Unfreeze":
                    Unfreeze u = new Unfreeze();
                    u.processesPCS = processesPCS;
                    command = u;
                    break;
                case "InjectDelay":
                    InjectDelay id = new InjectDelay();
                    id.processesPCS = processesPCS;
                    command = id;
                    break;
                case "LocalState":
                    LocalState ls = new LocalState();
                    ls.processesPCS = processesPCS;
                    command = ls;
                    break;
                case "Wait":
                    command = new Wait();
                    break;

                default:
                    throw new ArgumentException("Invalid command!");
            }
            return command;
        }

        private void saveProcessPCS(string pid, string url)
        {
            IProcessCreationService pcs = (IProcessCreationService)Activator.GetObject(
                typeof(IProcessCreationService),
                url);
            // pid, pcs remove service
            this.processesPCS.Add(pid, pcs);
        }

        private string findServerToConnect(string PID)
        {            
            foreach (KeyValuePair<string, string> server in this.masterServersUrl)
            {
                // do something with entry.Value or entry.Key
                if(server.Key == PID)
                {
                    return server.Value; // return server url
                }
            }
            return "";
        }

    }
}
