namespace Api.Domain.Search;

public class SearchDataValueParser : ISearchDataValueParser
{
    public (string? Shortcut, string? Value) ExtractShortcutAndValue(string value, List<string> knownShortcuts)
    {
        var shortcut = knownShortcuts.FirstOrDefault(s => value.StartsWith(s) && value.Length > s.Length);
        return shortcut == null ? (null, null) : (shortcut, value.Remove(0, shortcut.Length + 1));
    }
}