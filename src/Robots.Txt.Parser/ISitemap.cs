using System;
using System.Collections.Generic;

namespace Robots.Txt.Parser;

/// <summary>
/// Describes a Sitemap
/// </summary>
public interface ISitemap
{
    /// <summary>
    /// Url set included in the Sitemap
    /// </summary>
    IAsyncEnumerable<UrlSetItem> UrlSet { get; }
}

/// <summary>
/// Describes a Sitemap
/// </summary>
public class Sitemap : ISitemap
{
    public Sitemap(IAsyncEnumerable<UrlSetItem> urlSet)
    {
        UrlSet = urlSet;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<UrlSetItem> UrlSet { get; }
}

internal class SitemapIndex : Sitemap
{
    public SitemapIndex(IAsyncEnumerable<Uri> sitemapUris) : base(Empty<UrlSetItem>())
    {
        SitemapUris = sitemapUris;
    }

    public IAsyncEnumerable<Uri> SitemapUris { get; }

#pragma warning disable CS1998
    private static async IAsyncEnumerable<T> Empty<T>()
#pragma warning restore CS1998
    {
        yield break;
    }
}