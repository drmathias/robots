using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace Robots.Txt.Parser.Http;

/// <summary>
/// Client for retrieving robots.txt from a website
/// </summary>
public interface IRobotWebClient : IRobotClient
{
}

/// <summary>
/// Client for retrieving robots.txt from a website
/// </summary>
public interface IRobotWebClient<TWebsite> : IRobotWebClient
    where TWebsite : IWebsiteMetadata
{
}


/// <summary>
/// Client for retrieving robots.txt from a website
/// </summary>
public class RobotWebClient<TWebsite> : IRobotWebClient<TWebsite>
    where TWebsite : IWebsiteMetadata
{
    private readonly HttpClient _httpClient;

    public RobotWebClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
    }

    Uri IRobotClient.BaseAddress => TWebsite.BaseAddress;

    public async Task<IRobotsTxt> LoadRobotsTxtAsync(CancellationToken cancellationToken = default)
    {
        /*
           "The instructions must be accessible via HTTP [2] from the site that the instructions are to be applied to, as a resource of Internet
           Media Type [3] "text/plain" under a standard relative path on the server: "/robots.txt"."
        */
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(TWebsite.BaseAddress, "/robots.txt"));
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
            return new RobotsTxt(this, new Dictionary<ProductToken, HashSet<UrlRule>>(), new Dictionary<ProductToken, int>(), null, new HashSet<Uri>());
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
            return new RobotsTxt(this, userAgentRules, new Dictionary<ProductToken, int>(), null, new HashSet<Uri>());
        }

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await new RobotsTxtParser(this).ReadFromStreamAsync(stream, cancellationToken);
    }

    async Task<Sitemap?> IRobotClient.LoadSitemapsAsync(IEnumerable<Uri> uris, DateTime? modifiedSince, CancellationToken cancellationToken)
    {
        Sitemap? sitemap = null;

        foreach (var uri in uris)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Accept", "application/xml,text/plain,text/xml,*/*");
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            var parsedSitemap = response.Content.Headers.ContentType?.MediaType switch
            {
                MediaTypeNames.Text.Plain => await SimpleTextSitemapParser.ReadFromStreamAsync(stream, cancellationToken),
                MediaTypeNames.Text.Xml or MediaTypeNames.Application.Xml or _
                    => await SitemapParser.ReadFromStreamAsync(stream, modifiedSince, cancellationToken)
            };

            if (parsedSitemap is null)
            {
                continue;
            }

            if (sitemap is null)
            {
                sitemap = parsedSitemap;
                continue;
            }

            if (parsedSitemap is SitemapRoot sitemapRoot)
            {
                var sitemaps = await (this as IRobotWebClient).LoadSitemapsAsync(sitemapRoot.SitemapUris, modifiedSince, cancellationToken);
                if (sitemaps is not null) sitemap = sitemaps.Combine(sitemaps);
            }

            sitemap = sitemap.Combine(parsedSitemap);
        }

        return sitemap;
    }
}
