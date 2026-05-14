using System.Threading;
using Microsoft.CommandPalette.Extensions;
using Shmuelie.WinRTServer;
using Shmuelie.WinRTServer.CsWinRT;

namespace CmdPalBrowserBookmarks;

public static class Program
{
    [MTAThread]
    public static void Main(string[] args)
    {
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
}
