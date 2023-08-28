using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.FileProviders;
using Moq;
using Xunit;

namespace Robots.Txt.Parser.Tests.Unit;

public partial class RobotsTxtParserTests
{
    private readonly Mock<IRobotClient> _robotsClientMock;
    private readonly RobotsTxtParser _parser;

    public RobotsTxtParserTests()
    {
        _robotsClientMock = new Mock<IRobotClient>();
        _parser = new RobotsTxtParser(_robotsClientMock.Object);
    }

    [Fact]
    public async Task ReadFromStreamAsync_EmptyFile_LoadDefault()
    {
        // Arrange
        var file = "";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Assert
        robotsTxt.Should().NotBe(null);
    }

    [Fact]
    public async Task ReadFromStreamAsync_WithLineComments_CommentsIgnored()
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
    public async Task ReadFromStreamAsync_WithEndOfLineComments_CommentsIgnored()
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
    public async Task ReadFromStreamAsync_Under50KiB_DoNotThrow()
    {
        // Arrange
        var fileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
        var stream = fileProvider.GetFileInfo("under-50kib-robots.txt").CreateReadStream();

        // Act
        var parse = async () => await _parser.ReadFromStreamAsync(stream);

        // Assert
        await parse.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ReadFromStreamAsync_Over50KiB_ThrowOutOfMemoryException()
    {
        // Arrange
        var fileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
        var stream = fileProvider.GetFileInfo("over-50kib-robots.txt").CreateReadStream();

        // Act
        var parse = async () => await _parser.ReadFromStreamAsync(stream);

        // Assert
        await parse.Should().ThrowAsync<OutOfMemoryException>();
    }

    [Fact]
    public async Task ReadFromStreamAsync_InvalidProductToken_Ignore()
    {
        // Arrange
        var file =
@"User-agent: *
Disallow: /

User-agent: InvalidProductToken5
Disallow: 

User-agent: ValidProductToken
Disallow: 
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(file));

        // Act
        var robotsTxt = await _parser.ReadFromStreamAsync(stream);

        // Assert
        robotsTxt.Should().NotBe(null);
    }
}
