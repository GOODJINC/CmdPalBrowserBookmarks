using System.Diagnostics;
using Microsoft.Win32;

namespace CmdPalBrowserBookmarks.Commands;

internal static class UrlLauncher
{
    internal static void Open(string url, UrlOpenMode mode, BrowserLaunchTarget target)
    {
        if (target != BrowserLaunchTarget.Default && TryOpenInSpecificBrowser(url, mode, target))
        {
            return;
        }

        if (mode == UrlOpenMode.NewWindow && TryOpenInNewWindow(url, GetDefaultBrowserExecutablePath()))
        {
            return;
        }

        OpenDefault(url);
    }

    internal static BrowserLaunchTarget ReadTarget(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "edge" => BrowserLaunchTarget.Edge,
            "chrome" => BrowserLaunchTarget.Chrome,
            "firefox" => BrowserLaunchTarget.Firefox,
            _ => BrowserLaunchTarget.Default,
        };
    }

    internal static string WriteTarget(BrowserLaunchTarget target)
    {
        return target switch
        {
            BrowserLaunchTarget.Edge => "edge",
            BrowserLaunchTarget.Chrome => "chrome",
            BrowserLaunchTarget.Firefox => "firefox",
            _ => "default",
        };
    }

    private static bool TryOpenInSpecificBrowser(string url, UrlOpenMode mode, BrowserLaunchTarget target)
    {
        var browserPath = FindBrowserExecutable(target);
        if (string.IsNullOrWhiteSpace(browserPath))
        {
            return false;
        }

        return mode == UrlOpenMode.NewWindow
            ? TryOpenInNewWindow(url, browserPath)
            : TryOpenInBrowser(url, browserPath);
    }

    private static bool TryOpenInNewWindow(string url, string? browserPath)
    {
        if (string.IsNullOrWhiteSpace(browserPath))
        {
            return false;
        }

        try
        {
            var startInfo = BrowserStartInfo(browserPath);
            startInfo.ArgumentList.Add(IsFirefoxBrowser(browserPath) ? "-new-window" : "--new-window");
            startInfo.ArgumentList.Add(url);
            Process.Start(startInfo);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryOpenInBrowser(string url, string browserPath)
    {
        try
        {
            var startInfo = BrowserStartInfo(browserPath);
            startInfo.ArgumentList.Add(url);
            Process.Start(startInfo);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static ProcessStartInfo BrowserStartInfo(string browserPath)
    {
        return new ProcessStartInfo
        {
            FileName = browserPath,
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Normal,
        };
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

    private static string? FindBrowserExecutable(BrowserLaunchTarget target)
    {
        return target switch
        {
            BrowserLaunchTarget.Edge => FindExecutable("msedge.exe", [
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft", "Edge", "Application", "msedge.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft", "Edge", "Application", "msedge.exe"),
            ]),
            BrowserLaunchTarget.Chrome => FindExecutable("chrome.exe", [
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google", "Chrome", "Application", "chrome.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google", "Chrome", "Application", "chrome.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "Application", "chrome.exe"),
            ]),
            BrowserLaunchTarget.Firefox => FindExecutable("firefox.exe", [
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Mozilla Firefox", "firefox.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Mozilla Firefox", "firefox.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WindowsApps", "firefox.exe"),
            ]),
            _ => null,
        };
    }

    private static string? FindExecutable(string executableName, IReadOnlyList<string> fallbackPaths)
    {
        return FindAppPathExecutable(executableName) ??
            FindOnPath(executableName) ??
            fallbackPaths.FirstOrDefault(File.Exists);
    }

    private static string? FindAppPathExecutable(string executableName)
    {
        foreach (var root in new[] { Registry.CurrentUser, Registry.LocalMachine })
        {
            try
            {
                using var key = root.OpenSubKey($@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\{executableName}");
                var path = key?.GetValue(null) as string;
                if (!string.IsNullOrWhiteSpace(path))
                {
                    path = Environment.ExpandEnvironmentVariables(path.Trim('"'));
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
            }
            catch
            {
            }
        }

        return null;
    }

    private static string? FindOnPath(string executableName)
    {
        var pathValue = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathValue))
        {
            return null;
        }

        foreach (var directory in pathValue.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            try
            {
                var candidate = Path.Combine(directory, executableName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
            catch
            {
            }
        }

        return null;
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
