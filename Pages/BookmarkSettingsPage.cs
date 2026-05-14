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
        Icon = Icons.Settings;
        Title = "Browser Bookmark Settings";
        Name = "Settings";
        Commands =
        [
            new CommandContextItem(new RefreshBookmarksCommand(bookmarkIndex, onRefreshed))
            {
                Title = "Refresh now",
                Icon = Icons.Refresh,
            },
        ];
    }

    public override IContent[] GetContent()
    {
        return _settings.Settings.ToContent();
    }
}
