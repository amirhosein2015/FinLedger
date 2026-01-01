# Stage 1: Build Image
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /app

# Copy ONLY the src folder
COPY src/ ./src/

# Using ErrorOnDuplicatePublishOutputFiles=false 
# This is the industry-standard fix for Modular Monoliths with conflicting config files.
RUN dotnet publish "src/Modules/Ledger/FinLedger.Modules.Ledger.Api/FinLedger.Modules.Ledger.Api.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:ErrorOnDuplicatePublishOutputFiles=false

# Stage 2: Runtime Image (Lightweight Alpine)
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .

# Security hardening
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "FinLedger.Modules.Ledger.Api.dll"]
