namespace CmdPalBrowserBookmarks.Bookmarks;

internal static class BookmarkUrl
{
    internal static bool IsLaunchable(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return !string.Equals(uri.Scheme, "javascript", StringComparison.OrdinalIgnoreCase);
    }
}
