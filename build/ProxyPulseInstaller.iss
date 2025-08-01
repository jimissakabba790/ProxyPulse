#define MyAppName "ProxyPulse"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "ProxyPulse Team"
#define MyAppURL "https://github.com/jimissakabba790/ProxyPulse"
#define MyAppExeName "ProxyPulse.exe"

[Setup]
AppId={{A1B2C3D4-E5F6-4747-8899-AABBCCDDEEFF}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=..\LICENSE
OutputDir=..\artifacts
OutputBaseFilename=ProxyPulse-Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64
UninstallDisplayIcon={app}\{#MyAppExeName}
SetupIconFile=..\src\ProxyPulse.UI\ProxyPulse.ico

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Main application
Source: "..\src\ProxyPulse.UI\bin\Release\net7.0-windows\win-x64\publish\ProxyPulse.exe"; DestDir: "{app}"; Flags: ignoreversion

; WinDivert driver files
Source: "..\drivers\WinDivert64.sys"; DestDir: "{app}\drivers"; Flags: ignoreversion
Source: "..\drivers\WinDivert.dll"; DestDir: "{app}\drivers"; Flags: ignoreversion

; Configuration file
Source: "..\src\ProxyPulse.UI\appsettings.json"; DestDir: "{userappdata}\{#MyAppName}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Install and start WinDivert driver
Filename: "sc.exe"; Parameters: "create WinDivert type= kernel binPath= ""{app}\drivers\WinDivert64.sys"""; Flags: runhidden
Filename: "sc.exe"; Parameters: "start WinDivert"; Flags: runhidden

; Run the application
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Stop and remove WinDivert driver
Filename: "sc.exe"; Parameters: "stop WinDivert"; Flags: runhidden
Filename: "sc.exe"; Parameters: "delete WinDivert"; Flags: runhidden

[UninstallDelete]
Type: filesandordirs; Name: "{userappdata}\{#MyAppName}"
Type: filesandordirs; Name: "{app}"

[Code]
function InitializeSetup(): Boolean;
begin
  Result := True;
  
  // Check if running on 64-bit Windows
  if not IsWin64 then
  begin
    MsgBox('This application requires a 64-bit Windows system.', mbError, MB_OK);
    Result := False;
  end;
end;
