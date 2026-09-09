using System;
using System.Text.RegularExpressions;

namespace InteractiveStory.Api.Services;

public static class SlugGenerator
{
    public static string Generate(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Guid.NewGuid().ToString("N");

        string slug = title.ToLowerInvariant();
        // Remove invalid chars
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        // Convert multiple spaces into one hyphen
        slug = Regex.Replace(slug, @"\s+", "-").Trim('-');

        if (string.IsNullOrEmpty(slug))
            return Guid.NewGuid().ToString("N");

        // Add a short guid to guarantee uniqueness
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 6);
        return $"{slug}-{suffix}";
    }
}
