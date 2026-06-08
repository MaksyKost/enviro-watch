using EnviroWatch.Application.Interfaces;
using EnviroWatch.Infrastructure;
using EnviroWatch.Infrastructure.Auth;
using EnviroWatch.Infrastructure.ExternalClients;
using EnviroWatch.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnviroWatch.Infrastructure.DependencyInjection;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IDataSnapshotRepository, DataSnapshotRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        services.AddHttpClient<IOpenMeteoClient, OpenMeteoClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.open-meteo.com/v1/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        return services;
    }
}
