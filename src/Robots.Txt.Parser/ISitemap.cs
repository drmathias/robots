using System;
using System.Collections.Generic;
using System.Linq;

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
    public SitemapIndex(IAsyncEnumerable<Uri> sitemapUris) : base(AsyncEnumerable.Empty<UrlSetItem>())
    {
        SitemapUris = sitemapUris;
    }

    public IAsyncEnumerable<Uri> SitemapUris { get; }
}