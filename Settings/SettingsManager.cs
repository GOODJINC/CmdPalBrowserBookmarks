using CmdPalBrowserBookmarks.Bookmarks;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalBrowserBookmarks.Settings;

internal sealed class SettingsManager : JsonSettingsManager
{
    private const string Prefix = "browserBookmarks";

    private static string Key(string name) => $"{Prefix}.{name}";

    private readonly ToggleSetting _enableEdge = new(
        Key(nameof(EnableEdge)),
        "Microsoft Edge",
        "Read bookmarks from Microsoft Edge profiles.",
        true);

    private readonly ToggleSetting _enableChrome = new(
        Key(nameof(EnableChrome)),
        "Google Chrome",
        "Read bookmarks from Google Chrome profiles.",
        true);

    private readonly ToggleSetting _enableFirefox = new(
        Key(nameof(EnableFirefox)),
        "Mozilla Firefox",
        "Read bookmarks from Firefox places.sqlite profiles.",
        true);

    private readonly ChoiceSetSetting _edgeProfileMode = new(
        Key(nameof(EdgeProfileMode)),
        "Microsoft Edge profile mode",
        "Choose which Microsoft Edge profile should be searched.",
        ProfileModeChoices());

    private readonly ChoiceSetSetting _selectedEdgeProfile = new(
        Key(nameof(SelectedEdgeProfileId)),
        "Microsoft Edge profile",
        "Used when Microsoft Edge profile mode is set to Specific profile.",
        ChromiumProfileChoices(
            BookmarkSourceKind.Edge,
            "Microsoft Edge",
            Path.Combine(LocalAppData, "Microsoft", "Edge", "User Data")));

    private readonly ChoiceSetSetting _chromeProfileMode = new(
        Key(nameof(ChromeProfileMode)),
        "Google Chrome profile mode",
        "Choose which Google Chrome profile should be searched.",
        ProfileModeChoices());

    private readonly ChoiceSetSetting _selectedChromeProfile = new(
        Key(nameof(SelectedChromeProfileId)),
        "Google Chrome profile",
        "Used when Google Chrome profile mode is set to Specific profile.",
        ChromiumProfileChoices(
            BookmarkSourceKind.Chrome,
            "Google Chrome",
            Path.Combine(LocalAppData, "Google", "Chrome", "User Data")));

    private readonly ChoiceSetSetting _firefoxProfileMode = new(
        Key(nameof(FirefoxProfileMode)),
        "Mozilla Firefox profile mode",
        "Choose which Mozilla Firefox profile should be searched.",
        ProfileModeChoices());

    private readonly ChoiceSetSetting _selectedFirefoxProfile = new(
        Key(nameof(SelectedFirefoxProfileId)),
        "Mozilla Firefox profile",
        "Used when Mozilla Firefox profile mode is set to Specific profile.",
        FirefoxProfileChoices());

    private readonly ToggleSetting _enableHomePageSuggestions = new(
        Key(nameof(EnableHomePageSuggestions)),
        "Suggest matching bookmarks on the Command Palette home page",
        "When enabled, typing on the home page can show the best matching browser bookmark without exposing every bookmark as a separate command.",
        true);

    private readonly ToggleSetting _enableKoreanInitialConsonantSearch = new(
        Key(nameof(EnableKoreanInitialConsonantSearch)),
        "Korean initial consonant search",
        "Allow queries like ㄴㅇㅂ to match Korean bookmark titles like 네이버.",
        true);

    private readonly TextSetting _customChromiumUserDataFolders = new(
        Key(nameof(CustomChromiumUserDataFolders)),
        "Additional Chromium user data folders",
        "Optional semicolon-separated user data folders for portable Chromium-based browsers.",
        string.Empty)
    {
        Multiline = true,
        Placeholder = @"C:\Portable\Browser\User Data;D:\Profiles\Chromium\User Data",
    };

