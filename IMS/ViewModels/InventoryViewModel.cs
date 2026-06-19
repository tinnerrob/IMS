using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IMS.Models;
using IMS.Models.Enums;
using IMS.Services;

namespace IMS.ViewModels;

public partial class InventoryViewModel : ObservableObject
{
    private readonly IDataStoreService _dataStore;
    private readonly IAuthService _authService;

    public InventoryViewModel(IDataStoreService dataStore, IAuthService authService)
    {
        _dataStore = dataStore;
        _authService = authService;
    }

    public void Initialize()
    {
        Refresh();
    }

    [ObservableProperty]
    private List<AssetCategory> _categoryTree = new();

    [ObservableProperty]
    private AssetCategory? _selectedCategory;

    [ObservableProperty]
    private List<SerializedAsset> _assets = new();

    [ObservableProperty]
    private List<ProductCatalog> _products = new();

    [ObservableProperty]
    private List<BulkQuantityPool> _bulkPools = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isCategorySelected;

    [ObservableProperty]
    private string _selectedCategoryName = string.Empty;

    [RelayCommand]
    private void Refresh()
    {
        var orgId = _authService.CurrentOrganization?.OrgId ?? Guid.Empty;
        CategoryTree = _dataStore.GetCategoryTree(orgId);
        Products = _dataStore.GetAll<ProductCatalog>();
        BulkPools = _dataStore.GetAll<BulkQuantityPool>();
    }

    [RelayCommand]
    private void SelectCategory(AssetCategory category)
    {
        SelectedCategory = category;
        SelectedCategoryName = category.Name;
        IsCategorySelected = true;
        Assets = _dataStore.GetAssetsByCategory(category.CategoryId);
    }

    [RelayCommand]
    private void ClearCategory()
    {
        SelectedCategory = null;
        IsCategorySelected = false;
        SelectedCategoryName = string.Empty;
        Assets = new();
    }

    [RelayCommand]
    private void Search()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Refresh();
            return;
        }

        var orgId = _authService.CurrentOrganization?.OrgId ?? Guid.Empty;
        var query = SearchText.ToLower();

        Assets = _dataStore.GetAll<SerializedAsset>()
            .Where(a => a.OrgId == orgId && (
                a.SerialNumber.ToLower().Contains(query) ||
                a.BarcodeRfid.ToLower().Contains(query) ||
                a.Sku.ToLower().Contains(query)))
            .ToList();
    }
}
