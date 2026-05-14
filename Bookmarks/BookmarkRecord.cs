namespace CmdPalBrowserBookmarks.Bookmarks;

internal sealed record BookmarkRecord(
    string Title,
    string Url,
    BookmarkSourceKind Source,
    string BrowserName,
    string ProfileName,
    string FolderPath,
    DateTimeOffset? DateAdded,
    string SourceFilePath)
{
    public string Host
    {
        get
        {
            if (Uri.TryCreate(Url, UriKind.Absolute, out var uri) && !string.IsNullOrWhiteSpace(uri.Host))
            {
                return uri.Host;
            }

            return Url;
        }
    }

    public string SearchBlob => string.Join(
        ' ',
        Title,
        Url,
        Host,
        BrowserName,
        ProfileName,
        FolderPath);

    public string StableKey => $"{Source}|{ProfileName}|{Title}|{Url}";
}
