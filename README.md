# Browser Bookmarks for PowerToys Command Palette

PowerToys Command Palette에서 브라우저 북마크를 바로 검색하고 여는 확장입니다.

Edge, Chrome, Firefox에 저장된 북마크를 읽어 오기 때문에 Command Palette에 북마크를 따로 등록할 필요가 없습니다.

## 주요 기능

- Edge, Chrome, Firefox 북마크 자동 로드
- 브라우저 프로필 선택 지원
- Command Palette 메인 검색창에서 북마크 추천
- `??` 같은 별칭으로 확장 내부 검색 가능
- Enter로 기본 브라우저에서 바로 열기
- URL 기준 중복 북마크 제거
- 브라우저에서 추가/수정한 북마크를 캐시 변경 감지로 갱신
- URL 복사, 제목 복사 컨텍스트 명령 지원
- 별칭 경로에서 열어도 브라우저가 앞쪽에 표시되도록 지연 실행 처리

## 지원 브라우저

| 브라우저 | 읽는 데이터 |
| --- | --- |
| Microsoft Edge | Chromium `Bookmarks` 파일 |
| Google Chrome | Chromium `Bookmarks` 파일 |
| Mozilla Firefox | `places.sqlite` |

Portable Chromium 계열 브라우저는 설정에서 User Data 폴더를 추가할 수 있습니다.

## Command Palette 설정

`확장 > Browser Bookmarks`에서 설정할 수 있습니다.

- Microsoft Edge
- Google Chrome
- Mozilla Firefox
- All browser profiles
- Suggest matching bookmarks on the Command Palette home page
- Additional Chromium user data folders

메인 검색창에서 바로 북마크를 보려면 `대체 명령 > Search browser bookmarks`를 켜고, `전역 결과에 포함`도 켜세요.

## 빌드

필요한 도구:

- Visual Studio 2026 또는 Windows SDK 포함 빌드 환경
- .NET SDK
- PowerToys Command Palette

```powershell
cd D:\Projects\CmdPalBrowserBookmarks
dotnet restore .\CmdPalBrowserBookmarks.csproj
dotnet publish .\CmdPalBrowserBookmarks.csproj -c Release -r win-x64 --self-contained true --output .\bin\x64\Release\net9.0-windows10.0.26100.0\win-x64
```

## 개발 등록

개발 중에는 MSIX AppExtension 방식으로 등록합니다.

```powershell
Add-AppxPackage -Register .\bin\x64\Release\net9.0-windows10.0.26100.0\win-x64\AppxManifest.xml -DisableDevelopmentMode
```

등록 후 Command Palette에서 `reload`를 실행하거나 Command Palette를 다시 시작합니다.

## 테스트

1. Command Palette 열기
2. `reload` 실행
3. `네이버` 같은 북마크 이름 검색
4. 결과가 1개만 표시되는지 확인
5. Enter로 기본 브라우저에서 열리는지 확인
6. 별칭을 설정했다면 `??`로 들어가서 같은 검색/열기 테스트

## 프로젝트 구조

| 경로 | 역할 |
| --- | --- |
| `Bookmarks/` | 브라우저 북마크 탐색, 읽기, 캐시, 검색 점수 |
| `Commands/` | 북마크 열기, fallback 검색, 새로고침 명령 |
| `Pages/` | 북마크 목록 페이지와 설정 페이지 |
| `Settings/` | Command Palette 확장 설정 |
| `Package.appxmanifest` | Command Palette AppExtension 등록 정보 |

## 참고

현재 권장 등록 방식은 MSIX AppExtension입니다. Inno Setup 방식은 일반 설치 파일 제작에는 사용할 수 있지만, Command Palette 확장 검색/등록에는 MSIX AppExtension 방식이 더 안정적입니다.

