using System;
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
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        _robotsClientMock.Setup(callTo => callTo.LoadSitemapsAsync(It.IsAny<Uri>(), It.IsAny<DateTime?>(), It.IsAny<Func<Uri, bool>>(), It.IsAny<CancellationToken>()))
                         .Returns(Enumerable.Empty<UrlSetItem>().ToAsyncEnumerable());

        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Act
        await robotsTxt.LoadSitemapAsync().ToListAsync();

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            new Uri("https://www.github.com/sitemap.xml"),
            null,
            null,
            default), Times.Once);
    }

    [Fact]
    public async Task LoadSitemapAsync_MultipleSitemapDirectivesTopOfFile_LoadMultipleUniqueSitemapDirectives()
    {
        // Arrange
        var file =
@"Sitemap: https://www.github.com/sitemap.xml
Sitemap: https://www.github.com/sitemap-2.xml

User-agent: *
Disallow: /
";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        _robotsClientMock.Setup(callTo => callTo.LoadSitemapsAsync(It.IsAny<Uri>(), It.IsAny<DateTime?>(), It.IsAny<Func<Uri, bool>>(), It.IsAny<CancellationToken>()))
                         .Returns(Enumerable.Empty<UrlSetItem>().ToAsyncEnumerable());

        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Act
        await robotsTxt.LoadSitemapAsync().ToListAsync();

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            new Uri("https://www.github.com/sitemap.xml"),
            null,
            null,
            default), Times.Once);
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            new Uri("https://www.github.com/sitemap-2.xml"),
            null,
            null,
            default), Times.Once);
    }

    [Fact]
    public async Task LoadSitemapAsync_MultipleSitemapDirectivesUnderUserAgent_LoadMultipleUniqueSitemapDirectives()
    {
        // Arrange
        var file =
@"User-agent: *
Disallow: /

Sitemap: https://www.github.com/sitemap.xml
Sitemap: https://www.github.com/sitemap-2.xml";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        _robotsClientMock.Setup(callTo => callTo.LoadSitemapsAsync(It.IsAny<Uri>(), It.IsAny<DateTime?>(), It.IsAny<Func<Uri, bool>>(), It.IsAny<CancellationToken>()))
                         .Returns(Enumerable.Empty<UrlSetItem>().ToAsyncEnumerable());

        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Act
        await robotsTxt.LoadSitemapAsync().ToListAsync();

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            new Uri("https://www.github.com/sitemap.xml"),
            null,
            null,
            default), Times.Once);
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            new Uri("https://www.github.com/sitemap-2.xml"),
            null,
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
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        _robotsClientMock.Setup(callTo => callTo.LoadSitemapsAsync(It.IsAny<Uri>(), It.IsAny<DateTime?>(), It.IsAny<Func<Uri, bool>>(), It.IsAny<CancellationToken>()))
                         .Returns(Enumerable.Empty<UrlSetItem>().ToAsyncEnumerable());

        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Act
        await robotsTxt.LoadSitemapAsync().ToListAsync();

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            new Uri("https://www.github.com/sitemap.xml"),
            null,
            null,
            default), Times.Once);
    }

    [Fact]
    public async Task LoadSitemapAsync_MultipleSitemapDirectives_OnlyLoadDirectivesMatchingFilter()
    {
        // Arrange
        var file =
@"Sitemap: https://www.github.com/sitemap-products.xml
Sitemap: https://www.github.com/sitemap-categories.xml
Sitemap: https://www.github.com/sitemap-brands.xml

User-agent: *
Disallow: /
";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        _robotsClientMock.Setup(callTo => callTo.LoadSitemapsAsync(It.IsAny<Uri>(), It.IsAny<DateTime?>(), It.IsAny<Func<Uri, bool>>(), It.IsAny<CancellationToken>()))
                         .Returns(Enumerable.Empty<UrlSetItem>().ToAsyncEnumerable());

        Func<Uri, bool> sitemapLocationFilter = location => location.AbsolutePath.Contains("brands");

        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Act
        await robotsTxt.LoadSitemapAsync(sitemapLocationFilter: sitemapLocationFilter).ToListAsync();

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            new Uri("https://www.github.com/sitemap-products.xml"),
            It.IsAny<DateTime?>(),
            It.IsAny<Func<Uri, bool>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            new Uri("https://www.github.com/sitemap-categories.xml"),
            It.IsAny<DateTime?>(),
            It.IsAny<Func<Uri, bool>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            new Uri("https://www.github.com/sitemap-brands.xml"),
            null,
            sitemapLocationFilter,
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
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        _robotsClientMock.Setup(callTo => callTo.LoadSitemapsAsync(It.IsAny<Uri>(), It.IsAny<DateTime?>(), It.IsAny<Func<Uri, bool>>(), It.IsAny<CancellationToken>()))
                         .Returns(Enumerable.Empty<UrlSetItem>().ToAsyncEnumerable());

        var modifiedDate = new DateTime(2023, 01, 01);

        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Act
        await robotsTxt.LoadSitemapAsync(modifiedDate).ToListAsync();

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            It.IsAny<Uri>(),
            modifiedDate,
            null,
            default), Times.Once);
    }

    [Fact]
    public async Task LoadSitemapAsync_SitemapDirectiveExists_PassSitemapLocationFilter()
    {
        // Arrange
        var file =
@"Sitemap: https://www.github.com/sitemap.xml

User-agent: *
Disallow: /
";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        _robotsClientMock.Setup(callTo => callTo.LoadSitemapsAsync(It.IsAny<Uri>(), It.IsAny<DateTime?>(), It.IsAny<Func<Uri, bool>>(), It.IsAny<CancellationToken>()))
                         .Returns(Enumerable.Empty<UrlSetItem>().ToAsyncEnumerable());

        Func<Uri, bool> sitemapLocationFilter = location => location.AbsolutePath == "/sitemap.xml" || location.AbsolutePath.Contains("product");

        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Act
        await robotsTxt.LoadSitemapAsync(sitemapLocationFilter: sitemapLocationFilter).ToListAsync();

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            It.IsAny<Uri>(),
            null,
            sitemapLocationFilter,
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
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        _robotsClientMock.Setup(callTo => callTo.LoadSitemapsAsync(It.IsAny<Uri>(), It.IsAny<DateTime?>(), It.IsAny<Func<Uri, bool>>(), It.IsAny<CancellationToken>()))
                         .Returns(Enumerable.Empty<UrlSetItem>().ToAsyncEnumerable());

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Act
        await robotsTxt.LoadSitemapAsync(cancellationToken: cancellationToken).ToListAsync();

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            It.IsAny<Uri>(),
            null,
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
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        _robotsClientMock.Setup(callTo => callTo.LoadSitemapsAsync(It.IsAny<Uri>(), It.IsAny<DateTime?>(), It.IsAny<Func<Uri, bool>>(), It.IsAny<CancellationToken>()))
                         .Returns(Enumerable.Empty<UrlSetItem>().ToAsyncEnumerable());

        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Act
        await robotsTxt.LoadSitemapAsync().ToListAsync();

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            new Uri("https://www.github.com/sitemap.xml"),
            null,
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
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        _robotsClientMock.Setup(callTo => callTo.LoadSitemapsAsync(It.IsAny<Uri>(), It.IsAny<DateTime?>(), It.IsAny<Func<Uri, bool>>(), It.IsAny<CancellationToken>()))
                         .Returns(Enumerable.Empty<UrlSetItem>().ToAsyncEnumerable());

        var modifiedDate = new DateTime(2023, 01, 01);

        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Act
        await robotsTxt.LoadSitemapAsync(modifiedDate).ToListAsync();

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            It.IsAny<Uri>(),
            modifiedDate,
            null,
            default), Times.Once);
    }

    [Fact]
    public async Task LoadSitemapAsync_NoSitemapDirective_PassSitemapLocationFilter()
    {
        // Arrange
        var file =
@"User-agent: *
Disallow: /
";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        _robotsClientMock.Setup(callTo => callTo.LoadSitemapsAsync(It.IsAny<Uri>(), It.IsAny<DateTime?>(), It.IsAny<Func<Uri, bool>>(), It.IsAny<CancellationToken>()))
                         .Returns(Enumerable.Empty<UrlSetItem>().ToAsyncEnumerable());

        Func<Uri, bool> sitemapLocationFilter = location => location.AbsolutePath == "/sitemap.xml" || location.AbsolutePath.Contains("product");

        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Act
        await robotsTxt.LoadSitemapAsync(sitemapLocationFilter: sitemapLocationFilter).ToListAsync();

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            It.IsAny<Uri>(),
            null,
            sitemapLocationFilter,
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
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        _robotsClientMock.Setup(callTo => callTo.LoadSitemapsAsync(It.IsAny<Uri>(), It.IsAny<DateTime?>(), It.IsAny<Func<Uri, bool>>(), It.IsAny<CancellationToken>()))
                         .Returns(Enumerable.Empty<UrlSetItem>().ToAsyncEnumerable());

        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Act
        await robotsTxt.LoadSitemapAsync(cancellationToken: cancellationToken).ToListAsync();

        // Assert
        robotsTxt.Should().NotBe(null);
        _robotsClientMock.Verify(callTo => callTo.LoadSitemapsAsync(
            It.IsAny<Uri>(),
            null,
            null,
            cancellationToken), Times.Once);
    }
}
