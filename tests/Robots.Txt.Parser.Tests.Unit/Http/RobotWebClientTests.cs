using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Robots.Txt.Parser.Http;
using Xunit;

namespace Robots.Txt.Parser.Tests.Unit.Http;

public class RobotWebClientTests
{
    [Fact]
    public void RobotWebClient_TWebsite_SetBaseUrl()
    {
        // Arrange
        var httpClientMock = new Mock<HttpClient>();

        // Act
        IRobotClient robotWebClient = new RobotWebClient<GitHubWebsite>(httpClientMock.Object);

        // Assert
        robotWebClient.BaseAddress.Should().Be(GitHubWebsite.BaseAddress);
    }

    [Theory]
    [InlineData(500)]
    [InlineData(501)]
    [InlineData(503)]
    [InlineData(599)]
    public async Task LoadRobotsTxtAsync_5XXResponse_AssumeDisallowAll(int statusCode)
    {
        // Arrange
        var httpClientHandlerMock = new Mock<HttpClientHandler>();
        using var httpClient = new HttpClient(httpClientHandlerMock.Object);
        var robotWebClient = new RobotWebClient<GitHubWebsite>(httpClient);

        httpClientHandlerMock.SetupToRespondWith((HttpStatusCode)statusCode);

        // Act
        var robotsTxt = await robotWebClient.LoadRobotsTxtAsync();

        // Assert
        robotsTxt.Should().NotBe(null);
        var hasRules = robotsTxt.TryGetRules(ProductToken.Parse("SomeBot"), out var rules);
        hasRules.Should().Be(true);
        rules.IsAllowed("/").Should().Be(false);
    }

    [Theory]
    [InlineData(400)]
    [InlineData(401)]
    [InlineData(404)]
    [InlineData(499)]
    public async Task LoadRobotsTxtAsync_4XXResponse_AssumeAllowAll(int statusCode)
    {
        // Arrange
        var httpClientHandlerMock = new Mock<HttpClientHandler>();
        using var httpClient = new HttpClient(httpClientHandlerMock.Object);
        var robotWebClient = new RobotWebClient<GitHubWebsite>(httpClient);

        httpClientHandlerMock.SetupToRespondWith((HttpStatusCode)statusCode);

        // Act
        var robotsTxt = await robotWebClient.LoadRobotsTxtAsync();

        // Assert
        robotsTxt.Should().NotBe(null);
        var hasRules = robotsTxt.TryGetRules(ProductToken.Parse("SomeBot"), out var rules);
        hasRules.Should().Be(false);
        rules.IsAllowed("/").Should().Be(true);
    }

    [Theory]
    [InlineData(200)]
    [InlineData(201)]
    [InlineData(204)]
    [InlineData(299)]
    public async Task LoadRobotsTxtAsync_2XXResponse_ReturnRobotsTxt(int statusCode)
    {
        // Arrange
        var httpClientHandlerMock = new Mock<HttpClientHandler>();
        using var httpClient = new HttpClient(httpClientHandlerMock.Object);
        var robotWebClient = new RobotWebClient<GitHubWebsite>(httpClient);

        httpClientHandlerMock.SetupToRespondWith((HttpStatusCode)statusCode);

        // Act
        var robotsTxt = await robotWebClient.LoadRobotsTxtAsync();

        // Assert
        robotsTxt.Should().NotBeNull(null);
    }
}

public class GitHubWebsite : IWebsiteMetadata
{
    public static Uri BaseAddress => new("https://www.github.com");
}
