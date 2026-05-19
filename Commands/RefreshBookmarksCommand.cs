using CmdPalBrowserBookmarks.Bookmarks;
using CmdPalBrowserBookmarks.Settings;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalBrowserBookmarks.Commands;

internal sealed partial class RefreshBookmarksCommand : InvokableCommand
{
    private readonly BookmarkIndex _bookmarkIndex;
    private readonly SettingsManager _settings;
    private readonly Action _onRefreshed;

    public RefreshBookmarksCommand(BookmarkIndex bookmarkIndex, SettingsManager settings, Action onRefreshed)
    {
        _bookmarkIndex = bookmarkIndex;
        _settings = settings;
        _onRefreshed = onRefreshed;
        Id = "CmdPalBrowserBookmarks.RefreshBookmarks";
        Name = settings.Strings.Refresh;
        Icon = Icons.Refresh;
    }

    public void RefreshText()
    {
        Name = _settings.Strings.Refresh;
    }

    public override ICommandResult Invoke()
    {
        try
        {
            _bookmarkIndex.Invalidate();
            var count = _bookmarkIndex.GetBookmarks().Count;
            _onRefreshed();
            return CommandResult.ShowToast(_settings.Strings.LoadedBookmarks(count));
        }
        catch (Exception ex)
        {
            return CommandResult.ShowToast(_settings.Strings.FailedToRefreshBookmarks(ex.Message));
        }
    }
}
