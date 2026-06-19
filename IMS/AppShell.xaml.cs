using IMS.Views;

namespace IMS;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for navigation
        Routing.RegisterRoute("Login", typeof(LoginPage));
    }
}
