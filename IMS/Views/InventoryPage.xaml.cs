using IMS.Helpers;
using IMS.ViewModels;

namespace IMS.Views;

public partial class InventoryPage : ContentPage, IInitializablePage
{
    private readonly InventoryViewModel _viewModel;
    private bool _isInitialized;

    public InventoryPage(InventoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (!_isInitialized)
        {
            _isInitialized = true;
            _viewModel.Initialize();
        }
    }

    public void Initialize()
    {
        if (!_isInitialized)
        {
            _isInitialized = true;
            _viewModel.Initialize();
        }
    }
}
