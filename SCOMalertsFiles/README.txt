
scomadmin.pw - *** DO NOT INCLUDE when distributing.***


CryptPassword.exe - used to create password file
					-e password -- will encrypt password and store in file enpw	
					-d          -- will decrypt the file enpw

enpw - default password file

README.txt - this file

SCOMalerts.cs - code used to create the executable SCOMalerts.exe

SCOMalerts.exe - application that will generate an event in SCOM
	If you run the code without any parameters the following message will be displayed
		No paramters were specified for this Application.  Use -help for complete details of parameters to add.

SCOMalerts.exe -help
Wellspan application for creating Alerts in SCOM for the Specififed node (FQDN).
The Command Options are as Follows:
    -help --help /help -? --? /? - will display the help message.
    -svr    - FQDN of the server for the alert
    -msg    - Alert detail to be displayed
    -level  - 1, 2 4 (critical, Warning, Informational)
    -eventNumber - 8100 for testing
    -type   - is the server type.  Valid: Node, Windows, Unix, NetApp.  Default or incorrect is Node

 *the parameters are case sensitive.

SCOMalerts.exe.config - config file used by SCOMalerts.exe *** Clean before distributing ***
	Several key value pairs are used to connect to the scom server, specify the log file, and enable debug
		key="SCOMuser" value="scom.admin.user"
		key="SCOMpasswd" value="password.file"
		key="SCOMserver" value="scomserver.domain.org"
		key="Domain" value="domain.org"
		key="logfile" value=".\SCOMalerts-log.txt"
		key="debug" value="true"/>
		
SCOMalerts-log.txt - log file specified in the config file


DLL references required to compile the code listed below.  They are in the DLLs folder.

Custom DLLs
===========
CommandLine.Utility.dll 
CryptPassword.dll
SCOM DLLs
=========
Microsoft.EnterpriseManagement.Core.dll
Microsoft.EnterpriseManagement.OperationsManager.Common.dll
Microsoft.EnterpriseManagement.OperationsManager.dll
Microsoft.EnterpriseManagement.Runtime.dll
located in C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\PublicAssemblies

scomalerts.zip - USe the zip file to include VS project.  
WSHSCOMalerts - VS code folder
