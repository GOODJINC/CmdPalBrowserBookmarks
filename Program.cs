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

    [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    [MTAThread]
    public static void Main(string[] args)
    {
        if (TryOpenUrlAfterDelay(args))
        {
            return;
        }

        if (args.Length == 0 || !string.Equals(args[0], "-RegisterProcessAsComServer", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox(
                IntPtr.Zero,
                "This is a Microsoft PowerToys Command Palette extension and cannot be run directly.\n\nPlease open PowerToys Command Palette to use this extension.",
                "Browser Bookmarks for Command Palette",
                0x00000040); // MB_OK | MB_ICONINFORMATION
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
            var target = args.Length >= 3
                ? UrlLauncher.ReadTarget(args[2])
                : BrowserLaunchTarget.Default;
            UrlLauncher.Open(url, openMode.Value, target);
        }
        catch
        {
        }

        return true;
    }
}
