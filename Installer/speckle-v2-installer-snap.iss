;defining variables
#define AppName      "Speckle@Arup v2 SNAP Connector"
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
OutputBaseFilename=Speckle@ArupSNAPConnector-v{#AppVersion}
SetupIconFile=..\ConnectorSNAP\ConnectorSNAP\icon.ico
Compression=lzma
SolidCompression=yes
ChangesAssociations=yes
PrivilegesRequired=lowest
VersionInfoVersion={#AppVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Components]
Name: SNAP; Description: Speckle for SNAP (s8i files) - v{#AppVersion};  Types: full
Name: kits; Description: Speckle Kit;  Types: full custom; Flags: fixed

[Types]
Name: "full"; Description: "Full installation"
Name: "custom"; Description: "Custom installation"; Flags: iscustom

[Dirs]
Name: "{app}"; Permissions: everyone-full

[Files]
;SNAP
Source: "..\ConnectorSNAP\ConnectorSNAP\bin\Release\*"; DestDir: "{userappdata}\Arup\SpeckleSNAP\"; Flags: ignoreversion recursesubdirs; Components: SNAP
Source: "..\Objects\Converters\ConverterSNAP\ConverterSNAP\bin\Release\Objects.Converter.SNAP.dll"; DestDir: "{userappdata}\Speckle\Kits\Objects"; Flags: ignoreversion recursesubdirs; Components: SNAP

;kits
Source: "..\Objects\Objects\bin\Release\netstandard2.0\Objects.dll"; DestDir: "{userappdata}\Speckle\Kits\Objects"; Flags: ignoreversion recursesubdirs; Components: kits

[InstallDelete]
Type: filesandordirs; Name: "{userappdata}\Arup\SpeckleSNAP\*"
Type: files; Name: "{userappdata}\Speckle\Kits\Objects\Objects.Converter.SNAP.dll"

[Icons]
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{userappdata}\Microsoft\Windows\Start Menu\Programs\Arup\SpeckleSNAPV2"; Filename: "{userappdata}\Arup\SpeckleSNAP\ConnectorSNAP.exe";
