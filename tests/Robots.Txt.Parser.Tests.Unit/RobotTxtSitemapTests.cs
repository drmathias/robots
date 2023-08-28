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
    public async Task LoadSitemapAsync_SitemapDirectiveExists_LoadSitemapDirective()
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
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            It.Is<IEnumerable<Uri>>(uris => uris.SequenceEqual(new[] { new Uri("https://www.github.com/sitemap.xml") })),
            null,
            default), Times.Once);
    }

    [Fact]
    public async Task LoadSitemapAsync_MultipleSitemapDirectives_LoadMultipleUniqueSitemapDirectives()
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
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            It.Is<IEnumerable<Uri>>(uris => uris.SequenceEqual(new[]
            {
                new Uri("https://www.github.com/sitemap.xml"),
                new Uri("https://www.github.com/sitemap-2.xml"),
            })),
            null,
            default), Times.Once);
    }

    [Fact]
    public async Task LoadSitemapAsync_MultipleSitemapDirectives_RetrieveOneIfDuplicateSitemapDirectives()
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
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            It.Is<IEnumerable<Uri>>(uris => uris.SequenceEqual(new[]
            {
                new Uri("https://www.github.com/sitemap.xml"),
            })),
            null,
            default), Times.Once);
    }

    [Fact]
    public async Task LoadSitemapAsync_SitemapDirectiveExists_PassModifiedDate()
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
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            It.IsAny<IEnumerable<Uri>>(),
            modifiedDate,
            default), Times.Once);
    }

    [Fact]
    public async Task LoadSitemapAsync_SitemapDirectiveExists_PassCancellationToken()
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
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            It.IsAny<IEnumerable<Uri>>(),
            null,
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task LoadSitemapAsync_NoSitemapDirective_TryLoadDefaultSitemapIfNoneSpecified()
    {
        // Arrange
        var file =
@"User-agent: *
Disallow: /
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        var baseAddress = new Uri("https://github.com");
        _robotsClientMock.Setup(callTo => callTo.BaseAddress).Returns(baseAddress);

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);
        await robotsTxt.LoadSitemapAsync();

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            It.Is<IEnumerable<Uri>>(uris => uris.SequenceEqual(new[]
            {
                new Uri("https://github.com/sitemap.xml"),
            })),
            null,
            default), Times.Once);
    }

    [Fact]
    public async Task LoadSitemapAsync_NoSitemapDirective_PassModifiedDate()
    {
        // Arrange
        var file =
@"User-agent: *
Disallow: /
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        _robotsClientMock.Setup(callTo => callTo.BaseAddress).Returns(new Uri("https://github.com"));

        var modifiedDate = new DateTime(2023, 01, 01);

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);
        await robotsTxt.LoadSitemapAsync(modifiedDate);

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            It.IsAny<IEnumerable<Uri>>(),
            modifiedDate,
            default), Times.Once);
    }

    [Fact]
    public async Task LoadSitemapAsync_NoSitemapDirective_PassCancellationToken()
    {
        // Arrange
        var file =
@"User-agent: *
Disallow: /
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        _robotsClientMock.Setup(callTo => callTo.BaseAddress).Returns(new Uri("https://github.com"));

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);
        await robotsTxt.LoadSitemapAsync(cancellationToken: cancellationToken);

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            It.IsAny<IEnumerable<Uri>>(),
            null,
            cancellationToken), Times.Once);
    }
}
