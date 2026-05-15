using System.Text.Json;
using CmdPalBrowserBookmarks.Settings;

namespace CmdPalBrowserBookmarks.Bookmarks;

internal static class BrowserProfileDiscovery
{
    internal static BookmarkSourceCatalog Discover(SettingsManager settings)
    {
        var chromiumProfiles = new List<ChromiumProfile>();
        var firefoxProfiles = new List<FirefoxProfile>();
        var watchedFiles = new List<string>();

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        if (settings.EnableEdge)
        {
            AddChromiumProfiles(
                chromiumProfiles,
                watchedFiles,
                BookmarkSourceKind.Edge,
                "Microsoft Edge",
                Path.Combine(localAppData, "Microsoft", "Edge", "User Data"),
                settings.IncludeAllProfiles);
        }

        if (settings.EnableChrome)
        {
            AddChromiumProfiles(
                chromiumProfiles,
                watchedFiles,
                BookmarkSourceKind.Chrome,
                "Google Chrome",
                Path.Combine(localAppData, "Google", "Chrome", "User Data"),
                settings.IncludeAllProfiles);
        }

        foreach (var customPath in ParseCustomChromiumPaths(settings.CustomChromiumUserDataFolders))
        {
            AddChromiumProfiles(
                chromiumProfiles,
                watchedFiles,
                BookmarkSourceKind.Chromium,
                "Chromium",
                customPath,
                settings.IncludeAllProfiles);
        }

        if (settings.EnableFirefox)
        {
            foreach (var firefoxRoot in GetFirefoxRoots(roamingAppData, localAppData))
            {
                AddFirefoxProfiles(
                    firefoxProfiles,
                    watchedFiles,
                    firefoxRoot,
                    settings.IncludeAllProfiles);
            }
        }

        return new BookmarkSourceCatalog(
            chromiumProfiles,
            firefoxProfiles,
            watchedFiles.Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
    }

    private static void AddChromiumProfiles(
        List<ChromiumProfile> profiles,
        List<string> watchedFiles,
        BookmarkSourceKind source,
        string browserName,
        string userDataPath,
        bool includeAllProfiles)
    {
        if (!Directory.Exists(userDataPath))
        {
            return;
        }

        var localStatePath = Path.Combine(userDataPath, "Local State");
        watchedFiles.Add(localStatePath);

        var profileMetadata = ReadChromiumProfileMetadata(localStatePath);
        List<string> candidates;
        try
        {
            candidates = Directory.EnumerateDirectories(userDataPath)
                .Where(path => File.Exists(Path.Combine(path, "Bookmarks")))
                .OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch
        {
            return;
        }

        if (!includeAllProfiles)
        {
            var preferredProfile = GetPreferredChromiumProfile(candidates, profileMetadata.LastUsedProfileId);
            candidates = preferredProfile is null ? [] : [preferredProfile];
        }

        foreach (var profilePath in candidates)
        {
            var profileId = Path.GetFileName(profilePath);
            var profileName = profileMetadata.ProfileNames.TryGetValue(profileId, out var friendlyName)
                ? friendlyName
                : profileId;

            var profile = new ChromiumProfile(source, browserName, userDataPath, profilePath, profileId, profileName);
            profiles.Add(profile);
            watchedFiles.Add(profile.BookmarksPath);
        }
    }

    private static ChromiumProfileMetadata ReadChromiumProfileMetadata(string localStatePath)
    {
        var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string? lastUsedProfileId = null;
        if (!File.Exists(localStatePath))
        {
            return new ChromiumProfileMetadata(names, lastUsedProfileId);
        }

        try
        {
            using var stream = SharedRead(localStatePath);
            using var document = JsonDocument.Parse(stream);
            if (!document.RootElement.TryGetProperty("profile", out var profile))
            {
                return new ChromiumProfileMetadata(names, lastUsedProfileId);
            }

            if (profile.TryGetProperty("last_used", out var lastUsedElement))
            {
                lastUsedProfileId = lastUsedElement.GetString();
            }

            if (!profile.TryGetProperty("info_cache", out var infoCache) ||
                infoCache.ValueKind != JsonValueKind.Object)
            {
                return new ChromiumProfileMetadata(names, lastUsedProfileId);
            }

            foreach (var property in infoCache.EnumerateObject())
            {
                if (property.Value.TryGetProperty("name", out var nameElement))
                {
                    var name = nameElement.GetString();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        names[property.Name] = name;
                    }
                }
            }
        }
        catch
        {
        }

        return new ChromiumProfileMetadata(names, lastUsedProfileId);
    }

    private static string? GetPreferredChromiumProfile(IReadOnlyList<string> candidates, string? lastUsedProfileId)
    {
        if (candidates.Count == 0)
        {
            return null;
        }

        var lastUsedProfile = candidates.FirstOrDefault(path =>
            string.Equals(Path.GetFileName(path), lastUsedProfileId, StringComparison.OrdinalIgnoreCase));
        if (lastUsedProfile is not null)
        {
            return lastUsedProfile;
        }

        var defaultProfile = candidates.FirstOrDefault(path =>
            string.Equals(Path.GetFileName(path), "Default", StringComparison.OrdinalIgnoreCase));
        return defaultProfile ?? candidates.FirstOrDefault();
    }

    private static void AddFirefoxProfiles(
        List<FirefoxProfile> profiles,
        List<string> watchedFiles,
        string firefoxRoot,
        bool includeAllProfiles)
    {
        var profilesIniPath = Path.Combine(firefoxRoot, "profiles.ini");
        watchedFiles.Add(profilesIniPath);

        if (!File.Exists(profilesIniPath))
        {
            return;
        }

        List<FirefoxProfile> profileEntries;
        try
        {
            profileEntries = ParseFirefoxProfilesIni(profilesIniPath, firefoxRoot)
                .Where(profile => File.Exists(profile.PlacesPath))
                .OrderBy(profile => profile.ProfileName, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }
        catch
        {
            return;
        }

        if (!includeAllProfiles)
        {
            profileEntries = profileEntries
                .OrderByDescending(profile => profile.ProfileId.Contains("default", StringComparison.OrdinalIgnoreCase))
                .Take(1)
                .ToList();
        }

        foreach (var profile in profileEntries)
        {
            profiles.Add(profile);
            watchedFiles.Add(profile.PlacesPath);
            watchedFiles.Add(profile.PlacesPath + "-wal");
            watchedFiles.Add(profile.PlacesPath + "-shm");
        }
    }

    private static IEnumerable<string> GetFirefoxRoots(string roamingAppData, string localAppData)
    {
        yield return Path.Combine(roamingAppData, "Mozilla", "Firefox");

        var packagesPath = Path.Combine(localAppData, "Packages");
        if (!Directory.Exists(packagesPath))
        {
            yield break;
        }

        IReadOnlyList<string> packagePaths;
        try
        {
            packagePaths = Directory.EnumerateDirectories(packagesPath, "Mozilla.Firefox_*").ToArray();
        }
        catch
        {
            yield break;
        }

        foreach (var packagePath in packagePaths)
        {
            yield return Path.Combine(packagePath, "LocalCache", "Roaming", "Mozilla", "Firefox");
        }
    }

    private static IReadOnlyList<FirefoxProfile> ParseFirefoxProfilesIni(string profilesIniPath, string firefoxRoot)
    {
        var profiles = new List<FirefoxProfile>();
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawLine in File.ReadLines(profilesIniPath))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith(';') || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                AddFirefoxProfile(values);
                values.Clear();
                continue;
            }

            var equalsIndex = line.IndexOf('=');
            if (equalsIndex > 0)
            {
                values[line[..equalsIndex].Trim()] = line[(equalsIndex + 1)..].Trim();
            }
        }

