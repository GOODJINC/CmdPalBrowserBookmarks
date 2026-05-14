using CmdPalBrowserBookmarks.Bookmarks;
using CmdPalBrowserBookmarks.Commands;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalBrowserBookmarks;

internal static class BookmarkItemFactory
{
    internal static CommandItem CreateCommandItem(BookmarkRecord bookmark)
    {
        return ApplyCommonProperties(new CommandItem(new OpenBookmarkCommand(bookmark)), bookmark);
    }

    internal static ListItem CreateListItem(BookmarkRecord bookmark)
    {
        var item = new ListItem(new OpenBookmarkCommand(bookmark));
        ApplyCommonProperties(item, bookmark);
        item.TextToSuggest = bookmark.Title;
        item.Section = bookmark.BrowserName;
        return item;
    }

    private static T ApplyCommonProperties<T>(T item, BookmarkRecord bookmark)
        where T : CommandItem
    {
        item.Title = bookmark.Title;
        item.Subtitle = BuildSubtitle(bookmark);
        item.Icon = Icons.Bookmarks;
        item.MoreCommands =
        [
            new CommandContextItem(new CopyTextCommand(bookmark.Url))
            {
                Title = "Copy URL",
                Subtitle = bookmark.Url,
                Icon = Icons.Copy,
            },
            new CommandContextItem(new CopyTextCommand(bookmark.Title))
            {
                Title = "Copy title",
                Subtitle = bookmark.Title,
                Icon = Icons.Copy,
            },
        ];

        return item;
    }

    private static string BuildSubtitle(BookmarkRecord bookmark)
    {
        var location = string.IsNullOrWhiteSpace(bookmark.FolderPath)
            ? bookmark.ProfileName
            : $"{bookmark.ProfileName} / {bookmark.FolderPath}";

        return $"{bookmark.BrowserName} - {location} - {bookmark.Host}";
    }
}
