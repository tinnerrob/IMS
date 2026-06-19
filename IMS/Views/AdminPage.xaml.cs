using IMS.Helpers;
using IMS.ViewModels;

namespace IMS.Views;

public partial class AdminPage : ContentPage, IInitializablePage
{
    private readonly AdminViewModel _viewModel;
    private bool _isInitialized;

    public AdminPage(AdminViewModel viewModel)
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
