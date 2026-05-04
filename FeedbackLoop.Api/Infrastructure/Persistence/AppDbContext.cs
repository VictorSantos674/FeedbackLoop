using FeedbackLoop.Api.Domain.Common;
using FeedbackLoop.Api.Domain.Entities;
using FeedbackLoop.Api.Domain.Enums;
using FeedbackLoop.Api.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace FeedbackLoop.Api.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly ICurrentWorkspaceContext _currentWorkspaceContext;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentWorkspaceContext currentWorkspaceContext)
        : base(options)
    {
        _currentWorkspaceContext = currentWorkspaceContext;
    }

    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<StatusHistory> StatusHistory => Set<StatusHistory>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureWorkspace(modelBuilder);
        ConfigureUser(modelBuilder);
        ConfigureBoard(modelBuilder);
        ConfigurePost(modelBuilder);
        ConfigureVote(modelBuilder);
        ConfigureComment(modelBuilder);
        ConfigureStatusHistory(modelBuilder);
        ConfigureRefreshToken(modelBuilder);

        ApplyWorkspaceFilters(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampWorkspaceScopedEntities();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        StampWorkspaceScopedEntities();
        return base.SaveChanges();
    }

    private static void ConfigureWorkspace(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Workspace>(entity =>
        {
            entity.ToTable("workspaces");
            entity.HasIndex(workspace => workspace.Slug).IsUnique();
            entity.Property(workspace => workspace.Name).HasMaxLength(160).IsRequired();
            entity.Property(workspace => workspace.Slug).HasMaxLength(120).IsRequired();
        });
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasIndex(user => new { user.WorkspaceId, user.Email }).IsUnique();
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.Email).HasMaxLength(254).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(user => user.DisplayName).HasMaxLength(120).IsRequired();
            entity.Property(user => user.Role).HasConversion<string>().HasMaxLength(32);

            entity
                .HasOne(user => user.Workspace)
                .WithMany(workspace => workspace.Users)
                .HasForeignKey(user => user.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureBoard(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Board>(entity =>
        {
            entity.ToTable("boards");
            entity.HasIndex(board => new { board.WorkspaceId, board.Slug }).IsUnique();
            entity.Property(board => board.Name).HasMaxLength(160).IsRequired();
            entity.Property(board => board.Slug).HasMaxLength(120).IsRequired();
            entity.Property(board => board.Description).HasMaxLength(500);

            entity
                .HasOne(board => board.Workspace)
                .WithMany(workspace => workspace.Boards)
                .HasForeignKey(board => board.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigurePost(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("posts");
            entity.HasIndex(post => new { post.WorkspaceId, post.BoardId, post.Status });
            entity.HasIndex(post => post.EndUserToken);
            entity.Property(post => post.Title).HasMaxLength(180).IsRequired();
            entity.Property(post => post.Description).HasMaxLength(4000).IsRequired();
            entity.Property(post => post.EndUserName).HasMaxLength(160);
            entity.Property(post => post.EndUserEmail).HasMaxLength(254);
            entity.Property(post => post.Status)
                .HasConversion<string>()
                .HasMaxLength(32)
                .HasDefaultValue(PostStatus.Open);

            entity
                .HasOne(post => post.Board)
                .WithMany(board => board.Posts)
                .HasForeignKey(post => post.BoardId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureVote(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vote>(entity =>
        {
            entity.ToTable("votes");
            entity.HasIndex(vote => new { vote.PostId, vote.EndUserToken }).IsUnique();

            entity
                .HasOne(vote => vote.Post)
                .WithMany(post => post.Votes)
                .HasForeignKey(vote => vote.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureComment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("comments");
            entity.HasIndex(comment => new { comment.PostId, comment.CreatedAtUtc });
            entity.Property(comment => comment.Body).HasMaxLength(3000).IsRequired();

            entity
                .HasOne(comment => comment.Post)
                .WithMany(post => post.Comments)
                .HasForeignKey(comment => comment.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(comment => comment.AuthorUser)
                .WithMany()
                .HasForeignKey(comment => comment.AuthorUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureStatusHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StatusHistory>(entity =>
        {
            entity.ToTable("status_history");
            entity.HasIndex(history => new { history.PostId, history.ChangedAtUtc });
            entity.Property(history => history.FromStatus).HasConversion<string>().HasMaxLength(32);
            entity.Property(history => history.ToStatus).HasConversion<string>().HasMaxLength(32);

            entity
                .HasOne(history => history.Post)
                .WithMany(post => post.StatusHistory)
                .HasForeignKey(history => history.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(history => history.ChangedByUser)
                .WithMany(user => user.StatusChanges)
                .HasForeignKey(history => history.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureRefreshToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasIndex(token => token.Token).IsUnique();
            entity.HasIndex(token => token.WorkspaceId);
            entity.HasIndex(token => new { token.UserId, token.FamilyId });
            entity.HasIndex(token => new { token.UserId, token.UserAgentHash });
            entity.Property(token => token.Token).HasMaxLength(128).IsRequired();
            entity.Property(token => token.UserAgentHash).HasMaxLength(128);
            entity.Ignore(token => token.IsExpired);
            entity.Ignore(token => token.IsRevoked);

            entity
                .HasOne(token => token.User)
                .WithMany(user => user.RefreshTokens)
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ApplyWorkspaceFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasQueryFilter(entity =>
            !_currentWorkspaceContext.WorkspaceId.HasValue
            || entity.WorkspaceId == _currentWorkspaceContext.WorkspaceId.Value);

        modelBuilder.Entity<Board>().HasQueryFilter(entity =>
            !_currentWorkspaceContext.WorkspaceId.HasValue
            || entity.WorkspaceId == _currentWorkspaceContext.WorkspaceId.Value);

        modelBuilder.Entity<Post>().HasQueryFilter(entity =>
            !_currentWorkspaceContext.WorkspaceId.HasValue
            || entity.WorkspaceId == _currentWorkspaceContext.WorkspaceId.Value);

        modelBuilder.Entity<Vote>().HasQueryFilter(entity =>
            !_currentWorkspaceContext.WorkspaceId.HasValue
            || entity.WorkspaceId == _currentWorkspaceContext.WorkspaceId.Value);

        modelBuilder.Entity<Comment>().HasQueryFilter(entity =>
            !_currentWorkspaceContext.WorkspaceId.HasValue
            || entity.WorkspaceId == _currentWorkspaceContext.WorkspaceId.Value);

        modelBuilder.Entity<StatusHistory>().HasQueryFilter(entity =>
            !_currentWorkspaceContext.WorkspaceId.HasValue
            || entity.WorkspaceId == _currentWorkspaceContext.WorkspaceId.Value);

        modelBuilder.Entity<RefreshToken>().HasQueryFilter(entity =>
            !_currentWorkspaceContext.WorkspaceId.HasValue
            || entity.WorkspaceId == _currentWorkspaceContext.WorkspaceId.Value);
    }

    private void StampWorkspaceScopedEntities()
    {
        var workspaceId = _currentWorkspaceContext.WorkspaceId;
        if (!workspaceId.HasValue)
        {
            return;
        }

        foreach (var entry in ChangeTracker.Entries<IWorkspaceScopedEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.WorkspaceId == Guid.Empty)
            {
                entry.Entity.WorkspaceId = workspaceId.Value;
            }
        }
    }
}
