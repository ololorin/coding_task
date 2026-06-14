# --- build ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first (better layer caching) using just the project files.
COPY src/CurrencyConverter.Domain/*.csproj src/CurrencyConverter.Domain/
COPY src/CurrencyConverter.Api/*.csproj src/CurrencyConverter.Api/
RUN dotnet restore src/CurrencyConverter.Api/CurrencyConverter.Api.csproj

COPY src/ src/
RUN dotnet publish src/CurrencyConverter.Api/CurrencyConverter.Api.csproj \
    -c Release -o /app/publish --no-restore

# --- runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "CurrencyConverter.Api.dll"]
