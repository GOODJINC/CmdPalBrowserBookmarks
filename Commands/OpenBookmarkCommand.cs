using System.Diagnostics;
using System.Text;
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
            OpenAfterPaletteDismisses();
            return CommandResult.Dismiss();
        }
        catch (Exception ex)
        {
            return CommandResult.ShowToast($"Failed to open bookmark: {ex.Message}");
        }
    }

    private void OpenAfterPaletteDismisses()
    {
        var processPath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(processPath))
        {
            OpenDirectly();
            return;
        }

        var encodedUrl = Convert.ToBase64String(Encoding.UTF8.GetBytes(_bookmark.Url));
        var startInfo = new ProcessStartInfo
        {
            FileName = processPath,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        };

        startInfo.ArgumentList.Add(Program.OpenUrlAfterDelayArgument);
        startInfo.ArgumentList.Add(encodedUrl);
        Process.Start(startInfo);
    }

    private void OpenDirectly()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = _bookmark.Url,
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Normal,
        });
    }
}
