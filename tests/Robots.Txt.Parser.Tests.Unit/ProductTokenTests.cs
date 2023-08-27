using System;
using FluentAssertions;
using Xunit;

namespace Robots.Txt.Parser.Tests.Unit;

public class ProductTokenTests
{
    [Fact]
    public void Parse_Empty_ThrowArgumentOutOfRangeException()
    {
        // Arrange
        var value = "";

        // Act
        var parse = () => ProductToken.Parse(value);

        // Assert
        parse.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Parse_WildcardCharacterAsSubstring_ThrowArgumentOutOfRangeException()
    {
        // Arrange
        var value = "*SomeBot";

        // Act
        var parse = () => ProductToken.Parse(value);

        // Assert
        parse.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Parse_InvalidCharacter_ThrowArgumentOutOfRangeException()
    {
        // Arrange
        var value = "SomeBot1";

        // Act
        var parse = () => ProductToken.Parse(value);

        // Assert
        parse.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Parse_Wildcard_DoesNotThrow()
    {
        // Arrange
        var value = "*";

        // Act
        var parse = () => ProductToken.Parse(value);

        // Assert
        parse.Should().NotThrow();
    }

    [Fact]
    public void Parse_ValidProductToken_DoesNotThrow()
    {
        // Arrange
        var value = "S-o-m-e_B-o-t";

        // Act
        var parse = () => ProductToken.Parse(value);

        // Assert
        parse.Should().NotThrow();
    }

    [Fact]
    public void TryParse_Empty_ReturnFalse()
    {
        // Arrange
        var value = "";

        // Act
        var canParse = ProductToken.TryParse(value, out var productToken);

        // Assert
        canParse.Should().Be(false);
        productToken.Should().Be(ProductToken.Wildcard);
    }

    [Fact]
    public void TryParse_WildcardCharacterAsSubstring_ReturnFalse()
    {
        // Arrange
        var value = "*SomeBot";

        // Act
        var canParse = ProductToken.TryParse(value, out var productToken);

        // Assert
        canParse.Should().Be(false);
        productToken.Should().Be(ProductToken.Wildcard);
    }

    [Fact]
    public void TryParse_InvalidCharacter_ReturnFalse()
    {
        // Arrange
        var value = "SomeBot1";

        // Act
        var canParse = ProductToken.TryParse(value, out var productToken);

        // Assert
        canParse.Should().Be(false);
        productToken.Should().Be(ProductToken.Wildcard);
    }

    [Fact]
    public void TryParse_Wildcard_ReturnTrue()
    {
        // Arrange
        var value = "*";

        // Act
        var canParse = ProductToken.TryParse(value, out var productToken);

        // Assert
        canParse.Should().Be(true);
        productToken.Should().Be(ProductToken.Wildcard);
    }

    [Fact]
    public void TryParse_ValidProductToken_ReturnTrue()
    {
        // Arrange
        var value = "S-o-m-e_B-o-t";

        // Act
        var canParse = ProductToken.TryParse(value, out var productToken);

        // Assert
        canParse.Should().Be(true);
        productToken.Should().Be(ProductToken.Parse(value));
    }

    [Fact]
    public void Equals_SpecificTokenAndWildcard_NotEqual()
    {
        // Arrange
        var a = ProductToken.Wildcard;
        var bString = "SomeBot";
        var b = ProductToken.Parse(bString);

        // Act
        var isEqualProductToken = a.Equals(b);
        var isEqualString = a.Equals(bString);
        var isEqualObject = a.Equals((object)b);
        var isHashCodeEqual = a.GetHashCode() == b.GetHashCode();

        // Assert
        isEqualProductToken.Should().Be(false);
        isEqualString.Should().Be(false);
        isEqualObject.Should().Be(false);
        isHashCodeEqual.Should().Be(false);
    }

    [Fact]
    public void Equals_SpecificTokenAndOtherSpecificToken_NotEqual()
    {
        // Arrange
        var a = ProductToken.Parse("SomeBot");
        var bString = "AnotherBot";
        var b = ProductToken.Parse(bString);

        // Act
        var isEqual = a.Equals(b);
        var isEqualString = a.Equals(bString);
        var isEqualObject = a.Equals((object)b);
        var isHashCodeEqual = a.GetHashCode() == b.GetHashCode();

        // Assert
        isEqual.Should().Be(false);
        isEqualString.Should().Be(false);
        isEqualObject.Should().Be(false);
        isHashCodeEqual.Should().Be(false);
    }

    [Fact]
    public void Equals_WildcardAndWildcard_Equal()
    {
        // Arrange
        var a = ProductToken.Wildcard;
        var bString = "*";
        var b = ProductToken.Parse(bString);

        // Act
        var isEqual = a.Equals(b);
        var isEqualString = a.Equals(bString);
        var isEqualObject = a.Equals((object)b);
        var isHashCodeEqual = a.GetHashCode() == b.GetHashCode();

        // Assert
        isEqual.Should().Be(true);
        isEqualString.Should().Be(true);
        isEqualObject.Should().Be(true);
        isHashCodeEqual.Should().Be(true);
    }

    [Fact]
    public void Equals_SpecificTokenAndSpecificTokenSameCase_Equal()
    {
        // Arrange
        var a = ProductToken.Parse("SomeBot");
        var bString = "SomeBot";
        var b = ProductToken.Parse(bString);

        // Act
        var isEqualProductToken = a.Equals(b);
        var isEqualString = a.Equals(bString);
        var isEqualObject = a.Equals((object)b);
        var isHashCodeEqual = a.GetHashCode() == b.GetHashCode();

        // Assert
        isEqualProductToken.Should().Be(true);
        isEqualString.Should().Be(true);
        isEqualObject.Should().Be(true);
        isHashCodeEqual.Should().Be(true);
    }

    [Fact]
    public void Equals_SpecificTokenAndSpecificTokenDifferentCase_Equal()
    {
        // Arrange
        var a = ProductToken.Parse("SOMEBot");
        var bString = "SomeBot";
        var b = ProductToken.Parse(bString);

        // Act
        var isEqualProductToken = a.Equals(b);
        var isEqualString = a.Equals(bString);
        var isEqualObject = a.Equals((object)b);
        var isHashCodeEqual = a.GetHashCode() == b.GetHashCode();

        // Assert
        isEqualProductToken.Should().Be(true);
        isEqualString.Should().Be(true);
        isEqualObject.Should().Be(true);
        isHashCodeEqual.Should().Be(true);
    }
}
