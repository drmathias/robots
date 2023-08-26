using System;
using System.Runtime.Serialization;

namespace Robots.Txt.Parser;

/// <summary>
/// Exception raised when parsing a Sitemap
/// </summary>
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

    protected SitemapException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
