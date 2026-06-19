using IMS.Models;
using IMS.Models.Enums;

namespace IMS.Services;

public class AuthService : IAuthService
{
    private readonly IFakeDataService _fakeData;

    public AuthService(IFakeDataService fakeData)
    {
        _fakeData = fakeData;
    }

    public TenantUser? CurrentUser { get; private set; }
    public Organization? CurrentOrganization { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;

    public bool Login(string username, string password)
    {
        var user = _fakeData.TenantUsers
            .FirstOrDefault(u => u.Username == username
                && u.PasswordHash == password);

        if (user != null)
        {
            CurrentUser = user;
            CurrentOrganization = _fakeData.Organizations.FirstOrDefault(o => o.OrgId == user.OrgId);
            return true;
        }
        return false;
    }

    public void Logout()
    {
        CurrentUser = null;
        CurrentOrganization = null;
    }

    public bool HasPermission(string permission)
    {
        if (CurrentUser == null) return false;

        return CurrentUser.UserType switch
        {
            UserType.System_Admin => true, // Full access
            UserType.Service_Specialist => permission switch
            {
                "read_schedule" or "read_leases" or "read_inventory" or
                "write_smr" or "read_smr" or "read_customers" or
                "read_reports" => true,
                _ => false
            },
            UserType.Logistics_Desk => permission switch
            {
                "read_schedule" or "read_leases" or "read_inventory" or
                "read_smr" or "read_customers" or "read_reports" or
                "write_leases" or "write_customers" or "write_schedule" or
                "write_inventory" => true,
                _ => false
            },
            UserType.Customer_Contact => permission switch
            {
                "read_schedule" or "read_leases" or "read_inventory" or
                "read_smr" or "read_reports" => true,
                _ => false
            },
            _ => false
        };
    }
}
