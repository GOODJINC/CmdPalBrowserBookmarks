using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalBrowserBookmarks;

internal static class Icons
{
    internal static IconInfo Bookmarks { get; } = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");

    internal static IconInfo Settings { get; } = new("\uE713");

    internal static IconInfo Refresh { get; } = new("\uE72C");

    internal static IconInfo Open { get; } = new("\uE8A7");

    internal static IconInfo OpenInNewWindow { get; } = new("\uE8A7");

    internal static IconInfo Copy { get; } = new("\uE8C8");
}
