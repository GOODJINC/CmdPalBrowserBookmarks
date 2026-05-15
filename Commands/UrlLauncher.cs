using System.Diagnostics;
using Microsoft.Win32;

namespace CmdPalBrowserBookmarks.Commands;

internal static class UrlLauncher
{
    internal static void Open(string url, UrlOpenMode mode)
    {
        if (mode == UrlOpenMode.NewWindow && TryOpenInNewWindow(url))
        {
            return;
        }

        OpenDefault(url);
    }

    private static bool TryOpenInNewWindow(string url)
    {
        var browserPath = GetDefaultBrowserExecutablePath();
        if (string.IsNullOrWhiteSpace(browserPath))
        {
            return false;
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = browserPath,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Normal,
            };

            if (IsFirefoxBrowser(browserPath))
            {
                startInfo.ArgumentList.Add("-new-window");
            }
            else
            {
                startInfo.ArgumentList.Add("--new-window");
            }

            startInfo.ArgumentList.Add(url);
            Process.Start(startInfo);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void OpenDefault(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Normal,
        });
    }

    private static string? GetDefaultBrowserExecutablePath()
    {
        return GetDefaultBrowserExecutablePath("https") ?? GetDefaultBrowserExecutablePath("http");
    }

    private static string? GetDefaultBrowserExecutablePath(string scheme)
    {
        var progId = Registry.GetValue(
            $@"HKEY_CURRENT_USER\Software\Microsoft\Windows\Shell\Associations\UrlAssociations\{scheme}\UserChoice",
            "ProgId",
            null) as string;

        if (string.IsNullOrWhiteSpace(progId))
        {
            return null;
        }

        using var commandKey = Registry.ClassesRoot.OpenSubKey($@"{progId}\shell\open\command");
        var command = commandKey?.GetValue(null) as string;
        return TryGetExecutablePath(command);
    }

    private static string? TryGetExecutablePath(string? command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return null;
        }

        command = Environment.ExpandEnvironmentVariables(command.Trim());
        string candidate;
        if (command.StartsWith('"'))
        {
            var closingQuoteIndex = command.IndexOf('"', 1);
            if (closingQuoteIndex <= 1)
            {
                return null;
            }

            candidate = command[1..closingQuoteIndex];
        }
        else
        {
            var exeIndex = command.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
            if (exeIndex < 0)
            {
                return null;
            }

            candidate = command[..(exeIndex + ".exe".Length)];
        }

        return File.Exists(candidate) ? candidate : null;
    }

    private static bool IsFirefoxBrowser(string browserPath)
    {
        var fileName = Path.GetFileNameWithoutExtension(browserPath);
        return fileName.Contains("firefox", StringComparison.OrdinalIgnoreCase) ||
            fileName.Contains("floorp", StringComparison.OrdinalIgnoreCase) ||
            fileName.Contains("librewolf", StringComparison.OrdinalIgnoreCase);
    }
}
