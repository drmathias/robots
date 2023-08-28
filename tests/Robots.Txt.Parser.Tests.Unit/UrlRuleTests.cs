using System;
using System.Text;
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
    public void Matches_PercentEncodedCharacterBothLowercase_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path%3c");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path%3c");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_PercentEncodedCharacterBothUppercase_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path%3C");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path%3C");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_PercentEncodedCharacterRuleLowercasePathUppercase_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path%3c");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path%3C");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_PercentEncodedCharacterRuleUppercasePathLowercase_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path%3C");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path%3c");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_PercentEncodedCharacterForwardSlashBothUrl_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path%2F");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path%2F");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_PercentEncodedCharacterForwardSlashOnlyInRule_ReturnFalse()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path%2F");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path/");

        // Assert
        matches.Should().Be(false);
    }

    [Fact]
    public void Matches_PercentEncodedCharacterForwardSlashOnlyInPath_ReturnFalse()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path/");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path%2F");

        // Assert
        matches.Should().Be(false);
    }

    [Fact]
    public void Matches_PercentEncodedCharacterAsteriskBothUrl_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some%2Apath");

        // Act
        var matches = urlRule.Pattern.Matches("/some%2Apath");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_PercentEncodedCharacterAsteriskOnlyInRule_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some%2Apath");

        // Act
        var matches = urlRule.Pattern.Matches("/some*path");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_PercentEncodedCharacterAsteriskOnlyInPath_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some*path");

        // Act
        var matches = urlRule.Pattern.Matches("/some%2Apath");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_PercentEncodedCharacterReservedBothUrl_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some%24path");

        // Act
        var matches = urlRule.Pattern.Matches("/some%24path");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_PercentEncodedCharacterReservedOnlyInRule_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some%24path");

        // Act
        var matches = urlRule.Pattern.Matches("/some$path");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_PercentEncodedCharacterReservedOnlyInPath_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some$path");

        // Act
        var matches = urlRule.Pattern.Matches("/some%24path");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_PercentEncodedCharacterNotSpecialLowercaseOnlyInRule_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path%7e");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path~");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_PercentEncodedCharacterNotSpecialLowercaseOnlyInPath_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path~");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path%7e");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_PercentEncodedCharacterNotSpecialUppercaseOnlyInRule_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path%7E");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path~");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_PercentEncodedCharacterNotSpecialUppercaseOnlyInPath_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/some/path~");

        // Act
        var matches = urlRule.Pattern.Matches("/some/path%7E");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_UnescapedQueryStringInRuleAndPath_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/foo/bar?baz=https://foo.bar");

        // Act
        var matches = urlRule.Pattern.Matches("/foo/bar?baz=https://foo.bar");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_UnescapedQueryStringInRuleButPathEscaped_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/foo/bar?baz=https://foo.bar");

        // Act
        var matches = urlRule.Pattern.Matches("/foo/bar?baz=https%3A%2F%2Ffoo.bar");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_UnescapedQueryStringInPathButRuleEscaped_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/foo/bar?baz=https%3A%2F%2Ffoo.bar");

        // Act
        var matches = urlRule.Pattern.Matches("/foo/bar?baz=https://foo.bar");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_ExistingUnencodedUtf8Character_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/foo/bar/ツ");

        // Act
        var matches = urlRule.Pattern.Matches("/foo/bar/ツ");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_ExistingEncodedUtf8Character_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/foo/bar/%E3%83%84");

        // Act
        var matches = urlRule.Pattern.Matches("/foo/bar/%E3%83%84");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_ExistingEncodedUtf8CharacterRuleOnly_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/foo/bar/ツ");

        // Act
        var matches = urlRule.Pattern.Matches("/foo/bar/%E3%83%84");

        // Assert
        matches.Should().Be(true);
    }

    [Fact]
    public void Matches_ExistingEncodedUtf8CharacterPathOnly_ReturnTrue()
    {
        // Arrange
        var urlRule = new UrlRule(RuleType.Disallow, "/foo/bar/%E3%83%84");

        // Act
        var matches = urlRule.Pattern.Matches("/foo/bar/ツ");

        // Assert
        matches.Should().Be(true);
    }
}
