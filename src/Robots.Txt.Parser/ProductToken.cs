using System;
using System.Text.RegularExpressions;

namespace Robots.Txt.Parser;

/// <summary>
/// Crawler name, used as the User-agent value within a robots.txt file
/// </summary>
public partial class ProductToken : IEquatable<string>, IEquatable<ProductToken>
{
    public static readonly ProductToken Wildcard = new("*");
    private static readonly Regex ValidationPattern = ProductTokenValidationRegex();

    private readonly string _value;

    private ProductToken(string value) => _value = value;

    /// <summary>
    /// Parses a <see cref="ProductToken"/>
    /// </summary>
    /// <param name="value">Raw product token value</param>
    /// <returns><see cref="ProductToken"/> that identifies a robot rule group</returns>
    /// <exception cref="ArgumentOutOfRangeException">Product token is formatted incorrectly</exception>
    public static ProductToken Parse(string value)
    {
        if (value != Wildcard._value && !ValidationPattern.IsMatch(value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                "Must contain only uppercase and lowercase letters (\"a-z\" and \"A-Z\"), underscores (\"_\"), and hyphens (\"-\")");
        }

        return new ProductToken(value);
    }

    /// <summary>
    /// Attempts to parse a <see cref="ProductToken"/>
    /// </summary>
    /// <param name="value">Raw product token value</param>
    /// <param name="productToken">Parsed product token, or wildcard if the token could not be parsed</param>
    /// <returns>True if the <see cref="ProductToken"/> could be parsed; otherwise false</returns>
    public static bool TryParse(string value, out ProductToken productToken)
    {
        productToken = Wildcard;
        if (value != Wildcard._value && !ValidationPattern.IsMatch(value)) return false;
        productToken = new ProductToken(value);
        return true;
    }

    /*
      Crawlers set their own name, which is called a product token, to find relevant groups.
      The product token MUST contain only uppercase and lowercase letters ("a-z" and "A-Z"), underscores ("_"), and hyphens ("-").
    */
    [GeneratedRegex("^[a-zA-Z-_]+$", RegexOptions.Compiled)]
    private static partial Regex ProductTokenValidationRegex();

    /*
      Crawlers MUST use case-insensitive matching to find the group that matches the product token and then obey the rules of the group.
    */

    /// <summary>
    /// Assesses product token equality. Product tokens are case-insensitive.
    /// </summary>
    /// <param name="obj">Comparison value</param>
    /// <returns>True if the product token is equal; otherwise false</returns>
    public override bool Equals(object? obj) =>
        obj is string otherString
            ? Equals(otherString)
            : obj is ProductToken otherToken && Equals(otherToken);

    /// <summary>
    /// Assesses product token equality. Product tokens are case-insensitive.
    /// </summary>
    /// <param name="other">Comparison value</param>
    /// <returns>True if the product token is equal; otherwise false</returns>
    public bool Equals(string? other) => other is not null && _value.Equals(other, StringComparison.InvariantCultureIgnoreCase);

    /// <summary>
    /// Assesses product token equality. Product tokens are case-insensitive.
    /// </summary>
    /// <param name="other">Comparison value</param>
    /// <returns>True if the product token is equal; otherwise false</returns>
    public bool Equals(ProductToken? other) =>
        other is not null && _value.Equals(other._value, StringComparison.InvariantCultureIgnoreCase);

    /// <inheritdoc />
    public override int GetHashCode() => _value.ToUpperInvariant().GetHashCode();

    /// <inheritdoc />
    public override string ToString() => _value;
}
