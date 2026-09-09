# Interactive Story Backend Architecture Principles

This document outlines the core architectural and design principles of the Interactive Story Backend to ensure consistency during development.

## 1. Minimal APIs
The application is built using ASP.NET Core Minimal APIs instead of the traditional MVC / Controllers structure. 
- Endpoints are grouped logically in static classes as extension methods on `IEndpointRouteBuilder`.
- Refer to `PlayerEndpoints.cs` and `AdminEndpoints.cs`.

## 2. Entity Framework Core & SQLite
- We use EF Core as the ORM and SQLite as the relational database.
- For performance under concurrent read/write loads (typical for playthroughs tracking), **WAL (Write-Ahead Logging)** mode is explicitly enabled at startup (`PRAGMA journal_mode=WAL;`).
- Migrations are managed via the `dotnet ef` CLI tools.

## 3. Separation of Concerns (Admin vs. Player)
- **Admin**: Responsible for creating, updating, validating, and deleting story structures. Interacts strictly with the `Story` entity.
- **Player**: Responsible for engaging with the stories. Interacts heavily with `Playthrough` and `PlaythroughHistory` entities.

## 4. JSON-Based Story Definition
- The structure of a story (Nodes, Choices, Links, Ending States) is stored as a serialized JSON string in the `Story.Json` property.
- When traversing a story, the engine deserializes the JSON to read the tree rather than joining dozens of normalized relational tables, significantly reducing query overhead.

## 5. Caching for Performance
- Story Definitions (the deserialized JSON tree) are cached in memory via the `StoryCacheService` to prevent continuous database hits and deserialization penalties during high-volume player interactions.
- Cache invalidation occurs automatically when an Admin updates a story.

## 6. Nullable References & Required Properties
- By default, required strings (like `Title` or `Json`) are initialized with `= null!;` to satisfy the C# compiler, while EF Core makes them required columns in the DB.
- Truly optional business features use nullable primitives (e.g. `string? Password`, `int? MaxPlaythroughs`).

## 7. Security and Validation
- Business logic validation (such as Playthrough Limits or Passwords) is handled directly in the endpoint delegates before altering the database state.
- Media upload endpoints sanitize and validate file extensions before writing to disk.
