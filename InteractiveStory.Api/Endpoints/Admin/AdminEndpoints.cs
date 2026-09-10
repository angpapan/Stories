using InteractiveStory.Api.Data;
using InteractiveStory.Api.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InteractiveStory.Api.Endpoints.Admin;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder routes)
    {
        var authGroup = routes.MapGroup("/api/admin");

        authGroup.MapPost("/login", async (LoginRequest req, AppDbContext db, HttpContext ctx) =>
        {
            var user = await db.Users.SingleOrDefaultAsync(u => u.Username == "admin" && u.Password == req.Password);
            if (user != null)
            {
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Username) };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                return Results.Ok();
            }
            return Results.Unauthorized();
        });

        authGroup.MapPost("/logout", async (HttpContext ctx) =>
        {
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Ok();
        });

        var group = routes.MapGroup("/api/admin/stories").RequireAuthorization();

        group.MapGetStories();
        group.MapGetStory();
        group.MapGetStoryPlaythroughs();
        group.MapCreateStory();
        group.MapUpdateStory();
        group.MapDeleteStory();
        group.MapUploadStoryMedia();
        group.MapValidateStory();
    }
}

public class LoginRequest
{
    public string Password { get; set; } = string.Empty;
}
