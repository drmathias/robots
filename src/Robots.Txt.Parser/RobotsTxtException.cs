using System;

namespace Robots.Txt.Parser;

/// <summary>
/// Exception raised when parsing a robots.txt file
/// </summary>
[Serializable]
public class RobotsTxtException : Exception
{
    internal RobotsTxtException()
    {
    }

    internal RobotsTxtException(string? message) : base(message)
    {
    }

    internal RobotsTxtException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
