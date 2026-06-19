namespace IMS;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Start at the Login page
        MainPage = new AppShell();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
        window.Title = "IMS - Inventory Management System";
        window.MinimumWidth = 1024;
        window.MinimumHeight = 768;
        return window;
    }
}
