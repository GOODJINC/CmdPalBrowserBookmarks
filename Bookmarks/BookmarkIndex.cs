using System.Text;
using CmdPalBrowserBookmarks.Settings;

namespace CmdPalBrowserBookmarks.Bookmarks;

internal sealed class BookmarkIndex
{
    private readonly object _gate = new();
    private readonly SettingsManager _settings;
    private readonly ChromiumBookmarkReader _chromiumReader = new();
    private readonly FirefoxBookmarkReader _firefoxReader = new();
    private IReadOnlyList<BookmarkRecord>? _bookmarks;
    private SourceSnapshot? _snapshot;
    private DateTimeOffset _lastSnapshotCheckUtc = DateTimeOffset.MinValue;

    public BookmarkIndex(SettingsManager settings)
    {
        _settings = settings;
    }

    public BookmarkSearchOptions SearchOptions => new(_settings.EnableKoreanInitialConsonantSearch);

    public IReadOnlyList<BookmarkRecord> GetBookmarks()
    {
        var catalog = BrowserProfileDiscovery.Discover(_settings);
        var snapshot = SourceSnapshot.Capture(_settings, catalog);

        lock (_gate)
        {
            if (_bookmarks is not null && snapshot.Equals(_snapshot))
            {
                return _bookmarks;
            }
        }

        var bookmarks = LoadBookmarks(catalog);

        lock (_gate)
        {
            _snapshot = snapshot;
            _bookmarks = bookmarks;
            _lastSnapshotCheckUtc = DateTimeOffset.UtcNow;
            return _bookmarks;
        }
    }

    public IReadOnlyList<BookmarkRecord> GetCachedBookmarks()
    {
        lock (_gate)
        {
            return _bookmarks ?? [];
        }
    }

    public bool HasChanges()
    {
        lock (_gate)
        {
            if (_bookmarks is not null && DateTimeOffset.UtcNow - _lastSnapshotCheckUtc < TimeSpan.FromSeconds(2))
            {
                return false;
            }
        }

        var catalog = BrowserProfileDiscovery.Discover(_settings);
        var snapshot = SourceSnapshot.Capture(_settings, catalog);

        lock (_gate)
        {
            _lastSnapshotCheckUtc = DateTimeOffset.UtcNow;
            return _bookmarks is null || !snapshot.Equals(_snapshot);
        }
    }

    public void Invalidate()
    {
        lock (_gate)
        {
            _bookmarks = null;
            _snapshot = null;
            _lastSnapshotCheckUtc = DateTimeOffset.MinValue;
        }
    }

    private IReadOnlyList<BookmarkRecord> LoadBookmarks(BookmarkSourceCatalog catalog)
    {
        var bookmarks = new List<BookmarkRecord>();

        foreach (var profile in catalog.ChromiumProfiles)
        {
            TryAdd(bookmarks, () => _chromiumReader.Read(profile));
        }

        foreach (var profile in catalog.FirefoxProfiles)
        {
            TryAdd(bookmarks, () => _firefoxReader.Read(profile));
        }

        return bookmarks
            .GroupBy(bookmark => bookmark.StableKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(bookmark => bookmark.Title, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(bookmark => bookmark.BrowserName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(bookmark => bookmark.ProfileName, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    private static void TryAdd(List<BookmarkRecord> target, Func<IReadOnlyList<BookmarkRecord>> read)
    {
        try
        {
            target.AddRange(read());
        }
        catch
        {
        }
    }

    private sealed record SourceSnapshot(string Signature)
    {
        internal static SourceSnapshot Capture(SettingsManager settings, BookmarkSourceCatalog catalog)
        {
            var builder = new StringBuilder();
            builder.AppendLine(settings.EnableEdge.ToString());
            builder.AppendLine(settings.EnableChrome.ToString());
            builder.AppendLine(settings.EnableFirefox.ToString());
            builder.AppendLine(settings.EdgeProfileMode.ToString());
            builder.AppendLine(settings.SelectedEdgeProfileId);
            builder.AppendLine(string.Join(';', settings.SelectedEdgeProfileIds.OrderBy(id => id, StringComparer.OrdinalIgnoreCase)));
            builder.AppendLine(settings.ChromeProfileMode.ToString());
            builder.AppendLine(settings.SelectedChromeProfileId);
            builder.AppendLine(string.Join(';', settings.SelectedChromeProfileIds.OrderBy(id => id, StringComparer.OrdinalIgnoreCase)));
            builder.AppendLine(settings.FirefoxProfileMode.ToString());
            builder.AppendLine(settings.SelectedFirefoxProfileId);
            builder.AppendLine(string.Join(';', settings.SelectedFirefoxProfileIds.OrderBy(id => id, StringComparer.OrdinalIgnoreCase)));
            builder.AppendLine(settings.CustomChromiumUserDataFolders);

            foreach (var path in catalog.WatchedFiles.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                var state = ReadFileState(path);
                builder.Append(path).Append('|').Append(state).AppendLine();
            }

            return new SourceSnapshot(builder.ToString());
        }

        private static string ReadFileState(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return "missing";
                }

                var info = new FileInfo(path);
                return $"{info.LastWriteTimeUtc.Ticks}:{info.Length}";
            }
            catch
            {
                return "unknown";
            }
        }
    }
}
