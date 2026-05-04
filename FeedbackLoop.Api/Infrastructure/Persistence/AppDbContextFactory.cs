using FeedbackLoop.Api.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FeedbackLoop.Api.Infrastructure.Persistence;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=feedbackloop;Username=postgres;Password=postgres")
            .Options;

        return new AppDbContext(options, new DesignTimeWorkspaceContext());
    }

    private sealed class DesignTimeWorkspaceContext : ICurrentWorkspaceContext
    {
        public Guid? WorkspaceId => null;

        public void SetWorkspace(Guid workspaceId)
        {
        }
    }
}
