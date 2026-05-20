using CmdPalBrowserBookmarks.Bookmarks;
using CmdPalBrowserBookmarks.Pages;
using CmdPalBrowserBookmarks.Settings;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalBrowserBookmarks.Commands;

internal sealed partial class BookmarkFallbackCommandItem : FallbackCommandItem
{
    private const string FallbackId = "CmdPalBrowserBookmarks.SearchBookmarksFallback";
    private const int MinimumQueryLength = 1;

    private readonly BookmarkIndex _bookmarkIndex;
    private readonly SettingsManager _settings;

    public BookmarkFallbackCommandItem(BookmarkIndex bookmarkIndex, SettingsManager settings)
        : base(settings.Strings.SearchBrowserBookmarks, FallbackId)
    {
        _bookmarkIndex = bookmarkIndex;
        _settings = settings;
        Icon = Icons.Bookmarks;
        RefreshText();
    }

    public override void UpdateQuery(string query)
    {
        var searchText = query.Trim();
        if (searchText.Length < MinimumQueryLength)
        {
            SetDefaultState();
            return;
        }

        var bookmark = BookmarkSearch.FindBestMatch(
            GetSearchableBookmarks(),
            searchText,
            _bookmarkIndex.SearchOptions);
        if (bookmark is null)
        {
            SetNoMatchState(searchText);
            return;
        }

        Title = bookmark.Title;
        Subtitle = $"{_settings.Strings.OpenBookmarkPrefix} - {BookmarkItemFactory.BuildSubtitle(bookmark)}";
        Icon = Icons.Bookmarks;
        Command = new OpenBookmarkCommand(bookmark, _settings);
        MoreCommands = BookmarkItemFactory.CreateContextCommands(bookmark, _settings);
    }

    public void RefreshText()
    {
        SetDefaultState();
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
        Title = _settings.Strings.SearchBrowserBookmarks;
        Subtitle = _settings.Strings.TypeBookmarkTitleUrlOrFolder;
        Icon = Icons.Bookmarks;
        Command = new BookmarksPage(_bookmarkIndex, _settings);
        MoreCommands = [];
    }

    private void SetNoMatchState(string searchText)
    {
        Title = _settings.Strings.SearchBookmarksFor(searchText);
        Subtitle = _settings.Strings.ContinueSearching;
        Icon = Icons.Bookmarks;
        Command = new BookmarksPage(_bookmarkIndex, _settings);
        MoreCommands = [];
    }
}
