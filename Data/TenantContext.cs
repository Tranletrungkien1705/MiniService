namespace MiniService.Data;

public interface ITenantContext { Guid OrgId { get; set; } }

public sealed class TenantContext : ITenantContext
{
    public static readonly Guid DefaultOrgId = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public const string DefaultApiKey = "demo-service";
    public const string CookieName = "org_key";
    public Guid OrgId { get; set; } = DefaultOrgId;
}
