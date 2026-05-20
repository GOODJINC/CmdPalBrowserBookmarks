# Browser Bookmarks for Command Palette

[English](README.md) | 한국어

Microsoft PowerToys Command Palette에서 브라우저 북마크를 바로 검색하고 열 수 있는 확장입니다.

Browser Bookmarks는 Microsoft Edge, Google Chrome, Mozilla Firefox에 저장된 북마크를 읽어오므로 Command Palette에 같은 북마크를 다시 등록할 필요가 없습니다.

[Privacy Policy](PRIVACY.md) | [개인정보처리방침](PRIVACY.ko.md)

## 설치

Microsoft Store 출시를 준비 중입니다.

Store 페이지가 공개되면 이곳에 Store 링크를 추가할 예정입니다.

현재는 최신 GitHub Release를 사용할 수 있습니다.

https://github.com/GOODJINC/CmdPalBrowserBookmarks/releases

## 주요 기능

- Edge, Chrome, Firefox 북마크 검색
- Command Palette 홈 화면에서 북마크 검색
- 기본 브라우저 또는 선택한 브라우저로 북마크 열기
- 특정 프로필 또는 여러 프로필 선택
- 한국어/영어 UI 지원
- `ㄴㅇㅂ`로 `네이버`를 찾는 한글 초성 검색
- 북마크 결과에서 URL, 제목, Markdown 링크 복사
- 브라우저 변경 후 북마크 캐시 새로고침
- 로컬 전용 북마크 처리

## 지원 브라우저

| 브라우저 | 데이터 원본 |
| --- | --- |
| Microsoft Edge | Chromium `Bookmarks` 파일 |
| Google Chrome | Chromium `Bookmarks` 파일 |
| Mozilla Firefox | Firefox `places.sqlite` 데이터베이스 |

Portable Chromium 계열 브라우저는 사용자 지정 User Data 폴더를 설정해 추가할 수 있습니다.

## 사용 방법

1. 확장을 설치합니다.
2. PowerToys Command Palette를 엽니다.
3. 확장이 바로 보이지 않으면 `reload`를 실행합니다.
4. 북마크 이름, URL, 한글 초성을 검색합니다.
5. `Enter`를 눌러 선택한 북마크를 엽니다.

Command Palette 홈 화면에서 북마크 검색 결과를 바로 보려면 다음 옵션을 켜세요.

```text
Extensions > Browser Bookmarks > Fallback command > Include in global results
```

## 설정

아래 위치에서 설정할 수 있습니다.

```text
Command Palette Settings > Extensions > Browser Bookmarks
```

주요 설정:

- 사용할 브라우저
- 북마크를 열 브라우저
- 언어
- 홈 화면 추천
- 한글 초성 검색
- 브라우저 프로필 선택
- Portable Chromium User Data 폴더

## 단축키

| 단축키 | 동작 |
| --- | --- |
| `Enter` | 북마크 열기 |
| `Shift` + `Enter` | 새 브라우저 창에서 열기 |
| `Ctrl` + `C` | URL 복사 |
| `Ctrl` + `Shift` + `C` | 제목 복사 |
| `Ctrl` + `Alt` + `C` | Markdown 링크 복사 |

## 개인정보

북마크 데이터는 사용자 기기에서 로컬로 읽고 처리됩니다.

이 확장은 사용자의 북마크 데이터를 개발자 또는 외부 서버로 업로드, 판매, 공유, 전송하지 않습니다.

자세한 내용은 [PRIVACY.ko.md](PRIVACY.ko.md)를 확인하세요.

## 개발

필요한 도구:

- Windows 11
- Command Palette가 포함된 Microsoft PowerToys
- .NET SDK
- Windows SDK / Visual Studio 빌드 도구

빌드:

```powershell
dotnet restore .\CmdPalBrowserBookmarks.csproj
dotnet build .\CmdPalBrowserBookmarks.csproj -c Release -r win-x64 --self-contained true
```

Store MSIX 패키지 생성:

```powershell
dotnet build .\CmdPalBrowserBookmarks.csproj --configuration Release -p:GenerateAppxPackageOnBuild=true -p:Platform=x64 -p:AppxPackageDir="AppPackages\Store\x64\"
dotnet build .\CmdPalBrowserBookmarks.csproj --configuration Release -p:GenerateAppxPackageOnBuild=true -p:Platform=ARM64 -p:AppxPackageDir="AppPackages\Store\ARM64\"
```

## 프로젝트 구조

| 경로 | 역할 |
| --- | --- |
| `Bookmarks/` | 브라우저 북마크 탐색, 파싱, 캐시, 검색 |
| `Commands/` | 북마크 동작과 Command Palette 명령 |
| `Pages/` | Command Palette 페이지 |
| `Settings/` | 확장 설정 |
| `Localization/` | 영어/한국어 문자열 |
| `Assets/` | 앱 및 Store 아이콘 |
| `Package.appxmanifest` | MSIX 및 Command Palette 확장 manifest |
