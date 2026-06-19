using IMS.ViewModels;

namespace IMS.Views;

public partial class ExceptionsPage : ContentPage
{
    public ExceptionsPage(ExceptionsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
