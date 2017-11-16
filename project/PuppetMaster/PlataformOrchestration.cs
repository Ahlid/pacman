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
    public class PlataformOrchestration
    {
        private Dictionary<String, IProcessCreationService> processesPCS;


        public PlataformOrchestration()
        {
            this.processesPCS = new Dictionary<string, IProcessCreationService>();
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
                        command.Execute(parameters, processesPCS);
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
                    command.Execute(parameters, processesPCS);
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

            switch (commandName)
            {
                case "StartClient":
                    command = new StartClient();
                    saveProcessPCS(parameters[0], parameters[1]);
                    break;
                case "StartServer":
                    command = new StartServer();
                    saveProcessPCS(parameters[0], parameters[1]);
                    break;
                case "GlobalStatus":
                    command = new GlobalStatus();
                    break;
                case "Crash":
                    command = new Crash();
                    break;
                case "Freeze":
                    command = new Freeze();
                    break;
                case "Unfreeze":
                    command = new Unfreeze();
                    break;
                case "InjectDelay":
                    command = new InjectDelay();
                    break;
                case "LocalState":
                    command = new LocalState();
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

    }
}
