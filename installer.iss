; Inno Setup Script for Traveller Genesis v4.0.0
; Requires Inno Setup 6.0 or later (download from https://jrsoftware.org/isinfo.php)

#define MyAppName "Traveller Genesis"
#define MyAppVersion "4.0.0"
#define MyAppPublisher "Roy Martin"
#define MyAppURL "https://github.com/rtrm/TravellerGenesis"
#define MyAppExeName "TravellerGenesis.exe"
#define MyCLIExeName "TravellerGenesisCLI.exe"

[Setup]
AppId={{8F4A7C3D-2B1E-4F9A-A5C8-9D6E3B2F1A7C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=LICENSE
OutputDir=installer_output
OutputBaseFilename=TravellerGenesis-{#MyAppVersion}-Setup
SetupIconFile=TravellerGenesis\TravellerGenesis.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "addtopath"; Description: "Add CLI to PATH environment variable"; GroupDescription: "Additional options:"; Flags: unchecked

[Files]
Source: "TravellerGenesis\bin\Release\net10.0-windows\win-x64\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "TravellerSystemsGenerator\bin\Release\net10.0\win-x64\publish\{#MyCLIExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "TravellerGenesis\TravellerGenesis.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autoprograms}\{#MyAppName} CLI"; Filename: "{app}\{#MyCLIExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: postinstall nowait skipifsilent

[Code]
const
    EnvironmentKey = 'Environment';
    WM_SETTINGCHANGE = $001A;

procedure AddToPath();
var
    Paths: string;
    InstallPath: string;
    ResultCode: Integer;
begin
    InstallPath := ExpandConstant('{app}');

    if RegQueryStringValue(HKEY_CURRENT_USER, EnvironmentKey, 'Path', Paths) then
    begin
        if Pos(';' + Uppercase(InstallPath) + ';', ';' + Uppercase(Paths) + ';') = 0 then
        begin
            if Paths <> '' then
                Paths := Paths + ';' + InstallPath
            else
                Paths := InstallPath;

            RegWriteStringValue(HKEY_CURRENT_USER, EnvironmentKey, 'Path', Paths);
            SendBroadcastMessage(WM_SETTINGCHANGE, 0, 0);
        end;
    end;
end;

procedure RemoveFromPath();
var
    Paths: string;
    InstallPath: string;
    P: Integer;
begin
    InstallPath := ExpandConstant('{app}');

    if RegQueryStringValue(HKEY_CURRENT_USER, EnvironmentKey, 'Path', Paths) then
    begin
        P := Pos(';' + Uppercase(InstallPath) + ';', ';' + Uppercase(Paths) + ';');
        if P > 0 then
        begin
            Delete(Paths, P - 1, Length(InstallPath) + 1);
            RegWriteStringValue(HKEY_CURRENT_USER, EnvironmentKey, 'Path', Paths);
            SendBroadcastMessage(WM_SETTINGCHANGE, 0, 0);
        end;
    end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
    if CurStep = ssPostInstall then
    begin
        if WizardIsTaskSelected('addtopath') then
        begin
            AddToPath();
        end;
    end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
    if CurUninstallStep = usPostUninstall then
    begin
        RemoveFromPath();
    end;
end;

[Messages]
WelcomeLabel1=Welcome to the [name/ver] Setup Wizard
WelcomeLabel2=This will install [name/ver] on your computer.%n%nIncludes both the Traveller Genesis GUI application and the Traveller Genesis CLI tool. Both are self-contained and require no additional software.%n%nIt is recommended that you close all other applications before continuing.
