using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.CommandPalette.Extensions;
using Shmuelie.WinRTServer;
using Shmuelie.WinRTServer.CsWinRT;

namespace CmdPalBrowserBookmarks;

public static class Program
{
    internal const string OpenUrlAfterDelayArgument = "--open-url-after-delay";

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
        if (args.Length < 2 || !string.Equals(args[0], OpenUrlAfterDelayArgument, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        try
        {
            var url = Encoding.UTF8.GetString(Convert.FromBase64String(args[1]));
            Thread.Sleep(200);
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Normal,
            });
        }
        catch
        {
        }

        return true;
    }
}
