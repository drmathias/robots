using System;
using System.Runtime.Serialization;

namespace Robots.Txt.Parser;

/// <summary>
/// Exception raised when parsing a robots.txt file
/// </summary>
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

    protected RobotsTxtException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
