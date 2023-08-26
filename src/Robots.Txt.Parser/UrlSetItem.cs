using System;

namespace Robots.Txt.Parser;

/// <summary>
/// Url item described in a sitemap
/// </summary>
/// <param name="Location">URL location</param>
/// <param name="LastModified">Date that the contents of the URL was last modified</param>
/// <param name="ChangeFrequency">Hint for how often the URL is expected to change</param>
/// <param name="Priority">Hint for the priority that should be assigned to the URL</param>
public record UrlSetItem(Uri Location, DateTime? LastModified, ChangeFrequency? ChangeFrequency, decimal? Priority);

internal record SitemapItem(Uri Location, DateTime? LastModified);

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