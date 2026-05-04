# FeedbackLoop

Ferramenta B2B para coletar feedback de usuarios e gerenciar o roadmap de produto, uma alternativa simples e acessivel ao Canny.

## Como Funciona

Empresas instalam um widget JavaScript no proprio produto:

```html
<script src="https://cdn.feedbackloop.app/widget.umd.js"></script>
<script>
  FeedbackLoop.init({
    boardSlug: "meu-produto",
    endUserToken: "uuid-do-usuario",
    endUserName: "Joao Silva",
    apiBaseUrl: "https://api.feedbackloop.app"
  });
</script>
```

Os usuarios finais sugerem features e votam. A equipe gerencia tudo pelo painel administrativo.

## Stack

| Camada | Tecnologia |
| --- | --- |
| Backend | .NET 8, ASP.NET Core, EF Core, PostgreSQL |
| Painel | React 18, TypeScript, TanStack Query, Zustand |
| Widget | React 18, Vite, bundle UMD self-contained |
| Auth | JWT + Refresh Token com rotacao e deteccao de reutilizacao |
| Testes | xUnit + Moq, Vitest + Testing Library |

## Rodar Localmente Com Docker

```bash
cp .env.example .env
docker compose up --build
```

Acesse:

- Painel: http://localhost:3000
- API + Swagger: http://localhost:5000/swagger

## Rodar Sem Docker

### API

```bash
export ConnectionStrings__FeedbackLoopDb="Host=localhost;Database=feedbackloop;Username=postgres;Password=postgres"
export Jwt__Secret="segredo-forte-aqui-com-32-caracteres"

cd FeedbackLoop.Api
dotnet run
```

### Painel

```bash
cd feedbackloop-app
npm install
npm run dev
```

### Widget

```bash
cd feedbackloop-widget
npm install
npm run build
```

Depois abra `feedbackloop-widget/test.html` no navegador.

## Testes

```bash
# API (16 testes)
dotnet test FeedbackLoop.sln

# Painel (9 testes)
cd feedbackloop-app && npm run test

# Widget (9 testes)
cd feedbackloop-widget && npm run test
```

## Arquitetura

### Multi-Tenancy

Cada empresa cliente e um `Workspace`. O `WorkspaceId` e extraido do JWT e aplicado como filtro global no EF Core. Dados de workspaces diferentes nao se cruzam nas queries autenticadas.

### Widget Embarcavel

Bundle UMD self-contained com React embarcado, Shadow DOM para isolamento total de CSS e API publica `FeedbackLoop.init`. O produto cliente nao precisa instalar dependencias.

### Seguranca de Autenticacao

- Refresh tokens salvos como SHA-256 hash no banco
- Rotacao obrigatoria a cada uso
- Deteccao de reutilizacao: token revogado usado revoga a familia inteira
- Access token do painel fica apenas em memoria no Zustand, nunca no `localStorage`
- Refresh token persiste no `localStorage` para sobreviver ao reload

### Painel Administrativo

O painel usa filtros refletidos na URL via `useSearchParams`, cache com TanStack Query, mutations para alteracao de status e interceptor Axios com fila de requests enquanto o refresh token e renovado.

## Endpoints Principais

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `GET /api/boards`
- `POST /api/boards`
- `GET /api/boards/{boardId}/posts`
- `PATCH /api/boards/{boardId}/posts/{postId}/status`
- `GET /api/widget/{boardSlug}/posts`
- `POST /api/widget/{boardSlug}/posts`
- `POST /api/widget/{boardSlug}/posts/{postId}/vote`

## Deploy no Railway

Ordem recomendada:

1. Crie o PostgreSQL no Railway.
2. Publique a API usando `FeedbackLoop.Api/Dockerfile`.
3. Configure as variaveis da API:

```env
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__FeedbackLoopDb=Host=...;Database=...;Username=...;Password=...
Jwt__Secret=troque-por-um-segredo-forte-com-32-caracteres
Jwt__Issuer=feedbackloop-api
Jwt__Audience=feedbackloop-clients
Cors__AllowedOrigins__0=https://url-do-painel.up.railway.app
```

4. Publique o painel usando `feedbackloop-app/Dockerfile`.
5. Configure `VITE_API_BASE_URL` no painel antes do build:

```env
VITE_API_BASE_URL=https://url-da-api.up.railway.app
```

6. Depois que o painel estiver no ar, volte na API e confirme que `Cors__AllowedOrigins__0` aponta para a URL final do painel.

As migrations do EF Core sao aplicadas automaticamente no startup da API, inclusive em `Production`.

## Roadmap

- [ ] Notificacoes por e-mail ao mudar status de um post
- [ ] Comentarios da equipe nos posts
- [ ] Analytics de votos por periodo
- [ ] Integracao com Slack via webhook
- [ ] Plano de billing com Stripe
- [ ] Deploy one-click no Railway
