namespace CmdPalBrowserBookmarks.Bookmarks;

internal static class BookmarkSearch
{
    internal static int Score(BookmarkRecord bookmark, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return 1;
        }

        var comparison = StringComparison.CurrentCultureIgnoreCase;
        var title = bookmark.Title;

        if (title.Equals(query, comparison))
        {
            return 1000;
        }

        if (title.StartsWith(query, comparison))
        {
            return 800;
        }

        if (title.Contains(query, comparison))
        {
            return 600;
        }

        if (bookmark.Host.Contains(query, comparison))
        {
            return 450;
        }

        if (bookmark.Url.Contains(query, comparison))
        {
            return 300;
        }

        if (bookmark.FolderPath.Contains(query, comparison) || bookmark.ProfileName.Contains(query, comparison))
        {
            return 150;
        }

        return 0;
    }
}
