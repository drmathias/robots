using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace Robots.Txt.Parser.Tests.Unit;

public partial class RobotsTxtParserTests
{
    [Fact]
    public async Task OriginalRobotsTxt_Basic()
    {
        // Arrange
        var file =
@"User-agent: *
Disallow: /
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Assert
        robotsTxt.Should().NotBe(null);
    }

    [Fact]
    public async Task OriginalRobotsTxt_WithLineComments_CommentsIgnored()
    {
        // Arrange
        var file =
@"# This is a basic robots.txt file
User-agent: *
Disallow: /
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Assert
        robotsTxt.Should().NotBe(null);
    }

    [Fact]
    public async Task OriginalRobotsTxt_WithEndOfLineComments_CommentsIgnored()
    {
        // Arrange
        var file =
@"User-agent: * # This line specifies any user agent
Disallow: / # Directs the crawler to ignore the entire website
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Assert
        robotsTxt.Should().NotBe(null);
    }

    [Fact]
    public async Task ExtendedRobotsTxt_LoadSitemapAsync_LoadSitemapDirective()
    {
        // Arrange
        var file =
@"Sitemap: https://www.github.com/sitemap.xml

User-agent: *
Disallow: /
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);
        await robotsTxt.LoadSitemapAsync();

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsWebClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            It.Is<IEnumerable<Uri>>(uris => uris.SequenceEqual(new[] { new Uri("https://www.github.com/sitemap.xml") })),
            null,
            default), Times.Once);
    }

    [Fact]
    public async Task ExtendedRobotsTxt_LoadSitemapAsync_LoadMultipleUniqueSitemapDirectives()
    {
        // Arrange
        var file =
@"Sitemap: https://www.github.com/sitemap.xml
Sitemap: https://www.github.com/sitemap-2.xml

User-agent: *
Disallow: /
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);
        await robotsTxt.LoadSitemapAsync();

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsWebClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            It.Is<IEnumerable<Uri>>(uris => uris.SequenceEqual(new[]
            {
                new Uri("https://www.github.com/sitemap.xml"),
                new Uri("https://www.github.com/sitemap-2.xml"),
            })),
            null,
            default), Times.Once);
    }

    [Fact]
    public async Task ExtendedRobotsTxt_LoadSitemapAsync_RetrieveOneIfDuplicateSitemapDirectives()
    {
        // Arrange
        var file =
@"Sitemap: https://www.github.com/sitemap.xml
Sitemap: https://www.github.com/sitemap.xml

User-agent: *
Disallow: /
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);
        await robotsTxt.LoadSitemapAsync();

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsWebClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            It.Is<IEnumerable<Uri>>(uris => uris.SequenceEqual(new[]
            {
                new Uri("https://www.github.com/sitemap.xml"),
            })),
            null,
            default), Times.Once);
    }

    [Fact]
    public async Task ExtendedRobotsTxt_LoadSitemapAsync_PassModifiedDate()
    {
        // Arrange
        var file =
@"Sitemap: https://www.github.com/sitemap.xml

User-agent: *
Disallow: /
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        var modifiedDate = new DateTime(2023, 01, 01);

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);
        await robotsTxt.LoadSitemapAsync(modifiedDate);

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsWebClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            It.IsAny<IEnumerable<Uri>>(),
            modifiedDate,
            default), Times.Once);
    }

    [Fact]
    public async Task ExtendedRobotsTxt_LoadSitemapAsync_PassCancellationToken()
    {
        // Arrange
        var file =
@"Sitemap: https://www.github.com/sitemap.xml

User-agent: *
Disallow: /
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);
        await robotsTxt.LoadSitemapAsync(cancellationToken: cancellationToken);

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsWebClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            It.IsAny<IEnumerable<Uri>>(),
            null,
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task StandardRobotsTxt_LoadSitemapAsync_TryLoadDefaultSitemapIfNoneSpecified()
    {
        // Arrange
        var file =
@"User-agent: *
Disallow: /
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        var baseAddress = new Uri("https://github.com");
        _robotsWebClientMock.Setup(callTo => callTo.BaseAddress).Returns(baseAddress);

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);
        await robotsTxt.LoadSitemapAsync();

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsWebClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            It.Is<IEnumerable<Uri>>(uris => uris.SequenceEqual(new[]
            {
                new Uri("https://github.com/sitemap.xml"),
            })),
            null,
            default), Times.Once);
    }

    [Fact]
    public async Task StandardRobotsTxt_LoadSitemapAsync_PassModifiedDate()
    {
        // Arrange
        var file =
@"User-agent: *
Disallow: /
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        _robotsWebClientMock.Setup(callTo => callTo.BaseAddress).Returns(new Uri("https://github.com"));

        var modifiedDate = new DateTime(2023, 01, 01);

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);
        await robotsTxt.LoadSitemapAsync(modifiedDate);

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsWebClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            It.IsAny<IEnumerable<Uri>>(),
            modifiedDate,
            default), Times.Once);
    }

    [Fact]
    public async Task StandardRobotsTxt_LoadSitemapAsync_PassCancellationToken()
    {
        // Arrange
        var file =
@"User-agent: *
Disallow: /
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        _robotsWebClientMock.Setup(callTo => callTo.BaseAddress).Returns(new Uri("https://github.com"));

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);
        await robotsTxt.LoadSitemapAsync(cancellationToken: cancellationToken);

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsWebClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            It.IsAny<IEnumerable<Uri>>(),
            null,
            cancellationToken), Times.Once);
    }
}
