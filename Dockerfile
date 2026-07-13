FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY FCG.CatalogAPI.sln .
COPY FCG.CatalogAPI.API/FCG.CatalogAPI.API.csproj FCG.CatalogAPI.API/
COPY FCG.CatalogAPI.Application/FCG.CatalogAPI.Application.csproj FCG.CatalogAPI.Application/
COPY FCG.CatalogAPI.Domain/FCG.CatalogAPI.Domain.csproj FCG.CatalogAPI.Domain/
COPY FCG.CatalogAPI.Infrastructure/FCG.CatalogAPI.Infrastructure.csproj FCG.CatalogAPI.Infrastructure/

RUN dotnet restore

COPY . .

RUN dotnet publish FCG.CatalogAPI.sln -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "FCG.CatalogAPI.API.dll"]
