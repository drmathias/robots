using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Robots.Txt.Parser;

/// <summary>
/// Client for retrieving robots.txt
/// </summary>
public interface IRobotClient
{
    /// <summary>
    /// Loads and parses the <see cref="IRobotsTxt"/> file from the website
    /// </summary>
    /// <exception cref="HttpRequestException">Thrown if a status code that cannot be handled is returned.</exception>
    Task<IRobotsTxt> LoadRobotsTxtAsync(Uri url, CancellationToken cancellationToken = default);

    protected internal IAsyncEnumerable<UrlSetItem> LoadSitemapsAsync(Uri uri, DateTime? modifiedSince = null, Func<Uri, bool>? sitemapLocationFilter = null, CancellationToken cancellationToken = default);
}
