Table of Contents
===

- [Overview](#overview)
  - [Design Considerations](#design-considerations)
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
Supports the proposed [RFC9309](https://datatracker.ietf.org/doc/html/rfc9309) standard, as well as the following common, non-standard directives:

- Sitemap
- Host
- Crawl-delay

## Design Considerations

This library is based upon `HttpClient`, making it very familiar, easy to use and adaptable to your needs. Since you have full control over the `HttpClient`, you are able to configure custom message handlers to intercept outgoing requests and responses. For example, you may want to add custom headers on a request, configure additional logging or set up a retry policy.

Some websites can have very large sitemaps. For this reason, async streaming is supported as the preferred way of parsing sitemaps.

There is also the possibility to extend this library to support protocols other than HTTP, such as FTP.

# Features

| Name | Supported | Priority |
|------|-----------|----------|
| HTTP/HTTPS | ✔️ | |
| FTPS/FTPS | ❌ | 0.1 |
| Wildcard (`*`) User-agent | ✔️ | |
| Allow & disallow rules | ✔️ | |
| End-of-match (`$`) and wildcard (`*`) paths | ✔️ | |
| Sitemap entries | ✔️ | |
| Host directive | ✔️ | |
| Crawl-delay directive | ✔️ | |
| RSS 2.0 feeds | ❌ | 0.8 |
| Atom 0.3/1.0 feeds | ❌ | 0.8 |
| Sitemaps XML format | ✔️ | |
| Simple text sitemaps | ✔️ | |
| Async streaming of sitemaps | ✔️ | |
| Cancellation token support | ✔️ | |
| Memory management | ✔️ | |

# Usage

Install the package via NuGet.

```sh
dotnet add package Robots.Txt.Parser
```

## Minimal Example

First, create an instance of `RobotWebClient`.

### With Dependency Injection

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHttpClient<IRobotWebClient, RobotWebClient>();
}
```

### Without Dependency Injection

```csharp
using var httpClient = new HttpClient();
var robotWebClient = new RobotWebClient(httpClient);
```

## Web Crawler Example

Optionally, specify message handlers to modify the HTTP pipeline. For example, you may want to throttle the rate of your requests, to responsibily crawl a large sitemap. You can achieve this by adding a custom `HttpMessageHandler` to the pipeline.

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
    services.AddHttpClient<IRobotWebClient, RobotWebClient>()
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
var robotWebClient = new RobotWebClient(httpClient);
```

## Retrieving the Sitemap

```csharp
var robotsTxt = await robotWebClient.LoadRobotsTxtAsync(new Uri("https://github.com"));
// providing a datetime only retrieves sitemap items modified since this datetime
var modifiedSince = new DateTime(2023, 01, 01);
// sitemaps are iterated asynchronously
// even if robots.txt does not contain sitemap directive, looks for a sitemap at {url}/sitemap.xml
await foreach(var item in robotsTxt.LoadSitemapAsync(modifiedSince))
{
}
```

## Checking a Rule

```csharp
var robotsTxt = await robotWebClient.LoadRobotsTxtAsync(new Uri("https://github.com"));
// if rules for the specific robot are not present, it falls back to the wildcard *
var hasAnyRulesDefined = robotsTxt.TryGetRules(ProductToken.Parse("SomeBot"), out var rules);
// even if no wildcard rules exist, an empty rule-checker is returned
var isAllowed = rules.IsAllowed("/some/path");
```

## Getting Preferred Host

```csharp
var robotsTxt = await robotWebClient.LoadRobotsTxtAsync(new Uri("https://github.com"));
// host value will fall back to provided host, if no directive exists
var hasHostDirective = robotsTxt.TryGetHost(out var host);
```

## Getting Crawl Delay

```csharp
var robotsTxt = await robotWebClient.LoadRobotsTxtAsync(new Uri("https://github.com"));
// if rules for the specific robot are not present, it falls back to the wildcard *
// if no Crawl-delay directive exists, crawl delay will be 0
var hasCrawlDelayDirective = robotsTxt.TryGetCrawlDelay(ProductToken.Parse("SomeBot"), out var crawlDelay);
```

# Contributing

Issues and pull requests are encouraged. For large or breaking changes, it is suggested to open an issue first, to discuss before proceeding.

If you find this project useful, please give it a star.