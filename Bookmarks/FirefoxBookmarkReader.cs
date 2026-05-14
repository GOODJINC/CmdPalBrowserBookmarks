using Microsoft.Data.Sqlite;

namespace CmdPalBrowserBookmarks.Bookmarks;

internal sealed class FirefoxBookmarkReader
{
    public IReadOnlyList<BookmarkRecord> Read(FirefoxProfile profile)
    {
        if (!File.Exists(profile.PlacesPath))
        {
            return [];
        }

        try
        {
            return ReadDatabase(profile, profile.PlacesPath);
        }
        catch
        {
            return ReadDatabaseCopy(profile);
        }
    }

    private static IReadOnlyList<BookmarkRecord> ReadDatabaseCopy(FirefoxProfile profile)
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "CmdPalBrowserBookmarks", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        var tempPlacesPath = Path.Combine(tempDirectory, "places.sqlite");

        try
        {
            File.Copy(profile.PlacesPath, tempPlacesPath, overwrite: true);
            CopySidecar(profile.PlacesPath + "-wal", tempPlacesPath + "-wal");
            CopySidecar(profile.PlacesPath + "-shm", tempPlacesPath + "-shm");
            return ReadDatabase(profile, tempPlacesPath);
        }
        finally
        {
            TryDeleteDirectory(tempDirectory);
        }
    }

    private static IReadOnlyList<BookmarkRecord> ReadDatabase(FirefoxProfile profile, string databasePath)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadOnly,
        }.ToString();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT b.id, b.parent, b.type, b.title, b.dateAdded, p.url
            FROM moz_bookmarks b
            LEFT JOIN moz_places p ON p.id = b.fk
            WHERE b.type IN (1, 2)
            ORDER BY b.position
            """;

        using var reader = command.ExecuteReader();
        var nodes = new Dictionary<long, FirefoxBookmarkNode>();

        while (reader.Read())
        {
            var node = new FirefoxBookmarkNode(
                reader.GetInt64(0),
                reader.GetInt64(1),
                reader.GetInt32(2),
                reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetInt64(4),
                reader.IsDBNull(5) ? string.Empty : reader.GetString(5));

            nodes[node.Id] = node;
        }

        var bookmarks = new List<BookmarkRecord>();
        foreach (var node in nodes.Values.Where(node => node.Type == 1))
        {
            if (!BookmarkUrl.IsLaunchable(node.Url))
            {
                continue;
            }

            bookmarks.Add(new BookmarkRecord(
                BookmarkText.TitleOrFallback(node.Title, node.Url),
                node.Url,
                BookmarkSourceKind.Firefox,
                profile.BrowserName,
                profile.ProfileName,
                BuildFolderPath(node.ParentId, nodes),
                ReadFirefoxDate(node.DateAdded),
                profile.PlacesPath));
        }

        return bookmarks;
    }

    private static string BuildFolderPath(long parentId, IReadOnlyDictionary<long, FirefoxBookmarkNode> nodes)
    {
        var names = new List<string>();
        var seen = new HashSet<long>();
        var current = parentId;

        while (nodes.TryGetValue(current, out var node) && seen.Add(current))
        {
            if (node.Type == 2 && !string.IsNullOrWhiteSpace(node.Title))
            {
                names.Add(node.Title);
            }

            current = node.ParentId;
        }

        names.Reverse();
        return string.Join(" / ", names);
    }

    private static DateTimeOffset? ReadFirefoxDate(long? microsecondsSinceUnixEpoch)
    {
        if (microsecondsSinceUnixEpoch is not { } value || value <= 0)
        {
            return null;
        }

        try
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(value / 1000);
        }
        catch
        {
            return null;
        }
    }

    private static void CopySidecar(string sourcePath, string targetPath)
    {
        if (File.Exists(sourcePath))
        {
            File.Copy(sourcePath, targetPath, overwrite: true);
        }
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
        }
    }

    private sealed record FirefoxBookmarkNode(
        long Id,
        long ParentId,
        int Type,
        string Title,
        long? DateAdded,
        string Url);
}
