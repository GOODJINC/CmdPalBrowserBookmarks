using CmdPalBrowserBookmarks.Bookmarks;
using CmdPalBrowserBookmarks.Commands;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.System;

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

    internal static IContextItem[] CreateContextCommands(BookmarkRecord bookmark)
    {
        return
        [
            new CommandContextItem(new OpenBookmarkCommand(bookmark, UrlOpenMode.NewWindow))
            {
                Title = "Open in new window",
                Subtitle = bookmark.Url,
                Icon = Icons.OpenInNewWindow,
                RequestedShortcut = KeyChordHelpers.FromModifiers(shift: true, vkey: VirtualKey.Enter),
            },
            new CommandContextItem(new CopyTextCommand(bookmark.Url))
            {
                Title = "Copy URL",
                Subtitle = bookmark.Url,
                Icon = Icons.Copy,
                RequestedShortcut = KeyChordHelpers.FromModifiers(ctrl: true, vkey: VirtualKey.C),
            },
            new CommandContextItem(new CopyTextCommand(bookmark.Title))
            {
                Title = "Copy title",
                Subtitle = bookmark.Title,
                Icon = Icons.Copy,
                RequestedShortcut = KeyChordHelpers.FromModifiers(ctrl: true, shift: true, vkey: VirtualKey.C),
            },
            new CommandContextItem(new CopyTextCommand(BuildMarkdownLink(bookmark)))
            {
                Title = "Copy Markdown link",
                Subtitle = BuildMarkdownLink(bookmark),
                Icon = Icons.Copy,
                RequestedShortcut = KeyChordHelpers.FromModifiers(ctrl: true, alt: true, vkey: VirtualKey.C),
            },
        ];
    }

    private static T ApplyCommonProperties<T>(T item, BookmarkRecord bookmark)
        where T : CommandItem
    {
        item.Title = bookmark.Title;
        item.Subtitle = BuildSubtitle(bookmark);
        item.Icon = Icons.Bookmarks;
        item.MoreCommands = CreateContextCommands(bookmark);

        return item;
    }

    internal static string BuildSubtitle(BookmarkRecord bookmark)
    {
        var location = string.IsNullOrWhiteSpace(bookmark.FolderPath)
            ? bookmark.ProfileName
            : $"{bookmark.ProfileName} / {bookmark.FolderPath}";

        return $"{bookmark.BrowserName} - {location} - {bookmark.Host}";
    }

    private static string BuildMarkdownLink(BookmarkRecord bookmark)
    {
        return $"[{EscapeMarkdownText(bookmark.Title)}](<{EscapeMarkdownUrl(bookmark.Url)}>)";
    }

    private static string EscapeMarkdownText(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("[", "\\[", StringComparison.Ordinal)
            .Replace("]", "\\]", StringComparison.Ordinal);
    }

    private static string EscapeMarkdownUrl(string value)
    {
        return value.Replace(">", "%3E", StringComparison.Ordinal);
    }
}
