using IMS.ViewModels;

namespace IMS.Views;

public partial class InventoryPage : ContentPage
{
    public InventoryPage(InventoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
