using System.Collections.Concurrent;
using System.Text.Json;
using InteractiveStory.Api.Data;
using InteractiveStory.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace InteractiveStory.Api.Services;

public class StoryCacheService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentDictionary<string, StoryDefinition> _cache = new();

    public StoryCacheService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<StoryDefinition?> GetStoryDefinitionAsync(string storyId)
    {
        if (_cache.TryGetValue(storyId, out var cachedDef))
        {
            return cachedDef;
        }

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var story = await db.Stories.AsNoTracking().FirstOrDefaultAsync(s => s.Id == storyId);
        if (story == null)
            return null;

        var def = JsonSerializer.Deserialize<StoryDefinition>(story.Json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (def != null)
        {
            _cache[storyId] = def;
        }

        return def;
    }

    public void InvalidateCache(string storyId)
    {
        _cache.TryRemove(storyId, out _);
    }
}
