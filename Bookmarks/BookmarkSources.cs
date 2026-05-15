namespace CmdPalBrowserBookmarks.Bookmarks;

internal sealed record ChromiumProfile(
    BookmarkSourceKind Source,
    string BrowserName,
    string UserDataPath,
    string ProfilePath,
    string ProfileId,
    string ProfileName)
{
    public string BookmarksPath => Path.Combine(ProfilePath, "Bookmarks");
}

internal sealed record FirefoxProfile(
    string BrowserName,
    string ProfilePath,
    string ProfileId,
    string ProfileName,
    bool IsDefault)
{
    public string PlacesPath => Path.Combine(ProfilePath, "places.sqlite");
}

internal sealed record BookmarkSourceCatalog(
    IReadOnlyList<ChromiumProfile> ChromiumProfiles,
    IReadOnlyList<FirefoxProfile> FirefoxProfiles,
    IReadOnlyList<string> WatchedFiles);
