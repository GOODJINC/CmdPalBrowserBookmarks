using CmdPalBrowserBookmarks.Bookmarks;
using CmdPalBrowserBookmarks.Commands;
using CmdPalBrowserBookmarks.Settings;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalBrowserBookmarks.Pages;

internal sealed partial class BookmarkSettingsPage : ContentPage
{
    private readonly SettingsManager _settings;
    private readonly BookmarkIndex _bookmarkIndex;
    private readonly Action _onRefreshed;

    public BookmarkSettingsPage(SettingsManager settings, BookmarkIndex bookmarkIndex, Action onRefreshed)
    {
        _settings = settings;
        _bookmarkIndex = bookmarkIndex;
        _onRefreshed = onRefreshed;
        Id = "CmdPalBrowserBookmarks.Settings";
        Icon = Icons.Settings;
        Title = settings.Strings.BrowserBookmarkSettings;
        Name = settings.Strings.Settings;
        Commands = CreateCommands();
    }

    private ICommandContextItem[] CreateCommands()
    {
        return
        [
            new CommandContextItem(new RefreshBookmarksCommand(_bookmarkIndex, _settings, _onRefreshed))
            {
                Title = _settings.Strings.RefreshNow,
                Icon = Icons.Refresh,
            },
            new CommandContextItem(new ReloadCommandPaletteCommand(_settings))
            {
                Title = _settings.Strings.ReloadCommandPalette,
                Subtitle = _settings.Strings.ReloadCommandPaletteSubtitle,
                Icon = Icons.Refresh,
            },
        ];
    }

    public void RefreshText()
    {
        Title = _settings.Strings.BrowserBookmarkSettings;
        Name = _settings.Strings.Settings;
        Commands =
        [
            .. CreateCommands(),
        ];
    }

    public override IContent[] GetContent()
    {
        return _settings.BasicSettings.ToContent();
    }
}
