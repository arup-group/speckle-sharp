;defining variables
#define AppName      "Speckle@Arup v2 Auto Updater"
#define AppPublisher "Speckle@Arup"
#define AppURL       "https://speckle.arup.com"
#define SpeckleFolder "{localappdata}\Speckle"
#define UpdaterFilename       "SpeckleUpdater.exe"

[Setup]
AppId="3629c643-aad8-4e2e-ad3e-2d0c35bbe86f"
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
OutputBaseFilename=Speckle@ArupAutoUpdater
SetupIconFile=..\Installer\Updater\favicon.ico
Compression=lzma
SolidCompression=yes
ChangesAssociations=yes
PrivilegesRequired=lowest
VersionInfoVersion={#AppVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: updates; Description: "Notify me when a new version of Speckle is available!";

[Dirs]
Name: "{app}"; Permissions: everyone-full

[Files]
;updater
Source: "Updater\bin\Release\*"; DestDir: "{#SpeckleFolder}"; Flags: ignoreversion recursesubdirs;

[Icons]
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{group}\Check for Speckle Updates"; Filename: "{#SpeckleFolder}\{#UpdaterFilename}"; Parameters: "-showprogress"
Name: "{userappdata}\Microsoft\Windows\Start Menu\Programs\Startup\Speckle@Arup"; Filename: "{#SpeckleFolder}\{#UpdaterFilename}"; Tasks: updates

