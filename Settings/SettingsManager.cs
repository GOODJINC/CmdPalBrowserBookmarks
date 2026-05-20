using System.Text.Json;
using CmdPalBrowserBookmarks.Bookmarks;
using CmdPalBrowserBookmarks.Localization;
using Microsoft.CommandPalette.Extensions.Toolkit;
using ToolkitSettings = Microsoft.CommandPalette.Extensions.Toolkit.Settings;

namespace CmdPalBrowserBookmarks.Settings;

internal sealed class SettingsManager : JsonSettingsManager
{
    private const string Prefix = "browserBookmarks";

    private readonly ToolkitSettings _basicSettings = new();
    private readonly ToggleSetting _enableEdge;
    private readonly ToggleSetting _enableChrome;
    private readonly ToggleSetting _enableFirefox;
    private readonly ChoiceSetSetting _uiLanguage;
    private readonly IReadOnlyList<ChromiumProfile> _edgeProfiles;
    private readonly ChoiceSetSetting _edgeProfileMode;
    private readonly ChoiceSetSetting _selectedEdgeProfile;
    private readonly IReadOnlyList<ProfileToggleSetting> _selectedEdgeProfileToggles;
    private readonly IReadOnlyList<ChromiumProfile> _chromeProfiles;
    private readonly ChoiceSetSetting _chromeProfileMode;
    private readonly ChoiceSetSetting _selectedChromeProfile;
    private readonly IReadOnlyList<ProfileToggleSetting> _selectedChromeProfileToggles;
    private readonly IReadOnlyList<FirefoxProfile> _firefoxProfiles;
    private readonly ChoiceSetSetting _firefoxProfileMode;
    private readonly ChoiceSetSetting _selectedFirefoxProfile;
    private readonly IReadOnlyList<ProfileToggleSetting> _selectedFirefoxProfileToggles;
    private readonly ChoiceSetSetting _launchBrowserMode;
    private readonly ToggleSetting _enableHomePageSuggestions;
    private readonly ToggleSetting _enableKoreanInitialConsonantSearch;
    private readonly TextSetting _customChromiumUserDataFolders;

