;defining variables
#define AppName      "Speckle@Arup v2 GSA Connector"
#define AppPublisher "Speckle@Arup"
#define AppURL       "https://docs.speckle.arup.com"
#define SpeckleFolder "{localappdata}\Speckle"

[Setup]
AppId="f9556ce4-23c7-45e7-9af6-32aeec5073ad"
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
OutputBaseFilename=Speckle@ArupGSAConnector-v{#AppVersion}
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
Name: gsa2; Description: Speckle for Oasys GSA (New UI) - v{#AppVersion};  Types: full
Name: kits; Description: Speckle Kit;  Types: full custom; Flags: fixed

[Types]
Name: "full"; Description: "Full installation"
Name: "custom"; Description: "Custom installation"; Flags: iscustom

[Dirs]
Name: "{app}"; Permissions: everyone-full

[Files]
;gsa
Source: "..\ConnectorGSA\ConnectorGSA\bin\Release\*"; DestDir: "{userappdata}\Oasys\SpeckleGSA\"; Flags: ignoreversion recursesubdirs; Components: gsa
Source: "..\Objects\Converters\ConverterGSA\ConverterGSA\bin\Release\Objects.Converter.GSA.dll"; DestDir: "{userappdata}\Speckle\Kits\Objects"; Flags: ignoreversion recursesubdirs; Components: gsa

;gsa2
Source: "..\ConnectorGSA\ConnectorGSA2\bin\Release\net48\*"; DestDir: "{userappdata}\Oasys\SpeckleGSA2\"; Flags: ignoreversion recursesubdirs; Components: gsa2
Source: "..\Objects\Converters\ConverterGSA\ConverterGSA\bin\Release\Objects.Converter.GSA.dll"; DestDir: "{userappdata}\Speckle\Kits\Objects"; Flags: ignoreversion recursesubdirs; Components: gsa2

;kits
Source: "..\Objects\Objects\bin\Release\netstandard2.0\Objects.dll"; DestDir: "{userappdata}\Speckle\Kits\Objects"; Flags: ignoreversion recursesubdirs; Components: kits

[InstallDelete]
Type: filesandordirs; Name: "{userappdata}\Oasys\SpeckleGSA\*" Components: gsa
Type: filesandordirs; Name: "{userappdata}\Oasys\SpeckleGSA2\*"; Components: gsa2
Type: files; Name: "{userappdata}\Speckle\Kits\Objects\Objects.Converter.GSA.dll" Components: gsa gsa2

[Icons]
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{userappdata}\Microsoft\Windows\Start Menu\Programs\Oasys\SpeckleGSAV2"; Filename: "{userappdata}\Oasys\SpeckleGSA\ConnectorGSA.exe";
