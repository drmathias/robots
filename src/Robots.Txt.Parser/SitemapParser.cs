using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Robots.Txt.Parser;

/// <summary>
/// Parses a <see cref="Sitemap"/> XML document
/// </summary>
public class SitemapParser
{
    private const int ByteCount50MiB = 52_428_800;

    private static readonly XNamespace sitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";

    /// <summary>
    /// Parses a <see cref="Sitemap"/> from a <see cref="Stream"/>
    /// </summary>
    /// <param name="stream">Sitemap document stream</param>
    /// <param name="modifiedSince">Filters the sitemap on the modified date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The parsed <see cref="Sitemap"/></returns>
    /// <exception cref="SitemapException">Raised when there is an error parsing the Sitemap</exception>
    public static async Task<Sitemap> ReadFromStreamAsync(Stream stream, DateTime? modifiedSince = null, CancellationToken cancellationToken = default)
    {
        try
        {
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });
            await reader.MoveToContentAsync();

            return reader switch
            {
                XmlReader when reader.NamespaceURI == sitemapNamespace && reader.Name == "urlset"
                    => await ParseUrlSet(stream, reader, modifiedSince, cancellationToken),
                XmlReader when reader.NamespaceURI == sitemapNamespace && reader.Name == "sitemapindex"
                    => await ParseSitemapIndex(stream, reader, modifiedSince, cancellationToken),
                _ => throw new SitemapException("Unable to find root sitemap element")
            };
        }
        catch (Exception e) when (e is not SitemapException)
        {
            throw new SitemapException("Unable to parse sitemap", e);
        }
    }

    private static async Task<SitemapIndex> ParseSitemapIndex(Stream stream, XmlReader reader, DateTime? modifiedSince, CancellationToken cancellationToken)
    {
        await reader.ReadAsync();

        var uris = new HashSet<Uri>();
        while (!reader.EOF && reader.ReadState is ReadState.Interactive && !cancellationToken.IsCancellationRequested)
        {
            if (reader.NodeType is not XmlNodeType.Element || reader.Name != "sitemap" || reader.NamespaceURI != sitemapNamespace)
            {
                await reader.ReadAsync();
                continue;
            }

            var node = (XElement)await XNode.ReadFromAsync(reader, cancellationToken);

            if (stream.Position > ByteCount50MiB) throw new SitemapException("Reached parsing limit");

            var lastModifiedString = node.Element(sitemapNamespace + "lastmod")?.Value;
            DateTime? lastModified = lastModifiedString is not null ? DateTime.Parse(lastModifiedString) : null;

            if (modifiedSince is not null && lastModified is not null && lastModified < modifiedSince) continue;

            var location = new Uri(node.Element(sitemapNamespace + "loc")!.Value);

            uris.Add(location);
        }
        return new SitemapIndex(uris);
    }

    private static async Task<Sitemap> ParseUrlSet(Stream stream, XmlReader reader, DateTime? modifiedSince, CancellationToken cancellationToken)
    {
        await reader.ReadAsync();

        var items = new HashSet<UrlSetItem>();
        while (!reader.EOF && reader.ReadState is ReadState.Interactive && !cancellationToken.IsCancellationRequested)
        {
            if (reader.NodeType is not XmlNodeType.Element || reader.Name != "url" || reader.NamespaceURI != sitemapNamespace)
            {
                await reader.ReadAsync();
                continue;
            }

            var node = (XElement)await XNode.ReadFromAsync(reader, cancellationToken);

            if (stream.Position > ByteCount50MiB) throw new SitemapException("Reached parsing limit");

            var lastModifiedString = node.Element(sitemapNamespace + "lastmod")?.Value;
            DateTime? lastModified = lastModifiedString is not null ? DateTime.Parse(lastModifiedString) : null;

            if (modifiedSince is not null && lastModified is not null && lastModified < modifiedSince) continue;

            var location = new Uri(node.Element(sitemapNamespace + "loc")!.Value);
            var changeFrequencyString = node.Element(sitemapNamespace + "changefreq")?.Value;
            var priorityString = node.Element(sitemapNamespace + "priority")?.Value;
            ChangeFrequency? changeFrequency = changeFrequencyString is not null
                ? Enum.Parse<ChangeFrequency>(changeFrequencyString, ignoreCase: true)
                : null;
            decimal? priority = priorityString is not null ? decimal.Parse(priorityString) : null;

            items.Add(new UrlSetItem(location, lastModified, changeFrequency, priority));
        }
        return new Sitemap(items);
    }
}
