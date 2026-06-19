using IMS.Services;
using IMS.Views;

namespace IMS.Controls;

public partial class MainLayout : ContentView
{
    private readonly Dictionary<string, View> _pageViews = new();
    private string _currentPage = "Dashboard";
    private Button? _activeButton;

    // Modern sidebar color scheme
    private static readonly Color InactiveBg = Colors.Transparent;
    private static readonly Color InactiveFg = Color.FromArgb("#9CA3AF");
    private static readonly Color ActiveBg = Color.FromArgb("#5B4DFF");
    private static readonly Color ActiveFg = Color.FromArgb("#FFFFFF");

    public MainLayout()
    {
        InitializeComponent();
        UpdateUserInfo();
        // Load the Dashboard page by default
        LoadPage("Dashboard");
        UpdateActiveButton("Dashboard");
    }

    public void UpdateUserInfo()
    {
        var authService = IPlatformApplication.Current?.Services?.GetService<IAuthService>();
        if (authService == null) return;

        var user = authService.CurrentUser;
        var org = authService.CurrentOrganization;

        if (user != null)
        {
            UserNameLabel.Text = user.DisplayName ?? user.Username ?? "User";
            UserRoleLabel.Text = user.UserType.ToString().Replace("_", " ");
            UserEmailLabel.Text = user.Email;
            // Get first letter for avatar
            UserAvatarLabel.Text = (user.DisplayName ?? user.Username ?? "U")[..1].ToUpper();
        }

        if (org != null)
        {
            OrgNameLabel.Text = org.OrgName;
        }
    }

    public void NavigateTo(string pageName)
    {
        if (string.IsNullOrEmpty(pageName) || pageName == _currentPage) return;

        _currentPage = pageName;
        LoadPage(pageName);
        UpdateActiveButton(pageName);
    }

    private void OnNavClicked(object? sender, EventArgs e)
    {
        if (sender is Button button)
        {
            var pageName = button.Text;
            NavigateTo(pageName);
        }
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        var authService = IPlatformApplication.Current?.Services?.GetService<IAuthService>();
        authService?.Logout();

        // Navigate back to login via the AppShell
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("//Login");
        }
    }

    private void LoadPage(string pageName)
    {
        if (_pageViews.TryGetValue(pageName, out var existingView))
        {
            ContentArea.Content = existingView;
            return;
        }

        var services = IPlatformApplication.Current?.Services;
        if (services == null) return;

        ContentPage? page = pageName switch
        {
            "Dashboard" => services.GetService<DashboardPage>(),
            "Inventory" => services.GetService<InventoryPage>(),
            "Scheduling" => services.GetService<SchedulingPage>(),
            "SMR" => services.GetService<SmrPage>(),
            "Customers" => services.GetService<CustomersPage>(),
            "Leases" => services.GetService<LeasesPage>(),
            "Exceptions" => services.GetService<ExceptionsPage>(),
            "Reports" => services.GetService<ReportsPage>(),
            "Admin" => services.GetService<AdminPage>(),
            _ => null
        };

        if (page?.Content != null)
        {
            // Preserve the BindingContext from the page when extracting its content
            page.Content.BindingContext = page.BindingContext;
            _pageViews[pageName] = page.Content;
            ContentArea.Content = page.Content;
        }
    }

    private void UpdateActiveButton(string pageName)
    {
        // Reset previous active button to default style
        if (_activeButton != null)
        {
            SetButtonStyle(_activeButton, "SidebarNavItem");
        }

        // Find and highlight the new active button
        Button? newActive = pageName switch
        {
            "Dashboard" => NavDashboard,
            "Inventory" => NavInventory,
            "Scheduling" => NavScheduling,
            "SMR" => NavSmr,
            "Customers" => NavCustomers,
            "Leases" => NavLeases,
            "Exceptions" => NavExceptions,
            "Reports" => NavReports,
            "Admin" => NavAdmin,
            _ => null
        };

        if (newActive != null)
        {
            SetButtonStyle(newActive, "SidebarNavItemActive");
            _activeButton = newActive;
        }
    }

    private static void SetButtonStyle(Button button, string styleKey)
    {
        try
        {
            if (Application.Current?.Resources.TryGetValue(styleKey, out var resource) == true && resource is Style style)
            {
                button.Style = style;
            }
        }
        catch
        {
            // Fallback: apply inline styling if resource not found
            if (styleKey == "SidebarNavItemActive")
            {
                button.BackgroundColor = ActiveBg;
                button.TextColor = ActiveFg;
            }
            else
            {
                button.BackgroundColor = InactiveBg;
                button.TextColor = InactiveFg;
            }
        }
    }
}
