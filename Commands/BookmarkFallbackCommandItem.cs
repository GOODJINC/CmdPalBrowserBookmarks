using CmdPalBrowserBookmarks.Bookmarks;
using CmdPalBrowserBookmarks.Pages;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalBrowserBookmarks.Commands;

internal sealed partial class BookmarkFallbackCommandItem : FallbackCommandItem
{
    private const string FallbackId = "CmdPalBrowserBookmarks.SearchBookmarksFallback";
    private const int MinimumQueryLength = 2;

    private readonly BookmarkIndex _bookmarkIndex;

    public BookmarkFallbackCommandItem(BookmarkIndex bookmarkIndex)
        : base("Search browser bookmarks", FallbackId)
    {
        _bookmarkIndex = bookmarkIndex;
        Icon = Icons.Bookmarks;
        SetDefaultState();
    }

    public override void UpdateQuery(string query)
    {
        var searchText = query.Trim();
        if (searchText.Length < MinimumQueryLength)
        {
            SetDefaultState();
            return;
        }

        var bookmarks = GetSearchableBookmarks();
        var match = bookmarks
            .Select(bookmark => new
            {
                Bookmark = bookmark,
                Score = BookmarkSearch.Score(bookmark, searchText),
            })
            .Where(result => result.Score > 0)
            .OrderByDescending(result => result.Score)
            .ThenBy(result => result.Bookmark.Title, StringComparer.CurrentCultureIgnoreCase)
            .FirstOrDefault();

        if (match is null)
        {
            SetNoMatchState(searchText);
            return;
        }

        var bookmark = match.Bookmark;
        Title = bookmark.Title;
        Subtitle = $"Open bookmark - {BookmarkItemFactory.BuildSubtitle(bookmark)}";
        Icon = Icons.Bookmarks;
        Command = new OpenBookmarkCommand(bookmark);
        MoreCommands = BookmarkItemFactory.CreateContextCommands(bookmark);
    }

    private IReadOnlyList<BookmarkRecord> GetSearchableBookmarks()
    {
        try
        {
            if (_bookmarkIndex.HasChanges())
            {
                return _bookmarkIndex.GetBookmarks();
            }

            return _bookmarkIndex.GetCachedBookmarks();
        }
        catch
        {
            return [];
        }
    }

    private void SetDefaultState()
    {
        Title = "Search browser bookmarks";
        Subtitle = "Type a bookmark title, URL, or folder";
        Icon = Icons.Bookmarks;
        Command = new BookmarksPage(_bookmarkIndex);
        MoreCommands = [];
    }

    private void SetNoMatchState(string searchText)
    {
        Title = $"Search browser bookmarks for \"{searchText}\"";
        Subtitle = "Open Browser Bookmarks and continue searching";
        Icon = Icons.Bookmarks;
        Command = new BookmarksPage(_bookmarkIndex);
        MoreCommands = [];
    }
}
