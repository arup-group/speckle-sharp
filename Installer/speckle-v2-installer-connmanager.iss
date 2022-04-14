;defining variables
#define AppName      "Speckle@Arup v2 AccountManager"

#define AppPublisher "Speckle@Arup"
#define AppURL       "https://speckle.arup.com"
#define SpeckleFolder "{localappdata}\Speckle"
#define AnalyticsFolder "{localappdata}\SpeckleAnalytics"      
#define AnalyticsFilename       "analytics.exe"

[Setup]
AppId={{96fff0bc-c9f2-4654-868c-1aeb9554c0c5}
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
OutputBaseFilename=Speckle@ArupInstaller-v{#AppVersion}
SetupIconFile=..\Installer\ConnectionManager\SpeckleConnectionManagerUI\Assets\favicon.ico
Compression=lzma
SolidCompression=yes
ChangesAssociations=yes
PrivilegesRequired=lowest
VersionInfoVersion={#AppVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl" 

[Messages]
WelcomeLabel2=This will install [name/ver] on your computer.%n%nIt is recommended that you close all other applications before continuing.%n%nThis installer is intended for Arup staff only, and will replace the Speckle Systems / AEC Systems Ltd Speckle Manager with Arup's own account manager.

[Components]
Name: connectionmanager; Description: Speckle@Arup ConnectionManager - v{#AppVersion};  Types: full; Flags: fixed

[Types]
Name: "full"; Description: "Full installation"

[Tasks]
Name: updates; Description: "Auto update, make sure I always have the best Speckle!";

[Dirs]
Name: "{app}"; Permissions: everyone-full

[Files]
;connectionmanager
Source: "ConnectionManager\SpeckleConnectionManager\bin\Release\net5.0\win10-x64\*"; DestDir: "{userappdata}\speckle-connection-manager\"; Flags: ignoreversion recursesubdirs; Components: connectionmanager
Source: "ConnectionManager\SpeckleConnectionManagerUI\bin\Release\net5.0\win10-x64\*"; DestDir: "{userappdata}\speckle-connection-manager-ui\"; Flags: ignoreversion recursesubdirs; Components: connectionmanager

;analytics
Source: "Analytics\bin\Release\net461\*"; DestDir: "{#AnalyticsFolder}"; Flags: ignoreversion recursesubdirs;

[UninstallDelete]
Type: filesandordirs; Name: "{userappdata}\speckle-connection-manager"
Type: filesandordirs; Name: "{userappdata}\speckle-connection-manager-ui"

[Registry]
; Set url protocol to save auth details
Root: HKCU; Subkey: "Software\Classes\speckle"; ValueType: "string"; ValueData: "URL:speckle"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\speckle"; ValueType: "string"; ValueName: "URL Protocol"; ValueData: ""
Root: HKCU; Subkey: "Software\Classes\speckle\DefaultIcon"; ValueType: "string"; ValueData: "{userappdata}\speckle-connection-manager\SpeckleConnectionManager.exe,0"
Root: HKCU; Subkey: "Software\Classes\speckle\shell\open\command"; ValueType: "string"; ValueData: """{userappdata}\speckle-connection-manager\SpeckleConnectionManager.exe"" ""%1"""

[Icons]
Name: "{group}\Speckle@Arup AccountManager"; Filename: "{userappdata}\speckle-connection-manager-ui\SpeckleConnectionManagerUI.exe";

[Run]
Filename: "{userappdata}\speckle-connection-manager-ui\SpeckleConnectionManagerUI.exe"; Description: "Authenticate with the Speckle Server"; Flags: nowait postinstall skipifsilent
Filename: "{#AnalyticsFolder}\analytics.exe"; Parameters: "{#AppVersion} {#GetEnv('ENABLE_TELEMETRY_DOMAIN')} {#GetEnv('POSTHOG_API_KEY')}"; Description: "Send anonymous analytics to Arup. No project data or personally identifiable information will be sent."

;checks if minimun requirements are met
[Code]
function IsDotNetDetected(version: string; service: cardinal): boolean;
// Indicates whether the specified version and service pack of the .NET Framework is installed.
//
// version -- Specify one of these strings for the required .NET Framework version:
//    'v1.1.4322'     .NET Framework 1.1
//    'v2.0.50727'    .NET Framework 2.0
//    'v3.0'          .NET Framework 3.0
//    'v3.5'          .NET Framework 3.5
//    'v4\Client'     .NET Framework 4.0 Client Profile
//    'v4\Full'       .NET Framework 4.0 Full Installation
//    'v4.7'          .NET Framework 4.5
//
// service -- Specify any non-negative integer for the required service pack level:
//    0               No service packs required
//    1, 2, etc.      Service pack 1, 2, etc. required
var
    key: string;
    install, release, serviceCount: cardinal;
    check47, success: boolean;
begin
    // .NET 4.5 installs as update to .NET 4.0 Full
    if version = 'v4.7' then begin
        version := 'v4\Full';
        check47 := true;
    end else
        check47 := false;

    // installation key group for all .NET versions
    key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + version;

    // .NET 3.0 uses value InstallSuccess in subkey Setup
    if Pos('v3.0', version) = 1 then begin
        success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
    end else begin
        success := RegQueryDWordValue(HKLM, key, 'Install', install);
    end;

    // .NET 4.0/4.5 uses value Servicing instead of SP
    if Pos('v4', version) = 1 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
    end else begin
        success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
    end;

    // .NET 4.5 uses additional value Release
    if check47 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Release', release);
        success := success and (release >= 378389);
    end;

    result := success and (install = 1) and (serviceCount >= service);
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{localappdata}\Programs\speckle-manager\Uninstall SpeckleManager.exe'), '/currentuser /S', '', SW_HIDE, ewWaitUntilTerminated, ResultCode)
end;
