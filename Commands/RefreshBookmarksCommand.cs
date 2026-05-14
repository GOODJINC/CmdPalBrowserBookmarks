using CmdPalBrowserBookmarks.Bookmarks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalBrowserBookmarks.Commands;

internal sealed partial class RefreshBookmarksCommand : InvokableCommand
{
    private readonly BookmarkIndex _bookmarkIndex;
    private readonly Action _onRefreshed;

    public RefreshBookmarksCommand(BookmarkIndex bookmarkIndex, Action onRefreshed)
    {
        _bookmarkIndex = bookmarkIndex;
        _onRefreshed = onRefreshed;
        Name = "Refresh";
        Icon = Icons.Refresh;
    }

    public override ICommandResult Invoke()
    {
        _bookmarkIndex.Invalidate();
        var count = _bookmarkIndex.GetBookmarks().Count;
        _onRefreshed();
        return CommandResult.ShowToast($"Loaded {count:N0} browser bookmarks.");
    }
}
