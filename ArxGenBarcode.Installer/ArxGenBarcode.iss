#ifndef UNICODE
#error Unicode Inno Setup is required to compile this script
#endif

#define MyAppName "ArxGenBarcode"
#define MyBuildDir "..\ArxGenBarcode\Bin\Release"

#define MySetupBaseName "setup_ArxGenBarcode_"

#define MyAppExeName "ArxGenBarcode.exe"

;Get vesion
#dim Version[4]
#expr ParseVersion(MyBuildDir + "\" + MyAppExeName, Version[0], Version[1], Version[2], Version[3])
#define MyAppVersion Str(Version[0]) + "." + Str(Version[1]) + "." + Str(Version[2]) + "." + Str(Version[3])

[Setup]
AppId={{E611011D-03C9-49D1-8D79-5EF1053F1D6B}
AppName="{#MyAppName}"
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher=Ivan Lezhnev
AppPublisherURL=https://vk.com/arxont
DefaultGroupName={#MyAppName}
PrivilegesRequired=lowest
OutputBaseFilename={#MySetupBaseName}{#MyAppVersion}
Compression=lzma
SolidCompression=yes
DefaultDirName={userdocs}\{#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
SetupIconFile=..\{#MyAppName}\icon.ico

DisableReadyPage=no
DisableReadyMemo=no

ShowLanguageDialog=no
AppContact=info@itchita.ru
MinVersion=0,6.1

[Files]
Source: "{#MyBuildDir}\*.exe"; DestDir: "{app}"; Flags: ignoreversion ; Permissions: everyone-full
Source: "{#MyBuildDir}\*.dll"; DestDir: "{app}"; Flags: ignoreversion ; Permissions: everyone-full

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{userdesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Run]  
Filename: "{app}\{#MyAppExeName}"; Flags: nowait postinstall skipifsilent runasoriginaluser; Description: "Запустить после установки?"

[UninstallRun]
Filename: {sys}\taskkill.exe; Parameters: "/f /im {#MyAppExeName}"; Flags: skipifdoesntexist runhidden

[Code]
#include "scripts\UnInstallPrevision.iss"

procedure CurStepChanged(CurStep: TSetupStep);
begin
   UnInstallPrevision(CurStep);                    
end;