        AddFirefoxProfile(values);
        return profiles;

        void AddFirefoxProfile(Dictionary<string, string> entry)
        {
            if (!entry.TryGetValue("Path", out var path) || string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var isRelative = !entry.TryGetValue("IsRelative", out var relative) || relative == "1";
            var profilePath = isRelative
                ? Path.GetFullPath(Path.Combine(firefoxRoot, path.Replace('/', Path.DirectorySeparatorChar)))
                : path;

            var id = Path.GetFileName(profilePath);
            var name = entry.TryGetValue("Name", out var configuredName) && !string.IsNullOrWhiteSpace(configuredName)
                ? configuredName
                : id;

            profiles.Add(new FirefoxProfile("Mozilla Firefox", profilePath, id, name));
        }
    }

    private static IEnumerable<string> ParseCustomChromiumPaths(string rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            yield break;
        }

        foreach (var part in rawValue.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var path = Environment.ExpandEnvironmentVariables(part.Trim().Trim('"'));
            if (Directory.Exists(path))
            {
                yield return path;
            }
        }
    }

    private static FileStream SharedRead(string path)
    {
        return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
    }

    private sealed record ChromiumProfileMetadata(
        IReadOnlyDictionary<string, string> ProfileNames,
        string? LastUsedProfileId);
}
