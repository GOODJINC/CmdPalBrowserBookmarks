using CmdPalBrowserBookmarks.Bookmarks;
using CmdPalBrowserBookmarks.Commands;
using CmdPalBrowserBookmarks.Settings;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalBrowserBookmarks.Pages;

internal sealed partial class BookmarkSettingsPage : ContentPage
{
    private readonly SettingsManager _settings;

    public BookmarkSettingsPage(SettingsManager settings, BookmarkIndex bookmarkIndex, Action onRefreshed)
    {
        _settings = settings;
        Id = "CmdPalBrowserBookmarks.Settings";
        Icon = Icons.Settings;
        Title = settings.Strings.BrowserBookmarkSettings;
        Name = settings.Strings.Settings;
        Commands =
        [
            new CommandContextItem(new RefreshBookmarksCommand(bookmarkIndex, settings, onRefreshed))
            {
                Title = settings.Strings.RefreshNow,
                Icon = Icons.Refresh,
            },
            new CommandContextItem(new ReloadCommandPaletteCommand(settings))
            {
                Title = settings.Strings.ReloadCommandPalette,
                Subtitle = settings.Strings.ReloadCommandPaletteSubtitle,
                Icon = Icons.Refresh,
            },
        ];
    }

    public void RefreshText()
    {
        Title = _settings.Strings.BrowserBookmarkSettings;
        Name = _settings.Strings.Settings;
    }

    public override IContent[] GetContent()
    {
        return _settings.BasicSettings.ToContent();
    }
}
