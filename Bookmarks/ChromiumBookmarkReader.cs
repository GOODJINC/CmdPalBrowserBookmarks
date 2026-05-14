using System.Text.Json;

namespace CmdPalBrowserBookmarks.Bookmarks;

internal sealed class ChromiumBookmarkReader
{
    public IReadOnlyList<BookmarkRecord> Read(ChromiumProfile profile)
    {
        if (!File.Exists(profile.BookmarksPath))
        {
            return [];
        }

        using var stream = SharedRead(profile.BookmarksPath);
        using var document = JsonDocument.Parse(stream);

        if (!document.RootElement.TryGetProperty("roots", out var roots) || roots.ValueKind != JsonValueKind.Object)
        {
            return [];
        }

        var bookmarks = new List<BookmarkRecord>();
        foreach (var root in roots.EnumerateObject())
        {
            var rootName = FriendlyRootName(root.Name);
            VisitNode(profile, root.Value, [rootName], bookmarks);
        }

        return bookmarks;
    }

    private static void VisitNode(
        ChromiumProfile profile,
        JsonElement node,
        IReadOnlyList<string> folders,
        List<BookmarkRecord> bookmarks)
    {
        if (!node.TryGetProperty("type", out var typeElement))
        {
            return;
        }

        var type = typeElement.GetString();
        if (string.Equals(type, "url", StringComparison.OrdinalIgnoreCase))
        {
            var title = GetString(node, "name");
            var url = GetString(node, "url");
            if (!BookmarkUrl.IsLaunchable(url))
            {
                return;
            }

            bookmarks.Add(new BookmarkRecord(
                BookmarkText.TitleOrFallback(title, url),
                url,
                profile.Source,
                profile.BrowserName,
                profile.ProfileName,
                string.Join(" / ", folders.Where(folder => !string.IsNullOrWhiteSpace(folder))),
                ReadChromiumDate(node),
                profile.BookmarksPath));

            return;
        }

        if (!string.Equals(type, "folder", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var folderName = GetString(node, "name");
        var childFolders = string.IsNullOrWhiteSpace(folderName)
            ? folders
            : folders.Concat([folderName]).ToArray();

        if (!node.TryGetProperty("children", out var children) || children.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var child in children.EnumerateArray())
        {
            VisitNode(profile, child, childFolders, bookmarks);
        }
    }

    private static string FriendlyRootName(string rootName)
    {
        return rootName switch
        {
            "bookmark_bar" => "Bookmarks bar",
            "other" => "Other bookmarks",
            "synced" => "Mobile bookmarks",
            _ => rootName,
        };
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) ? value.GetString() ?? string.Empty : string.Empty;
    }

    private static DateTimeOffset? ReadChromiumDate(JsonElement element)
    {
        var raw = GetString(element, "date_added");
        if (!long.TryParse(raw, out var microsecondsSince1601) || microsecondsSince1601 <= 0)
        {
            return null;
        }

        try
        {
            return DateTimeOffset.FromFileTime(microsecondsSince1601 * 10);
        }
        catch
        {
            return null;
        }
    }

    private static FileStream SharedRead(string path)
    {
        return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
    }
}
