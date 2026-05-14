using CmdPalBrowserBookmarks.Bookmarks;
using CmdPalBrowserBookmarks.Commands;
using CmdPalBrowserBookmarks.Pages;
using CmdPalBrowserBookmarks.Settings;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalBrowserBookmarks;

internal sealed partial class BrowserBookmarksCommandsProvider : CommandProvider
{
    private readonly object _gate = new();
    private readonly SettingsManager _settings = new();
    private readonly BookmarkIndex _bookmarkIndex;
    private ICommandItem[] _topLevelCommands = [];
    private IReadOnlyList<BookmarkRecord> _loadedBookmarks = [];
    private bool _hasLoadedBookmarks;
    private bool _refreshInProgress;

    public BrowserBookmarksCommandsProvider()
    {
        Id = "CmdPalBrowserBookmarks";
        DisplayName = "Browser Bookmarks";
        Icon = Icons.Bookmarks;
        Frozen = false;
        Settings = _settings.Settings;

        _bookmarkIndex = new BookmarkIndex(_settings);
        _settings.Settings.SettingsChanged += (_, _) =>
        {
            _bookmarkIndex.Invalidate();
            lock (_gate)
            {
                _loadedBookmarks = [];
                _hasLoadedBookmarks = false;
            }

            RebuildTopLevelCommands([], false);
            RaiseItemsChanged(_topLevelCommands.Length);
            QueueBookmarkRefresh();
        };

        RebuildTopLevelCommands([], false);
        QueueBookmarkRefresh();
    }

    public override ICommandItem[] TopLevelCommands()
    {
        if (_hasLoadedBookmarks && _bookmarkIndex.HasChanges())
        {
            QueueBookmarkRefresh();
        }

        lock (_gate)
        {
            return _topLevelCommands;
        }
    }

    private void QueueBookmarkRefresh()
    {
        lock (_gate)
        {
            if (_refreshInProgress)
            {
                return;
            }

            _refreshInProgress = true;
        }

        _ = Task.Run(() =>
        {
            try
            {
                var bookmarks = _bookmarkIndex.GetBookmarks();
                lock (_gate)
                {
                    _loadedBookmarks = bookmarks;
                    _hasLoadedBookmarks = true;
                }

                RebuildTopLevelCommands(bookmarks, true);
                RaiseItemsChanged(_topLevelCommands.Length);
            }
            finally
            {
                lock (_gate)
                {
                    _refreshInProgress = false;
                }
            }
        });
    }

    private void RebuildTopLevelCommands(IReadOnlyList<BookmarkRecord> bookmarks, bool hasLoadedBookmarks)
    {
        var bookmarksSubtitle = hasLoadedBookmarks
            ? $"{bookmarks.Count:N0} bookmarks from enabled browsers"
            : "Loading bookmarks from enabled browsers";

        List<ICommandItem> commands =
        [
            new CommandItem(new BookmarksPage(_bookmarkIndex))
            {
                Title = "Browser Bookmarks",
                Subtitle = bookmarksSubtitle,
                Icon = Icons.Bookmarks,
            },
            new CommandItem(new BookmarkSettingsPage(_settings, _bookmarkIndex, () =>
            {
                var currentBookmarks = _loadedBookmarks;
                RebuildTopLevelCommands(currentBookmarks, _hasLoadedBookmarks);
                RaiseItemsChanged(_topLevelCommands.Length);
            }))
            {
                Title = "Browser Bookmark Settings",
                Subtitle = "Choose Edge, Chrome, Firefox, profiles, and top-level results",
                Icon = Icons.Settings,
            },
            new CommandItem(new RefreshBookmarksCommand(_bookmarkIndex, () =>
            {
                QueueBookmarkRefresh();
                RaiseItemsChanged(_topLevelCommands.Length);
            }))
            {
                Title = "Refresh Browser Bookmarks",
                Subtitle = "Reload bookmark files from enabled browsers",
                Icon = Icons.Refresh,
            },
        ];

        if (hasLoadedBookmarks && _settings.ShowBookmarksAtTopLevel)
        {
            commands.AddRange(bookmarks
                .Take(_settings.MaxTopLevelBookmarks)
                .Select(BookmarkItemFactory.CreateCommandItem));
        }

        lock (_gate)
        {
            _topLevelCommands = commands.ToArray();
        }
    }
}
