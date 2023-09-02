using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
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
            var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });
            await reader.MoveToContentAsync();

            return reader switch
            {
                XmlReader when reader.NamespaceURI == sitemapNamespace && reader.Name == "urlset"
                    => new Sitemap(ParseUrlSet(reader, () => stream.Position, modifiedSince, cancellationToken)),
                XmlReader when reader.NamespaceURI == sitemapNamespace && reader.Name == "sitemapindex"
                    => new SitemapIndex(ParseSitemapIndex(reader, () => stream.Position, modifiedSince, cancellationToken)),
                _ => throw new SitemapException("Unable to find root sitemap element")
            };
        }
        catch (Exception e) when (e is not SitemapException)
        {
            throw new SitemapException("Unable to parse sitemap", e);
        }
    }

    private static async IAsyncEnumerable<Uri> ParseSitemapIndex(XmlReader reader, Func<long> getByteCount, DateTime? modifiedSince, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        try
        {
            await reader.ReadAsync();

            while (!reader.EOF && reader.ReadState is ReadState.Interactive && !cancellationToken.IsCancellationRequested)
            {
                if (reader.NodeType is not XmlNodeType.Element || reader.Name != "sitemap" || reader.NamespaceURI != sitemapNamespace)
                {
                    await reader.ReadAsync();
                    continue;
                }

                XElement node;
                try
                {
                    node = (XElement)await XNode.ReadFromAsync(reader, cancellationToken);
                }
                catch (Exception e)
                {
                    throw new SitemapException("Unable to parse sitemap item", e);
                }

                if (getByteCount() > ByteCount50MiB) throw new SitemapException("Reached parsing limit");

                Uri location;
                try
                {
                    var lastModifiedString = node.Element(sitemapNamespace + "lastmod")?.Value;
                    DateTime? lastModified = lastModifiedString is not null ? DateTime.Parse(lastModifiedString) : null;
                    if (modifiedSince is not null && lastModified is not null && lastModified < modifiedSince) continue;
                    location = new Uri(node.Element(sitemapNamespace + "loc")!.Value);
                }
                catch (Exception e)
                {
                    throw new SitemapException("Unable to parse sitemap item", e);
                }

                yield return location;
            }
        }
        finally
        {
            reader.Dispose();
        }
    }

    private static async IAsyncEnumerable<UrlSetItem> ParseUrlSet(XmlReader reader, Func<long> getByteCount, DateTime? modifiedSince, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        try
        {
            await reader.ReadAsync();

            while (!reader.EOF && reader.ReadState is ReadState.Interactive && !cancellationToken.IsCancellationRequested)
            {
                if (reader.NodeType is not XmlNodeType.Element || reader.Name != "url" || reader.NamespaceURI != sitemapNamespace)
                {
                    await reader.ReadAsync();
                    continue;
                }

                XElement node;
                try
                {
                    node = (XElement)await XNode.ReadFromAsync(reader, cancellationToken);
                }
                catch (Exception e)
                {
                    throw new SitemapException("Unable to parse sitemap item", e);
                }

                if (getByteCount() > ByteCount50MiB) throw new SitemapException("Reached parsing limit");

                Uri location;
                DateTime? lastModified;
                ChangeFrequency? changeFrequency;
                decimal? priority;

                try
                {
                    var lastModifiedString = node.Element(sitemapNamespace + "lastmod")?.Value;
                    lastModified = lastModifiedString is not null ? DateTime.Parse(lastModifiedString) : null;

                    if (modifiedSince is not null && lastModified is not null && lastModified < modifiedSince) continue;

                    location = new Uri(node.Element(sitemapNamespace + "loc")!.Value);
                    var changeFrequencyString = node.Element(sitemapNamespace + "changefreq")?.Value;
                    var priorityString = node.Element(sitemapNamespace + "priority")?.Value;
                    changeFrequency = changeFrequencyString is not null
                        ? Enum.Parse<ChangeFrequency>(changeFrequencyString, ignoreCase: true)
                        : null;
                    priority = priorityString is not null ? decimal.Parse(priorityString) : null;
                }
                catch (Exception e)
                {
                    throw new SitemapException("Unable to parse sitemap item", e);
                }

                yield return new UrlSetItem(location, lastModified, changeFrequency, priority);
            }
        }
        finally
        {
            reader.Dispose();
        }
    }
}
