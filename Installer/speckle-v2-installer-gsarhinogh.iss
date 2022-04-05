	;defining variables
#define AppName      "Speckle@Arup v2 GSA Connector + Rhino/Gh Connectors"
#define AppVersion  GetFileVersion("..\ConnectorGSA\ConnectorGSA\bin\Release\ConnectorGSA.exe")

#define Rhino6Version  GetFileVersion("..\ConnectorRhino\ConnectorRhino6\bin\Release\SpeckleConnectorRhino.rhp")
#define Rhino7Version  GetFileVersion("..\ConnectorRhino\ConnectorRhino7\bin\Release\SpeckleConnectorRhino.rhp")
#define GrasshopperVersion  GetFileVersion("..\ConnectorGrasshopper\ConnectorGrasshopper\bin\SpeckleConnectorGrasshopper.dll")

#define AppPublisher "Speckle@Arup"
#define AppURL       "https://speckle.arup.com"
#define SpeckleFolder "{localappdata}\Speckle"

[Setup]
AppId="f1c9752c-a655-4787-b821-573259e9f3bf"
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
OutputBaseFilename=Speckle@ArupGSARhinoGhConnector-v{#AppVersion}
SetupIconFile=..\ConnectorGSA\ConnectorGSA\icon.ico
Compression=lzma
SolidCompression=yes
ChangesAssociations=yes
PrivilegesRequired=lowest
VersionInfoVersion={#AppVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Components]
Name: gsa; Description: Speckle for Oasys GSA - v{#AppVersion};  Types: full
Name: rhino6; Description: Speckle for Rhino 6 - v{#Rhino6Version};  Types: full
Name: rhino7; Description: Speckle for Rhino 7 - v{#Rhino7Version};  Types: full
Name: gh; Description: Speckle for Grasshopper - v{#GrasshopperVersion};  Types: full
Name: kits; Description: Speckle Kit - v{#AppVersion};  Types: full custom; Flags: fixed

[Types]
Name: "full"; Description: "Full installation"
Name: "custom"; Description: "Custom installation"; Flags: iscustom

[Dirs]
Name: "{app}"; Permissions: everyone-full

[Files]
;gsa
Source: "..\ConnectorGSA\ConnectorGSA\bin\Release\*"; DestDir: "{userappdata}\Oasys\SpeckleGSA\"; Flags: ignoreversion recursesubdirs; Components: gsa
Source: "..\Objects\Converters\ConverterGSA\ConverterGSA\bin\Release\Objects.Converter.GSA.dll"; DestDir: "{userappdata}\Speckle\Kits\Objects"; Flags: ignoreversion recursesubdirs; Components: gsa

;rhino6                                                                                                                                    
Source: "..\ConnectorRhino\ConnectorRhino6\bin\Release\*"; DestDir: "{userappdata}\McNeel\Rhinoceros\6.0\Plug-ins\SpeckleRhino2 (8dd5f30b-a13d-4a24-abdc-3e05c8c87143)\"; Flags: ignoreversion recursesubdirs; Components: rhino6

;rhino7
Source: "..\ConnectorRhino\ConnectorRhino7\bin\Release\*"; DestDir: "{userappdata}\McNeel\Rhinoceros\7.0\Plug-ins\SpeckleRhino2 (8dd5f30b-a13d-4a24-abdc-3e05c8c87143)\"; Flags: ignoreversion recursesubdirs; Components: rhino7

;gh
Source: "..\ConnectorGrasshopper\ConnectorGrasshopper\bin\net472\*"; DestDir: "{userappdata}\Grasshopper\Libraries\SpeckleGrasshopper2\"; Flags: ignoreversion recursesubdirs; Components: gh
Source: "..\Objects\Converters\ConverterRhinoGh\ConverterRhino6\bin\Release\netstandard2.0\Objects.Converter.Rhino6.dll"; DestDir: "{userappdata}\Speckle\Kits\Objects\"; Flags: ignoreversion recursesubdirs; Components: rhino6 gh
Source: "..\Objects\Converters\ConverterRhinoGh\ConverterRhino7\bin\Release\net48\Objects.Converter.Rhino7.dll"; DestDir: "{userappdata}\Speckle\Kits\Objects\"; Flags: ignoreversion recursesubdirs; Components: rhino7 gh

;kits
Source: "..\Objects\Objects\bin\Release\netstandard2.0\Objects.dll"; DestDir: "{userappdata}\Speckle\Kits\Objects"; Flags: ignoreversion recursesubdirs; Components: kits

[InstallDelete]
Type: filesandordirs; Name: "{userappdata}\Oasys\SpeckleGSA\*"
Type: files; Name: "{userappdata}\Speckle\Kits\Objects\Objects.Converter.GSA.dll"
Type: files; Name: "{userappdata}\Speckle\Kits\Objects\Objects.Converter.Rhino6.dll"
Type: files; Name: "{userappdata}\Speckle\Kits\Objects\Objects.Converter.Rhino7.dll"
Type: filesandordirs; Name: "{userappdata}\McNeel\Rhinoceros\6.0\Plug-ins\SpeckleRhino2 (8dd5f30b-a13d-4a24-abdc-3e05c8c87143)\*"
Type: filesandordirs; Name: "{userappdata}\McNeel\Rhinoceros\7.0\Plug-ins\SpeckleRhino2 (8dd5f30b-a13d-4a24-abdc-3e05c8c87143)\*"
Type: filesandordirs; Name: "{userappdata}\Grasshopper\Libraries\SpeckleGrasshopper2\*"

[Registry]
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: string; ValueName: "Name"; ValueData: "Speckle";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: string; ValueName: "RegPath"; ValueData: "\\HKEY_CURRENT_USER\Software\McNeel\Rhinoceros\6.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: dword; ValueName: "DirectoryInstall"; ValueData: "0";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: dword; ValueName: "IsDotNETPlugIn"; ValueData: "1";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: dword; ValueName: "LoadMode"; ValueData: "2";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: dword; ValueName: "Type"; ValueData: "16";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143\CommandList"; ValueType: string; ValueName: "Speckle"; ValueData: "2;Speckle";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\6.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143\PlugIn"; ValueType: string; ValueName: "FileName"; ValueData: "{userappdata}\McNeel\Rhinoceros\6.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143\SpeckleConnectorRhino.rhp";  
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: string; ValueName: "Name"; ValueData: "Speckle";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: string; ValueName: "RegPath"; ValueData: "\\HKEY_CURRENT_USER\Software\McNeel\Rhinoceros\7.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: dword; ValueName: "DirectoryInstall"; ValueData: "0";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: dword; ValueName: "IsDotNETPlugIn"; ValueData: "1";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: dword; ValueName: "LoadMode"; ValueData: "2";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143"; ValueType: dword; ValueName: "Type"; ValueData: "16";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143\CommandList"; ValueType: string; ValueName: "Speckle"; ValueData: "2;Speckle";
Root: HKCU; Subkey: "SOFTWARE\McNeel\Rhinoceros\7.0\Plug-ins\8dd5f30b-a13d-4a24-abdc-3e05c8c87143\PlugIn"; ValueType: string; ValueName: "FileName"; ValueData: "{userappdata}\McNeel\Rhinoceros\7.0\Plug-ins\SpeckleRhino2 (8dd5f30b-a13d-4a24-abdc-3e05c8c87143)\SpeckleConnectorRhino.rhp";  

[Icons]
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{userappdata}\Microsoft\Windows\Start Menu\Programs\Oasys\SpeckleGSAV2"; Filename: "{userappdata}\Oasys\SpeckleGSA\ConnectorGSA.exe";
