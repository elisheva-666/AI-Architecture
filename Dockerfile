# ─── Stage 1: Build ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file first and restore (layer is cached unless csproj changes)
COPY ["final final api/ChineseAuction.Api/ChineseAuction.Api.csproj", "ChineseAuction.Api/"]
RUN dotnet restore "ChineseAuction.Api/ChineseAuction.Api.csproj"

# Copy the rest of the source and publish
COPY "final final api/ChineseAuction.Api/" "ChineseAuction.Api/"
WORKDIR "/src/ChineseAuction.Api"
RUN dotnet publish "ChineseAuction.Api.csproj" -c Release -o /app/publish --no-restore

# ─── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create directories the app writes to at runtime
RUN mkdir -p /app/logs /app/wwwroot /app/Reports

# Port 8080 is the default for ASP.NET Core 8 in containers
EXPOSE 8080

COPY --from=build /app/publish .

# Override the connection string at runtime via environment variable:
#   docker run -e ConnectionStrings__DefaultConnection="Server=host.docker.internal;..."
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ChineseAuction.Api.dll"]
