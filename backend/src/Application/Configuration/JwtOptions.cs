namespace EnviroWatch.Application.Configuration;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string Secret { get; set; }

    public string Issuer { get; set; } = "envirowatch";

    public string Audience { get; set; } = "envirowatch";

    public int ExpirationMinutes { get; set; } = 60;
}
