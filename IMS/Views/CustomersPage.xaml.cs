using IMS.Helpers;
using IMS.ViewModels;

namespace IMS.Views;

public partial class CustomersPage : ContentPage, IInitializablePage
{
    private readonly CustomersViewModel _viewModel;
    private bool _isInitialized;

    public CustomersPage(CustomersViewModel viewModel)
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