    public SettingsManager()
    {
        var settingsDirectory = Path.Combine(LocalAppData, "CmdPalBrowserBookmarks");
        Directory.CreateDirectory(settingsDirectory);
        FilePath = Path.Combine(settingsDirectory, "settings.json");

        Strings = LocalizedStrings.For(ReadPersistedUiLanguage(FilePath));
        var roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        _uiLanguage = new ChoiceSetSetting(
            Key(nameof(UiLanguage)),
            Strings.UiLanguageLabel,
            Strings.UiLanguageDescription,
            UiLanguageChoices());

        _enableEdge = new ToggleSetting(
            Key(nameof(EnableEdge)),
            "Microsoft Edge",
            Strings.ReadEdgeBookmarks,
            true);

        _enableChrome = new ToggleSetting(
            Key(nameof(EnableChrome)),
            "Google Chrome",
            Strings.ReadChromeBookmarks,
            true);

        _enableFirefox = new ToggleSetting(
            Key(nameof(EnableFirefox)),
            "Mozilla Firefox",
            Strings.ReadFirefoxBookmarks,
            true);

        _edgeProfileMode = new ChoiceSetSetting(
            Key(nameof(EdgeProfileMode)),
            Strings.EdgeProfileMode,
            Strings.ChooseEdgeProfileMode,
            ProfileModeChoices());

        _edgeProfiles = BrowserProfileDiscovery.GetChromiumProfiles(
            BookmarkSourceKind.Edge,
            "Microsoft Edge",
            Path.Combine(LocalAppData, "Microsoft", "Edge", "User Data")).Profiles;
        _selectedEdgeProfile = new ChoiceSetSetting(
            Key(nameof(SelectedEdgeProfileId)),
            Strings.EdgeProfile,
            Strings.SpecificProfileDescription("Microsoft Edge"),
            ProfileChoices(_edgeProfiles));
        _selectedEdgeProfileToggles = CreateProfileToggles(
            nameof(SelectedEdgeProfileIds),
            "Microsoft Edge",
            _edgeProfiles,
            _selectedEdgeProfile.Value);

        _chromeProfileMode = new ChoiceSetSetting(
            Key(nameof(ChromeProfileMode)),
            Strings.ChromeProfileMode,
            Strings.ChooseChromeProfileMode,
            ProfileModeChoices());

        _chromeProfiles = BrowserProfileDiscovery.GetChromiumProfiles(
            BookmarkSourceKind.Chrome,
            "Google Chrome",
            Path.Combine(LocalAppData, "Google", "Chrome", "User Data")).Profiles;
        _selectedChromeProfile = new ChoiceSetSetting(
            Key(nameof(SelectedChromeProfileId)),
            Strings.ChromeProfile,
            Strings.SpecificProfileDescription("Google Chrome"),
            ProfileChoices(_chromeProfiles));
        _selectedChromeProfileToggles = CreateProfileToggles(
            nameof(SelectedChromeProfileIds),
            "Google Chrome",
            _chromeProfiles,
            _selectedChromeProfile.Value);

        _firefoxProfileMode = new ChoiceSetSetting(
            Key(nameof(FirefoxProfileMode)),
            Strings.FirefoxProfileMode,
            Strings.ChooseFirefoxProfileMode,
            ProfileModeChoices());

        _firefoxProfiles = BrowserProfileDiscovery.GetFirefoxProfiles(roamingAppData, LocalAppData);
        _selectedFirefoxProfile = new ChoiceSetSetting(
            Key(nameof(SelectedFirefoxProfileId)),
            Strings.FirefoxProfile,
            Strings.SpecificProfileDescription("Mozilla Firefox"),
            ProfileChoices(_firefoxProfiles));
        _selectedFirefoxProfileToggles = CreateProfileToggles(
            nameof(SelectedFirefoxProfileIds),
            "Mozilla Firefox",
            _firefoxProfiles,
            _selectedFirefoxProfile.Value);

        _launchBrowserMode = new ChoiceSetSetting(
            Key(nameof(LaunchBrowserMode)),
            Strings.LaunchBrowserMode,
            Strings.LaunchBrowserModeDescription,
            LaunchBrowserChoices());

        _enableHomePageSuggestions = new ToggleSetting(
            Key(nameof(EnableHomePageSuggestions)),
            Strings.EnableHomePageSuggestions,
            Strings.EnableHomePageSuggestionsDescription,
            true);

        _enableKoreanInitialConsonantSearch = new ToggleSetting(
            Key(nameof(EnableKoreanInitialConsonantSearch)),
            Strings.KoreanInitialSearch,
            Strings.KoreanInitialSearchDescription,
            true);

        _customChromiumUserDataFolders = new TextSetting(
            Key(nameof(CustomChromiumUserDataFolders)),
            Strings.AdditionalChromiumFolders,
            Strings.AdditionalChromiumFoldersDescription,
            string.Empty)
        {
            Multiline = true,
            Placeholder = @"C:\Portable\Browser\User Data;D:\Profiles\Chromium\User Data",
        };

        Settings.Add(_uiLanguage);
        Settings.Add(_enableEdge);
        Settings.Add(_enableChrome);
        Settings.Add(_enableFirefox);
        Settings.Add(_edgeProfileMode);
        Settings.Add(_selectedEdgeProfile);
        AddProfileTogglesToSettings(_selectedEdgeProfileToggles);
        Settings.Add(_chromeProfileMode);
        Settings.Add(_selectedChromeProfile);
        AddProfileTogglesToSettings(_selectedChromeProfileToggles);
        Settings.Add(_firefoxProfileMode);
        Settings.Add(_selectedFirefoxProfile);
        AddProfileTogglesToSettings(_selectedFirefoxProfileToggles);
        Settings.Add(_launchBrowserMode);
        Settings.Add(_enableHomePageSuggestions);
        Settings.Add(_enableKoreanInitialConsonantSearch);
        Settings.Add(_customChromiumUserDataFolders);

        _basicSettings.Add(_uiLanguage);
        _basicSettings.Add(_enableEdge);
        _basicSettings.Add(_enableChrome);
        _basicSettings.Add(_enableFirefox);
        _basicSettings.Add(_launchBrowserMode);
        _basicSettings.Add(_enableHomePageSuggestions);
        _basicSettings.Add(_enableKoreanInitialConsonantSearch);

        LoadSettings();
        NormalizeUiLanguageValue();
        Strings = LocalizedStrings.For(UiLanguage);
        ApplyLocalizedSettingsText();

        _basicSettings.SettingsChanged += OnVisibleSettingsChanged;
    }

