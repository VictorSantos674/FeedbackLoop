# FeedbackLoop

SaaS B2B para coleta de feedback, votacao de sugestoes e gestao colaborativa de roadmap.

## Estrutura inicial

- `FeedbackLoop.Api`: ASP.NET Core Web API em .NET 8, com dominio, application services, infraestrutura, repositorios e controllers separados por pasta.
- `feedbackloop-app`: painel administrativo em React 18 + TypeScript + Vite.
- `feedbackloop-widget`: widget embarcavel em React + Vite, configurado para gerar bundle UMD.

O SDK .NET esta fixado em `global.json` para `8.0.420`.

## Rodando a API

```powershell
dotnet restore
dotnet run --project FeedbackLoop.Api
```

Em desenvolvimento, a documentacao Swagger fica disponivel em `/swagger`.

Configure a connection string `FeedbackLoopDb` em `FeedbackLoop.Api/appsettings.Development.json` ou via variavel de ambiente:

```powershell
$env:ConnectionStrings__FeedbackLoopDb="Host=localhost;Port=5432;Database=feedbackloop;Username=postgres;Password=postgres"
```

## Migrations

```powershell
dotnet tool install --global dotnet-ef
dotnet ef database update --project FeedbackLoop.Api
```

A migration atual versionada e `AddRefreshTokens`.

## Frontend

```powershell
cd feedbackloop-app
npm install
npm run dev
```

## Widget

```powershell
cd feedbackloop-widget
npm install
npm run build
```

O build do widget gera um bundle UMD em `feedbackloop-widget/dist`.

## Endpoints de autenticacao

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`

## Endpoints do produto

- `GET /api/boards`
- `GET /api/boards/{id}`
- `POST /api/boards`
- `PUT /api/boards/{id}`
- `DELETE /api/boards/{id}`
- `GET /api/boards/{boardId}/posts`
- `GET /api/boards/{boardId}/posts/{postId}`
- `PATCH /api/boards/{boardId}/posts/{postId}/status`
- `GET /api/boards/{boardId}/posts/{postId}/history`

## Endpoints publicos do widget

- `GET /api/widget/{boardSlug}/posts`
- `POST /api/widget/{boardSlug}/posts`
- `POST /api/widget/{boardSlug}/posts/{postId}/vote`