    private static string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    private static string RoamingAppData => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    public bool EnableEdge => _enableEdge.Value;

    public bool EnableChrome => _enableChrome.Value;

    public bool EnableFirefox => _enableFirefox.Value;

    public BrowserProfileMode EdgeProfileMode => ReadProfileMode(_edgeProfileMode.Value);

    public string SelectedEdgeProfileId => _selectedEdgeProfile.Value ?? string.Empty;

    public BrowserProfileMode ChromeProfileMode => ReadProfileMode(_chromeProfileMode.Value);

    public string SelectedChromeProfileId => _selectedChromeProfile.Value ?? string.Empty;

    public BrowserProfileMode FirefoxProfileMode => ReadProfileMode(_firefoxProfileMode.Value);

    public string SelectedFirefoxProfileId => _selectedFirefoxProfile.Value ?? string.Empty;

    public bool EnableHomePageSuggestions => _enableHomePageSuggestions.Value;

    public bool EnableKoreanInitialConsonantSearch => _enableKoreanInitialConsonantSearch.Value;

    public string CustomChromiumUserDataFolders => _customChromiumUserDataFolders.Value ?? string.Empty;

    public SettingsManager()
    {
        var settingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CmdPalBrowserBookmarks");

        Directory.CreateDirectory(settingsDirectory);
        FilePath = Path.Combine(settingsDirectory, "settings.json");

        Settings.Add(_enableEdge);
        Settings.Add(_enableChrome);
        Settings.Add(_enableFirefox);
        Settings.Add(_edgeProfileMode);
        Settings.Add(_selectedEdgeProfile);
        Settings.Add(_chromeProfileMode);
        Settings.Add(_selectedChromeProfile);
        Settings.Add(_firefoxProfileMode);
        Settings.Add(_selectedFirefoxProfile);
        Settings.Add(_enableHomePageSuggestions);
        Settings.Add(_enableKoreanInitialConsonantSearch);
        Settings.Add(_customChromiumUserDataFolders);

        LoadSettings();

        Settings.SettingsChanged += (_, _) => SaveSettings();
    }

    private static List<ChoiceSetSetting.Choice> ProfileModeChoices()
    {
        return
        [
            new("Recently used/default profile", "recent"),
            new("Specific profile", "selected"),
            new("All profiles", "all"),
        ];
    }

    private static List<ChoiceSetSetting.Choice> ChromiumProfileChoices(
        BookmarkSourceKind source,
        string browserName,
        string userDataPath)
    {
        var choices = new List<ChoiceSetSetting.Choice>
        {
            new("Automatic fallback", "auto"),
        };

        var profiles = BrowserProfileDiscovery.GetChromiumProfiles(source, browserName, userDataPath).Profiles;
        choices.AddRange(profiles.Select(profile =>
            new ChoiceSetSetting.Choice(ProfileChoiceLabel(profile.ProfileName, profile.ProfileId), profile.ProfileId)));

        return choices;
    }

    private static List<ChoiceSetSetting.Choice> FirefoxProfileChoices()
    {
        var choices = new List<ChoiceSetSetting.Choice>
        {
            new("Automatic fallback", "auto"),
        };

        var profiles = BrowserProfileDiscovery.GetFirefoxProfiles(RoamingAppData, LocalAppData);
        choices.AddRange(profiles.Select(profile =>
            new ChoiceSetSetting.Choice(ProfileChoiceLabel(profile.ProfileName, profile.ProfileId), profile.ProfileId)));

        return choices;
    }

    private static string ProfileChoiceLabel(string profileName, string profileId)
    {
        return string.Equals(profileName, profileId, StringComparison.OrdinalIgnoreCase)
            ? profileId
            : $"{profileName} ({profileId})";
    }

    private static BrowserProfileMode ReadProfileMode(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "all" => BrowserProfileMode.All,
            "selected" => BrowserProfileMode.Selected,
            _ => BrowserProfileMode.Recent,
        };
    }
}
