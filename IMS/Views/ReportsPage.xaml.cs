using IMS.Helpers;
using IMS.ViewModels;

namespace IMS.Views;

public partial class ReportsPage : ContentPage, IInitializablePage
{
    private readonly ReportsViewModel _viewModel;
    private bool _isInitialized;

    public ReportsPage(ReportsViewModel viewModel)
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
