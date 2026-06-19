using IMS.ViewModels;

namespace IMS.Views;

public partial class SmrPage : ContentPage
{
    public SmrPage(SmrViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
