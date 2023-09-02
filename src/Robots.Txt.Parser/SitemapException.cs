using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Robots.Txt.Parser;

/// <summary>
/// Exception raised when parsing a sitemap
/// </summary>
[Serializable]
public class SitemapException : Exception
{
    internal SitemapException()
    {
    }

    internal SitemapException(string? message) : base(message)
    {
    }

    internal SitemapException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    [ExcludeFromCodeCoverage]
    protected SitemapException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
