FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY FCG.CatalogAPI.API/FCG.CatalogAPI.API.csproj FCG.CatalogAPI.API/
COPY FCG.CatalogAPI.Application/FCG.CatalogAPI.Application.csproj FCG.CatalogAPI.Application/
COPY FCG.CatalogAPI.Domain/FCG.CatalogAPI.Domain.csproj FCG.CatalogAPI.Domain/
COPY FCG.CatalogAPI.Infrastructure/FCG.CatalogAPI.Infrastructure.csproj FCG.CatalogAPI.Infrastructure/

# Restaura so a API: a solucao inclui o projeto de testes, que nao entra na imagem.
RUN dotnet restore FCG.CatalogAPI.API/FCG.CatalogAPI.API.csproj

COPY . .

# Publica o projeto da API, nao a solucao: a solucao agora inclui o projeto de testes,
# que nao deve entrar na imagem.
RUN dotnet publish FCG.CatalogAPI.API/FCG.CatalogAPI.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "FCG.CatalogAPI.API.dll"]
