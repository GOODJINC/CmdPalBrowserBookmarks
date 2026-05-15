namespace CmdPalBrowserBookmarks.Bookmarks;

internal static class BookmarkSearch
{
    internal static IEnumerable<BookmarkRecord> FindMatches(IEnumerable<BookmarkRecord> bookmarks, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return bookmarks
                .OrderBy(bookmark => bookmark.Title, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(bookmark => bookmark.BrowserName, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(bookmark => bookmark.ProfileName, StringComparer.CurrentCultureIgnoreCase);
        }

        return bookmarks
            .Select(bookmark => new
            {
                Bookmark = bookmark,
                Score = Score(bookmark, query),
            })
            .Where(result => result.Score > 0)
            .OrderByDescending(result => result.Score)
            .ThenBy(result => result.Bookmark.Title, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(result => result.Bookmark.BrowserName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(result => result.Bookmark.ProfileName, StringComparer.CurrentCultureIgnoreCase)
            .Select(result => result.Bookmark);
    }

    internal static BookmarkRecord? FindBestMatch(IEnumerable<BookmarkRecord> bookmarks, string query)
    {
        return FindMatches(bookmarks, query).FirstOrDefault();
    }

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
