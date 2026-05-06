using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FeedbackLoop.Api.Domain.DTOs.Auth;
using FeedbackLoop.Api.Domain.DTOs.Boards;
using FeedbackLoop.Api.Domain.DTOs.Common;
using FeedbackLoop.Api.Domain.DTOs.Posts;
using FeedbackLoop.Api.Domain.DTOs.Votes;
using FeedbackLoop.Api.Domain.Enums;
using FeedbackLoop.Api.Infrastructure.Persistence;
using FeedbackLoop.Api.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace FeedbackLoop.Api.Tests.Integration;

public sealed class ApiWorkflowTests
{
    private const string JwtSecret = "integration-test-secret-with-enough-length";

    [Fact]
    public async Task AdminCanCreateBoardAndManageWidgetFeedback()
    {
        await using var factory = CreateFactory(nameof(AdminCanCreateBoardAndManageWidgetFeedback));
        var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            "Ana Admin",
            "ana.workflow@example.com",
            "StrongPassword123!",
            "Workflow Workspace"));

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();
        auth!.AccessToken.Should().NotBeNullOrWhiteSpace();
        auth.RefreshToken.Should().NotBeNullOrWhiteSpace();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var createBoardResponse = await client.PostAsJsonAsync("/api/boards", new CreateBoardRequest(
            "Product Roadmap",
            "Public board used by the embeddable widget",
            true));

        createBoardResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var board = await createBoardResponse.Content.ReadFromJsonAsync<BoardResponse>();
        board.Should().NotBeNull();
        board!.Slug.Should().Be("product-roadmap");

        var endUserToken = Guid.NewGuid().ToString();
        var createPostResponse = await client.PostAsJsonAsync($"/api/widget/{board.Slug}/posts", new CreatePostRequest(
            "Add Slack notifications",
            "Notify product managers when a feature request changes status.",
            endUserToken,
            "Customer User"));

        createPostResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdPost = await createPostResponse.Content.ReadFromJsonAsync<PostResponse>();
        createdPost.Should().NotBeNull();
        createdPost!.Status.Should().Be(PostStatus.Open);
        createdPost.HasVoted.Should().BeFalse();

        var voteResponse = await client.PostAsJsonAsync(
            $"/api/widget/{board.Slug}/posts/{createdPost.Id}/vote",
            new ToggleVoteRequest(endUserToken));

        voteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var vote = await voteResponse.Content.ReadFromJsonAsync<VoteResult>();
        vote.Should().BeEquivalentTo(new VoteResult(true, 1));

        var updateStatusResponse = await client.PatchAsJsonAsync(
            $"/api/boards/{board.Id}/posts/{createdPost.Id}/status",
            new UpdatePostStatusRequest(PostStatus.Planned));

        updateStatusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedPost = await updateStatusResponse.Content.ReadFromJsonAsync<PostResponse>();
        updatedPost!.Status.Should().Be(PostStatus.Planned);
        updatedPost.VoteCount.Should().Be(1);

        var dashboardResponse = await client.GetAsync("/api/boards");

        dashboardResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var boards = await dashboardResponse.Content.ReadFromJsonAsync<List<BoardSummaryResponse>>();
        boards.Should().ContainSingle();
        boards![0].PostCount.Should().Be(1);
        boards[0].VoteCount.Should().Be(1);

        var widgetPostsResponse = await client.GetAsync($"/api/widget/{board.Slug}/posts?endUserToken={endUserToken}");

        widgetPostsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var widgetPosts = await widgetPostsResponse.Content.ReadFromJsonAsync<PagedResult<PostResponse>>();
        widgetPosts!.Items.Should().ContainSingle(post =>
            post.Id == createdPost.Id
            && post.Status == PostStatus.Planned
            && post.VoteCount == 1
            && post.HasVoted == true);

        var historyResponse = await client.GetAsync($"/api/boards/{board.Id}/posts/{createdPost.Id}/history");

        historyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await historyResponse.Content.ReadFromJsonAsync<List<StatusHistoryEntry>>();
        history.Should().ContainSingle(entry =>
            entry.From == PostStatus.Open
            && entry.To == PostStatus.Planned
            && entry.ChangedBy == "Ana Admin");
    }

    [Fact]
    public async Task HealthEndpointReturnsOk()
    {
        await using var factory = CreateFactory(nameof(HealthEndpointReturnsOk));

        var response = await factory.CreateClient().GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
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

    private sealed class TestTransactionRunner : ITransactionRunner
    {
        public Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
        {
            return action(cancellationToken);
        }
    }
}
