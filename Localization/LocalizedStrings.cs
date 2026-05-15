using System.Globalization;
using CmdPalBrowserBookmarks.Bookmarks;
using CmdPalBrowserBookmarks.Settings;

namespace CmdPalBrowserBookmarks.Localization;

internal sealed class LocalizedStrings
{
    private readonly bool _korean;

    private LocalizedStrings(bool korean)
    {
        _korean = korean;
    }

    internal static LocalizedStrings For(UiLanguage language)
    {
        return language switch
        {
            UiLanguage.Korean => new LocalizedStrings(true),
            UiLanguage.English => new LocalizedStrings(false),
            _ => new LocalizedStrings(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("ko", StringComparison.OrdinalIgnoreCase)),
        };
    }

    internal string BrowserBookmarks => _korean ? "브라우저 북마크" : "Browser Bookmarks";

    internal string BrowserBookmarkSettings => _korean ? "브라우저 북마크 설정" : "Browser Bookmark Settings";

    internal string Settings => _korean ? "설정" : "Settings";

    internal string Search => _korean ? "검색" : "Search";

    internal string SearchBrowserBookmarks => _korean ? "브라우저 북마크 검색" : "Search browser bookmarks";

    internal string SearchBrowserBookmarksPlaceholder => _korean ? "브라우저 북마크 검색" : "Search browser bookmarks";

    internal string LoadingBookmarks => _korean ? "활성화된 브라우저의 북마크를 불러오는 중" : "Loading bookmarks from enabled browsers";

    internal string BookmarksFromEnabledBrowsers(int count)
    {
        return _korean ? $"활성화된 브라우저에서 {count:N0}개 북마크" : $"{count:N0} bookmarks from enabled browsers";
    }

    internal string SettingsSubtitle => _korean
        ? "브라우저, 검색 추천, 실행 브라우저 선택"
        : "Choose browsers, suggestions, and launch browser";

    internal string AdvancedProfileSettings => _korean ? "고급 프로필 설정" : "Advanced Profile Settings";

    internal string AdvancedProfileSettingsSubtitle => _korean
        ? "브라우저별 검색 프로필과 추가 Chromium 폴더 설정"
        : "Choose per-browser search profiles and additional Chromium folders";

    internal string AdvancedProfileSettingsNotice => _korean
        ? "> 프로필 모드를 바꾼 뒤에는 **Save**를 누르고 이 화면을 다시 열어 주세요. 선택할 프로필 목록이 새 모드에 맞게 갱신됩니다."
        : "> After changing a profile mode, select **Save** and reopen this page. The profile choices will refresh for the new mode.";

    internal string RefreshBrowserBookmarks => _korean ? "브라우저 북마크 새로고침" : "Refresh Browser Bookmarks";

    internal string RefreshBrowserBookmarksSubtitle => _korean ? "활성화된 브라우저의 북마크 파일 다시 읽기" : "Reload bookmark files from enabled browsers";

    internal string Refresh => _korean ? "새로고침" : "Refresh";

    internal string RefreshNow => _korean ? "지금 새로고침" : "Refresh now";

    internal string ReloadCommandPalette => _korean ? "명령 팔레트 다시 로드" : "Reload Command Palette";

    internal string ReloadCommandPaletteSubtitle => _korean
        ? "언어와 설정 문구를 즉시 갱신합니다."
        : "Refresh language and settings labels immediately.";

    internal string FailedToReloadCommandPalette(string message)
    {
        return _korean ? $"명령 팔레트 다시 로드 실패: {message}" : $"Failed to reload Command Palette: {message}";
    }

    internal string LoadedBookmarks(int count)
    {
        return _korean ? $"브라우저 북마크 {count:N0}개를 불러왔습니다." : $"Loaded {count:N0} browser bookmarks.";
    }

    internal string FailedToRefreshBookmarks(string message)
    {
        return _korean ? $"브라우저 북마크 새로고침 실패: {message}" : $"Failed to refresh browser bookmarks: {message}";
    }

    internal string FailedToOpenBookmark(string message)
    {
        return _korean ? $"북마크 열기 실패: {message}" : $"Failed to open bookmark: {message}";
    }

    internal string Open => _korean ? "열기" : "Open";

    internal string OpenInNewWindow => _korean ? "새 창에서 열기" : "Open in new window";

    internal string CopyUrl => _korean ? "URL 복사" : "Copy URL";

    internal string CopyTitle => _korean ? "제목 복사" : "Copy title";

    internal string CopyMarkdownLink => _korean ? "Markdown 링크 복사" : "Copy Markdown link";

    internal string OpenBookmarkPrefix => _korean ? "북마크 열기" : "Open bookmark";

    internal string TypeBookmarkTitleUrlOrFolder => _korean ? "북마크 제목, URL, 폴더를 입력하세요" : "Type a bookmark title, URL, or folder";

    internal string SearchBookmarksFor(string searchText)
    {
        return _korean ? $"\"{searchText}\" 북마크 검색" : $"Search browser bookmarks for \"{searchText}\"";
    }

    internal string ContinueSearching => _korean
        ? "브라우저 북마크를 열고 계속 검색"
        : "Open Browser Bookmarks and continue searching";

    internal string UiLanguageLabel => _korean ? "UI 언어" : "UI language";

