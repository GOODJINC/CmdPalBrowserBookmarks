using CmdPalBrowserBookmarks.Bookmarks;
using CmdPalBrowserBookmarks.Settings;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalBrowserBookmarks.Pages;

internal sealed partial class BookmarksPage : DynamicListPage
{
    private const int MaxPageResults = 250;
    private readonly BookmarkIndex _bookmarkIndex;
    private readonly SettingsManager _settings;

    public BookmarksPage(BookmarkIndex bookmarkIndex, SettingsManager settings)
    {
        _bookmarkIndex = bookmarkIndex;
        _settings = settings;
        Icon = Icons.Bookmarks;
        Title = settings.Strings.BrowserBookmarks;
        Name = settings.Strings.Search;
        PlaceholderText = settings.Strings.SearchBrowserBookmarksPlaceholder;
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
            return BookmarkSearch.FindMatches(bookmarks, query, _bookmarkIndex.SearchOptions)
                .Take(MaxPageResults)
                .Select(bookmark => BookmarkItemFactory.CreateListItem(bookmark, _settings))
                .ToArray();
        }
        catch
        {
            return [];
        }
    }
}
