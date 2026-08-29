#define MyAppName "Nexa Tweaks"
#define MyAppVersion "0.1.0"
#define MyAppPublisher "Nexa Tweaks"
#define MyAppExeName "NexaTweaks.exe"

[Setup]
AppId={{5F0D9C36-9C1E-4B7E-9C0B-2A9C7B8C6E11}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\dist
OutputBaseFilename=NexaTweaks-Setup-0.1.0
SetupIconFile=..\src\NexaTweaks.App\Assets\icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
SignedUninstaller=yes
; "signtool" must be configured once per machine in the Inno Setup IDE:
; Tools > Configure Sign Tools > add a tool named "signtool" with a command like:
;   signtool.exe sign /f "C:\path\to\NexaTweaksCodeSign.pfx" /p $qCODESIGN_PASSWORD$q /fd SHA256 /tr http://timestamp.digicert.com /td SHA256 $f
; (Inno Setup expands $f to the file path; using an env var keeps the password out of this script.)
SignTool=signtool

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "Crear un acceso directo en el Escritorio"; GroupDescription: "Accesos directos adicionales:"

[Files]
Source: "..\publish\*.exe"; DestDir: "{app}"; Flags: ignoreversion signonce
Source: "..\publish\*.dll"; DestDir: "{app}"; Flags: ignoreversion signonce
Source: "..\publish\*.json"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Desinstalar {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; NexaTweaks.exe's own manifest requires elevation (requireAdministrator). Setup already runs
; elevated, so by default it would launch this via CreateProcess with its own admin token - but
; CreateProcess never honors a target's requireAdministrator manifest and fails with error 740
; (ERROR_ELEVATION_REQUIRED) even though the caller is already admin. "runascurrentuser" makes
; Inno Setup launch it via ShellExecute as the original user instead, which does read the
; manifest and triggers a proper UAC prompt.
Filename: "{app}\{#MyAppExeName}"; Description: "Ejecutar {#MyAppName} ahora"; Flags: nowait postinstall skipifsilent runascurrentuser
