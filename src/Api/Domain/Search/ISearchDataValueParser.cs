namespace Api.Domain.Search;

public interface ISearchDataValueParser
{
    (string? Shortcut, string? Value) ExtractShortcutAndValue(string value, List<string> knownShortcuts);
}