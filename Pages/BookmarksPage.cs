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
        try
        {
            var bookmarks = _bookmarkIndex.GetBookmarks();
            return BookmarkSearch.FindMatches(bookmarks, query)
                .Take(MaxPageResults)
                .Select(BookmarkItemFactory.CreateListItem)
                .ToArray();
        }
        catch
        {
            return [];
        }
    }
}
