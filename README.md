# FCG Catalog API

Microsserviço responsável pelo catálogo de jogos e biblioteca dos usuários da plataforma FCG (Fiap Cloud Games).

## Funcionalidades

- CRUD de jogos no catálogo
- Biblioteca pessoal de jogos por usuário
- Inicia o fluxo de compra publicando `OrderPlacedEvent`
- Consome `PaymentProcessedEvent` para adicionar jogos à biblioteca

## Tecnologias

- .NET 8 (ASP.NET Core Web API)
- Entity Framework Core + SQL Server
- JWT Bearer Authentication
- MassTransit + RabbitMQ
- Swagger / OpenAPI

## Como executar

### Pré-requisitos

- .NET SDK 8
- SQL Server (local ou container)
- RabbitMQ (local ou container)

### Executar localmente

```bash
# Nenhuma credencial é versionada: a string de conexão e a chave JWT vêm do ambiente
# (no cluster, dos Secrets do Kubernetes). Sem elas a API sobe, mas não conecta no banco.
export ConnectionStrings__DefaultConnection='Server=127.0.0.1;Database=FCG_Catalog;User Id=sa;Password=<sua-senha>;TrustServerCertificate=True'
export Jwt__SecretKey='<a mesma chave usada pelo users-api e pelo Kong>'

dotnet run --project FCG.CatalogAPI.API
```

A API estará disponível em `http://localhost:5085`.

### Com Docker

```bash
docker build -t fcg-catalog-api .
docker run -p 5002:8080 fcg-catalog-api
```

### Com Docker Compose

No repositório [fcg-orchestration](https://github.com/gustavoaa-dev/fcg-orchestration), execute:

```bash
docker-compose up -d
```

## Variáveis de ambiente

O `appsettings.json` **não** carrega senha nem chave JWT: as credenciais vêm só daqui — no cluster, dos Secrets do Kubernetes (ver [fcg-orchestration](https://github.com/gustavoaa-dev/fcg-orchestration), seção *Segredos*).

| Variável | Descrição | Padrão |
|---|---|---|
| `RABBITMQ_HOST` | Host do RabbitMQ | `localhost` |
| `ConnectionStrings__DefaultConnection` | String de conexão SQL Server | — |
| `Jwt__SecretKey` | Chave de assinatura JWT | — |
| `Jwt__Issuer` | Emissor do token JWT | `FCG.UsersAPI` |
| `Jwt__Audience` | Audiência do token JWT | `FCG.Client` |

## Endpoints

| Método | Rota | Autenticação | Descrição |
|---|---|---|---|
| GET | `/api/jogos` | Autenticado | Listar jogos |
| GET | `/api/jogos/{id}` | Autenticado | Detalhes do jogo |
| POST | `/api/jogos` | Admin | Criar jogo |
| DELETE | `/api/jogos/{id}` | Admin | Remover jogo |
| POST | `/api/jogos/{id}/comprar` | Autenticado | Iniciar compra |
| GET | `/api/biblioteca/{userId}` | Autenticado | Ver biblioteca |

## Fluxo de eventos

1. **Publica** `OrderPlacedEvent` ao iniciar uma compra
2. **Consome** `PaymentProcessedEvent` - se aprovado, adiciona o jogo à biblioteca