    private static string Key(string name) => $"{Prefix}.{name}";

    private static string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public LocalizedStrings Strings { get; private set; }

    public ToolkitSettings BasicSettings => _basicSettings;

    public event EventHandler? SettingsChanged;

    public UiLanguage UiLanguage => ReadUiLanguage(_uiLanguage.Value);

    public bool EnableEdge => _enableEdge.Value;

    public bool EnableChrome => _enableChrome.Value;

    public bool EnableFirefox => _enableFirefox.Value;

    public BrowserProfileMode EdgeProfileMode => ReadProfileMode(_edgeProfileMode.Value);

    public string SelectedEdgeProfileId => NormalizeProfileId(_selectedEdgeProfile.Value);

    public IReadOnlySet<string> SelectedEdgeProfileIds => SelectedProfileIds(_selectedEdgeProfileToggles);

    public BrowserProfileMode ChromeProfileMode => ReadProfileMode(_chromeProfileMode.Value);

    public string SelectedChromeProfileId => NormalizeProfileId(_selectedChromeProfile.Value);

    public IReadOnlySet<string> SelectedChromeProfileIds => SelectedProfileIds(_selectedChromeProfileToggles);

    public BrowserProfileMode FirefoxProfileMode => ReadProfileMode(_firefoxProfileMode.Value);

    public string SelectedFirefoxProfileId => NormalizeProfileId(_selectedFirefoxProfile.Value);

    public IReadOnlySet<string> SelectedFirefoxProfileIds => SelectedProfileIds(_selectedFirefoxProfileToggles);

    public BrowserLaunchMode LaunchBrowserMode => ReadLaunchBrowserMode(_launchBrowserMode.Value);

    public bool EnableHomePageSuggestions => _enableHomePageSuggestions.Value;

    public bool EnableKoreanInitialConsonantSearch => _enableKoreanInitialConsonantSearch.Value;

    public string CustomChromiumUserDataFolders => _customChromiumUserDataFolders.Value ?? string.Empty;

    public ToolkitSettings CreateProfileSettings()
    {
        var profileSettings = new ToolkitSettings();

        if (EnableEdge)
        {
            profileSettings.Add(_edgeProfileMode);
            if (EdgeProfileMode == BrowserProfileMode.Selected)
            {
                profileSettings.Add(_selectedEdgeProfile);
            }
            else if (EdgeProfileMode == BrowserProfileMode.Multiple)
            {
                AddProfileTogglesToSettings(profileSettings, _selectedEdgeProfileToggles);
            }
        }

        if (EnableChrome || !string.IsNullOrWhiteSpace(CustomChromiumUserDataFolders))
        {
            profileSettings.Add(_chromeProfileMode);
            if (ChromeProfileMode == BrowserProfileMode.Selected)
            {
                profileSettings.Add(_selectedChromeProfile);
            }
            else if (ChromeProfileMode == BrowserProfileMode.Multiple)
            {
                AddProfileTogglesToSettings(profileSettings, _selectedChromeProfileToggles);
            }
        }

        if (EnableFirefox)
        {
            profileSettings.Add(_firefoxProfileMode);
            if (FirefoxProfileMode == BrowserProfileMode.Selected)
            {
                profileSettings.Add(_selectedFirefoxProfile);
            }
            else if (FirefoxProfileMode == BrowserProfileMode.Multiple)
            {
                AddProfileTogglesToSettings(profileSettings, _selectedFirefoxProfileToggles);
            }
        }

        profileSettings.Add(_customChromiumUserDataFolders);
        profileSettings.SettingsChanged += OnVisibleSettingsChanged;
        return profileSettings;
    }

    private List<ChoiceSetSetting.Choice> UiLanguageChoices()
    {
        return
        [
            new(Strings.English, "en"),
            new(Strings.Korean, "ko"),
        ];
    }

    private List<ChoiceSetSetting.Choice> ProfileModeChoices()
    {
        return
        [
            new(Strings.RecentDefaultProfile, "recent"),
            new(Strings.SpecificProfile, "selected"),
            new(Strings.MultipleProfiles, "multiple"),
            new(Strings.AllProfiles, "all"),
        ];
    }

