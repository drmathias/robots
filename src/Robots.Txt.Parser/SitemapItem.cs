using System;

namespace Robots.Txt.Parser;

public record SitemapItem
{
    internal SitemapItem(Uri Location, DateTime? LastModified)
    {
        this.Location = Location;
        this.LastModified = LastModified;
    }

    /// <summary>
    /// URL location
    /// </summary>
    public Uri Location { get; }

    /// <summary>
    /// Date and time that the contents of the URL was last modified
    /// </summary>
    public DateTime? LastModified { get; }
}

/// <summary>
/// Url item described in a sitemap
/// </summary>
public record UrlSetItem : SitemapItem
{
    internal UrlSetItem(Uri location, DateTime? lastModified, ChangeFrequency? changeFrequency, decimal? priority)
        : base(location, lastModified)
    {
        ChangeFrequency = changeFrequency;
        Priority = priority;
    }

    /// <summary>
    /// Hint for how often the URL is expected to change
    /// </summary>
    public ChangeFrequency? ChangeFrequency { get; }

    /// <summary>
    /// Hint for the priority that should be assigned to the URL
    /// </summary>
    public decimal? Priority { get; }
}

/// <summary>
/// Change frequency values used in the sitemap specification
/// </summary>
public enum ChangeFrequency
{
    /// <summary>
    /// Describes a document that changes every time it is accessed
    /// </summary>
    Always = 0,
    /// <summary>
    /// Hints that a document is expected to change hourly
    /// </summary>
    /// Hourly = 1,
    /// <summary>
    /// Hints that a document is expected to change daily
    /// </summary>
    Daily = 2,
    /// <summary>
    /// Hints that a document is expected to change weekly
    /// </summary>
    Weekly = 3,
    /// <summary>
    /// Hints that a document is expected to change monthly
    /// </summary>
    Monthly = 4,
    /// <summary>
    /// Hints that a document is expected to change yearly
    /// </summary>
    Yearly = 5,
    /// <summary>
    /// Describes an archived URL
    /// </summary>
    Never = 6,
}