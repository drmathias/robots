using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Robots.Txt.Parser;

/// <summary>
/// Provides the ability to parse robots.txt 
/// </summary>
public class RobotsTxtParser
{
    private const long ByteCount500KiB = 500 * 1024;

    private static readonly string UserAgentDirective = "User-agent: ";
    private static readonly string CrawlDelayDirective = "Crawl-delay: ";
    private static readonly string HostDirective = "Host: ";
    private static readonly string SitemapDirective = "Sitemap: ";
    private static readonly string AllowDirective = "Allow: ";
    private static readonly string DisallowDirective = "Disallow: ";

    private readonly IRobotClient _robotClient;

    /// <summary>
    /// Creates a robots.txt parser
    /// </summary>
    /// <param name="robotClient">Client used to send requests to the website</param>
    public RobotsTxtParser(IRobotClient robotClient)
    {
        _robotClient = robotClient;
    }

    /// <summary>
    /// Parses <see cref="RobotsTxt"/> from a <see cref="Stream"/>
    /// </summary>
    /// <param name="stream">The input stream</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Parsed <see cref="IRobotsTxt"/></returns>
    /// <exception cref="RobotsTxtException">Raised when there is an error parsing the robots.txt file</exception>
    public async Task<IRobotsTxt> ReadFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        string? line;

        string? host = null;
        var sitemaps = new HashSet<Uri>();

        var previousLineWasUserAgent = false;
        /*
          Crawlers MUST use case-insensitive matching to find the group that matches the product token
        */
        var currentUserAgents = new HashSet<ProductToken>();
        var userAgentRules = new Dictionary<ProductToken, HashSet<UrlRule>>();
        var userAgentCrawlDirectives = new Dictionary<ProductToken, int>();

        try
        {
            /*
              The file MUST be UTF-8 encoded
            */
            using var streamReader = new StreamReader(stream, Encoding.UTF8);
            while ((line = await streamReader.ReadLineAsync(cancellationToken)) is not null && !cancellationToken.IsCancellationRequested)
            {
                if (stream.Position > ByteCount500KiB) throw new RobotsTxtException("Reached parsing limit");

                if (line.StartsWith('#')) continue;

                if (line.StartsWith(UserAgentDirective, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!previousLineWasUserAgent) currentUserAgents.Clear();
                    var currentUserAgent = GetValueOfDirective(line, UserAgentDirective);
                    if (ProductToken.TryParse(currentUserAgent, out var productToken))
                    {
                        currentUserAgents.Add(productToken);
                        userAgentRules.TryAdd(productToken, new HashSet<UrlRule>());
                        previousLineWasUserAgent = true;
                    }
                    continue;
                }

                if (currentUserAgents.Count == 0)
                {
                    if (line.StartsWith(SitemapDirective, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var sitemapValue = GetValueOfDirective(line, SitemapDirective);
                        if (Uri.TryCreate(sitemapValue, UriKind.Absolute, out var sitemapAddress)) sitemaps.Add(sitemapAddress);
                    }
                    else if (host is null && line.StartsWith(HostDirective, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var hostValue = GetValueOfDirective(line, HostDirective);
                        if (Uri.IsWellFormedUriString(hostValue, UriKind.Absolute)
                            && Uri.TryCreate(hostValue, UriKind.Absolute, out var uri)) hostValue = uri.Host;
                        var hostNameType = Uri.CheckHostName(hostValue);
                        if (hostNameType != UriHostNameType.Unknown && hostNameType != UriHostNameType.Basic) host = hostValue;
                    }
                }
                else
                {
                    if (line.StartsWith(DisallowDirective, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var disallowValue = GetValueOfDirective(line, DisallowDirective);
                        foreach (var userAgent in currentUserAgents) userAgentRules[userAgent].Add(new UrlRule(RuleType.Disallow, disallowValue));
                    }
                    else if (line.StartsWith(AllowDirective, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var allowedValue = GetValueOfDirective(line, AllowDirective);
                        foreach (var userAgent in currentUserAgents) userAgentRules[userAgent].Add(new UrlRule(RuleType.Allow, allowedValue));
                    }
                    else if (line.StartsWith(CrawlDelayDirective, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var crawlDelayValue = GetValueOfDirective(line, CrawlDelayDirective);
                        if (int.TryParse(crawlDelayValue, out var parsedCrawlDelay))
                        {
                            foreach (var userAgent in currentUserAgents) userAgentCrawlDirectives.TryAdd(userAgent, parsedCrawlDelay);
                        }
                    }
                }

                previousLineWasUserAgent = false;
            }

            return new RobotsTxt(_robotClient, userAgentRules, userAgentCrawlDirectives, host, sitemaps);
        }
        catch (Exception e) when (e is not RobotsTxtException)
        {
            throw new RobotsTxtException("Unable to parse robots.txt", e);
        }
    }

    private static string GetValueOfDirective(string line, string directive)
    {
        var lineWithoutDirective = line[directive.Length..];
        var endOfValueIndex = lineWithoutDirective.IndexOf(' ');
        if (endOfValueIndex == -1) endOfValueIndex = lineWithoutDirective.Length;
        return lineWithoutDirective[..endOfValueIndex];
    }
}
