# Build context must be the repository root:
#   docker build -t lab-tracker-api -f LabConsumableExpiryTracker.API/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first for layer caching; the API project pulls in
# Application + Infrastructure via ProjectReferences.
COPY LabConsumableExpiryTracker.slnx global.json ./
COPY LabConsumableExpiryTracker.API/LabConsumableExpiryTracker.API.csproj LabConsumableExpiryTracker.API/
COPY LabConsumableExpiryTracker.Application/LabConsumableExpiryTracker.Application.csproj LabConsumableExpiryTracker.Application/
COPY LabConsumableExpiryTracker.Domain/LabConsumableExpiryTracker.Domain.csproj LabConsumableExpiryTracker.Domain/
COPY LabConsumableExpiryTracker.Infrastructure/LabConsumableExpiryTracker.Infrastructure.csproj LabConsumableExpiryTracker.Infrastructure/
RUN dotnet restore LabConsumableExpiryTracker.API/LabConsumableExpiryTracker.API.csproj

COPY LabConsumableExpiryTracker.API/ LabConsumableExpiryTracker.API/
COPY LabConsumableExpiryTracker.Application/ LabConsumableExpiryTracker.Application/
COPY LabConsumableExpiryTracker.Domain/ LabConsumableExpiryTracker.Domain/
COPY LabConsumableExpiryTracker.Infrastructure/ LabConsumableExpiryTracker.Infrastructure/
RUN dotnet publish LabConsumableExpiryTracker.API/LabConsumableExpiryTracker.API.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
RUN mkdir -p /app/Storage/Images /app/logs && chown -R app:app /app
USER app
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=90s --retries=5 \
    CMD ["curl", "--fail", "--silent", "http://localhost:8080/health"]
ENTRYPOINT ["dotnet", "LabConsumableExpiryTracker.API.dll"]
