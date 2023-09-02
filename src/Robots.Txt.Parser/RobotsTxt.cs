using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Robots.Txt.Parser;

/// <summary>
/// A representation of the contained directives in a robots.txt file
/// </summary>
public interface IRobotsTxt
{
    /// <summary>
    /// Retrieves the sitemap
    /// </summary>
    /// <param name="modifiedSince">Filter to retrieve site maps modified after this date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A sitemap, or null or no sitemap is found</returns>
    IAsyncEnumerable<UrlSetItem> LoadSitemapAsync(DateTime? modifiedSince = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the crawl delay specified for a User-Agent
    /// </summary>
    /// <param name="userAgent">User-Agent header to retrieve rules for</param>
    /// <param name="crawlDelay">The crawl delay in seconds</param>
    /// <returns>True if a crawl delay directive exists; otherwise false</returns>
    bool TryGetCrawlDelay(ProductToken userAgent, out int crawlDelay);

    /// <summary>
    /// Retrieves the website host
    /// </summary>
    /// <param name="host">The website host address</param>
    /// <returns>True if the host directive exists; otherwise false</returns>
    bool TryGetHost(out string host);

    /// <summary>
    /// Retrieves rules which apply to a User-Agent
    /// </summary>
    /// <param name="userAgent">User-Agent header to retrieve rules for</param>
    /// <param name="ruleChecker">A rule checker for the User-Agent</param>
    /// <returns>True if any rules are found; otherwise false</returns>
    bool TryGetRules(ProductToken userAgent, out IRobotRuleChecker ruleChecker);
}

/// <summary>
/// A representation of the contained directives in a robots.txt file
/// </summary>
public class RobotsTxt : IRobotsTxt
{
    private readonly IRobotClient _client;

    private readonly IReadOnlyDictionary<ProductToken, HashSet<UrlRule>> _userAgentRules;
    private readonly IReadOnlyDictionary<ProductToken, int> _userAgentCrawlDirectives;
    private readonly HashSet<ProductToken> _userAgents;
    private readonly string? _host;
    private readonly HashSet<Uri> _sitemapUrls;

    internal RobotsTxt(IRobotClient client,
                       IReadOnlyDictionary<ProductToken, HashSet<UrlRule>> userAgentRules,
                       IReadOnlyDictionary<ProductToken, int> userAgentCrawlDirectives,
                       string? host,
                       HashSet<Uri> sitemapUrls)
    {
        _client = client;
        _userAgentRules = userAgentRules;
        _userAgentCrawlDirectives = userAgentCrawlDirectives;
        _userAgents = _userAgentRules.Keys.Concat(_userAgentCrawlDirectives.Keys).ToHashSet();
        _host = host;
        _sitemapUrls = sitemapUrls;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<UrlSetItem> LoadSitemapAsync(DateTime? modifiedSince = default, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var urls = _sitemapUrls.Count != 0 ? _sitemapUrls.AsEnumerable() : new[] { new Uri(_client.BaseAddress, "/sitemap.xml") };
        foreach (var url in urls)
        {
            await foreach (var item in _client.LoadSitemapsAsync(url, modifiedSince, cancellationToken))
            {
                yield return item;
            }
        }
    }

    /// <inheritdoc />
    public bool TryGetCrawlDelay(ProductToken userAgent, out int crawlDelay)
    {
        var userAgentMatch = _userAgentCrawlDirectives.TryGetValue(userAgent, out crawlDelay);
        if (!userAgentMatch)
        {
            if (_userAgents.Contains(userAgent)) return false;
            return _userAgentCrawlDirectives.TryGetValue(ProductToken.Wildcard, out crawlDelay);
        }

        return true;
    }

    /// <inheritdoc />
    public bool TryGetHost(out string host)
    {
        host = _host ?? _client.BaseAddress.Host;
        return _host is not null;
    }

    /// <inheritdoc />
    public bool TryGetRules(ProductToken userAgent, out IRobotRuleChecker ruleChecker)
    {
        if (!_userAgentRules.TryGetValue(userAgent, out var rules) && !_userAgentRules.TryGetValue(ProductToken.Wildcard, out rules))
        {
            ruleChecker = new RobotRuleChecker(new HashSet<UrlRule>());
            return false;
        }

        ruleChecker = new RobotRuleChecker(rules);
        return true;
    }
}
