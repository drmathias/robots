using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Robots.Txt.Parser;

public interface IRobotsTxt
{
    /// <summary>
    /// Retrieves the sitemap
    /// </summary>
    /// <param name="modifiedSince">Filter to retrieve site maps modified after this date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A sitemap, or null or no sitemap is found</returns>
    ValueTask<ISitemap?> LoadSitemapAsync(DateTime? modifiedSince = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the crawl delay specified for a User-Agent
    /// </summary>
    /// <param name="userAgent">User-Agent header to retrieve rules for</param>
    /// <param name="crawlDelay">The crawl delay in seconds</param>
    /// <returns>True if a crawl delay directive exists; otherwise false</returns>
    bool TryGetCrawlDelay(string userAgent, out int crawlDelay);

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
    bool TryGetRules(string userAgent, out IRobotRuleChecker ruleChecker);
}

public class RobotsTxt : IRobotsTxt
{
    private readonly IRobotClient _client;

    private readonly IReadOnlyDictionary<string, HashSet<UrlRule>> _userAgentRules;
    private readonly IReadOnlyDictionary<string, int> _userAgentCrawlDirectives;
    private readonly HashSet<string> _userAgents;
    private readonly string? _host;
    private readonly HashSet<Uri> _sitemapUrls;

    internal RobotsTxt(IRobotClient client,
                       IReadOnlyDictionary<string, HashSet<UrlRule>> userAgentRules,
                       IReadOnlyDictionary<string, int> userAgentCrawlDirectives,
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
    public async ValueTask<ISitemap?> LoadSitemapAsync(DateTime? modifiedSince = default, CancellationToken cancellationToken = default)
        => _sitemapUrls.Count != 0
            ? await _client.LoadSitemapsAsync(_sitemapUrls, modifiedSince, cancellationToken)
            : await _client.LoadSitemapsAsync(new[] { new Uri(_client.BaseAddress, "/sitemap.xml") }, modifiedSince, cancellationToken);

    /// <inheritdoc />
    public bool TryGetCrawlDelay(string userAgent, out int crawlDelay)
    {
        var userAgentMatch = _userAgentCrawlDirectives.TryGetValue(userAgent, out crawlDelay);
        if (!userAgentMatch)
        {
            if (_userAgents.Contains(userAgent)) return false;
            return _userAgentCrawlDirectives.TryGetValue("*", out crawlDelay);
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
    public bool TryGetRules(string userAgent, out IRobotRuleChecker ruleChecker)
    {
        if (!_userAgentRules.TryGetValue(userAgent, out var rules) && !_userAgentRules.TryGetValue("*", out rules))
        {
            ruleChecker = new RobotRuleChecker(new HashSet<UrlRule>());
            return false;
        }

        ruleChecker = new RobotRuleChecker(rules);
        return true;
    }
}
