using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Diagnostics;
using System.Security;
using Microsoft.EnterpriseManagement;
using Microsoft.EnterpriseManagement.Administration;
using Microsoft.EnterpriseManagement.Common;
using Microsoft.EnterpriseManagement.Configuration;
using Microsoft.EnterpriseManagement.Monitoring;
using Microsoft.EnterpriseManagement.Runtime;
using CommandLine.Utility;
using CryptPassword;

namespace SCOMalerts
{
    class LogHandle
    {
        private static string filelog = ConfigurationManager.AppSettings["logfile"];
        private static System.Random Rand = new Random();
        private static Int32 SessionId = Rand.Next(100000000, 200000000);

        public void WriteOut(string line)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filelog, true))
            {
                file.WriteLine(DateTime.Now.ToString() + " " + SessionId + ": " + line);
                file.Flush();
                file.Close();
            }
        }

    }
    class SCOMsdk
    {
        private static readonly string SCOMuser = ConfigurationManager.AppSettings["SCOMuser"];
        private static readonly string SCOMpasswd = ConfigurationManager.AppSettings["SCOMpasswd"];  // this is the encrypted file.
        private static readonly string Domain = ConfigurationManager.AppSettings["Domain"];
        private static readonly string SCOMserver = ConfigurationManager.AppSettings["SCOMserver"];
        private static readonly bool debug = bool.Parse(ConfigurationManager.AppSettings["debug"]);
        public string EventId = "X";
        public string SCOMinfo(string AlertText, string AlertObj, int LevelId, int EventNumber, string ObjType)
        {
            ManagementGroup mg;
            LogHandle loghandle = new LogHandle();
            string fullAgentName = AlertObj;

            try
            {
                // function to connect to SCOMserver

                // Define the server that you want to connect to.
                string serverName = SCOMserver;

                string name = SCOMuser;
                string userDomain = Domain;

                // encrypted password file read
                string SCOMpasswdEn;
                try
                {
                    SCOMpasswdEn = System.IO.File.ReadAllText(SCOMpasswd); //store the password file name in the App.config
                    loghandle.WriteOut("SCOM password retrieved");
                }
                catch (Exception e)
                {
                    loghandle.WriteOut("Encrypted password retrieval failed for file " + SCOMpasswd + " Error: " + e.Message);
                    throw new InvalidOperationException("Encrypted password retrieval failed for file " + SCOMpasswd + " Error: " + e.Message);
                }
                string dee = CryptPassword.AESEncryptionUtility.Decrypt(SCOMpasswdEn, "Encrypt_password"); //return System.Text.Encoding.Unicode.GetString(decryptedData);
                //Console.WriteLine("de: {0}   en: {1}", dee, SCOMpasswdEn);

                // Get password.
                SecureString password = new SecureString();
                char[] passwordChars = dee.ToCharArray();
                foreach (char c in passwordChars)
                {
                    password.AppendChar(c);
                }

                try
                {
                    ManagementGroupConnectionSettings mgSettings = new ManagementGroupConnectionSettings(serverName);
                    mgSettings.UserName = name;
                    mgSettings.Domain = userDomain;
                    mgSettings.Password = password;
                    loghandle.WriteOut("Connecting to the SDK Service as user: " + name + " on server: " + SCOMserver + " pwd: " + password);

                    //ManagementGroup mg = ManagementGroup.Connect(mgSettings);
                    mg = ManagementGroup.Connect(mgSettings);
                    
                    if (mg.IsConnected)
                    {
                        loghandle.WriteOut("Connection succeeded.(1)");
                        loghandle.WriteOut("Connected " + mg.IsConnected + "  Name: " + mg.Name);

                        // Retrieve all Windows, Unix, and Network nodes
                        try
                        {
                            if (debug) Console.WriteLine("Inserting custom event for " + fullAgentName);
                            loghandle.WriteOut("Inserting custom event for " + fullAgentName);

                            //string query = "Name = 'Microsoft.Windows.Computer'";
                            //ManagementPackClassCriteria WinClassCriteria = new ManagementPackClassCriteria(query);

                            // Microsoft.Windows.Computer  Microsoft.unix.computer  System.NetworkManagement.Node
                            //string query = "Name = 'Microsoft.Windows.Server.2003.Computer'";
                            // Get the windows Server monitoring class.
                            //Microsoft.EnterpriseManagement.Configuration.MonitoringClassCriteria;

                            // There should only be one item in the monitoringClasses collection.

                            //IList<ManagementPackClass> monitoringClasses = mg.EntityTypes.GetClasses();   // All classes

                            List<MonitoringObject> targets = new List<MonitoringObject>();

                            // Get all instances of windows computers from the management group.
                            if (ObjType == "Windows")
                            {
                                ManagementPackClassCriteria WinClassCriteria = new ManagementPackClassCriteria("Name = 'Microsoft.Windows.Computer'");
                                //ManagementPackClassCriteria WinClassCriteria = new ManagementPackClassCriteria("Name = 'Microsoft.Windows.Server.OperatingSystem'");
                                IList<ManagementPackClass> monitoringClasses = mg.EntityTypes.GetClasses(WinClassCriteria);
                                //List<MonitoringObject> targets = new List<MonitoringObject>();
                                IObjectReader<MonitoringObject> reader = mg.EntityObjects.GetObjectReader<MonitoringObject>(monitoringClasses[0], ObjectQueryOptions.Default);
                                targets.AddRange(reader);
                                loghandle.WriteOut("Retrieved Windows Targets in SCOM");
                            }
                            // Get all instances of unix computers from the management group.
                            // Not using this at this time
                            if (ObjType == "Unix") { 
                                ManagementPackClassCriteria UnixClassCriteria = new ManagementPackClassCriteria("Name = 'Microsoft.unix.computer'");
                                IList<ManagementPackClass> monitoringClasses = mg.EntityTypes.GetClasses(UnixClassCriteria);
                                //List<MonitoringObject> targets = new List<MonitoringObject>();
                                IObjectReader<MonitoringObject> reader = mg.EntityObjects.GetObjectReader<MonitoringObject>(monitoringClasses[0], ObjectQueryOptions.Default);
                                targets.AddRange(reader);
                                loghandle.WriteOut("Retrieved Unix Targets in SCOM");
                            }

                            if (ObjType == "Node")
                            {
                                // Get all instances of network nodes from the management group
                                ManagementPackClassCriteria NetClassCriteria = new ManagementPackClassCriteria("Name = 'System.NetworkManagement.Node'");
                                IList<ManagementPackClass> monitoringClasses = mg.EntityTypes.GetClasses(NetClassCriteria);
                                if (monitoringClasses.Count != 1)
                                    throw new InvalidOperationException("Expected one monitoring class object for System.NetworkManagement.Node");
                                //List<MonitoringObject> targets = new List<MonitoringObject>();
                                IObjectReader<MonitoringObject> reader = mg.EntityObjects.GetObjectReader<MonitoringObject>(monitoringClasses[0], ObjectQueryOptions.Default);
                                targets.AddRange(reader);
                                loghandle.WriteOut("Retrieved Node Targets in SCOM");
                            }
                            if (ObjType == "NetApp")
                            {
                                // Get all instances of network nodes from the management group
                                ManagementPackClassCriteria NetClassCriteria = new ManagementPackClassCriteria("Name = 'WSH.NETAPP.Volume.Class'");
                                IList<ManagementPackClass> monitoringClasses = mg.EntityTypes.GetClasses(NetClassCriteria);
                                if (monitoringClasses.Count != 1)
                                    throw new InvalidOperationException("Expected one monitoring class object for WSH.NETAPP.Volume.Class");
                                //List<MonitoringObject> targets = new List<MonitoringObject>();
                                IObjectReader<MonitoringObject> reader = mg.EntityObjects.GetObjectReader<MonitoringObject>(monitoringClasses[0], ObjectQueryOptions.Default);
                                targets.AddRange(reader);
                                loghandle.WriteOut("Retrieved Node Targets in SCOM");
                            }

                            int MOIndex = 0;
                            if (targets.Count > 0)
                            {
                                foreach (MonitoringObject obj in targets)
                                {
                                    //if (fullAgentName == (obj.Path))     // for windows servers
                                    //int result = string.Compare(fullAgentName, obj.Path, StringComparison.CurrentCultureIgnoreCase);
                                    //if (fullAgentName == (obj.DisplayName))    // for windows Computers,Node, Unix/Linux Computer
                                    int result = string.Compare(fullAgentName, obj.DisplayName, StringComparison.CurrentCultureIgnoreCase);
                                    if ( result == 0)    // for windows Computers,Node, Unix/Linux Computer
                                    {
                                        if (debug) Console.WriteLine("Monitoring Object DisplayName: " + obj.DisplayName + "   Index: " + MOIndex);   // + Environment.NewLine);
                                        if (debug) Console.WriteLine("Monitoring Object FullName: " + obj.FullName);
                                        if (debug) Console.WriteLine("Monitoring Object ID: " + obj.Id);
                                        if (debug) Console.WriteLine("Monitoring Object Name:" + obj.Name);
                                        if (debug) Console.WriteLine("Monitoring Object Path:" + obj.Path);
                                        loghandle.WriteOut("Monitoring Object DisplayName: " + obj.DisplayName + "   Index: " + MOIndex);   // + Environment.NewLine);
                                        loghandle.WriteOut("Monitoring Object FullName: " + obj.FullName);
                                        loghandle.WriteOut("Monitoring Object ID: "+obj.Id);
                                        loghandle.WriteOut("Monitoring Object Name:"+obj.Name);
                                        loghandle.WriteOut("Monitoring Object Path:"+obj.Path);
                                        loghandle.WriteOut("Found Target " + fullAgentName);
                                        break;
                                    }
                                    ++MOIndex;
                                }

                            }
                            loghandle.WriteOut("Object index is : " + MOIndex);
                            // Generate alert for a specific node using --> CustomMonitoringEventMessage;
                            CustomMonitoringEvent monitoringEvent = new CustomMonitoringEvent("WSH.Alert", EventNumber);
                            //CustomMonitoringEvent monitoringEvent = new CustomMonitoringEvent(fullAgentName, EventNumber);
                            monitoringEvent.Channel = "Application";
                            //monitoringEvent.LoggingComputer = fullAgentName;    // "AnyMachine";
                            monitoringEvent.LoggingComputer = targets[MOIndex].DisplayName;
                            monitoringEvent.Message = new CustomMonitoringEventMessage(AlertText);  //AlertText
                            monitoringEvent.User = "AnyUser";
                            monitoringEvent.LevelId = LevelId;
                            // 1-Error, 2-Warning, 4-Information, 8-Success Audit, 16-Failure Audit.
                            //monitoringEvent.EventData = "test event data";

                            if ( MOIndex > targets.Count - 1)
                                throw new InvalidOperationException("the monitoring object was not found " + fullAgentName);

                            // Insert the event in the Operations Manager database
                             targets[MOIndex].InsertCustomMonitoringEvent(monitoringEvent);
                            EventId = monitoringEvent.OriginalId.ToString();
                            loghandle.WriteOut("Successfully inserted custom event. "+EventId);
                            if (debug) Console.WriteLine("Successfully inserted custom event.");
                        }
                        catch (Exception e)
                        {
                            loghandle.WriteOut("Error retrieving Node info or generating event " + e.Message);
                            throw new InvalidOperationException("Error retrieving Node info or generating event " + e.Message);
                        }
                    }
                    else
                    {
                        loghandle.WriteOut("Not connected to an SDK Service.");
                        throw new InvalidOperationException("Not connected to an SDK Service.");
                    }
                }
                catch (ServerDisconnectedException sde)
                {
                    loghandle.WriteOut("SCOM Connection failed. " + sde.Message);
                    if (sde.InnerException != null)
                        loghandle.WriteOut(sde.InnerException.Message);
                }
                catch (ServiceNotRunningException snr)
                {
                    loghandle.WriteOut(snr.Message);
                }
            }  // end of very first try.  Catchall for an error
            catch (Exception e)
            {
                loghandle.WriteOut("Error connecting to SCOM " + e.Message);
                throw new InvalidOperationException("Error connecting to SCOM " + e.Message);
            }
            return EventId;
        }
    }
    class Program
    {
        static int Main(string[] args)
        {
            LogHandle loghandle = new LogHandle();
            SCOMsdk SCOMcon = new SCOMsdk();
            bool debug = bool.Parse(ConfigurationManager.AppSettings["debug"]);

            // Make sure we have arguments on the command line
            if (args.Length <= 0)
            {
                loghandle.WriteOut("No paramters were specified for this Application.  Use -help for complete details of parameters to add.");
                if (debug) Console.WriteLine("No paramters were specified for this Application.  Use -help for complete details of parameters to add.");
                return 1;
            }

            // Parse the command line arguments and store in MainArgs
            Arguments MainArgs = new Arguments(args);

            // iterate through the command line parameters and write to file
            foreach (KeyValuePair<string, string> item in MainArgs.ParamsAll()) { loghandle.WriteOut("Key: " + item.Key + "\tValue: " + item.Value); }
 
            // iterate through the command line parameters and write to console
            foreach (KeyValuePair<string, string> item in MainArgs.ParamsAll()) { if (debug) Console.WriteLine("Key: {0}\t\tValue: {1}", item.Key, item.Value); }            

            // Check if help was specified
            if (MainArgs["help"] == "true" || MainArgs["?"] == "true") { if (debug) Console.WriteLine("\n"); HelpInfo(); return 0; }

            // These parameters are passed to the application.
            string Node = MainArgs["svr"];
            string Alert = MainArgs["msg"];
            int Level = int.Parse(MainArgs["level"]);
            int EventNumber = int.Parse(MainArgs["eventNumber"]);
            string Type = MainArgs["type"];
            if (Type == "Windows" || Type == "Unix" || Type == "Node" || Type == "NetApp")
            {
                loghandle.WriteOut("Found a valid node type: "+Type);
            } else
            {
                Type = "Node";
                loghandle.WriteOut("No valid node type, default to: " + Type);
            }
            if (debug) Console.WriteLine("Node type is: "+Type);

            // LevelId: 1-Error, 2-Warning, 4-Information, 8-Success Audit, 16-Failure Audit.
            loghandle.WriteOut("Beginning to create SCOM event");
            loghandle.WriteOut("Node: " + Node + "   Level: " +Level+"  Alert: "+Alert);
            if (debug) Console.WriteLine("Beginning to create SCOM Event");
            if (debug) Console.WriteLine("Node: " + Node + "   Level: " + Level + "  Alert: " + Alert);
            string CIN = SCOMcon.SCOMinfo(Alert, Node, Level, EventNumber, Type);

            loghandle.WriteOut("AlertText: " + Alert + "  Node: " + Node + "  CIN: " + CIN);

            //if(debug) System.Threading.Thread.Sleep(5000);
            //if (debug) Console.WriteLine("Press and key to continue");
            //if (debug) Console.ReadKey();

            return 0;
        }
        static void HelpInfo()
        {
            // Help Display
            string HelpMsg = Environment.GetCommandLineArgs()[0] + "\n";
            HelpMsg += @"Application for creating Alerts in SCOM for the Specififed node (FQDN).
The Command Options are as Follows:
    -help --help /help -? --? /? - will display the help message.
    -svr    - FQDN of the server for the alert
    -msg    - Alert detail to be displayed
    -level  - 1, 2 4 (critical, Warning, Informational)
    -eventNumber - 8100 for testing
    -type   - is the server type.  Valid: Node, Windows, Unix, NetApp.  Default or incorrect is Node

 *the parameters are case sensitive.
            ";
            Console.WriteLine(HelpMsg);
            //Console.ReadKey();
        }

    }
}
