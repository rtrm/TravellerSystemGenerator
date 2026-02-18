; Inno Setup Script for Traveller System Generator v2.0.0
; Requires Inno Setup 6.0 or later (download from https://jrsoftware.org/isinfo.php)

#define MyAppName "Traveller System Generator"
#define MyAppVersion "2.0.0"
#define MyAppPublisher "RTRM"
#define MyAppURL "https://github.com/rtrm/TravellerSystemGenerator"
#define MyAppExeName "TravellerSystemsGenerator.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
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
OutputBaseFilename=TravellerSystemGenerator-{#MyAppVersion}-Setup
SetupIconFile=
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
Name: "addtopath"; Description: "Add to PATH environment variable"; GroupDescription: "Additional options:"; Flags: unchecked

[Files]
Source: "TravellerSystemsGenerator\bin\Release\net10.0\win-x64\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Parameters: "-h"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: postinstall nowait skipifsilent

[Code]
const
    EnvironmentKey = 'Environment';

procedure AddToPath();
var
    Paths: string;
    InstallPath: string;
    ResultCode: Integer;
begin
    InstallPath := ExpandConstant('{app}');

    // Get current PATH from registry
    if RegQueryStringValue(HKEY_CURRENT_USER, EnvironmentKey, 'Path', Paths) then
    begin
        // Check if already in PATH
        if Pos(';' + Uppercase(InstallPath) + ';', ';' + Uppercase(Paths) + ';') = 0 then
        begin
            // Add to PATH
            if Paths <> '' then
                Paths := Paths + ';' + InstallPath
            else
                Paths := InstallPath;

            RegWriteStringValue(HKEY_CURRENT_USER, EnvironmentKey, 'Path', Paths);

            // Notify system of environment change
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
WelcomeLabel2=This will install [name/ver] on your computer.%n%nThis is a self-contained application that includes all required .NET components. No additional software installation is required.%n%nIt is recommended that you close all other applications before continuing.
