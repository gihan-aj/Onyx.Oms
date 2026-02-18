namespace Onyx.Oms.Infrastructure.Identity;

public class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public string Authority { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string MetadataUrl { get; set; } = string.Empty;
    public bool RequireHttpsMetadata { get; set; } = true;
}
