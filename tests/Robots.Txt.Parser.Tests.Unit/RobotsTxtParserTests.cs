using System;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.FileProviders;
using Moq;
using Xunit;

namespace Robots.Txt.Parser.Tests.Unit;

public partial class RobotsTxtParserTests
{
    private readonly Mock<IRobotClient> _robotsWebClientMock;
    private readonly RobotsTxtParser _parser;

    public RobotsTxtParserTests()
    {
        _robotsWebClientMock = new Mock<IRobotClient>();
        _parser = new RobotsTxtParser(_robotsWebClientMock.Object);
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
}
