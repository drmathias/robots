using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Robots.Txt.Parser.Tests.Unit;

public partial class RobotsTxtParserTests
{
    [Fact]
    public async Task NoMatchedRules_CrawlDelayNotSpecified_DefaultCrawlDelay()
    {
        // Arrange
        var file =
@"User-agent: AnotherBot
Crawl-delay: 10
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Assert
        robotsTxt.Should().NotBe(null);
        robotsTxt.TryGetCrawlDelay(ProductToken.Parse("SomeBot"), out var crawlDelay).Should().Be(false);
        crawlDelay.Should().Be(0);
    }

    [Fact]
    public async Task WildcardUserAgent_CrawlDelayNotSpecified_DefaultCrawlDelay()
    {
        // Arrange
        var file =
@"User-agent: *
Disallow: 
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Assert
        robotsTxt.Should().NotBe(null);
        robotsTxt.TryGetCrawlDelay(ProductToken.Parse("SomeBot"), out var crawlDelay).Should().Be(false);
        crawlDelay.Should().Be(0);
    }

    [Fact]
    public async Task WildcardUserAgent_CrawlDelaySpecified_ReturnCrawlDelay()
    {
        // Arrange
        var file =
@"User-agent: *
Crawl-delay: 10 
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Assert
        robotsTxt.Should().NotBe(null);
        robotsTxt.TryGetCrawlDelay(ProductToken.Parse("SomeBot"), out var crawlDelay).Should().Be(true);
        crawlDelay.Should().Be(10);
    }

    [Fact]
    public async Task WildcardUserAgent_NonStandardCaseCrawlDelaySpecified_ReturnCrawlDelay()
    {
        // Arrange
        var file =
@"User-agent: *
crawl-delay: 10 
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Assert
        robotsTxt.Should().NotBe(null);
        robotsTxt.TryGetCrawlDelay(ProductToken.Parse("SomeBot"), out var crawlDelay).Should().Be(true);
        crawlDelay.Should().Be(10);
    }

    [Fact]
    public async Task MatchedUserAgent_NoCrawlDelaySpecified_DefaultCrawlDelay()
    {
        // Arrange
        var file =
@"User-agent: *
Crawl-delay: 10

User-agent: SomeBot
Disallow: /some/path
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Assert
        robotsTxt.Should().NotBe(null);
        robotsTxt.TryGetCrawlDelay(ProductToken.Parse("SomeBot"), out var crawlDelay).Should().Be(false);
        crawlDelay.Should().Be(0);
    }

    [Fact]
    public async Task MatchedUserAgent_CrawlDelaySpecified_ReturnCrawlDelay()
    {
        // Arrange
        var file =
@"User-agent: *
Crawl-delay: 10

User-agent: SomeBot
Crawl-delay: 5 
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Assert
        robotsTxt.Should().NotBe(null);
        robotsTxt.TryGetCrawlDelay(ProductToken.Parse("SomeBot"), out var crawlDelay).Should().Be(true);
        crawlDelay.Should().Be(5);
    }

    [Fact]
    public async Task MatchedMultiLineUserAgent_NoCrawlDelaySpecified_DefaultCrawlDelay()
    {
        // Arrange
        var file =
@"User-agent: *
Crawl-delay: 10

User-agent: SomeBot
User-agent: SomeOtherBot
Disallow: /some/path
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Assert
        robotsTxt.Should().NotBe(null);
        robotsTxt.TryGetCrawlDelay(ProductToken.Parse("SomeBot"), out var crawlDelay).Should().Be(false);
        crawlDelay.Should().Be(0);
    }

    [Fact]
    public async Task MatchedMultiLineUserAgent_CrawlDelaySpecified_ReturnCrawlDelay()
    {
        // Arrange
        var file =
@"User-agent: *
Crawl-delay: 10

User-agent: SomeBot
User-agent: SomeOtherBot
Crawl-delay: 5 
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Assert
        robotsTxt.Should().NotBe(null);
        robotsTxt.TryGetCrawlDelay(ProductToken.Parse("SomeBot"), out var crawlDelay).Should().Be(true);
        crawlDelay.Should().Be(5);
    }

    [Fact]
    public async Task MatchedDuplicateGroupUserAgent_CrawlDelaySpecified_ReturnFirstCrawlDelay()
    {
        // Arrange
        var file =
@"User-agent: *
Crawl-delay: 10

User-agent: SomeBot
Crawl-delay: 15

User-agent: SomeBot
Crawl-delay: 5 
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Assert
        robotsTxt.Should().NotBe(null);
        robotsTxt.TryGetCrawlDelay(ProductToken.Parse("SomeBot"), out var crawlDelay).Should().Be(true);
        crawlDelay.Should().Be(15);
    }
}
