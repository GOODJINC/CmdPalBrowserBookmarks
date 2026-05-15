namespace CmdPalBrowserBookmarks.Bookmarks;

internal readonly record struct BookmarkSearchOptions(bool EnableKoreanInitialConsonantSearch)
{
    public static BookmarkSearchOptions Default { get; } = new(true);
}
