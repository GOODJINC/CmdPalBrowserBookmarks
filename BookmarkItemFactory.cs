using CmdPalBrowserBookmarks.Bookmarks;
using CmdPalBrowserBookmarks.Commands;
using CmdPalBrowserBookmarks.Settings;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.System;

namespace CmdPalBrowserBookmarks;

internal static class BookmarkItemFactory
{
    internal static CommandItem CreateCommandItem(BookmarkRecord bookmark, SettingsManager settings)
    {
        return ApplyCommonProperties(new CommandItem(new OpenBookmarkCommand(bookmark, settings)), bookmark, settings);
    }

    internal static ListItem CreateListItem(BookmarkRecord bookmark, SettingsManager settings)
    {
        var item = new ListItem(new OpenBookmarkCommand(bookmark, settings));
        ApplyCommonProperties(item, bookmark, settings);
        item.TextToSuggest = bookmark.Title;
        item.Section = bookmark.BrowserName;
        return item;
    }

    internal static IContextItem[] CreateContextCommands(BookmarkRecord bookmark, SettingsManager settings)
    {
        return
        [
            new CommandContextItem(new OpenBookmarkCommand(bookmark, settings, UrlOpenMode.NewWindow))
            {
                Title = settings.Strings.OpenInNewWindow,
                Subtitle = bookmark.Url,
                Icon = Icons.OpenInNewWindow,
                RequestedShortcut = KeyChordHelpers.FromModifiers(shift: true, vkey: VirtualKey.Enter),
            },
            new CommandContextItem(new CopyTextCommand(bookmark.Url))
            {
                Title = settings.Strings.CopyUrl,
                Subtitle = bookmark.Url,
                Icon = Icons.Copy,
                RequestedShortcut = KeyChordHelpers.FromModifiers(ctrl: true, vkey: VirtualKey.C),
            },
            new CommandContextItem(new CopyTextCommand(bookmark.Title))
            {
                Title = settings.Strings.CopyTitle,
                Subtitle = bookmark.Title,
                Icon = Icons.Copy,
                RequestedShortcut = KeyChordHelpers.FromModifiers(ctrl: true, shift: true, vkey: VirtualKey.C),
            },
            new CommandContextItem(new CopyTextCommand(BuildMarkdownLink(bookmark)))
            {
                Title = settings.Strings.CopyMarkdownLink,
                Subtitle = BuildMarkdownLink(bookmark),
                Icon = Icons.Copy,
                RequestedShortcut = KeyChordHelpers.FromModifiers(ctrl: true, alt: true, vkey: VirtualKey.C),
            },
        ];
    }

    private static T ApplyCommonProperties<T>(T item, BookmarkRecord bookmark, SettingsManager settings)
        where T : CommandItem
    {
        item.Title = bookmark.Title;
        item.Subtitle = BuildSubtitle(bookmark);
        item.Icon = Icons.Bookmarks;
        item.MoreCommands = CreateContextCommands(bookmark, settings);

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
