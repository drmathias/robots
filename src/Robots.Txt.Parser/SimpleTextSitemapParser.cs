using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Robots.Txt.Parser;

/// <summary>
/// Parses a <see cref="Sitemap"/> TXT document
/// </summary>
public static class SimpleTextSitemapParser
{
    private const int MaxLines = 50000;
    private const int ByteCount50MiB = 52_428_800;

    /// <summary>
    /// Parses a <see cref="Sitemap"/> from a <see cref="Stream"/>
    /// </summary>
    /// <param name="stream">Sitemap document stream</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The parsed <see cref="Sitemap"/></returns>
    /// <exception cref="SitemapException">Raised when there is an error parsing the Sitemap</exception>
    public static async IAsyncEnumerable<UrlSetItem> ReadFromStreamAsync(Stream stream, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        /*
          Each Sitemap file ... must be no larger than 50MB (52,428,800 bytes)
        */
        var maxLengthStream = new MaxLengthStream(stream, ByteCount50MiB);

        using var streamReader = new StreamReader(maxLengthStream);
        string? line;
        var lineCount = 0;
        while (((line = await ReadSitemapLine(streamReader, cancellationToken)) is not null) && !cancellationToken.IsCancellationRequested)
        {

            if (string.IsNullOrWhiteSpace(line)) continue;

            lineCount++;

            /*
              Each Sitemap file ... must have no more than 50,000 URLs
            */
            if (lineCount > MaxLines) throw new SitemapException("Reached line limit");

            /*
              The text file must have one URL per line. The URLs cannot contain embedded new lines.
              You must fully specify URLs, including the http.
              The text file must use UTF-8 encoding.
              The text file should contain no information other than the list of URLs.
              The text file should contain no header or footer information.
            */
            Uri location;
            try
            {
                location = new Uri(line);
            }
            catch (Exception e)
            {
                throw new SitemapException("Unable to parse sitemap item", e);
            }

            yield return new UrlSetItem(location, null, null, null);
        }

        static async Task<string?> ReadSitemapLine(StreamReader streamReader, CancellationToken cancellationToken)
        {
            try
            {
                return await streamReader.ReadLineAsync(cancellationToken);
            }
            catch (InvalidOperationException e)
            {
                throw new SitemapException("Unable to parse sitemap", e);
            }
        }
    }
}
