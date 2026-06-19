using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IMS.Models;
using IMS.Services;

namespace IMS.ViewModels;

public partial class CustomersViewModel : ObservableObject
{
    private readonly IDataStoreService _dataStore;
    private readonly IAuthService _authService;

    public CustomersViewModel(IDataStoreService dataStore, IAuthService authService)
    {
        _dataStore = dataStore;
        _authService = authService;
        Refresh();
    }

    [ObservableProperty]
    private List<Customer> _customers = new();

    [ObservableProperty]
    private List<Project> _projects = new();

    [ObservableProperty]
    private Customer? _selectedCustomer;

    [ObservableProperty]
    private bool _isCustomerSelected;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [RelayCommand]
    private void Refresh()
    {
        Customers = _dataStore.GetAll<Customer>().OrderBy(c => c.AccountName).ToList();
        Projects = _dataStore.GetAll<Project>().OrderByDescending(p => p.StartDate).ToList();
    }

    [RelayCommand]
    private void SelectCustomer(Customer customer)
    {
        SelectedCustomer = customer;
        IsCustomerSelected = true;
    }

    [RelayCommand]
    private void ClearSelection()
    {
        SelectedCustomer = null;
        IsCustomerSelected = false;
    }

    [RelayCommand]
    private void Search()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Refresh();
            return;
        }
        var query = SearchText.ToLower();
        Customers = _dataStore.GetAll<Customer>()
            .Where(c => c.AccountName.ToLower().Contains(query) ||
                        (c.ContactEmail?.ToLower().Contains(query) ?? false))
            .OrderBy(c => c.AccountName)
            .ToList();
    }
}
