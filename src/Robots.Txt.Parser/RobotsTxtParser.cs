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
    public async Task<IRobotsTxt> ReadFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        string? line;

        string? host = null;
        var sitemaps = new HashSet<Uri>();

        var previousLineWasUserAgent = false;
        /*
          Crawlers MUST use case-insensitive matching to find the group that matches the product token
        */
        var currentUserAgents = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var userAgentRules = new Dictionary<string, HashSet<UrlRule>>(StringComparer.OrdinalIgnoreCase);
        var userAgentCrawlDirectives = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        /*
          The file MUST be UTF-8 encoded
        */
        using var streamReader = new StreamReader(stream, Encoding.UTF8);
        while ((line = await streamReader.ReadLineAsync(cancellationToken)) is not null)
        {
            if (stream.Position > ByteCount500KiB) throw new OutOfMemoryException("Reached parsing limit");

            if (line.StartsWith('#')) continue;

            if (line.StartsWith(UserAgentDirective))
            {
                if (!previousLineWasUserAgent) currentUserAgents.Clear();
                var currentUserAgent = GetValueOfDirective(line, UserAgentDirective);
                currentUserAgents.Add(currentUserAgent);
                userAgentRules.TryAdd(currentUserAgent, new HashSet<UrlRule>());
                previousLineWasUserAgent = true;
                continue;
            }

            if (currentUserAgents.Count == 0)
            {
                if (line.StartsWith(SitemapDirective))
                {
                    var sitemapValue = GetValueOfDirective(line, SitemapDirective);
                    if (Uri.TryCreate(sitemapValue, UriKind.Absolute, out var sitemapAddress)) sitemaps.Add(sitemapAddress);
                }
                else if (host is null && line.StartsWith(HostDirective))
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
                if (line.StartsWith(DisallowDirective))
                {
                    var disallowValue = GetValueOfDirective(line, DisallowDirective);
                    foreach (var userAgent in currentUserAgents) userAgentRules[userAgent].Add(new UrlRule(RuleType.Disallow, disallowValue));
                }
                else if (line.StartsWith(AllowDirective))
                {
                    var allowedValue = GetValueOfDirective(line, AllowDirective);
                    foreach (var userAgent in currentUserAgents) userAgentRules[userAgent].Add(new UrlRule(RuleType.Allow, allowedValue));
                }
                else if (line.StartsWith(CrawlDelayDirective))
                {
                    var crawlDelayValue = GetValueOfDirective(line, CrawlDelayDirective);
                    if (int.TryParse(crawlDelayValue, out var parsedCrawlDelay))
                    {
                        foreach (var userAgent in currentUserAgents) userAgentCrawlDirectives[userAgent] = parsedCrawlDelay;
                    }
                }
            }

            previousLineWasUserAgent = false;
        }

        return new RobotsTxt(_robotClient, userAgentRules, userAgentCrawlDirectives, host, sitemaps);
    }

    private static string GetValueOfDirective(string line, string directive)
    {
        var lineWithoutDirective = line[directive.Length..];
        var endOfValueIndex = lineWithoutDirective.IndexOf(' ');
        if (endOfValueIndex == -1) endOfValueIndex = lineWithoutDirective.Length;
        return lineWithoutDirective[..endOfValueIndex];
    }
}
