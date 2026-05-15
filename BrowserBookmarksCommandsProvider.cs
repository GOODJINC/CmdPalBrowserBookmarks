using CmdPalBrowserBookmarks.Bookmarks;
using CmdPalBrowserBookmarks.Commands;
using CmdPalBrowserBookmarks.Pages;
using CmdPalBrowserBookmarks.Settings;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalBrowserBookmarks;

internal sealed partial class BrowserBookmarksCommandsProvider : CommandProvider
{
    private static readonly IFallbackCommandItem[] NoFallbackCommands = [];

    private readonly object _gate = new();
    private readonly SettingsManager _settings = new();
    private readonly BookmarkIndex _bookmarkIndex;
    private readonly BookmarkFallbackCommandItem _bookmarkFallbackCommand;
    private readonly IFallbackCommandItem[] _fallbackCommands;
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
        _bookmarkFallbackCommand = new BookmarkFallbackCommandItem(_bookmarkIndex);
        _fallbackCommands = [_bookmarkFallbackCommand];
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
            QueueBookmarkRefresh(notifyItemsChanged: true);
        };

        RebuildTopLevelCommands([], false);
        QueueBookmarkRefresh(notifyItemsChanged: false);
    }

    public override ICommandItem[] TopLevelCommands()
    {
        if (_hasLoadedBookmarks && _bookmarkIndex.HasChanges())
        {
            QueueBookmarkRefresh(notifyItemsChanged: true);
        }

        lock (_gate)
        {
            return _topLevelCommands;
        }
    }

    public override IFallbackCommandItem[] FallbackCommands()
    {
        return _settings.EnableHomePageSuggestions ? _fallbackCommands : NoFallbackCommands;
    }

    private void QueueBookmarkRefresh(bool notifyItemsChanged)
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
                if (notifyItemsChanged)
                {
                    RaiseItemsChanged(_topLevelCommands.Length);
                }
            }
            catch
            {
                var bookmarks = _bookmarkIndex.GetCachedBookmarks();
                if (bookmarks.Count > 0)
                {
                    RebuildTopLevelCommands(bookmarks, true);
                    if (notifyItemsChanged)
                    {
                        RaiseItemsChanged(_topLevelCommands.Length);
                    }
                }
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
                Subtitle = "Choose Edge, Chrome, Firefox, profiles, and home-page suggestions",
                Icon = Icons.Settings,
            },
            new CommandItem(new RefreshBookmarksCommand(_bookmarkIndex, () =>
            {
                var currentBookmarks = _bookmarkIndex.GetCachedBookmarks();
                lock (_gate)
                {
                    _loadedBookmarks = currentBookmarks;
                    _hasLoadedBookmarks = true;
                }

                RebuildTopLevelCommands(currentBookmarks, true);
                RaiseItemsChanged(_topLevelCommands.Length);
            }))
            {
                Title = "Refresh Browser Bookmarks",
                Subtitle = "Reload bookmark files from enabled browsers",
                Icon = Icons.Refresh,
            },
        ];

        lock (_gate)
        {
            _topLevelCommands = commands.ToArray();
        }
    }
}
