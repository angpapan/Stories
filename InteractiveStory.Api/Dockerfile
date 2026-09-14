FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["InteractiveStory.Api/InteractiveStory.Api.csproj", "InteractiveStory.Api/"]
RUN dotnet restore "InteractiveStory.Api/InteractiveStory.Api.csproj"

COPY . .
WORKDIR "/src/InteractiveStory.Api"
RUN dotnet build "InteractiveStory.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InteractiveStory.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InteractiveStory.Api.dll"]
