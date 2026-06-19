using IMS.Models;

namespace IMS.Services;

public interface IAuthService
{
    TenantUser? CurrentUser { get; }
    Organization? CurrentOrganization { get; }
    bool IsAuthenticated { get; }
    bool Login(string username, string password);
    void Logout();
    bool HasPermission(string permission);
}
