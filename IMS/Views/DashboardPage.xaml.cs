using IMS.Helpers;
using IMS.ViewModels;

namespace IMS.Views;

public partial class DashboardPage : ContentPage, IInitializablePage
{
    private readonly DashboardViewModel _viewModel;
    private bool _isInitialized;

    public DashboardPage(DashboardViewModel viewModel)
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
