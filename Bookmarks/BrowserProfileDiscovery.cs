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
                settings.EdgeProfileMode,
                settings.SelectedEdgeProfileId);
        }

        if (settings.EnableChrome)
        {
            AddChromiumProfiles(
                chromiumProfiles,
                watchedFiles,
                BookmarkSourceKind.Chrome,
                "Google Chrome",
                Path.Combine(localAppData, "Google", "Chrome", "User Data"),
                settings.ChromeProfileMode,
                settings.SelectedChromeProfileId);
        }

        foreach (var customPath in ParseCustomChromiumPaths(settings.CustomChromiumUserDataFolders))
        {
            AddChromiumProfiles(
                chromiumProfiles,
                watchedFiles,
                BookmarkSourceKind.Chromium,
                "Chromium",
                customPath,
                settings.ChromeProfileMode,
                settings.SelectedChromeProfileId);
        }

        if (settings.EnableFirefox)
        {
            AddFirefoxProfiles(
                firefoxProfiles,
                watchedFiles,
                GetFirefoxRoots(roamingAppData, localAppData),
                settings.FirefoxProfileMode,
                settings.SelectedFirefoxProfileId);
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
        BrowserProfileMode profileMode,
        string selectedProfileId)
    {
        var discovery = GetChromiumProfiles(source, browserName, userDataPath);
        if (discovery.LocalStatePath is not null)
        {
            watchedFiles.Add(discovery.LocalStatePath);
        }

        var selectedProfiles = SelectChromiumProfiles(
            discovery.Profiles,
            discovery.LastUsedProfileId,
            profileMode,
            selectedProfileId);

        foreach (var profile in selectedProfiles)
        {
            profiles.Add(profile);
            watchedFiles.Add(profile.BookmarksPath);
        }
    }

    internal static ChromiumProfileDiscoveryResult GetChromiumProfiles(
        BookmarkSourceKind source,
        string browserName,
        string userDataPath)
    {
        if (!Directory.Exists(userDataPath))
        {
            return new ChromiumProfileDiscoveryResult([], null, null);
        }

        var localStatePath = Path.Combine(userDataPath, "Local State");
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
            return new ChromiumProfileDiscoveryResult([], profileMetadata.LastUsedProfileId, localStatePath);
        }

        var profiles = candidates
            .Select(profilePath =>
            {
                var profileId = Path.GetFileName(profilePath);
                var profileName = profileMetadata.ProfileNames.TryGetValue(profileId, out var friendlyName)
                    ? friendlyName
                    : profileId;

                return new ChromiumProfile(source, browserName, userDataPath, profilePath, profileId, profileName);
            })
            .ToArray();

        return new ChromiumProfileDiscoveryResult(profiles, profileMetadata.LastUsedProfileId, localStatePath);
    }

    internal static IReadOnlyList<FirefoxProfile> GetFirefoxProfiles(string roamingAppData, string localAppData)
    {
        return GetFirefoxProfiles(GetFirefoxRoots(roamingAppData, localAppData));
    }

    private static IReadOnlyList<ChromiumProfile> SelectChromiumProfiles(
        IReadOnlyList<ChromiumProfile> profiles,
        string? lastUsedProfileId,
        BrowserProfileMode profileMode,
        string selectedProfileId)
    {
        return profileMode switch
        {
            BrowserProfileMode.All => profiles,
            BrowserProfileMode.Selected => SelectSpecificChromiumProfile(profiles, selectedProfileId, lastUsedProfileId),
            _ => SelectPreferredChromiumProfile(profiles, lastUsedProfileId),
        };
    }

    private static IReadOnlyList<ChromiumProfile> SelectSpecificChromiumProfile(
        IReadOnlyList<ChromiumProfile> profiles,
        string selectedProfileId,
        string? lastUsedProfileId)
    {
        var profile = profiles.FirstOrDefault(candidate =>
            ProfileMatches(candidate.ProfileId, candidate.ProfileName, selectedProfileId));
        return profile is null
            ? SelectPreferredChromiumProfile(profiles, lastUsedProfileId)
            : [profile];
    }

    private static IReadOnlyList<ChromiumProfile> SelectPreferredChromiumProfile(
        IReadOnlyList<ChromiumProfile> profiles,
        string? lastUsedProfileId)
    {
        var profile = GetPreferredChromiumProfile(profiles, lastUsedProfileId);
        return profile is null ? [] : [profile];
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

    private static ChromiumProfile? GetPreferredChromiumProfile(
        IReadOnlyList<ChromiumProfile> candidates,
        string? lastUsedProfileId)
    {
        if (candidates.Count == 0)
        {
            return null;
        }

        var lastUsedProfile = candidates.FirstOrDefault(path =>
            string.Equals(path.ProfileId, lastUsedProfileId, StringComparison.OrdinalIgnoreCase));
        if (lastUsedProfile is not null)
        {
            return lastUsedProfile;
        }

        var defaultProfile = candidates.FirstOrDefault(path =>
            string.Equals(path.ProfileId, "Default", StringComparison.OrdinalIgnoreCase));
        return defaultProfile ?? candidates.FirstOrDefault();
    }

    private static void AddFirefoxProfiles(
        List<FirefoxProfile> profiles,
        List<string> watchedFiles,
        IEnumerable<string> firefoxRoots,
        BrowserProfileMode profileMode,
        string selectedProfileId)
    {
        var discoveredProfiles = new List<FirefoxProfile>();
        foreach (var firefoxRoot in firefoxRoots.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var profilesIniPath = Path.Combine(firefoxRoot, "profiles.ini");
            watchedFiles.Add(profilesIniPath);

            if (!File.Exists(profilesIniPath))
            {
                continue;
            }

            try
            {
                discoveredProfiles.AddRange(
                    ParseFirefoxProfilesIni(profilesIniPath, firefoxRoot)
                        .Where(profile => File.Exists(profile.PlacesPath)));
            }
            catch
            {
            }
        }

        var selectedProfiles = SelectFirefoxProfiles(
            discoveredProfiles
                .DistinctBy(profile => profile.ProfilePath, StringComparer.OrdinalIgnoreCase)
                .OrderBy(profile => profile.ProfileName, StringComparer.CurrentCultureIgnoreCase)
                .ToArray(),
            profileMode,
            selectedProfileId);

        foreach (var profile in selectedProfiles)
        {
            profiles.Add(profile);
            watchedFiles.Add(profile.PlacesPath);
            watchedFiles.Add(profile.PlacesPath + "-wal");
            watchedFiles.Add(profile.PlacesPath + "-shm");
        }
    }

    private static IReadOnlyList<FirefoxProfile> GetFirefoxProfiles(IEnumerable<string> firefoxRoots)
    {
        var profiles = new List<FirefoxProfile>();
        foreach (var firefoxRoot in firefoxRoots.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var profilesIniPath = Path.Combine(firefoxRoot, "profiles.ini");
            if (!File.Exists(profilesIniPath))
            {
                continue;
            }

            try
            {
                profiles.AddRange(
                    ParseFirefoxProfilesIni(profilesIniPath, firefoxRoot)
                        .Where(profile => File.Exists(profile.PlacesPath)));
            }
            catch
            {
            }
        }

        return profiles
            .DistinctBy(profile => profile.ProfilePath, StringComparer.OrdinalIgnoreCase)
            .OrderBy(profile => profile.ProfileName, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<FirefoxProfile> SelectFirefoxProfiles(
        IReadOnlyList<FirefoxProfile> profiles,
        BrowserProfileMode profileMode,
        string selectedProfileId)
    {
        return profileMode switch
        {
            BrowserProfileMode.All => profiles,
            BrowserProfileMode.Selected => SelectSpecificFirefoxProfile(profiles, selectedProfileId),
            _ => SelectPreferredFirefoxProfile(profiles),
        };
    }

    private static IReadOnlyList<FirefoxProfile> SelectSpecificFirefoxProfile(
        IReadOnlyList<FirefoxProfile> profiles,
        string selectedProfileId)
    {
        var profile = profiles.FirstOrDefault(candidate =>
            ProfileMatches(candidate.ProfileId, candidate.ProfileName, selectedProfileId));
        return profile is null
            ? SelectPreferredFirefoxProfile(profiles)
            : [profile];
    }

    private static IReadOnlyList<FirefoxProfile> SelectPreferredFirefoxProfile(IReadOnlyList<FirefoxProfile> profiles)
    {
        var profile = profiles.FirstOrDefault(profile => profile.IsDefault) ??
            profiles.FirstOrDefault(profile => profile.ProfileId.Contains("default", StringComparison.OrdinalIgnoreCase)) ??
            profiles.FirstOrDefault();

        return profile is null ? [] : [profile];
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

            var isDefault = entry.TryGetValue("Default", out var defaultValue) && defaultValue == "1";

            profiles.Add(new FirefoxProfile("Mozilla Firefox", profilePath, id, name, isDefault));
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

    private static bool ProfileMatches(string profileId, string profileName, string selectedProfileId)
    {
        return !string.IsNullOrWhiteSpace(selectedProfileId) &&
            (string.Equals(profileId, selectedProfileId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(profileName, selectedProfileId, StringComparison.OrdinalIgnoreCase));
    }

    private sealed record ChromiumProfileMetadata(
        IReadOnlyDictionary<string, string> ProfileNames,
        string? LastUsedProfileId);

    internal sealed record ChromiumProfileDiscoveryResult(
        IReadOnlyList<ChromiumProfile> Profiles,
        string? LastUsedProfileId,
        string? LocalStatePath);
}
