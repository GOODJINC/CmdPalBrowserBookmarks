using System.Diagnostics;
using System.Text;
using CmdPalBrowserBookmarks.Bookmarks;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalBrowserBookmarks.Commands;

internal sealed partial class OpenBookmarkCommand : InvokableCommand
{
    private readonly BookmarkRecord _bookmark;
    private readonly UrlOpenMode _openMode;

    public OpenBookmarkCommand(BookmarkRecord bookmark, UrlOpenMode openMode = UrlOpenMode.Default)
    {
        _bookmark = bookmark;
        _openMode = openMode;
        Name = openMode == UrlOpenMode.NewWindow ? "Open in new window" : "Open";
        Icon = Icons.Open;
    }

    public override ICommandResult Invoke()
    {
        try
        {
            OpenAfterPaletteDismisses(_openMode);
            return CommandResult.Dismiss();
        }
        catch (Exception ex)
        {
            return CommandResult.ShowToast($"Failed to open bookmark: {ex.Message}");
        }
    }

    private void OpenAfterPaletteDismisses(UrlOpenMode openMode)
    {
        var processPath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(processPath))
        {
            UrlLauncher.Open(_bookmark.Url, openMode);
            return;
        }

        var encodedUrl = Convert.ToBase64String(Encoding.UTF8.GetBytes(_bookmark.Url));
        var openArgument = openMode == UrlOpenMode.NewWindow
            ? Program.OpenUrlInNewWindowAfterDelayArgument
            : Program.OpenUrlAfterDelayArgument;

        var startInfo = new ProcessStartInfo
        {
            FileName = processPath,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        };

        startInfo.ArgumentList.Add(openArgument);
        startInfo.ArgumentList.Add(encodedUrl);
        Process.Start(startInfo);
    }
}
