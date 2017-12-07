using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuppetMaster.Commands;
using Shared;
using System.Threading;

namespace PuppetMaster
{
    public class PlatformOrchestration
    {
        // MAP PID - PCS   remote service of server and replicas
        private Dictionary<String, IProcessCreationService> processesPCS;
        
        //Map PID - URI
        private Dictionary<string, string> serverURIs = new Dictionary<string, string>();

        private string msecRound = "20";
        private string numPlayers = "2";


        public PlatformOrchestration()
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
                resolveCommandWithParameters = line.Split(' ');
                commandName = resolveCommandWithParameters[0];
                //check if the command is a string and if is a comment
                if (commandName != null && commandName.Trim() != "" && !resolveCommandWithParameters[0].StartsWith("#"))
                {
                    parameters = new List<String>(resolveCommandWithParameters).GetRange(1, resolveCommandWithParameters.Length - 1).ToArray();
                    Console.WriteLine("Read Command: " + resolveCommandWithParameters[0]);
                    Console.WriteLine("    With Parameters: " + string.Join(" ", parameters));
                    Console.WriteLine();
                    try
                    {
                        execute(commandName, parameters);
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
                    execute(commandName, parameters);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine("***ERROR***");
                    Console.WriteLine("*" + e.Message + "*");
                    Console.WriteLine();
                }
            }
        }

        private void execute(string commandName, string[] parameters)
        {

            switch (commandName)
            {
                case "Configure":
                    msecRound = parameters[0];
                    numPlayers = parameters[1];
                    break;
                case "CreateServer":
                    saveProcessPCS(parameters[0], parameters[1]);
                    serverURIs.Add(parameters[0], parameters[2]);
                    break;
                case "StartServers":
                    foreach(string pid in this.serverURIs.Keys)
                    {
                        StartServer(pid, serverURIs[pid], this.serverURIs.Values.ToList(), msecRound, numPlayers);
                    }
                    break;
                case "StartClient":
                    // find the server url to which im going to connect
                    if (parameters.Length < 4) // there is not instructions
                    {
                        StartClientNotInstructed(parameters[0], parameters[2], serverURIs.Values.ToList());
                        saveProcessPCS(parameters[0], parameters[1]);
                        return;
                    }

                    string instructions = "";
                    string uniformFilepath = @"../../scripts/" + parameters[3];
                    // check if the file exists
                    if (File.Exists(uniformFilepath))
                    {
                        Console.WriteLine("*** Reading client moves trace file ***");
                        string[] lines = File.ReadAllLines(uniformFilepath);
                        foreach (string line in lines)
                        {
                            instructions += line + "\n";
                        }
                        Console.WriteLine("*** Finished reading client moves trace file ***");
                    }
          
                    if (instructions != "")
                    {
                        StartClientInstructed(parameters[0], parameters[2], instructions, serverURIs.Values.ToList());
                        saveProcessPCS(parameters[0], parameters[1]);
                        return;
                    }
                    else
                    {
                        Console.WriteLine("No file or no instructions");
                        return;
                    }

                    break;
                
                case "GlobalStatus":
                    GlobalStatus gs = new GlobalStatus();
                    gs.processesPCS = processesPCS;
                    gs.Execute(parameters);
                    break;
                case "Crash":
                    Crash c = new Crash();
                    c.processesPCS = processesPCS;
                    c.Execute(parameters);
                    break;
                case "Freeze":
                    Freeze f = new Freeze();
                    f.processesPCS = processesPCS;
                    f.Execute(parameters);
                    break;
                case "Unfreeze":
                    Unfreeze u = new Unfreeze();
                    u.processesPCS = processesPCS;
                    u.Execute(parameters);
                    break;
                case "InjectDelay":
                    InjectDelay id = new InjectDelay();
                    id.processesPCS = processesPCS;
                    id.Execute(parameters);
                    break;
                case "LocalState":
                    LocalState ls = new LocalState();
                    ls.processesPCS = processesPCS;
                    ls.Execute(parameters);
                    break;
                case "Wait":
                    new Wait().Execute(parameters);
                    break;

                default:
                    throw new ArgumentException("Invalid command!");
            }
        }

        private delegate void StartServerDelegate(string PID, string serverURL, string msecPerRound, string numPlayers, List<string> serverURLs);

        void StartServer(string PID, string serverURI, List<string> serverURIs, string msecPerRound, string numPlayers)
        {
            Thread t = new Thread(delegate ()
            {
                StartServerDelegate remoteCallStartServer;
                remoteCallStartServer = new StartServerDelegate(processesPCS[PID].StartServer);
                IAsyncResult asyncResult = remoteCallStartServer.BeginInvoke(PID, serverURI, msecRound, numPlayers, serverURIs, null, null);
                asyncResult.AsyncWaitHandle.WaitOne();
            });
            t.Start();
        }

        private delegate void StartClientDelegate(string PID, string clientURL, List<string> serverURLs);
        void StartClientNotInstructed(string PID, string clientURI, List<string> serverURIs)
        {
            Thread t = new Thread(delegate ()
            {
                StartClientDelegate remote = new StartClientDelegate(processesPCS[PID].StartClient);
                IAsyncResult asyncResult = remote.BeginInvoke(PID, clientURI, serverURIs, null, null);
                asyncResult.AsyncWaitHandle.WaitOne();
            });
            t.Start();
        }

        private delegate void StartClientWithInstructionsDelegate(string PID, string clientURL, string instructions, List<string> serverURLs);
        void StartClientInstructed(string PID, string clientURI, string instructions, List<string> serverURIs)
        {
            Thread t = new Thread(delegate ()
            {
                StartClientWithInstructionsDelegate remote = new StartClientWithInstructionsDelegate(processesPCS[PID].StartClient);
                IAsyncResult asyncResult = remote.BeginInvoke(PID, clientURI, instructions, serverURIs, null, null);
                asyncResult.AsyncWaitHandle.WaitOne();
            });
            t.Start();
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
