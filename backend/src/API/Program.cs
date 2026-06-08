using EnviroWatch.API.DependencyInjection;
using EnviroWatch.API.Hubs;
using EnviroWatch.API.Middleware;
using EnviroWatch.API.Services;
using EnviroWatch.Application.DependencyInjection;
using EnviroWatch.Application.Interfaces;
using EnviroWatch.Infrastructure;
using EnviroWatch.Infrastructure.Data;
using EnviroWatch.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is not configured.");

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddSingleton<IDataUpdateNotifier, SignalRDataUpdateNotifier>();

builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow
}))
.WithName("HealthCheck")
.WithTags("Health")
.WithOpenApi();

app.MapControllers();
app.MapHub<DashboardHub>(DashboardHub.HubPath);

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (app.Environment.IsDevelopment())
    {
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var snapshotRepository = scope.ServiceProvider.GetRequiredService<IDataSnapshotRepository>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        await UserSeeder.SeedAsync(userRepository, passwordHasher, logger);
        await DataSeeder.SeedAsync(snapshotRepository, logger);
    }
}

app.Run();

public partial class Program;
