using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

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

    [ExcludeFromCodeCoverage]
    protected RobotsTxtException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
