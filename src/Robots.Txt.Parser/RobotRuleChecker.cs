using System;
using System.Collections.Generic;
using System.Linq;

namespace Robots.Txt.Parser;

/// <summary>
/// Provides the ability to check accessibility of URLs for a robot
/// </summary>
public interface IRobotRuleChecker
{
    /// <summary>
    /// Checks if the robot is allowed to access the path
    /// </summary>
    /// <param name="path">The path to check</param>
    /// <returns>True if the robots is allowed to access the path; otherwise false</returns>
    bool IsAllowed(string path);
}

/// <summary>
/// Provides the ability to check accessibility of URLs for a robot
/// </summary>
/// <remarks>
/// Creates a rule checker with a specified set of rules
/// </remarks>
/// <param name="rules">A set of path rules</param>
public class RobotRuleChecker(HashSet<UrlRule> rules) : IRobotRuleChecker
{
    private readonly HashSet<UrlRule> _rules = rules;

    /// <inheritdoc />
    public bool IsAllowed(string path)
    {
        /*
            "The /robots.txt URL is always allowed"
        */
        if (_rules.Count == 0 || path == "/robots.txt") return true;
        var ruleMatch = _rules.Where(rule => rule.Pattern.Matches(path))
                              .OrderByDescending(rule => rule.Pattern.Length)
                              .ThenBy(rule => rule.Type, new RuleTypeComparer())
                              .FirstOrDefault();
        return ruleMatch is null || ruleMatch.Type == RuleType.Allow;
    }

    private class RuleTypeComparer : IComparer<RuleType>
    {
        public int Compare(RuleType ruleType, RuleType _) => ruleType switch
        {
            RuleType.Allow => -1,
            RuleType.Disallow => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(ruleType), "Invalid rule type")
        };
    }
}
