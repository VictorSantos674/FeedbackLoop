using System.Text;
using FeedbackLoop.Api.Application.Services;
using FeedbackLoop.Api.Domain.Enums;
using FeedbackLoop.Api.Infrastructure.Auth;
using FeedbackLoop.Api.Infrastructure.Persistence;
using FeedbackLoop.Api.Infrastructure.Tenancy;
using FeedbackLoop.Api.Infrastructure.Time;
using FeedbackLoop.Api.Middleware;
using FeedbackLoop.Api.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
            ?? ["http://localhost:3000", "http://127.0.0.1:3000"];

        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.AddScoped<ICurrentWorkspaceContext, CurrentWorkspaceContext>();
builder.Services.AddScoped<ISystemClock, SystemClock>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IVoteService, VoteService>();
builder.Services.AddScoped<INotificationService, LogNotificationService>();
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<IWorkspaceRepository, EfWorkspaceRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, EfRefreshTokenRepository>();
builder.Services.AddScoped<IBoardRepository, EfBoardRepository>();
builder.Services.AddScoped<IPostRepository, EfPostRepository>();
builder.Services.AddScoped<IVoteRepository, EfVoteRepository>();
builder.Services.AddScoped<IStatusHistoryRepository, EfStatusHistoryRepository>();
builder.Services.AddScoped<ITransactionRunner, EfTransactionRunner>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("FeedbackLoopDb")
        ?? "Host=localhost;Port=5432;Database=feedbackloop;Username=postgres;Password=postgres";

    options.UseNpgsql(connectionString);
});

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings are missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(UserRole.Admin.ToString()));
    options.AddPolicy("MemberOrAbove", policy => policy.RequireRole(UserRole.Admin.ToString(), UserRole.Member.ToString()));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseMiddleware<WorkspaceResolutionMiddleware>();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

app.Run();

public partial class Program;
