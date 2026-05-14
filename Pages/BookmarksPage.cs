using CmdPalBrowserBookmarks.Bookmarks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalBrowserBookmarks.Pages;

internal sealed partial class BookmarksPage : DynamicListPage
{
    private const int MaxPageResults = 250;
    private readonly BookmarkIndex _bookmarkIndex;

    public BookmarksPage(BookmarkIndex bookmarkIndex)
    {
        _bookmarkIndex = bookmarkIndex;
        Icon = Icons.Bookmarks;
        Title = "Browser Bookmarks";
        Name = "Search";
        PlaceholderText = "Search browser bookmarks";
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        if (!string.Equals(oldSearch, newSearch, StringComparison.Ordinal))
        {
            RaiseItemsChanged();
        }
    }

    public override IListItem[] GetItems()
    {
        var query = SearchText?.Trim() ?? string.Empty;
        var bookmarks = _bookmarkIndex.GetBookmarks();

        IEnumerable<BookmarkRecord> results = string.IsNullOrWhiteSpace(query)
            ? bookmarks
            : bookmarks
                .Select(bookmark => new
                {
                    Bookmark = bookmark,
                    Score = BookmarkSearch.Score(bookmark, query),
                })
                .Where(result => result.Score > 0)
                .OrderByDescending(result => result.Score)
                .ThenBy(result => result.Bookmark.Title, StringComparer.CurrentCultureIgnoreCase)
                .Select(result => result.Bookmark);

        return results
            .Take(MaxPageResults)
            .Select(BookmarkItemFactory.CreateListItem)
            .ToArray();
    }
}
