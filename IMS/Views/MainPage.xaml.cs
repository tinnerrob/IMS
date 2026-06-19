using IMS.Controls;

namespace IMS.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    public MainLayout MainLayout => MainLayoutControl;
}
