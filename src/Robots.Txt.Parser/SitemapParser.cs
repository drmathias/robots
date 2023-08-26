using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Robots.Txt.Parser;

/// <summary>
/// Parses a <see cref="Sitemap"/> document
/// </summary>
public class SitemapParser
{
    private static readonly XNamespace sitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";

    /// <summary>
    /// Parses a <see cref="Sitemap"/> from a <see cref="Stream"/>
    /// </summary>
    /// <param name="stream">Sitemap document stream</param>
    /// <param name="modifiedSince">Filters the sitemap on the modified date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The parsed <see cref="Sitemap"/></returns>
    /// <exception cref="SitemapException">Thrown when the sitemap document is formatted incorrectly</exception>
    public static async Task<Sitemap> ReadFromStreamAsync(Stream stream, DateTime? modifiedSince = null, CancellationToken cancellationToken = default)
    {
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
        var urlSetElement = document.Element(sitemapNamespace + "urlset");
        if (urlSetElement is not null) return ReadUrlSet(urlSetElement, modifiedSince);

        var sitemapIndexElement = document.Element(sitemapNamespace + "sitemapindex");
        if (sitemapIndexElement is not null) return ReadSitemapIndex(sitemapIndexElement, modifiedSince);

        throw new SitemapException("Unable to find root sitemap element");
    }

    private static SitemapRoot ReadSitemapIndex(XElement sitemapIndexElement, DateTime? modifiedSince)
    {
        var sitemapElements = sitemapIndexElement.Elements(sitemapNamespace + "sitemap");
        var sitemaps = sitemapElements
            .Select(sitemapElement =>
            {
                var location = new Uri(sitemapElement.Element(sitemapNamespace + "loc")!.Value);
                var lastModifiedString = sitemapElement.Element(sitemapNamespace + "lastmod")?.Value;
                DateTime? lastModified = null;
                if (lastModifiedString is not null)
                {
                    if (DateTime.TryParse(lastModifiedString, out var parsedLastModified)) lastModified = parsedLastModified;
                    else throw new SitemapException($"Could not parse last modified value {lastModifiedString} as date");
                }
                return new SitemapItem(location, lastModified);
            })
            .Where(sitemap => modifiedSince is null || sitemap.LastModified is null || sitemap.LastModified >= modifiedSince)
            .Select(sitemap => sitemap.Location)
            .ToHashSet();
        return new SitemapRoot(sitemaps);
    }

    private static Sitemap ReadUrlSet(XElement urlSetElement, DateTime? modifiedSince)
    {
        var urlElements = urlSetElement.Elements(sitemapNamespace + "url");
        var urlSet = urlElements
            .Select(urlElement =>
            {
                var location = new Uri(urlElement.Element(sitemapNamespace + "loc")!.Value);
                var lastModifiedString = urlElement.Element(sitemapNamespace + "lastmod")?.Value;
                var changeFrequencyString = urlElement.Element(sitemapNamespace + "changefreq")?.Value;
                var priorityString = urlElement.Element(sitemapNamespace + "priority")?.Value;
                DateTime? lastModified = null;
                if (lastModifiedString is not null)
                {
                    if (DateTime.TryParse(lastModifiedString, out var parsedLastModified)) lastModified = parsedLastModified;
                    else throw new SitemapException($"Could not parse last modified value {lastModifiedString} as date");
                }
                ChangeFrequency? changeFrequency = null;
                if (changeFrequencyString is not null)
                {
                    if (Enum.TryParse<ChangeFrequency>(changeFrequencyString, ignoreCase: true, out var parsedChangeFrequency))
                        changeFrequency = parsedChangeFrequency;
                    else throw new SitemapException($"Could not parse change frequency value {changeFrequencyString}");
                }
                decimal? priority = null;
                if (priorityString is not null)
                {
                    if (decimal.TryParse(priorityString, out var parsedPriority)) priority = parsedPriority;
                    else throw new SitemapException($"Could not parse priority value {priorityString} as decimal");
                }
                return new UrlSetItem(location, lastModified, changeFrequency, priority);
            })
            .Where(url => modifiedSince is null || url.LastModified is null || url.LastModified >= modifiedSince)
            .ToHashSet();

        return new Sitemap(urlSet);
    }
}
