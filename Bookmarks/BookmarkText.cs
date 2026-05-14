namespace CmdPalBrowserBookmarks.Bookmarks;

internal static class BookmarkText
{
    internal static string TitleOrFallback(string? title, string url)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            return title.Trim();
        }

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && !string.IsNullOrWhiteSpace(uri.Host))
        {
            return uri.Host;
        }

        return url;
    }
}
