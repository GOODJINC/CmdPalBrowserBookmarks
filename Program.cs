using System.Diagnostics;
using System.Text;
using System.Threading;
using CmdPalBrowserBookmarks.Commands;
using Microsoft.CommandPalette.Extensions;
using Shmuelie.WinRTServer;
using Shmuelie.WinRTServer.CsWinRT;

namespace CmdPalBrowserBookmarks;

public static class Program
{
    internal const string OpenUrlAfterDelayArgument = "--open-url-after-delay";
    internal const string OpenUrlInNewWindowAfterDelayArgument = "--open-url-in-new-window-after-delay";

    [MTAThread]
    public static void Main(string[] args)
    {
        if (TryOpenUrlAfterDelay(args))
        {
            return;
        }

        if (args.Length == 0 || !string.Equals(args[0], "-RegisterProcessAsComServer", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        using ManualResetEvent extensionDisposedEvent = new(false);
        ComServer server = new();

        BrowserBookmarksExtension extensionInstance = new(extensionDisposedEvent);
        server.RegisterClass<BrowserBookmarksExtension, IExtension>(() => extensionInstance);
        server.Start();

        extensionDisposedEvent.WaitOne();
        server.Stop();
        server.UnsafeDispose();
    }

    private static bool TryOpenUrlAfterDelay(string[] args)
    {
        var openMode = args.Length >= 2 && string.Equals(args[0], OpenUrlAfterDelayArgument, StringComparison.OrdinalIgnoreCase)
            ? UrlOpenMode.Default
            : args.Length >= 2 && string.Equals(args[0], OpenUrlInNewWindowAfterDelayArgument, StringComparison.OrdinalIgnoreCase)
                ? UrlOpenMode.NewWindow
                : (UrlOpenMode?)null;

        if (openMode is null)
        {
            return false;
        }

        try
        {
            var url = Encoding.UTF8.GetString(Convert.FromBase64String(args[1]));
            Thread.Sleep(200);
            UrlLauncher.Open(url, openMode.Value);
        }
        catch
        {
        }

        return true;
    }
}
