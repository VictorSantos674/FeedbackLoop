using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FeedbackLoop.Api.Domain.DTOs.Common;
using FeedbackLoop.Api.Domain.DTOs.Posts;
using FeedbackLoop.Api.Domain.DTOs.Votes;
using FeedbackLoop.Api.Domain.Entities;
using FeedbackLoop.Api.Domain.Enums;
using FeedbackLoop.Api.Infrastructure.Persistence;
using FeedbackLoop.Api.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace FeedbackLoop.Api.Tests.Integration;

public sealed class MultiTenancyTests
{
    private const string JwtSecret = "integration-test-secret-with-enough-length";

    [Fact]
    public async Task GetBoards_ReturnsOnlyCurrentWorkspaceBoards()
    {
        await using var factory = CreateFactory(nameof(GetBoards_ReturnsOnlyCurrentWorkspaceBoards));
        var workspaceA = Guid.NewGuid();
        var workspaceB = Guid.NewGuid();

        await SeedAsync(factory, db =>
        {
            db.Workspaces.AddRange(
                new Workspace { Id = workspaceA, Name = "Workspace A", Slug = "workspace-a" },
                new Workspace { Id = workspaceB, Name = "Workspace B", Slug = "workspace-b" });
            db.Boards.AddRange(
                new Board { Id = Guid.NewGuid(), WorkspaceId = workspaceA, Name = "Board A", Slug = "board-a" },
                new Board { Id = Guid.NewGuid(), WorkspaceId = workspaceB, Name = "Board B", Slug = "board-b" });
        });

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(workspaceA));

        var response = await client.GetAsync("/api/boards");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var boards = await response.Content.ReadFromJsonAsync<List<BoardSummaryDto>>();
        boards.Should().ContainSingle();
        boards![0].Slug.Should().Be("board-a");
    }

    [Fact]
    public async Task Widget_SameSlugDifferentWorkspace_ReturnsCorrectBoard()
    {
        await using var factory = CreateFactory(nameof(Widget_SameSlugDifferentWorkspace_ReturnsCorrectBoard));
        var workspaceA = Guid.NewGuid();
        var workspaceB = Guid.NewGuid();
        var boardA = Guid.NewGuid();
        var boardB = Guid.NewGuid();

        await SeedAsync(factory, db =>
        {
            db.Workspaces.AddRange(
                new Workspace { Id = workspaceA, Name = "Workspace A", Slug = "workspace-a" },
                new Workspace { Id = workspaceB, Name = "Workspace B", Slug = "workspace-b" });
            db.Boards.AddRange(
                new Board { Id = boardA, WorkspaceId = workspaceA, Name = "Roadmap A", Slug = "roadmap", CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Board { Id = boardB, WorkspaceId = workspaceB, Name = "Roadmap B", Slug = "roadmap", CreatedAtUtc = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc) });
            db.Posts.AddRange(
                new Post { Id = Guid.NewGuid(), WorkspaceId = workspaceA, BoardId = boardA, Title = "Post do workspace A", Description = "Desc", EndUserToken = Guid.NewGuid() },
                new Post { Id = Guid.NewGuid(), WorkspaceId = workspaceB, BoardId = boardB, Title = "Post do workspace B", Description = "Desc", EndUserToken = Guid.NewGuid() });
        });

        var response = await factory.CreateClient().GetAsync($"/api/widget/roadmap/posts?endUserToken={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<PostResponse>>();
        result!.Items.Should().ContainSingle(post => post.Title == "Post do workspace A");
        result.Items.Should().NotContain(post => post.Title == "Post do workspace B");
    }

    [Fact]
    public async Task GetPost_PostFromDifferentBoard_ReturnsNotFound()
    {
        await using var factory = CreateFactory(nameof(GetPost_PostFromDifferentBoard_ReturnsNotFound));
        var workspaceId = Guid.NewGuid();
        var boardA = Guid.NewGuid();
        var boardB = Guid.NewGuid();
        var postB = Guid.NewGuid();

        await SeedAsync(factory, db =>
        {
            db.Workspaces.Add(new Workspace { Id = workspaceId, Name = "Workspace", Slug = "workspace" });
            db.Boards.AddRange(
                new Board { Id = boardA, WorkspaceId = workspaceId, Name = "Board A", Slug = "board-a" },
                new Board { Id = boardB, WorkspaceId = workspaceId, Name = "Board B", Slug = "board-b" });
            db.Posts.Add(new Post { Id = postB, WorkspaceId = workspaceId, BoardId = boardB, Title = "Post board B", Description = "Desc", EndUserToken = Guid.NewGuid() });
        });

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(workspaceId));

        var response = await client.GetAsync($"/api/boards/{boardA}/posts/{postB}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ToggleVote_PostFromDifferentBoard_ReturnsNotFound()
    {
        await using var factory = CreateFactory(nameof(ToggleVote_PostFromDifferentBoard_ReturnsNotFound));
        var workspaceId = Guid.NewGuid();
        var boardA = Guid.NewGuid();
        var boardB = Guid.NewGuid();
        var postB = Guid.NewGuid();

        await SeedAsync(factory, db =>
        {
            db.Workspaces.Add(new Workspace { Id = workspaceId, Name = "Workspace", Slug = "workspace" });
            db.Boards.AddRange(
                new Board { Id = boardA, WorkspaceId = workspaceId, Name = "Board A", Slug = "board-a" },
                new Board { Id = boardB, WorkspaceId = workspaceId, Name = "Board B", Slug = "board-b" });
            db.Posts.Add(new Post { Id = postB, WorkspaceId = workspaceId, BoardId = boardB, Title = "Post board B", Description = "Desc", EndUserToken = Guid.NewGuid() });
        });

        var response = await factory.CreateClient().PostAsJsonAsync(
            $"/api/widget/board-a/posts/{postB}/vote",
            new ToggleVoteRequest(Guid.NewGuid().ToString()));
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.NotFound, body);
    }

    private static WebApplicationFactory<Program> CreateFactory(string databaseName)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("Jwt:Secret", JwtSecret);
                builder.UseSetting("Jwt:Issuer", "feedbackloop-api");
                builder.UseSetting("Jwt:Audience", "feedbackloop-clients");
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<DbContextOptions<AppDbContext>>();
                    services.RemoveAll<ITransactionRunner>();
                    services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(databaseName));
                    services.AddScoped<ITransactionRunner, TestTransactionRunner>();
                });
            });
    }

    private static async Task SeedAsync(WebApplicationFactory<Program> factory, Action<AppDbContext> seed)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        seed(db);
        await db.SaveChangesAsync();
    }

    private static string CreateToken(Guid workspaceId, UserRole role = UserRole.Member)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim("role", role.ToString()),
            new Claim("workspaceId", workspaceId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "feedbackloop-api",
            audience: "feedbackloop-clients",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed record BoardSummaryDto(
        Guid Id,
        string Name,
        string Slug,
        int PostCount,
        int OpenPostCount,
        int DonePostCount,
        int VoteCount,
        DateTime CreatedAt);

    private sealed class TestTransactionRunner : ITransactionRunner
    {
        public Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
        {
            return action(cancellationToken);
        }
    }
}
