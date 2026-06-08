using System.Text;
using EnviroWatch.Application.Authorization;
using EnviroWatch.Application.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace EnviroWatch.API.DependencyInjection;

public static class AuthServiceExtensions
{
    public static IServiceCollection AddApiAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .PostConfigure(options =>
            {
                var secret = configuration["JWT_SECRET"];
                if (!string.IsNullOrWhiteSpace(secret))
                {
                    options.Secret = secret;
                }
            })
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Secret) && options.Secret.Length >= 32,
                "JWT secret must be at least 32 characters.")
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole(Roles.Admin));
            options.AddPolicy("AnalystOrAbove", policy => policy.RequireRole(Roles.Admin, Roles.Analyst));
            options.AddPolicy("ViewerOrAbove", policy => policy.RequireRole(Roles.AnyAuthenticated));
        });

        return services;
    }

    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "EnviroWatch API",
                Version = "v1",
                Description = "Real-time environmental monitoring platform API."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter JWT token"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    private sealed class ConfigureJwtBearerOptions : IConfigureOptions<JwtBearerOptions>
    {
        private readonly JwtOptions _jwtOptions;

        public ConfigureJwtBearerOptions(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value;
        }

        public void Configure(JwtBearerOptions options)
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidAudience = _jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret)),
                RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                NameClaimType = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub
            };
        }
    }
}
