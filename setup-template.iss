#define AppVersion "0.2.3.0"

[Setup]
AppId={{C0B3B39B-7762-46CE-91B9-C19E3D2A282E}
AppName=Browser Bookmarks for Command Palette
AppVersion={#AppVersion}
AppPublisher=CmdPalBrowserBookmarks
DefaultDirName={localappdata}\Programs\CmdPalBrowserBookmarks
OutputDir=bin\Release\installer
OutputBaseFilename=CmdPalBrowserBookmarks-Setup-{#AppVersion}
Compression=lzma
SolidCompression=yes
MinVersion=10.0.22000
PrivilegesRequired=lowest
UninstallDisplayName=Browser Bookmarks for Command Palette

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "bin\Release\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\Browser Bookmarks for Command Palette"; Filename: "{app}\CmdPalBrowserBookmarks.exe"

[Registry]
Root: HKCU64; Subkey: "SOFTWARE\Classes\CLSID\{{F9BBE047-9AE1-4F40-A35D-5B8F89133E75}}"; ValueName: ""; ValueType: string; ValueData: "Browser Bookmarks for Command Palette"; Flags: uninsdeletekey
Root: HKCU64; Subkey: "SOFTWARE\Classes\CLSID\{{F9BBE047-9AE1-4F40-A35D-5B8F89133E75}}\LocalServer32"; ValueName: ""; ValueType: string; ValueData: """{app}\CmdPalBrowserBookmarks.exe"" -RegisterProcessAsComServer"; Flags: uninsdeletekey