    internal string UiLanguageDescription => _korean
        ? "확장 설정과 명령에 사용할 언어를 선택합니다. 변경 후 reload를 실행하면 전체 문구가 갱신됩니다."
        : "Choose the language used for extension settings and commands. Run reload after changing it to refresh all labels.";

    internal string SystemDefault => _korean ? "시스템 기본값" : "System default";

    internal string Korean => _korean ? "한국어" : "Korean";

    internal string English => _korean ? "영어" : "English";

    internal string ReadEdgeBookmarks => _korean ? "Microsoft Edge 프로필의 북마크를 읽습니다." : "Read bookmarks from Microsoft Edge profiles.";

    internal string ReadChromeBookmarks => _korean ? "Google Chrome 프로필의 북마크를 읽습니다." : "Read bookmarks from Google Chrome profiles.";

    internal string ReadFirefoxBookmarks => _korean ? "Firefox places.sqlite 프로필의 북마크를 읽습니다." : "Read bookmarks from Firefox places.sqlite profiles.";

    internal string EdgeProfileMode => _korean ? "Microsoft Edge 프로필 모드" : "Microsoft Edge profile mode";

    internal string ChromeProfileMode => _korean ? "Google Chrome 프로필 모드" : "Google Chrome profile mode";

    internal string FirefoxProfileMode => _korean ? "Mozilla Firefox 프로필 모드" : "Mozilla Firefox profile mode";

    internal string ChooseEdgeProfileMode => _korean ? "검색할 Microsoft Edge 프로필을 선택합니다." : "Choose which Microsoft Edge profile should be searched.";

    internal string ChooseChromeProfileMode => _korean ? "검색할 Google Chrome 프로필을 선택합니다." : "Choose which Google Chrome profile should be searched.";

    internal string ChooseFirefoxProfileMode => _korean ? "검색할 Mozilla Firefox 프로필을 선택합니다." : "Choose which Mozilla Firefox profile should be searched.";

    internal string EdgeProfile => _korean ? "Microsoft Edge 프로필" : "Microsoft Edge profile";

    internal string ChromeProfile => _korean ? "Google Chrome 프로필" : "Google Chrome profile";

    internal string FirefoxProfile => _korean ? "Mozilla Firefox 프로필" : "Mozilla Firefox profile";

    internal string SpecificProfileDescription(string browser)
    {
        return _korean
            ? $"{browser} 프로필 모드가 특정 프로필일 때 사용할 프로필을 선택합니다."
            : $"Choose the profile to use when {browser} profile mode is set to Specific profile.";
    }

    internal string RecentDefaultProfile => _korean ? "최근 사용/기본 프로필" : "Recently used/default profile";

    internal string SpecificProfile => _korean ? "특정 프로필" : "Specific profile";

    internal string MultipleProfiles => _korean ? "여러 프로필 선택" : "Selected profiles";

    internal string AllProfiles => _korean ? "모든 프로필" : "All profiles";

    internal string AutomaticFallback => _korean ? "자동 fallback" : "Automatic fallback";

    internal string MultipleProfileDescription(string browser)
    {
        return _korean
            ? $"{browser} 프로필 모드가 여러 프로필 선택일 때 검색에 포함합니다."
            : $"Include this profile when {browser} profile mode is set to Selected profiles.";
    }

    internal string EnableHomePageSuggestions => _korean
        ? "Command Palette 홈 화면에서 일치하는 북마크 추천"
        : "Suggest matching bookmarks on the Command Palette home page";

    internal string EnableHomePageSuggestionsDescription => _korean
        ? "켜면 홈 화면에서 입력할 때 모든 북마크를 명령으로 노출하지 않고 가장 잘 맞는 북마크를 추천합니다."
        : "When enabled, typing on the home page can show the best matching browser bookmark without exposing every bookmark as a separate command.";

    internal string KoreanInitialSearch => _korean ? "한글 초성 검색" : "Korean initial consonant search";

    internal string KoreanInitialSearchDescription => _korean
        ? "ㄴㅇㅂ 같은 입력으로 네이버 같은 한글 북마크 제목을 검색합니다."
        : "Allow queries like ㄴㅇㅂ to match Korean bookmark titles like 네이버.";

    internal string LaunchBrowserMode => _korean ? "북마크를 열 브라우저" : "Browser used to open bookmarks";

    internal string LaunchBrowserModeDescription => _korean
        ? "북마크를 기본 브라우저로 열지, 특정 브라우저로 열지, 북마크 출처 브라우저로 열지 선택합니다."
        : "Choose whether bookmarks open in the default browser, a specific browser, or the browser they came from.";

    internal string DefaultBrowser => _korean ? "기본 브라우저" : "Default browser";

    internal string SourceBrowser => _korean ? "북마크 출처 브라우저" : "Bookmark source browser";

    internal string AdditionalChromiumFolders => _korean ? "추가 Chromium User Data 폴더" : "Additional Chromium user data folders";

    internal string AdditionalChromiumFoldersDescription => _korean
        ? "Portable Chromium 계열 브라우저의 User Data 폴더를 세미콜론으로 구분해 입력합니다."
        : "Optional semicolon-separated user data folders for portable Chromium-based browsers.";

    internal string BrowserLabel(BookmarkSourceKind source)
    {
        return source switch
        {
            BookmarkSourceKind.Edge => "Microsoft Edge",
            BookmarkSourceKind.Chrome => "Google Chrome",
            BookmarkSourceKind.Firefox => "Mozilla Firefox",
            _ => "Chromium",
        };
    }
}
