using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Robots.Txt.Parser.Http;

/// <summary>
/// Client for retrieving robots.txt from a website
/// </summary>
public class RobotWebClient : IRobotClient
{
    private readonly HttpClient _httpClient;

    public RobotWebClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IRobotsTxt> LoadRobotsTxtAsync(Uri url, CancellationToken cancellationToken = default)
    {
        var baseUrl = new Uri(url.GetLeftPart(UriPartial.Authority));
        /*
           "The instructions must be accessible via HTTP [2] from the site that the instructions are to be applied to, as a resource of Internet
           Media Type [3] "text/plain" under a standard relative path on the server: "/robots.txt"."
        */
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUrl, "/robots.txt"));
        request.Headers.Add("Accept", "text/plain,*/*");
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var statusCodeNumber = (int)response.StatusCode;

        if (statusCodeNumber >= 400 && statusCodeNumber <= 499)
        {
            /*
                "Unavailable" means the crawler tries to fetch the robots.txt file and the server responds with status codes indicating that
                the resource in question is unavailable. For example, in the context of HTTP, such status codes are in the 400-499 range.

                If a server status code indicates that the robots.txt file is unavailable to the crawler,
                then the crawler MAY access any resources on the server.
            */
            return new RobotsTxt(this, baseUrl, new Dictionary<ProductToken, HashSet<UrlRule>>(), new Dictionary<ProductToken, TimeSpan>(), null, new HashSet<Uri>());
        }

        if (statusCodeNumber >= 500)
        {
            /*
                If the robots.txt file is unreachable due to server or network errors, this means the robots.txt file is undefined and the
                crawler MUST assume complete disallow. For example, in the context of HTTP, server errors are identified by status codes in
                the 500-599 range.
            */
            var userAgentRules = new Dictionary<ProductToken, HashSet<UrlRule>>
            {
                { ProductToken.Wildcard, new HashSet<UrlRule> { new (RuleType.Disallow, "/") } }
            };
            return new RobotsTxt(this, baseUrl, userAgentRules, new Dictionary<ProductToken, TimeSpan>(), null, []);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await new RobotsTxtParser(this, baseUrl).ReadFromStreamAsync(stream, cancellationToken);
    }

    async IAsyncEnumerable<UrlSetItem> IRobotClient.LoadSitemapsAsync(Uri uri, DateTime? modifiedSince, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Add("Accept", "application/xml,text/plain,text/xml,*/*");
        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode) yield break;

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        switch (response.Content.Headers.ContentType?.MediaType)
        {
            case MediaTypeNames.Text.Plain:
                await foreach (var urlSet in SimpleTextSitemapParser.ReadFromStreamAsync(stream, cancellationToken))
                {
                    yield return urlSet;
                }
                yield break;
            case MediaTypeNames.Text.Xml or MediaTypeNames.Application.Xml:
            default:
                var sitemap = await SitemapParser.ReadFromStreamAsync(stream, modifiedSince, cancellationToken);
                if (sitemap is SitemapIndex index)
                {
                    await foreach (var location in index.SitemapUris)
                    {
                        await foreach (var item in (this as IRobotClient).LoadSitemapsAsync(location, modifiedSince, cancellationToken))
                        {
                            yield return item;
                        }
                    }
                }
                else
                {
                    await foreach (var item in sitemap.UrlSet)
                    {
                        yield return item;
                    }
                }
                yield break;
        }
    }
}
