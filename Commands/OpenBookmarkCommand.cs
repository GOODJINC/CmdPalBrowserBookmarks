using System.Diagnostics;
using System.Text;
using CmdPalBrowserBookmarks.Bookmarks;
using CmdPalBrowserBookmarks.Settings;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalBrowserBookmarks.Commands;

internal sealed partial class OpenBookmarkCommand : InvokableCommand
{
    private readonly BookmarkRecord _bookmark;
    private readonly SettingsManager _settings;
    private readonly UrlOpenMode _openMode;

    public OpenBookmarkCommand(BookmarkRecord bookmark, SettingsManager settings, UrlOpenMode openMode = UrlOpenMode.Default)
    {
        _bookmark = bookmark;
        _settings = settings;
        _openMode = openMode;
        Name = openMode == UrlOpenMode.NewWindow ? settings.Strings.OpenInNewWindow : settings.Strings.Open;
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
            return CommandResult.ShowToast(_settings.Strings.FailedToOpenBookmark(ex.Message));
        }
    }

    private void OpenAfterPaletteDismisses(UrlOpenMode openMode)
    {
        var target = ResolveTarget(_settings.LaunchBrowserMode, _bookmark.Source);
        var processPath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(processPath))
        {
            UrlLauncher.Open(_bookmark.Url, openMode, target);
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
        startInfo.ArgumentList.Add(UrlLauncher.WriteTarget(target));
        Process.Start(startInfo);
    }

    private static BrowserLaunchTarget ResolveTarget(BrowserLaunchMode mode, BookmarkSourceKind source)
    {
        return mode switch
        {
            BrowserLaunchMode.Edge => BrowserLaunchTarget.Edge,
            BrowserLaunchMode.Chrome => BrowserLaunchTarget.Chrome,
            BrowserLaunchMode.Firefox => BrowserLaunchTarget.Firefox,
            BrowserLaunchMode.SourceBrowser => source switch
            {
                BookmarkSourceKind.Edge => BrowserLaunchTarget.Edge,
                BookmarkSourceKind.Chrome => BrowserLaunchTarget.Chrome,
                BookmarkSourceKind.Firefox => BrowserLaunchTarget.Firefox,
                _ => BrowserLaunchTarget.Default,
            },
            _ => BrowserLaunchTarget.Default,
        };
    }
}
