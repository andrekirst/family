using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace Api.Domain.Core;

[Owned]
public record Color
{
    public string Value { get; set; }

    public Color(string value)
    {
        Value = IsValidColor(value) ? value : throw new InvalidColorException(value);
    }

    private static bool IsValidColor(string value) =>
        KnownColors.Contains(value/*TODO*/)
        || RgbRegex.IsMatch(value)
        || RgbaRegex.IsMatch(value);

    private static readonly Regex RgbRegex = new Regex("^rgb\\(\\s*(0|[1-9]\\d?|1\\d\\d?|2[0-4]\\d|25[0-5])%?\\s*,\\s*(0|[1-9]\\d?|1\\d\\d?|2[0-4]\\d|25[0-5])%?\\s*,\\s*(0|[1-9]\\d?|1\\d\\d?|2[0-4]\\d|25[0-5])%?\\s*\\)$", RegexOptions.Compiled);
    private static readonly Regex RgbaRegex = new Regex("^rgba\\(\\s*(0|[1-9]\\d?|1\\d\\d?|2[0-4]\\d|25[0-5])%?\\s*,\\s*(0|[1-9]\\d?|1\\d\\d?|2[0-4]\\d|25[0-5])%?\\s*,\\s*(0|[1-9]\\d?|1\\d\\d?|2[0-4]\\d|25[0-5])%?\\s*,\\s*((0.[1-9])|[01])\\s*\\)$", RegexOptions.Compiled);

    private static readonly List<string> KnownColors = new List<string>
    {
        "aqua", "black", "blue", "fuchsia", "gray", "green", "lime", "maroon", "navy", "olive", "purple", "red", "silver", "teal", "white", "yellow"
    };
}