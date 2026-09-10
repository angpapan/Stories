using InteractiveStory.Api.Data;
using InteractiveStory.Api.Endpoints.Admin;
using InteractiveStory.Api.Endpoints.Player;
using InteractiveStory.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Authentication
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized;
            return System.Threading.Tasks.Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

// Configure DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=InteractiveStory.db";
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

// Configure services
builder.Services.AddSingleton<StoryCacheService>();

var app = builder.Build();

// Enable WAL mode at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Serve static files for media (and the react app if in wwwroot)
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Register endpoints
app.MapAdminEndpoints();
app.MapPlayerEndpoints();

// Fallback for React Router (SPA)
app.MapFallbackToFile("index.html");

app.Run();
