using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace InteractiveStory.Api.Endpoints.Player;

public static class PlayerEndpoints
{
    public static void MapPlayerEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api");

        group.MapGetStoryInfo();
        group.MapStartPlaythrough();
        group.MapGetCurrentNode();
        group.MapMakeChoice();
        group.MapGetPlaythroughHistory();
    }
}
