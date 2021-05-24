# SCOMAlerts
<h2>Generate SCOM events that can be turned into alerts</h2>

Uploaded the files.  Refer to the README.txt for description of the files.

SCOMalerts.exe, SCOMalerts.exe.config, CommandLineUtility.dll, CryptPassword.exe, CryptPassword.dll are the only files needed to generate an event in SCOM.

SCOMalerts.exe -help will give the command line parameters needed to generate the SCOM event.

SCOMalerts.exe.config needs to be confiigured for your environment.

CommandLineUtility.dll is used to handle command line parameters.

CryptPassowrd.exe is used to create the encrypted password to use with the scom admin account specified in the config file.

CryptPassword.dll is used by the CryptPassword.exe for creating the password

## Configure 
The first thing to do is create a password for the SCOM account used for the event creation.  CryptPassword.exe -e <password> will create a file **enpw**.  Rename this file.  This file is need to configure the SCOMalerts.exe to run and connect to the SCOM management server.
  
Next open the SCOMalerts.exe.config for editing.  This file containes the following information
```dos
		key="SCOMuser" value="scom.admin.user"
		key="SCOMpasswd" value="password.file"
		key="SCOMserver" value="scomserver.domain.org"
		key="Domain" value="domain.org"
		key="logfile" value=".\SCOMalerts-log.txt"
		key="debug" value="true"/>
```

SCOMuser is the user account that corresponds to the password hat was created in the previous step.

SCOMpasswd is the password file created earlier.

SCOMServer is the management server fully qualified domain name.

Once this is configured the application is ready to test.

Domain is the AD domain where the management server resides.

logfile is the location of the log file for debug and error messages to be written.

debug value is set to true or false.  This enables logging debug information to the logfile.

## Test Event

SCOMalerts -svr <Server_generating_event> -msg "Test event message" -level 4 -eventNumber 8100 -type Windows

## SCOMalerts.exe Help

```dos
The Command Options are as Follows:
    -help --help /help -? --? /? - will display the help message.
    -svr    - FQDN of the server for the alert
    -msg    - Alert detail to be displayed
    -level  - 1, 2 4 (critical, Warning, Informational)
    -eventNumber - 8100 for testing
    -type   - is the server type.  Valid: Node, Windows, Unix.  Default or incorrect is Node

 *the parameters are case sensitive.
```

## Visual Studio Code

Program.cs is the C# code used to create the application.

I have included the the full VS code that was used to create this application.  The folder SCOMalertsVScode contains the code.  Simply download this folder to your VS repo and run the sln file to load he program.  You may need to point the referense to the dll's in the SCOMalerts\bin folder.  There a 4 standard SCOM dll's that should be installed if you have setup VSAE.  CommandLine.Utility.dll and CryptPassword.dll are libraries I created to assist this program.  I use this in other programs, that is why I made them libraries.

## Management Pack to Alerts

In order to turn the vent into an alert, the event must have a rule to detect the event and generate an alert.  This MP takes advantage of this built-in functionality within SCOM.  In the MP folder there is a Custom.Event.Alert.xml file that is a self contained management pack.  Simply add this management pack to SCOM and check out the preconfigured rules.

I have also included 3 management pack fragments for different node types.  The application contains a parameter type.  This type is used for the types of objects.  The 3 types I have configured as Node, Windows, and Unix.  Each fragment is designed to work with a specific object type.
