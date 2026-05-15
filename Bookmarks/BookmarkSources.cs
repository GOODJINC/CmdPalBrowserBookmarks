namespace CmdPalBrowserBookmarks.Bookmarks;

internal interface IBookmarkProfile
{
    string ProfileId { get; }

    string ProfileName { get; }
}

internal sealed record ChromiumProfile(
    BookmarkSourceKind Source,
    string BrowserName,
    string UserDataPath,
    string ProfilePath,
    string ProfileId,
    string ProfileName) : IBookmarkProfile
{
    public string BookmarksPath => Path.Combine(ProfilePath, "Bookmarks");
}

internal sealed record FirefoxProfile(
    string BrowserName,
    string ProfilePath,
    string ProfileId,
    string ProfileName,
    bool IsDefault) : IBookmarkProfile
{
    public string PlacesPath => Path.Combine(ProfilePath, "places.sqlite");
}

internal sealed record BookmarkSourceCatalog(
    IReadOnlyList<ChromiumProfile> ChromiumProfiles,
    IReadOnlyList<FirefoxProfile> FirefoxProfiles,
    IReadOnlyList<string> WatchedFiles);
