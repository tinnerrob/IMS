using IMS.ViewModels;

namespace IMS.Views;

public partial class LeasesPage : ContentPage
{
    public LeasesPage(LeasesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
