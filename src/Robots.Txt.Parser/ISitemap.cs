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
    HashSet<UrlSetItem> UrlSet { get; }
}

/// <summary>
/// Describes a Sitemap
/// </summary>
public class Sitemap : ISitemap
{
    public Sitemap(HashSet<UrlSetItem> urlSet)
    {
        UrlSet = urlSet;
    }

    /// <inheritdoc />
    public HashSet<UrlSetItem> UrlSet { get; }

    internal Sitemap Combine(Sitemap other)
    {
        UrlSet.UnionWith(other.UrlSet);
        return this;
    }
}

internal class SitemapIndex : Sitemap
{
    public SitemapIndex(HashSet<Uri> sitemapUris) : base(new HashSet<UrlSetItem>())
    {
        SitemapUris = sitemapUris;
    }

    public HashSet<Uri> SitemapUris { get; }
}