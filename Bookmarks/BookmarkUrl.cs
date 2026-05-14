namespace CmdPalBrowserBookmarks.Bookmarks;

internal static class BookmarkUrl
{
    internal static bool IsLaunchable(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return !string.Equals(uri.Scheme, "javascript", StringComparison.OrdinalIgnoreCase);
    }

    internal static string NormalizeForComparison(string url)
    {
        var trimmedUrl = url.Trim();
        if (!Uri.TryCreate(trimmedUrl, UriKind.Absolute, out var uri))
        {
            return trimmedUrl;
        }

        var builder = new UriBuilder(uri)
        {
            Scheme = uri.Scheme.ToLowerInvariant(),
            Host = uri.Host.ToLowerInvariant(),
        };

        if ((builder.Scheme == Uri.UriSchemeHttp && builder.Port == 80)
            || (builder.Scheme == Uri.UriSchemeHttps && builder.Port == 443))
        {
            builder.Port = -1;
        }

        var normalized = builder.Uri.AbsoluteUri;
        if (string.IsNullOrEmpty(uri.Query) && string.IsNullOrEmpty(uri.Fragment))
        {
            normalized = normalized.TrimEnd('/');
        }

        return normalized;
    }
}
