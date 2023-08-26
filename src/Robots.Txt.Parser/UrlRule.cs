using System.Web;

namespace Robots.Txt.Parser;

/// <summary>
/// Describes a robots.txt rule for a URL
/// </summary>
/// <param name="Type">Rule type; either <see cref="RuleType.Allow"/> or <see cref="RuleType.Disallow"/></param>
/// <param name="Path">URL path</param>
public record UrlRule(RuleType Type, UrlPathPattern Path)
{
    /// <summary>
    /// Checks if a path matches the URL rule
    /// </summary>
    /// <param name="path">The URL path</param>
    /// <returns>True if the path matches or is a sub-path; otherwise false</returns>
    public bool Matches(UrlPath path) => !Path.IsEmpty && path.StartsWith(Path);
}

public class UrlPathPattern : UrlPath
{
    private UrlPathPattern(string value, bool exactMatch) : base(value)
    {
        ExactPattern = exactMatch;
    }

    public bool ExactPattern { get; }

    public static implicit operator UrlPathPattern(string value) => !value.EndsWith('$') ? new(value, false) : new(value[..^1], true);
}

public class UrlPath
{
    private readonly string _value;

    protected UrlPath(string value)
    {
        _value = HttpUtility.UrlDecode(value.Replace("%2F", "%252F"));
    }

    public int Length => _value.Length;

    public bool IsEmpty => _value == "";

    public bool StartsWith(UrlPath path) => _value.StartsWith(path._value);

    public static implicit operator UrlPath(string value) => new(value);
}

/// <summary>
/// Robots.txt rule type
/// </summary>
public enum RuleType
{
    Allow, Disallow
}