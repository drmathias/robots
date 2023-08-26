Table of Contents
===

- [Overview](#overview)
- [Why Build Yet Another Parser?](#why-build-yet-another-parser)
- [Features](#features)
- [Usage](#usage)
  - [Minimal Example](#minimal-example)
    - [With Dependency Injection](#with-dependency-injection)
    - [Without Dependency Injection](#without-dependency-injection)
  - [Web Crawler Example](#web-crawler-example)
    - [With Dependency Injection](#with-dependency-injection-1)
    - [Without Dependency Injection](#without-dependency-injection-1)
  - [Retrieving the Sitemap](#retrieving-the-sitemap)
  - [Checking a Rule](#checking-a-rule)
  - [Getting Preferred Host](#getting-preferred-host)
  - [Getting Crawl Delay](#getting-crawl-delay)
- [Contributing](#contributing)
    
# Overview

[![License](https://img.shields.io/github/license/drmathias/robots)](https://github.com/drmathias/robots/blob/main/LICENSE) [![Nuget](https://img.shields.io/nuget/v/Robots.Txt.Parser)](https://www.nuget.org/packages/Robots.Txt.Parser/) [![Continuous Integration Workflow](https://github.com/drmathias/robots/actions/workflows/ci.yml/badge.svg)](https://github.com/drmathias/robots/actions/workflows/ci.yml) [![Coverage Status](https://coveralls.io/repos/github/drmathias/robots/badge.svg?branch=main)](https://coveralls.io/github/drmathias/robots?branch=main)


Parse _robots.txt_ and _sitemaps_ using dotnet.
Supports the proposed [RFC9309](https://datatracker.ietf.org/doc/html/rfc9309#name-the-allow-and-disallow-line) standard, as well as the following common, non-standard directives:

- Sitemap
- Host
- Crawl-delay

# Why Build Yet Another Parser?

There are several _robots.txt_ and _sitemap_ parsers that already exist, however they all suffer from their lack of flexibility.

This library is based upon `HttpClient`, making it very familiar, easy to use and adaptable to your needs. Since you have full control over the `HttpClient`, you are able to configure custom message handlers to intercept outgoing requests and responses. For example, you may want to add custom headers on a request, configure additional logging or set up a retry policy.

There is also the possibility to extend this library to support protocols other than HTTP, such as FTP.

# Features

| Name | Supported | Priority |
|------|-----------|---------|
| HTTP/HTTPS | ✔️ | |
| FTPS/FTPS | ❌ | 0.1 |
| Wildcard (`*`) User-agent | ✔️ | |
| Allow & disallow rules | ✔️ | |
| End-of-match (`$`) and wildcard (`*`) paths | ❌ | 1.0 |
| Sitemap entries | ✔️ | |
| Host directive | ✔️ | |
| Crawl-delay directive | ✔️ | |
| Sitemaps XML format | ✔️ | |
| RSS 2.0 feeds | ❌ | 0.8 |
| Atom 0.3/1.0 feeds | ❌ | 0.8 |
| Simple text sitemaps | ❌ | 0.5 |
| Caching support | ❌ | 0.3 |

# Usage

Install the package via NuGet.

```sh
dotnet add package Robots.Txt.Parser
```

## Minimal Example

First, create an implementation of `IWebsiteMetadata` for the host address that you wish to use.

```csharp
public class GitHubWebsite : IWebsiteMetadata
{
    public static Uri BaseAddress => new("https://www.github.com");
}
```

Next, create an instance of `RobotWebClient<TWebsite>`.

### With Dependency Injection

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHttpClient<IRobotWebClient<GitHubWebsite>, RobotWebClient<GitHubWebsite>>();
}
```

### Without Dependency Injection

```csharp
using var httpClient = new HttpClient();
var robotWebClient = new RobotWebClient<GitHubWebsite>(httpClient);
```

## Web Crawler Example

Optionally, specify message handlers to modify the HTTP pipeline. For example, you may be attempting to crawl the website and therefore will want to reduce the rate of your requests, to do so responsibly. You can achieve this by adding a custom `HttpMessageHandler` to the pipeline.

```csharp
public class ResponsibleCrawlerHttpClientHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        return response;
    }
}
```

### With Dependency Injection

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.TryAddTransient<ResponsibleCrawlerHttpClientHandler>();
    services.AddHttpClient<IRobotWebClient<GitHubWebsite>, RobotWebClient<GitHubWebsite>>()
            .AddPrimaryHttpMessageHandler<ResponsibleCrawlerHttpClientHandler>();
}
```

### Without Dependency Injection

```csharp
var httpClientHandler = new HttpClientHandler
{
    InnerHandler = new ResponsibleCrawlerHttpClientHandler()
};
using var httpClient = new HttpClient(httpClientHandler);
var robotWebClient = new RobotWebClient<GitHubWebsite>(httpClient);
```

## Retrieving the Sitemap

```csharp
var robotsTxt = await robotWebClient.LoadRobotsTxtAsync();
// providing a datetime only retrieves sitemap items modified since this datetime
var modifiedSince = new DateTime(2023, 01, 01);
// sitemaps are scanned recursively and combined into single Sitemap object
// even if robots.txt does not contain sitemap directive, looks for a sitemap at {TWebsite.BaseAddress}/sitemap.xml
var sitemap = await robotsTxt.LoadSitemapAsync(modifiedSince);
```

## Checking a Rule

```csharp
var robotsTxt = await robotWebClient.LoadRobotsTxtAsync();
// if rules for the specific User-Agent are not present, it falls back to the wildcard *
var anyRulesDefined = robotsTxt.TryGetRules("SomeBotUserAgent", out var rules);
// even if no wildcard rules exist, an empty rule-checker is returned
var isAllowed = rules.IsAllowed("/some/path");
```

## Getting Preferred Host

```csharp
var robotsTxt = await robotWebClient.LoadRobotsTxtAsync();
// host value will fall back to TWebsite.BaseAddress host, if no directive exists
var hasHostDirective = robotsTxt.TryGetHost(out var host);
```

## Getting Crawl Delay

```csharp
var robotsTxt = await robotWebClient.LoadRobotsTxtAsync();
// if no Crawl-delay directive exists, crawl delay will be 0
var hasCrawlDelayDirective = robotsTxt.TryGetCrawlDelay(out var crawlDelay);
```

# Contributing

Issues and pull requests are encouraged. For large or breaking changes, it is suggested to open an issue first, to discuss before proceeding.

If you find this project useful, please give it a star.