# ============================================================
# Stage 1: Build
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first (layer-cache friendly)
COPY ["Presentation/Presentation.csproj",   "Presentation/"]
COPY ["Application/Application.csproj",     "Application/"]
COPY ["Domain/Domain.csproj",               "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj","Infrastructure/"]

# Restore dependencies
RUN dotnet restore "Presentation/Presentation.csproj"

# Copy the full source
COPY . .

# Publish the Presentation project in Release mode
RUN dotnet publish "Presentation/Presentation.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# ============================================================
# Stage 2: Runtime
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create the uploads directory so the volume mount has a base
RUN mkdir -p wwwroot/uploads/profiles

# Copy published output from build stage
# This includes Seed/*.json which Infrastructure copies to output via CopyToOutputDirectory
COPY --from=build /app/publish .

# Explicitly copy seed files — dotnet publish does not guarantee content files
# from referenced projects land in the output. AppContext.BaseDirectory/Seed/ is
# the first path DataSeeder.ResolveSeedPath() checks.
COPY --from=build /src/Infrastructure/Persistence/Seed ./Seed

EXPOSE 8080

# Use HTTP only inside the container; HTTPS is handled by the reverse proxy / host
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker

ENTRYPOINT ["dotnet", "Presentation.dll"]
