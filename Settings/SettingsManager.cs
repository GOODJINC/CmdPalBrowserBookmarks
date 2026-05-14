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

    private readonly ToggleSetting _includeAllProfiles = new(
        Key(nameof(IncludeAllProfiles)),
        "All browser profiles",
        "Read every detected browser profile instead of only the Default profile.",
        true);

    private readonly ToggleSetting _enableHomePageSuggestions = new(
        Key(nameof(EnableHomePageSuggestions)),
        "Suggest matching bookmarks on the Command Palette home page",
        "When enabled, typing on the home page can show the best matching browser bookmark without exposing every bookmark as a separate command.",
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

    public bool EnableEdge => _enableEdge.Value;

    public bool EnableChrome => _enableChrome.Value;

    public bool EnableFirefox => _enableFirefox.Value;

    public bool IncludeAllProfiles => _includeAllProfiles.Value;

    public bool EnableHomePageSuggestions => _enableHomePageSuggestions.Value;

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
        Settings.Add(_includeAllProfiles);
        Settings.Add(_enableHomePageSuggestions);
        Settings.Add(_customChromiumUserDataFolders);

        LoadSettings();

        Settings.SettingsChanged += (_, _) => SaveSettings();
    }
}
