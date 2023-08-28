using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace Robots.Txt.Parser.Tests.Unit;

public class SimpleTextSitemapParserTests
{
    [Fact]
    public async Task ReadAsStreamAsync_EmptyFile_ReturnEmptySitemap()
    {
        // Arrange
        var file = @"";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        // Act
        var sitemap = await SimpleTextSitemapParser.ReadFromStreamAsync(stream);

        // Assert
        sitemap.Should().NotBe(null);
        sitemap.UrlSet.Should().BeEmpty();
    }

    [Fact]
    public async Task ReadAsStreamAsync_InvalidFileStructure_ThrowSitemapException()
    {
        // Arrange
        var file =
@"<?xml version=""1.0"" encoding=""UTF-8""?>";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        // Act
        var parse = async () => await SimpleTextSitemapParser.ReadFromStreamAsync(stream);

        // Assert
        await parse.Should().ThrowAsync<SitemapException>();
    }

    [Fact]
    public async Task ReadAsStreamAsync_Over50000Lines_ThrowSitemapException()
    {
        // Arrange
        var fileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
        var stream = fileProvider.GetFileInfo("over-50k-lines-sitemap.txt").CreateReadStream();

        // Act
        var parse = async () => await SimpleTextSitemapParser.ReadFromStreamAsync(stream);

        // Assert
        await parse.Should().ThrowAsync<SitemapException>();
    }

    [Fact]
    public async Task ReadAsStreamAsync_Exactly50000Lines_DoNotThrow()
    {
        // Arrange
        var fileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
        var stream = fileProvider.GetFileInfo("exactly-50k-lines-sitemap.txt").CreateReadStream();

        // Act
        var parse = async () => await SimpleTextSitemapParser.ReadFromStreamAsync(stream);

        // Assert
        await parse.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ReadAsStreamAsync_Over50MiB_ThrowSitemapException()
    {
        // Arrange
        var fileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
        var stream = fileProvider.GetFileInfo("over-50mib-sitemap.txt").CreateReadStream();

        // Act
        var parse = async () => await SimpleTextSitemapParser.ReadFromStreamAsync(stream);

        // Assert
        await parse.Should().ThrowAsync<SitemapException>();
    }

    [Fact]
    public async Task ReadAsStreamAsync_Exactly50MiB_DoNotThrow()
    {
        // Arrange
        var fileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
        var stream = fileProvider.GetFileInfo("exactly-50mib-sitemap.txt").CreateReadStream();

        // Act
        var parse = async () => await SimpleTextSitemapParser.ReadFromStreamAsync(stream);

        // Assert
        await parse.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ReadAsStreamAsync_ValidFile_ReturnSitemap()
    {
        // Arrange
        var file = @"https://github.com/organisations
https://github.com/people";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        // Act
        var sitemap = await SimpleTextSitemapParser.ReadFromStreamAsync(stream);

        // Assert
        sitemap.Should().NotBe(null);
        sitemap.UrlSet.Should().BeEquivalentTo(new HashSet<UrlSetItem>
        {
            new (new Uri("https://github.com/organisations"), null, null, null),
            new (new Uri("https://github.com/people"), null, null, null),
        });
    }

    [Fact]
    public async Task ReadAsStreamAsync_ValidFileWithWhitespaceLines_ReturnSitemap()
    {
        // Arrange
        var file = @"
https://github.com/organisations
    
https://github.com/people";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        // Act
        var sitemap = await SimpleTextSitemapParser.ReadFromStreamAsync(stream);

        // Assert
        sitemap.Should().NotBe(null);
        sitemap.UrlSet.Should().BeEquivalentTo(new HashSet<UrlSetItem>
        {
            new (new Uri("https://github.com/organisations"), null, null, null),
            new (new Uri("https://github.com/people"), null, null, null),
        });
    }
}
