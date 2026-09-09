using System;
using InteractiveStory.Api.Services;
using Xunit;

namespace InteractiveStory.Tests;

public class SlugGeneratorTests
{
    [Fact]
    public void Generate_ValidTitle_ReturnsSlugWithGuid()
    {
        var title = "The Haunted House";
        var slug = SlugGenerator.Generate(title);
        
        Assert.StartsWith("the-haunted-house-", slug);
        Assert.True(slug.Length > "the-haunted-house-".Length);
    }

    [Fact]
    public void Generate_TitleWithSpecialChars_RemovesThem()
    {
        var title = "Hello World! @2023";
        var slug = SlugGenerator.Generate(title);

        Assert.StartsWith("hello-world-2023-", slug);
    }

    [Fact]
    public void Generate_EmptyTitle_ReturnsGuid()
    {
        var title = "";
        var slug = SlugGenerator.Generate(title);

        Assert.False(string.IsNullOrEmpty(slug));
        Assert.DoesNotContain("-", slug); // N format guid has no hyphens
    }
}
