using FluentAssertions;
using Xunit;

namespace Robots.Txt.Parser.Tests.Unit;

public class UrlRuleTests
{
    [Fact]
    public void Matches_EmptyRulePath_ReturnFalse()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path");

        // Assert
        matches.Should().Be(false);
    }

    [Fact]
    public void Matches_DifferentPath_ReturnFalse()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path");

        // Act
        var matches = urlRule.Pattern.Matches("/some/other/path");

        // Assert
        matches.Should().Be(false);
    }

    [Fact]
    public void Matches_DirectoryQualifier_ReturnFalse()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path/");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path");

        // Assert
        matches.Should().Be(false);
    }

    [Fact]
    public void Matches_ExactMatch_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_FileMatch_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path.html");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_SubdirectoryMatch_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path/subdirectory");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_OctectBothLowercase_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path%3c");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path%3c");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_OctectBothUppercase_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path%3C");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path%3C");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_OctectRuleLowercasePathUppercase_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path%3c");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path%3C");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_OctectRuleUppercasePathLowercase_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path%3C");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path%3c");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_OctectForwardSlashBothUrl_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path%2F");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path%2F");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_OctectForwardSlashOnlyInRule_ReturnFalse()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path%2F");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path/");

        // Assert
        matches.Should().Be(false);
    }

    [Fact]
    public void Matches_OctectForwardSlashOnlyInPath_ReturnFalse()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path/");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path%2F");

        // Assert
        matches.Should().Be(false);
    }

    [Fact]
    public void Matches_OctectNotForwardSlashLowercaseOnlyInRule_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path%7e");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path~");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_OctectNotForwardSlashLowercaseOnlyInPath_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path~");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path%7e");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_OctectNotForwardSlashUppercaseOnlyInRule_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path%7E");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path~");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_OctectNotForwardSlashUppercaseOnlyInPath_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path~");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path%7E");

        // Assert
        matches.Should().Be(true);
    }
}
