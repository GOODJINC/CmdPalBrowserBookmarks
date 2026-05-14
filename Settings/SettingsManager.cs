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

    private readonly ToggleSetting _showBookmarksAtTopLevel = new(
        Key(nameof(ShowBookmarksAtTopLevel)),
        "Show bookmarks on the Command Palette home page",
        "When enabled, bookmark items can be found directly by typing on the main Command Palette screen.",
        false);

    private readonly ChoiceSetSetting _maxTopLevelBookmarks = new(
        Key(nameof(MaxTopLevelBookmarks)),
        "Maximum home-page bookmark results",
        "Limits how many bookmark commands are exposed at the Command Palette home page.",
        [
            new ChoiceSetSetting.Choice("1,000", "1000"),
            new ChoiceSetSetting.Choice("100", "100"),
            new ChoiceSetSetting.Choice("250", "250"),
            new ChoiceSetSetting.Choice("500", "500"),
            new ChoiceSetSetting.Choice("2,000", "2000"),
            new ChoiceSetSetting.Choice("5,000", "5000"),
        ]);

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

    public bool ShowBookmarksAtTopLevel => _showBookmarksAtTopLevel.Value;

    public int MaxTopLevelBookmarks => int.TryParse(_maxTopLevelBookmarks.Value, out var value) ? value : 1000;

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
        Settings.Add(_showBookmarksAtTopLevel);
        Settings.Add(_maxTopLevelBookmarks);
        Settings.Add(_customChromiumUserDataFolders);

        LoadSettings();

        Settings.SettingsChanged += (_, _) => SaveSettings();
    }
}