    private List<ChoiceSetSetting.Choice> LaunchBrowserChoices()
    {
        return
        [
            new(Strings.DefaultBrowser, "default"),
            new(Strings.SourceBrowser, "source"),
            new("Microsoft Edge", "edge"),
            new("Google Chrome", "chrome"),
            new("Mozilla Firefox", "firefox"),
        ];
    }

    private static UiLanguage ReadPersistedUiLanguage(string settingsPath)
    {
        if (!File.Exists(settingsPath))
        {
            return UiLanguage.English;
        }

        try
        {
            using var stream = File.OpenRead(settingsPath);
            using var document = JsonDocument.Parse(stream);
            var key = Key(nameof(UiLanguage));
            if (document.RootElement.TryGetProperty(key, out var value))
            {
                return ReadUiLanguage(value.GetString());
            }
        }
        catch
        {
        }

        return UiLanguage.English;
    }

    private static UiLanguage ReadUiLanguage(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "ko" or "ko-kr" or "korean" => UiLanguage.Korean,
            "en" or "en-us" or "english" => UiLanguage.English,
            _ => UiLanguage.English,
        };
    }

    private static string UiLanguageCode(UiLanguage language)
    {
        return language switch
        {
            UiLanguage.Korean => "ko",
            _ => "en",
        };
    }

    private void NormalizeUiLanguageValue()
    {
        _uiLanguage.Value = UiLanguageCode(UiLanguage);
    }

    private static BrowserProfileMode ReadProfileMode(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "all" => BrowserProfileMode.All,
            "multiple" => BrowserProfileMode.Multiple,
            "selected" => BrowserProfileMode.Selected,
            _ => BrowserProfileMode.Recent,
        };
    }

    private static BrowserLaunchMode ReadLaunchBrowserMode(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "source" => BrowserLaunchMode.SourceBrowser,
            "edge" => BrowserLaunchMode.Edge,
            "chrome" => BrowserLaunchMode.Chrome,
            "firefox" => BrowserLaunchMode.Firefox,
            _ => BrowserLaunchMode.Default,
        };
    }

    private static string NormalizeProfileId(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "auto" : value.Trim();
    }

    private void OnVisibleSettingsChanged(object? sender, ToolkitSettings settings)
    {
        NormalizeUiLanguageValue();
        Strings = LocalizedStrings.For(UiLanguage);
        ApplyLocalizedSettingsText();
        SaveSettings();
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyLocalizedSettingsText()
    {
        _uiLanguage.Label = Strings.UiLanguageLabel;
        _uiLanguage.Description = Strings.UiLanguageDescription;
        _uiLanguage.Choices = UiLanguageChoices();

        _enableEdge.Label = "Microsoft Edge";
        _enableEdge.Description = Strings.ReadEdgeBookmarks;
        _enableChrome.Label = "Google Chrome";
        _enableChrome.Description = Strings.ReadChromeBookmarks;
        _enableFirefox.Label = "Mozilla Firefox";
        _enableFirefox.Description = Strings.ReadFirefoxBookmarks;

        _edgeProfileMode.Label = Strings.EdgeProfileMode;
        _edgeProfileMode.Description = Strings.ChooseEdgeProfileMode;
        _edgeProfileMode.Choices = ProfileModeChoices();
        _selectedEdgeProfile.Label = Strings.EdgeProfile;
        _selectedEdgeProfile.Description = Strings.SpecificProfileDescription("Microsoft Edge");
        _selectedEdgeProfile.Choices = ProfileChoices(_edgeProfiles);
        ApplyLocalizedProfileToggleText(_selectedEdgeProfileToggles, "Microsoft Edge");

        _chromeProfileMode.Label = Strings.ChromeProfileMode;
        _chromeProfileMode.Description = Strings.ChooseChromeProfileMode;
        _chromeProfileMode.Choices = ProfileModeChoices();
        _selectedChromeProfile.Label = Strings.ChromeProfile;
        _selectedChromeProfile.Description = Strings.SpecificProfileDescription("Google Chrome");
        _selectedChromeProfile.Choices = ProfileChoices(_chromeProfiles);
        ApplyLocalizedProfileToggleText(_selectedChromeProfileToggles, "Google Chrome");

        _firefoxProfileMode.Label = Strings.FirefoxProfileMode;
        _firefoxProfileMode.Description = Strings.ChooseFirefoxProfileMode;
        _firefoxProfileMode.Choices = ProfileModeChoices();
        _selectedFirefoxProfile.Label = Strings.FirefoxProfile;
        _selectedFirefoxProfile.Description = Strings.SpecificProfileDescription("Mozilla Firefox");
        _selectedFirefoxProfile.Choices = ProfileChoices(_firefoxProfiles);
        ApplyLocalizedProfileToggleText(_selectedFirefoxProfileToggles, "Mozilla Firefox");

        _launchBrowserMode.Label = Strings.LaunchBrowserMode;
        _launchBrowserMode.Description = Strings.LaunchBrowserModeDescription;
        _launchBrowserMode.Choices = LaunchBrowserChoices();

        _enableHomePageSuggestions.Label = Strings.EnableHomePageSuggestions;
        _enableHomePageSuggestions.Description = Strings.EnableHomePageSuggestionsDescription;
        _enableKoreanInitialConsonantSearch.Label = Strings.KoreanInitialSearch;
        _enableKoreanInitialConsonantSearch.Description = Strings.KoreanInitialSearchDescription;

        _customChromiumUserDataFolders.Label = Strings.AdditionalChromiumFolders;
        _customChromiumUserDataFolders.Description = Strings.AdditionalChromiumFoldersDescription;
        _customChromiumUserDataFolders.Placeholder = @"C:\Portable\Browser\User Data;D:\Profiles\Chromium\User Data";
    }

    private List<ChoiceSetSetting.Choice> ProfileChoices<TProfile>(IReadOnlyList<TProfile> profiles)
        where TProfile : IBookmarkProfile
    {
        var choices = new List<ChoiceSetSetting.Choice>
        {
            new(Strings.AutomaticFallback, "auto"),
        };

        choices.AddRange(profiles.Select(profile =>
            new ChoiceSetSetting.Choice(ProfileChoiceLabel(profile.ProfileName, profile.ProfileId), profile.ProfileId)));
        return choices;
    }

    private IReadOnlyList<ProfileToggleSetting> CreateProfileToggles<TProfile>(
        string settingName,
        string browserName,
        IReadOnlyList<TProfile> profiles,
        string? legacySelectedProfileId)
        where TProfile : IBookmarkProfile
    {
        return profiles
            .Select(profile =>
            {
                var isLegacySelected = ProfileMatches(profile.ProfileId, profile.ProfileName, legacySelectedProfileId);
                var setting = new ToggleSetting(
                    Key($"{settingName}.{EncodeKeySegment(profile.ProfileId)}"),
                    ProfileChoiceLabel(profile.ProfileName, profile.ProfileId),
                    Strings.MultipleProfileDescription(browserName),
                    isLegacySelected);
                return new ProfileToggleSetting(profile.ProfileId, setting);
            })
            .ToArray();
    }

    private void AddProfileTogglesToSettings(IReadOnlyList<ProfileToggleSetting> toggles)
    {
        AddProfileTogglesToSettings(Settings, toggles);
    }

    private void ApplyLocalizedProfileToggleText(IReadOnlyList<ProfileToggleSetting> toggles, string browserName)
    {
        foreach (var toggle in toggles)
        {
            toggle.Setting.Description = Strings.MultipleProfileDescription(browserName);
        }
    }

    private static void AddProfileTogglesToSettings(ToolkitSettings settings, IReadOnlyList<ProfileToggleSetting> toggles)
    {
        foreach (var toggle in toggles)
        {
            settings.Add(toggle.Setting);
        }
    }

    private static IReadOnlySet<string> SelectedProfileIds(IReadOnlyList<ProfileToggleSetting> toggles)
    {
        return toggles
            .Where(toggle => toggle.Setting.Value)
            .Select(toggle => toggle.ProfileId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static string ProfileChoiceLabel(string profileName, string profileId)
    {
        return string.Equals(profileName, profileId, StringComparison.OrdinalIgnoreCase)
            ? profileId
            : $"{profileName} ({profileId})";
    }

    private static bool ProfileMatches(string profileId, string profileName, string? selectedProfileId)
    {
        return !string.IsNullOrWhiteSpace(selectedProfileId) &&
            (string.Equals(profileId, selectedProfileId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(profileName, selectedProfileId, StringComparison.OrdinalIgnoreCase));
    }

    private static string EncodeKeySegment(string value)
    {
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private sealed record ProfileToggleSetting(string ProfileId, ToggleSetting Setting);
}
