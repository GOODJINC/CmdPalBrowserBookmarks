# Browser Bookmarks for Command Palette

English | [한국어](README.ko.md)

Search and open your browser bookmarks directly from Microsoft PowerToys Command Palette.

Browser Bookmarks reads bookmarks from Microsoft Edge, Google Chrome, and Mozilla Firefox, so you do not need to register the same bookmarks again inside Command Palette.

[Privacy Policy](PRIVACY.md) | [개인정보처리방침](PRIVACY.ko.md)

## Install

Microsoft Store release is being prepared.

After the Store listing is published, the Store link will be added here.

For now, use the latest GitHub release:

https://github.com/GOODJINC/CmdPalBrowserBookmarks/releases

## Features

- Search bookmarks from Edge, Chrome, and Firefox
- Search from the Command Palette home screen
- Open bookmarks with the default browser or a selected browser
- Choose browser profiles, including specific or multiple profiles
- Korean and English UI
- Korean initial consonant search, such as `ㄴㅇㅂ` for `네이버`
- Copy URL, title, or Markdown link from bookmark results
- Refresh bookmark cache after browser changes
- Local-only bookmark processing

## Supported Browsers

| Browser | Source |
| --- | --- |
| Microsoft Edge | Chromium `Bookmarks` file |
| Google Chrome | Chromium `Bookmarks` file |
| Mozilla Firefox | Firefox `places.sqlite` database |

Portable Chromium-based browsers can be added by setting custom User Data folders.

## Usage

1. Install the extension.
2. Open PowerToys Command Palette.
3. Run `reload` if the extension does not appear immediately.
4. Search for a bookmark name, URL, or Korean initial consonants.
5. Press `Enter` to open the selected bookmark.

To show bookmark matches directly on the Command Palette home screen, enable:

```text
Extensions > Browser Bookmarks > Fallback command > Include in global results
```

## Settings

Open:

```text
Command Palette Settings > Extensions > Browser Bookmarks
```

Available settings include:

- Enabled browsers
- Browser launch behavior
- Language
- Home screen suggestions
- Korean initial consonant search
- Browser profile selection
- Portable Chromium User Data folders

## Shortcuts

| Shortcut | Action |
| --- | --- |
| `Enter` | Open bookmark |
| `Shift` + `Enter` | Open in a new browser window |
| `Ctrl` + `C` | Copy URL |
| `Ctrl` + `Shift` + `C` | Copy title |
| `Ctrl` + `Alt` + `C` | Copy Markdown link |

## Privacy

Bookmark data is read and processed locally on your device.

This extension does not upload, sell, share, or transmit your bookmark data to the developer or to an external server.

See [PRIVACY.md](PRIVACY.md) for details.

## Development

Requirements:

- Windows 11
- Microsoft PowerToys with Command Palette
- .NET SDK
- Windows SDK / Visual Studio build tools

Build:

```powershell
dotnet restore .\CmdPalBrowserBookmarks.csproj
dotnet build .\CmdPalBrowserBookmarks.csproj -c Release -r win-x64 --self-contained true
```

Create Store MSIX packages:

```powershell
dotnet build .\CmdPalBrowserBookmarks.csproj --configuration Release -p:GenerateAppxPackageOnBuild=true -p:Platform=x64 -p:AppxPackageDir="AppPackages\Store\x64\"
dotnet build .\CmdPalBrowserBookmarks.csproj --configuration Release -p:GenerateAppxPackageOnBuild=true -p:Platform=ARM64 -p:AppxPackageDir="AppPackages\Store\ARM64\"
```

## Project Structure

| Path | Purpose |
| --- | --- |
| `Bookmarks/` | Browser bookmark discovery, parsing, caching, and search |
| `Commands/` | Bookmark actions and Command Palette commands |
| `Pages/` | Command Palette pages |
| `Settings/` | Extension settings |
| `Localization/` | English and Korean strings |
| `Assets/` | App and Store icons |
| `Package.appxmanifest` | MSIX and Command Palette extension manifest |
