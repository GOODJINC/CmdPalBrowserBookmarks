using System.Diagnostics;
using CmdPalBrowserBookmarks.Bookmarks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalBrowserBookmarks.Commands;

internal sealed partial class OpenBookmarkCommand : InvokableCommand
{
    private readonly BookmarkRecord _bookmark;

    public OpenBookmarkCommand(BookmarkRecord bookmark)
    {
        _bookmark = bookmark;
        Name = "Open";
        Icon = Icons.Open;
    }

    public override ICommandResult Invoke()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = _bookmark.Url,
                UseShellExecute = true,
            });

            return CommandResult.Dismiss();
        }
        catch (Exception ex)
        {
            return CommandResult.ShowToast($"Failed to open bookmark: {ex.Message}");
        }
    }
}
