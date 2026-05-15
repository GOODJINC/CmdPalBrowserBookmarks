namespace CmdPalBrowserBookmarks.Bookmarks;

internal static class BookmarkSearch
{
    internal static IEnumerable<BookmarkRecord> FindMatches(IEnumerable<BookmarkRecord> bookmarks, string query)
    {
        return FindMatches(bookmarks, query, BookmarkSearchOptions.Default);
    }

    internal static IEnumerable<BookmarkRecord> FindMatches(
        IEnumerable<BookmarkRecord> bookmarks,
        string query,
        BookmarkSearchOptions options)
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
                Score = Score(bookmark, query, options),
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
        return FindBestMatch(bookmarks, query, BookmarkSearchOptions.Default);
    }

    internal static BookmarkRecord? FindBestMatch(
        IEnumerable<BookmarkRecord> bookmarks,
        string query,
        BookmarkSearchOptions options)
    {
        return FindMatches(bookmarks, query, options).FirstOrDefault();
    }

    internal static int Score(BookmarkRecord bookmark, string query)
    {
        return Score(bookmark, query, BookmarkSearchOptions.Default);
    }

    internal static int Score(BookmarkRecord bookmark, string query, BookmarkSearchOptions options)
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

        if (options.EnableKoreanInitialConsonantSearch)
        {
            var initialScore = ScoreKoreanInitialConsonants(bookmark, query, comparison);
            if (initialScore > 0)
            {
                return initialScore;
            }
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

    private static int ScoreKoreanInitialConsonants(
        BookmarkRecord bookmark,
        string query,
        StringComparison comparison)
    {
        if (!KoreanInitialConsonants.IsInitialConsonantQuery(query))
        {
            return 0;
        }

        var normalizedQuery = KoreanInitialConsonants.NormalizeQuery(query);
        var titleInitials = KoreanInitialConsonants.FromText(bookmark.Title);
        if (titleInitials.Equals(normalizedQuery, comparison))
        {
            return 900;
        }

        if (titleInitials.StartsWith(normalizedQuery, comparison))
        {
            return 700;
        }

        if (titleInitials.Contains(normalizedQuery, comparison))
        {
            return 500;
        }

        var searchBlobInitials = KoreanInitialConsonants.FromText(bookmark.SearchBlob);
        return searchBlobInitials.Contains(normalizedQuery, comparison) ? 250 : 0;
    }
}
