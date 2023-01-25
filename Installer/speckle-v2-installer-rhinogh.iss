;defining variables
#define AppName      "Speckle@Arup v2 Rhino/Gh Connectors"

#define GrasshopperVersion  GetFileVersion("..\ConnectorRhino\ConnectorRhino7\bin\Release\win-x64\SpeckleConnectorGrasshopper.gha")
#define Rhino6Version  GetFileVersion("..\ConnectorRhino\ConnectorRhino6\bin\Release\win-x64\SpeckleConnectorRhino.rhp")
#define Rhino7Version  GetFileVersion("..\ConnectorRhino\ConnectorRhino7\bin\Release\win-x64\SpeckleConnectorRhino.rhp")

#define AppPublisher "Speckle@Arup"
#define AppURL       "https://speckle.arup.com"
#define SpeckleFolder "{localappdata}\Speckle"

[Setup]
AppId="4b0fc912-7e13-48d2-8284-eb1b54f73779"
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName={#SpeckleFolder}
DisableDirPage=yes
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
DisableWelcomePage=no
OutputDir="."
OutputBaseFilename=Speckle@ArupInstallerRhinoGh-v{#AppVersion}
SetupIconFile=..\Installer\ConnectionManager\SpeckleConnectionManagerUI\Assets\favicon.ico
Compression=lzma
SolidCompression=yes
ChangesAssociations=yes
PrivilegesRequired=lowest
VersionInfoVersion={#AppVersion}


[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Components]
Name: rhinogh6; Description: Speckle for Rhino 6 and Grasshoppper - v{#Rhino6Version};  Types: full
Name: rhinogh7; Description: Speckle for Rhino 7 and Grasshoppper - v{#Rhino7Version};  Types: full
Name: kits; Description: Speckle Kits - v;  Types: full custom; Flags: fixed

[Types]
Name: "full"; Description: "Full installation"
Name: "custom"; Description: "Custom installation"; Flags: iscustom

[Dirs]
Name: "{app}"; Permissions: everyone-full

[Files]
;rhinogh6                                                                                                                                    
Source: "..\ConnectorRhino\ConnectorRhino6\bin\Release\win-x64\*"; DestDir: "{userappdata}\McNeel\Rhinoceros\6.0\Plug-ins\SpeckleRhino2 (8dd5f30b-a13d-4a24-abdc-3e05c8c87143)\"; Flags: ignoreversion recursesubdirs; Components: rhinogh6
Source: "..\ConnectorRhino\ConnectorRhino\Toolbars\SpeckleConnectorRhino.rui"; DestDir: "{userappdata}\McNeel\Rhinoceros\6.0\Plug-ins\SpeckleRhino2 (8dd5f30b-a13d-4a24-abdc-3e05c8c87143)\"; Flags: ignoreversion recursesubdirs; Components: rhinogh6
Source: "..\Objects\Converters\ConverterRhinoGh\ConverterRhino6\bin\Release\netstandard2.0\Objects.Converter.Rhino6.dll"; DestDir: "{userappdata}\Speckle\Kits\Objects\"; Flags: ignoreversion recursesubdirs; Components: rhinogh6
Source: "..\Objects\Converters\ConverterRhinoGh\ConverterGrasshopper6\bin\Release\netstandard2.0\Objects.Converter.Grasshopper6.dll"; DestDir: "{userappdata}\Speckle\Kits\Objects\"; Flags: ignoreversion recursesubdirs; Components: rhinogh6

;rhinogh7
Source: "..\ConnectorRhino\ConnectorRhino7\bin\Release\win-x64\*"; DestDir: "{userappdata}\McNeel\Rhinoceros\7.0\Plug-ins\SpeckleRhino2 (8dd5f30b-a13d-4a24-abdc-3e05c8c87143)\"; Flags: ignoreversion recursesubdirs; Components: rhinogh7
Source: "..\ConnectorRhino\ConnectorRhino\Toolbars\SpeckleConnectorRhino.rui"; DestDir: "{userappdata}\McNeel\Rhinoceros\7.0\Plug-ins\SpeckleRhino2 (8dd5f30b-a13d-4a24-abdc-3e05c8c87143)\"; Flags: ignoreversion recursesubdirs; Components: rhinogh7
Source: "..\Objects\Converters\ConverterRhinoGh\ConverterRhino7\bin\Release\net48\Objects.Converter.Rhino7.dll"; DestDir: "{userappdata}\Speckle\Kits\Objects\"; Flags: ignoreversion recursesubdirs; Components: rhinogh7
Source: "..\Objects\Converters\ConverterRhinoGh\ConverterGrasshopper7\bin\Release\net48\Objects.Converter.Grasshopper7.dll"; DestDir: "{userappdata}\Speckle\Kits\Objects\"; Flags: ignoreversion recursesubdirs; Components: rhinogh7

;kits
Source: "..\Objects\Objects\bin\Release\netstandard2.0\Objects.dll"; DestDir: "{userappdata}\Speckle\Kits\Objects"; Flags: ignoreversion recursesubdirs; Components: kits

[InstallDelete]
Type: filesandordirs; Name: "{userappdata}\McNeel\Rhinoceros\6.0\Plug-ins\SpeckleRhino2 (8dd5f30b-a13d-4a24-abdc-3e05c8c87143)"
Type: filesandordirs; Name: "{userappdata}\McNeel\Rhinoceros\7.0\Plug-ins\SpeckleRhino2 (8dd5f30b-a13d-4a24-abdc-3e05c8c87143)"
Type: filesandordirs; Name: "{userappdata}\Grasshopper\Libraries\SpeckleGrasshopper2"

[Registry]
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\d648bb69-b992-4d34-906e-7a547374b86c"; ValueType: string; ValueName: "Name"; ValueData: "ConnectorRhino6"; 
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\d648bb69-b992-4d34-906e-7a547374b86c"; ValueType: string; ValueName: "RegPath"; ValueData: "\\HKEY_CURRENT_USER\Software\McNeel\Rhinoceros\6.0\Plug-ins\d648bb69-b992-4d34-906e-7a547374b86c";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\d648bb69-b992-4d34-906e-7a547374b86c"; ValueType: string; ValueName: "RuiFile"; ValueData: "{userappdata}\McNeel\Rhinoceros\6.0\Plug-ins\SpeckleRhino2 (8dd5f30b-a13d-4a24-abdc-3e05c8c87143)\SpeckleConnectorRhino.rui";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\d648bb69-b992-4d34-906e-7a547374b86c"; ValueType: dword; ValueName: "AddToHelpMenu"; ValueData: "0";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\d648bb69-b992-4d34-906e-7a547374b86c"; ValueType: dword; ValueName: "DirectoryInstall"; ValueData: "0";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\d648bb69-b992-4d34-906e-7a547374b86c"; ValueType: dword; ValueName: "IsDotNETPlugIn"; ValueData: "1";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\d648bb69-b992-4d34-906e-7a547374b86c"; ValueType: dword; ValueName: "LoadMode"; ValueData: "1";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\d648bb69-b992-4d34-906e-7a547374b86c"; ValueType: dword; ValueName: "Type"; ValueData: "16";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\d648bb69-b992-4d34-906e-7a547374b86c\CommandList"; ValueType: string; ValueName: "Speckle"; ValueData: "2;Speckle";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\d648bb69-b992-4d34-906e-7a547374b86c\PlugIn"; ValueType: string; ValueName: "FileName"; ValueData: "{userappdata}\McNeel\Rhinoceros\6.0\Plug-ins\SpeckleRhino2 (8dd5f30b-a13d-4a24-abdc-3e05c8c87143)\SpeckleConnectorRhino.rhp";  
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\a64acbf9-db82-4839-af99-57ed2e7989f4"; ValueType: string; ValueName: "Name"; ValueData: "ConnectorRhino7";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\a64acbf9-db82-4839-af99-57ed2e7989f4"; ValueType: string; ValueName: "RegPath"; ValueData: "\\HKEY_CURRENT_USER\Software\McNeel\Rhinoceros\7.0\Plug-ins\a64acbf9-db82-4839-af99-57ed2e7989f4";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\a64acbf9-db82-4839-af99-57ed2e7989f4"; ValueType: string; ValueName: "RuiFile"; ValueData: "{userappdata}\McNeel\Rhinoceros\7.0\Plug-ins\SpeckleRhino2 (8dd5f30b-a13d-4a24-abdc-3e05c8c87143)\SpeckleConnectorRhino.rui";  
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\a64acbf9-db82-4839-af99-57ed2e7989f4"; ValueType: dword; ValueName: "AddToHelpMenu"; ValueData: "0";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\a64acbf9-db82-4839-af99-57ed2e7989f4"; ValueType: dword; ValueName: "DirectoryInstall"; ValueData: "0";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\a64acbf9-db82-4839-af99-57ed2e7989f4"; ValueType: dword; ValueName: "IsDotNETPlugIn"; ValueData: "1";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\a64acbf9-db82-4839-af99-57ed2e7989f4"; ValueType: dword; ValueName: "LoadMode"; ValueData: "1";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\a64acbf9-db82-4839-af99-57ed2e7989f4"; ValueType: dword; ValueName: "Type"; ValueData: "16";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\a64acbf9-db82-4839-af99-57ed2e7989f4\CommandList"; ValueType: string; ValueName: "Speckle"; ValueData: "2;Speckle";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\a64acbf9-db82-4839-af99-57ed2e7989f4\PlugIn"; ValueType: string; ValueName: "FileName"; ValueData: "{userappdata}\McNeel\Rhinoceros\7.0\Plug-ins\SpeckleRhino2 (8dd5f30b-a13d-4a24-abdc-3e05c8c87143)\SpeckleConnectorRhino.rhp";  

Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\d648bb69-b992-4d34-906e-7a547374b86c"; Flags: uninsdeletekey;
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\a64acbf9-db82-4839-af99-57ed2e7989f4"; Flags: uninsdeletekey;

Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; Flags: deletekey;
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; Flags: deletekey;

[Icons]
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
