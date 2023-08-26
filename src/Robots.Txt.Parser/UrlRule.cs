using System.Linq;
using System.Web;

namespace Robots.Txt.Parser;

/// <summary>
/// Describes a robots.txt rule for a URL
/// </summary>
/// <param name="Type">Rule type; either <see cref="RuleType.Allow"/> or <see cref="RuleType.Disallow"/></param>
/// <param name="Pattern">URL path pattern</param>
public record UrlRule(RuleType Type, UrlPathPattern Pattern);

public class UrlPathPattern
{
    private readonly bool _matchSubPaths;
    private readonly string[] _patternParts;

    private UrlPathPattern(string value)
    {
        Length = value.Length;
        if (value.EndsWith('$')) value = value[..^1];
        else _matchSubPaths = true;
        _patternParts = value.Split('*', System.StringSplitOptions.None)
                             .Select(part => HttpUtility.UrlDecode(part.Replace("%2F", "%252F")))
                             .ToArray();
    }

    public int Length { get; }

    /// <summary>
    /// Checks if a path matches the URL rule
    /// </summary>
    /// <param name="path">The URL path</param>
    /// <returns>True if the path matches or is a sub-path; otherwise false</returns>
    public bool Matches(UrlPath path)
    {
        if (Length == 0 || path._value.IndexOf(_patternParts[0]) != 0) return false;
        var currentIndex = _patternParts[0].Length;
        for (var x = 1; x < _patternParts.Length; x++)
        {
            var matchIndex = path._value.IndexOf(_patternParts[x], currentIndex);
            if (matchIndex == -1) return false;
            currentIndex = matchIndex + _patternParts[x].Length;
        }
        return _matchSubPaths || currentIndex == path.Length;
    }

    public static implicit operator UrlPathPattern(string value) => new(value);
}

public class UrlPath
{
    internal readonly string _value;

    private UrlPath(string value)
    {
        _value = HttpUtility.UrlDecode(value.Replace("%2F", "%252F"));
    }

    public int Length => _value.Length;

    public static implicit operator UrlPath(string value) => new(value);
}

/// <summary>
/// Robots.txt rule type
/// </summary>
public enum RuleType
{
    Allow, Disallow
}