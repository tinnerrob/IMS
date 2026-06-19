using IMS.Controls;

namespace IMS.Views;

public partial class MainPage : ContentPage
{
    private bool _isFirstAppearance = true;

    public MainPage()
    {
        InitializeComponent();
    }

    public MainLayout MainLayout => MainLayoutControl;

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_isFirstAppearance)
        {
            _isFirstAppearance = false;
            // Load the default page after login when auth is available
            MainLayoutControl.LoadDefaultPage();
        }
    }
}
