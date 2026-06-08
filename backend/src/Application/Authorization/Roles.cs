using EnviroWatch.Domain.Models;

namespace EnviroWatch.Application.Authorization;

public static class Roles
{
    public const string Admin = nameof(UserRole.Admin);
    public const string Analyst = nameof(UserRole.Analyst);
    public const string Viewer = nameof(UserRole.Viewer);

    public const string AnalystOrAbove = $"{Admin},{Analyst}";
    public const string AnyAuthenticated = $"{Admin},{Analyst},{Viewer}";
}
