# Browser Bookmarks for PowerToys Command Palette

PowerToys Command Palette에서 Edge, Chrome, Firefox 북마크를 자동으로 읽어 바로 검색하고 여는 확장입니다.

## 동작 방식

- Edge/Chrome: `%LOCALAPPDATA%\Microsoft\Edge\User Data`, `%LOCALAPPDATA%\Google\Chrome\User Data` 아래 각 프로필의 `Bookmarks` JSON을 읽습니다.
- Firefox: `%APPDATA%\Mozilla\Firefox\profiles.ini`로 프로필을 찾고 `places.sqlite`를 읽습니다.
- 브라우저 데이터는 읽기 전용으로만 접근합니다.
- 북마크 파일의 수정 시간이나 설정이 바뀌면 캐시를 새로 만듭니다.
- 기본값으로 북마크를 Command Palette 홈의 top-level command로 노출하므로, 확장 안으로 들어가지 않고 `네이버`처럼 바로 검색할 수 있습니다.

## 설정

Command Palette의 확장 설정에서 다음을 조정할 수 있습니다.

- Microsoft Edge 연동
- Google Chrome 연동
- Mozilla Firefox 연동
- 모든 프로필 포함 여부
- 북마크를 Command Palette 홈 화면에 직접 노출할지 여부
- 홈 화면에 노출할 최대 북마크 수
- Portable Chromium 계열 브라우저의 추가 User Data 폴더

## 빌드

현재 워크스페이스에는 .NET SDK가 설치되어 있지 않아 여기서 직접 빌드는 확인하지 못했습니다. Windows 개발 환경에서 다음이 필요합니다.

- .NET 9 SDK
- PowerToys Command Palette
- 배포용 설치 파일을 만들려면 Inno Setup 6

```powershell
cd D:\Projects\CmdPalBrowserBookmarks
dotnet restore .\CmdPalBrowserBookmarks.csproj
dotnet publish .\CmdPalBrowserBookmarks.csproj -c Release -r win-x64 --self-contained true
```

설치 파일까지 만들려면:

```powershell
.\build-exe.ps1 -Version "0.1.0.0" -Platforms @("x64")
```

개발 중에는 Visual Studio에서 실행 인자를 `-RegisterProcessAsComServer`로 두고 실행한 뒤 Command Palette에서 `Reload Command Palette extensions`를 실행하세요.

## 참고한 공식 문서

- Command Palette 확장 개요: https://learn.microsoft.com/windows/powertoys/command-palette/extensibility-overview
- C# 확장 만들기: https://learn.microsoft.com/windows/powertoys/command-palette/creating-an-extension
- 확장 설정: https://learn.microsoft.com/windows/powertoys/command-palette/adding-extension-settings
- WinGet/설치 배포: https://learn.microsoft.com/windows/powertoys/command-palette/publish-extension-winget
