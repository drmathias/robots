using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Robots.Txt.Parser.Tests.Unit;

public partial class RobotsTxtParserTests
{
    [Fact]
    public async Task ExtendedRobotsTxt_NoMatchedRules_DefaultCrawlDelay()
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
        robotsTxt.TryGetCrawlDelay("SomeBot", out var crawlDelay).Should().Be(false);
        crawlDelay.Should().Be(0);
    }

    [Fact]
    public async Task ExtendedRobotsTxt_WildcardUserAgent_NoCrawlDelaySpecified()
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
        robotsTxt.TryGetCrawlDelay("SomeBot", out var crawlDelay).Should().Be(false);
        crawlDelay.Should().Be(0);
    }

    [Fact]
    public async Task ExtendedRobotsTxt_WildcardUserAgent_CrawlDelaySpecified()
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
        robotsTxt.TryGetCrawlDelay("SomeBot", out var crawlDelay).Should().Be(true);
        crawlDelay.Should().Be(10);
    }

    [Fact]
    public async Task ExtendedRobotsTxt_MatchUserAgent_NoCrawlDelaySpecified()
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
        robotsTxt.TryGetCrawlDelay("SomeBot", out var crawlDelay).Should().Be(false);
        crawlDelay.Should().Be(0);
    }

    [Fact]
    public async Task ExtendedRobotsTxt_MatchUserAgent_CrawlDelaySpecified()
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
        robotsTxt.TryGetCrawlDelay("SomeBot", out var crawlDelay).Should().Be(true);
        crawlDelay.Should().Be(5);
    }

    [Fact]
    public async Task ExtendedRobotsTxt_MultiLineMatchUserAgent_NoCrawlDelaySpecified()
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
        robotsTxt.TryGetCrawlDelay("SomeBot", out var crawlDelay).Should().Be(false);
        crawlDelay.Should().Be(0);
    }

    [Fact]
    public async Task ExtendedRobotsTxt_MultiLineMatchUserAgent_CrawlDelaySpecified()
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
        robotsTxt.TryGetCrawlDelay("SomeBot", out var crawlDelay).Should().Be(true);
        crawlDelay.Should().Be(5);
    }
}
