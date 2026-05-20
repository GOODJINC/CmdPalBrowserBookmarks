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
    private BookmarkFallbackCommandItem _bookmarkFallbackCommand;
    private IFallbackCommandItem[] _fallbackCommands;
    private readonly BookmarksPage _bookmarksPage;
    private readonly BookmarkSettingsPage _settingsPage;
    private readonly AdvancedProfileSettingsPage _advancedProfileSettingsPage;
    private readonly RefreshBookmarksCommand _refreshBookmarksCommand;
    private readonly CommandItem _bookmarksCommandItem;
    private readonly CommandItem _settingsCommandItem;
    private readonly CommandItem _advancedProfileSettingsCommandItem;
    private readonly CommandItem _refreshCommandItem;
    private ICommandItem[] _topLevelCommands = [];
    private IReadOnlyList<BookmarkRecord> _loadedBookmarks = [];
    private bool _hasLoadedBookmarks;
    private bool _refreshInProgress;

    public BrowserBookmarksCommandsProvider()
    {
        Id = "CmdPalBrowserBookmarks";
        DisplayName = _settings.Strings.BrowserBookmarks;
        Icon = Icons.Bookmarks;
        Frozen = false;
        Settings = _settings.BasicSettings;

        _bookmarkIndex = new BookmarkIndex(_settings);
        _bookmarkFallbackCommand = new BookmarkFallbackCommandItem(_bookmarkIndex, _settings);
        _fallbackCommands = [_bookmarkFallbackCommand];
        _bookmarksPage = new BookmarksPage(_bookmarkIndex, _settings);
        _settingsPage = new BookmarkSettingsPage(_settings, _bookmarkIndex, () =>
        {
            var currentBookmarks = _loadedBookmarks;
            UpdateTopLevelCommands(currentBookmarks, _hasLoadedBookmarks);
            RaiseItemsChanged(_topLevelCommands.Length);
        });
        _advancedProfileSettingsPage = new AdvancedProfileSettingsPage(_settings);
        _refreshBookmarksCommand = new RefreshBookmarksCommand(_bookmarkIndex, _settings, () =>
        {
            var currentBookmarks = _bookmarkIndex.GetCachedBookmarks();
            lock (_gate)
            {
                _loadedBookmarks = currentBookmarks;
                _hasLoadedBookmarks = true;
            }

            UpdateTopLevelCommands(currentBookmarks, true);
            RaiseItemsChanged(_topLevelCommands.Length);
        });

        _bookmarksCommandItem = new CommandItem(_bookmarksPage)
        {
            Icon = Icons.Bookmarks,
        };
        _settingsCommandItem = new CommandItem(_settingsPage)
        {
            Icon = Icons.Settings,
        };
        _advancedProfileSettingsCommandItem = new CommandItem(_advancedProfileSettingsPage)
        {
            Icon = Icons.Settings,
        };
        _refreshCommandItem = new CommandItem(_refreshBookmarksCommand)
        {
            Icon = Icons.Refresh,
        };
        _topLevelCommands =
        [
            _bookmarksCommandItem,
            _settingsCommandItem,
            _advancedProfileSettingsCommandItem,
            _refreshCommandItem,
        ];

        _settings.SettingsChanged += (_, _) =>
        {
            _bookmarkIndex.Invalidate();
            lock (_gate)
            {
                _loadedBookmarks = [];
                _hasLoadedBookmarks = false;
            }

            RefreshFallbackCommand();
            DisplayName = _settings.Strings.BrowserBookmarks;
            UpdateTopLevelCommands([], false);
            RaiseItemsChanged(_topLevelCommands.Length);
            QueueBookmarkRefresh(notifyItemsChanged: true);
        };

        UpdateTopLevelCommands([], false);
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

    private void RefreshFallbackCommand()
    {
        _bookmarkFallbackCommand = new BookmarkFallbackCommandItem(_bookmarkIndex, _settings);
        _fallbackCommands = [_bookmarkFallbackCommand];
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

                UpdateTopLevelCommands(bookmarks, true);
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
                    UpdateTopLevelCommands(bookmarks, true);
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

    private void UpdateTopLevelCommands(IReadOnlyList<BookmarkRecord> bookmarks, bool hasLoadedBookmarks)
    {
        var bookmarksSubtitle = hasLoadedBookmarks
            ? _settings.Strings.BookmarksFromEnabledBrowsers(bookmarks.Count)
            : _settings.Strings.LoadingBookmarks;

        _bookmarksPage.RefreshText();
        _settingsPage.RefreshText();
        _advancedProfileSettingsPage.RefreshText();
        _refreshBookmarksCommand.RefreshText();
        _bookmarkFallbackCommand.RefreshText();

        _bookmarksCommandItem.Title = _settings.Strings.BrowserBookmarks;
        _bookmarksCommandItem.Subtitle = bookmarksSubtitle;
        _settingsCommandItem.Title = _settings.Strings.BrowserBookmarkSettings;
        _settingsCommandItem.Subtitle = _settings.Strings.SettingsSubtitle;
        _advancedProfileSettingsCommandItem.Title = _settings.Strings.AdvancedProfileSettings;
        _advancedProfileSettingsCommandItem.Subtitle = _settings.Strings.AdvancedProfileSettingsSubtitle;
        _refreshCommandItem.Title = _settings.Strings.RefreshBrowserBookmarks;
        _refreshCommandItem.Subtitle = _settings.Strings.RefreshBrowserBookmarksSubtitle;

        lock (_gate)
        {
            _topLevelCommands =
            [
                _bookmarksCommandItem,
                _settingsCommandItem,
                _advancedProfileSettingsCommandItem,
                _refreshCommandItem,
            ];
        }
    }
}
