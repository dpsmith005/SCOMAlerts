# SCOMAlerts
<h2>Generate SCOM events that can be turned into alerts</h2>
<p>I am starting to add the code and instructions for this repository.  Eventually the code and instructions for installation and use will be added.</p>

Uploaded the files.  Refer to the README.txt for description of the files.

SCOMalerts.exe, SCOMalerts.exe.config, CommandLineUtility.dll, CryptPassword.exe, CryptPassword.dll are the only files needed to generate an event in SCOM.

SCOMalerts.exe -help will give the command line parameters needed to generate the SCOM event.

SCOMalerts.exe.config needs to be confiigured for your environment.

CommandLineUtility.dll is used to handle command line parameters.

CryptPassowrd.exe is used to create the encrypted password to use with the scom admin account specified in the config file.

CryptPassword.dll is used by the CryptPassword.exe for creating the password

## Configure 
The first thing to do is create a password for the SCOM account used for the event creation.  CryptPassword.exe -e <password> will create a file **enpw**.  Rename this file.  This file iss need to configure the SCOMalerts.exe to run and connect to the SCOM management server.
  
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
    -type   - is the server type.  Valid: Node, Windows, Unix, NetApp.  Default or incorrect is Node

 *the parameters are case sensitive.
```
