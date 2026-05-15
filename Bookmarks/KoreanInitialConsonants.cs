using System.Text;

namespace CmdPalBrowserBookmarks.Bookmarks;

internal static class KoreanInitialConsonants
{
    private const int HangulBase = 0xAC00;
    private const int HangulLast = 0xD7A3;
    private const int SyllableCountPerInitial = 21 * 28;

    private static readonly char[] Initials =
    [
        'ㄱ', 'ㄲ', 'ㄴ', 'ㄷ', 'ㄸ', 'ㄹ', 'ㅁ', 'ㅂ', 'ㅃ', 'ㅅ',
        'ㅆ', 'ㅇ', 'ㅈ', 'ㅉ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ',
    ];

    internal static bool IsInitialConsonantQuery(string query)
    {
        var hasInitial = false;
        foreach (var character in query)
        {
            if (char.IsWhiteSpace(character))
            {
                continue;
            }

            if (!IsInitialConsonant(character))
            {
                return false;
            }

            hasInitial = true;
        }

        return hasInitial;
    }

    internal static string NormalizeQuery(string query)
    {
        var builder = new StringBuilder(query.Length);
        foreach (var character in query)
        {
            if (char.IsWhiteSpace(character))
            {
                continue;
            }

            builder.Append(character);
        }

        return builder.ToString();
    }

    internal static string FromText(string text)
    {
        var builder = new StringBuilder(text.Length);
        foreach (var character in text)
        {
            if (TryGetInitial(character, out var initial))
            {
                builder.Append(initial);
            }
        }

        return builder.ToString();
    }

    private static bool TryGetInitial(char character, out char initial)
    {
        if (IsInitialConsonant(character))
        {
            initial = character;
            return true;
        }

        if (character is >= (char)HangulBase and <= (char)HangulLast)
        {
            var initialIndex = (character - HangulBase) / SyllableCountPerInitial;
            initial = Initials[initialIndex];
            return true;
        }

        initial = default;
        return false;
    }

    private static bool IsInitialConsonant(char character)
    {
        return Initials.Contains(character);
    }
}
