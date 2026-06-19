using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IMS.Controls;
using IMS.Models;
using IMS.Models.Enums;
using IMS.Services;

namespace IMS.ViewModels;

/// <summary>
/// Represents a draggable item in the left panel lists.
/// </summary>
public partial class DraggableSchedulingItem : ObservableObject
{
    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _subtitle = string.Empty;

    [ObservableProperty]
    private string _itemType = "Customer"; // "Customer", "Project", "Category", "Asset", "Labor"

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private Color _statusColor = Colors.Transparent;

    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private bool _isExpandable;

    [ObservableProperty]
    private List<DraggableSchedulingItem> _children = new();

    // References
    [ObservableProperty]
    private Guid? _customerId;

    [ObservableProperty]
    private Guid? _projectId;

    [ObservableProperty]
    private Guid? _categoryId;

    [ObservableProperty]
    private Guid? _assetId;

    [ObservableProperty]
    private string? _sku;

    [ObservableProperty]
    private int _availableQuantity;

    // Labor-specific
    [ObservableProperty]
    private Guid? _technicianId;

    [ObservableProperty]
    private decimal? _hourlyRate;

    public string DragData => $"{ItemType}:{CustomerId}:{ProjectId}:{CategoryId}:{AssetId}:{Sku}:{TechnicianId}:{HourlyRate}";
}

/// <summary>
/// Represents an editable event detail for the detail panel.
/// </summary>
public partial class EditableEventDetail : ObservableObject
{
    [ObservableProperty]
    private Guid _eventId;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _eventType = string.Empty;

    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private DateTime _startDate;

    [ObservableProperty]
    private DateTime _endDate;

    [ObservableProperty]
    private string _tier = "Soft Hold";

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private string _details = string.Empty;

    [ObservableProperty]
    private string _technicianName = string.Empty;

    [ObservableProperty]
    private decimal _estimatedHours;

    [ObservableProperty]
    private decimal _hourlyRate;

    [ObservableProperty]
    private bool _hasConflict;

    [ObservableProperty]
    private string _conflictDescription = string.Empty;
}

public partial class SchedulingViewModel : ObservableObject
{
    private readonly IDataStoreService _dataStore;
    private readonly IFakeDataService _fakeData;
    private readonly IAuthService _authService;

    public SchedulingViewModel(IDataStoreService dataStore, IFakeDataService fakeData, IAuthService authService)
    {
        _dataStore = dataStore;
        _fakeData = fakeData;
        _authService = authService;
    }

    public void Initialize()
    {
        Refresh();
    }

    // ─── Core Data ───
    [ObservableProperty]
    private List<ScheduleAllocation> _allocations = new();

    [ObservableProperty]
    private List<Project> _projects = new();

    [ObservableProperty]
    private List<Customer> _customers = new();

    [ObservableProperty]
    private List<TenantUser> _technicians = new();

    [ObservableProperty]
    private Project? _selectedProject;

    [ObservableProperty]
    private DateTime _viewStartDate = DateTime.Today.AddMonths(-1);

    [ObservableProperty]
    private DateTime _viewEndDate = DateTime.Today.AddMonths(3);

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _assetSearchText = string.Empty;

    [ObservableProperty]
    private List<ScheduleAllocation> _filteredAllocations = new();

    [ObservableProperty]
    private List<CalendarEvent> _calendarEvents = new();

    [ObservableProperty]
    private List<CalendarTreeNode> _calendarTreeNodes = new();

    [ObservableProperty]
    private CalendarEvent? _selectedEvent;

    [ObservableProperty]
    private bool _isEventSelected;

    [ObservableProperty]
    private int _totalConflicts;

    [ObservableProperty]
    private int _totalAllocations;

    [ObservableProperty]
    private string _dateRangeText = string.Empty;

    // ─── View Mode ───
    [ObservableProperty]
    private CalendarViewMode _calendarViewMode = CalendarViewMode.GroupByCustomer;

    [ObservableProperty]
    private string _viewModeLabel = "Customer View";

    partial void OnCalendarViewModeChanged(CalendarViewMode value)
    {
        ViewModeLabel = value == CalendarViewMode.GroupByCustomer ? "Customer View" : "Resource View";
        BuildCalendarTree();
    }

    // ─── Left Panel Data ───
    [ObservableProperty]
    private List<DraggableSchedulingItem> _customerProjectItems = new();

    [ObservableProperty]
    private List<DraggableSchedulingItem> _filteredCustomerProjectItems = new();

    [ObservableProperty]
    private List<DraggableSchedulingItem> _resourceItems = new();

    [ObservableProperty]
    private List<DraggableSchedulingItem> _filteredResourceItems = new();

    [ObservableProperty]
    private string _customerSearchText = string.Empty;

    [ObservableProperty]
    private string _resourceSearchText = string.Empty;

    // ─── Detail Panel ───
    [ObservableProperty]
    private EditableEventDetail? _editingDetail;

    [ObservableProperty]
    private bool _isDetailPanelVisible;

    [ObservableProperty]
    private string _detailPanelTitle = "Event Details";

    [RelayCommand]
    private void Refresh()
    {
        Projects = _dataStore.GetAll<Project>().OrderBy(p => p.StartDate).ToList();
        Customers = _dataStore.GetAll<Customer>().OrderBy(c => c.AccountName).ToList();
        Technicians = _dataStore.GetAll<TenantUser>()
            .Where(u => u.UserType == UserType.Service_Specialist || u.UserType == UserType.System_Admin)
            .OrderBy(u => u.DisplayName)
            .ToList();
        Allocations = _dataStore.GetAll<ScheduleAllocation>()
            .OrderBy(a => a.StartDate)
            .ToList();

        if (SelectedProject != null)
        {
            FilteredAllocations = Allocations.Where(a => a.ProjectId == SelectedProject.ProjectId).ToList();
        }
        else
        {
            FilteredAllocations = Allocations;
        }

        LoadLeftPanelData();
        BuildCalendarTree();
        UpdateCalendarEvents();
        UpdateStats();
    }

    // ─── Left Panel Data Loading ───

    private void LoadLeftPanelData()
    {
        LoadCustomerProjectItems();
        LoadResourceItems();
    }

    private void LoadCustomerProjectItems()
    {
        var items = new List<DraggableSchedulingItem>();

        foreach (var customer in Customers)
        {
            var customerProjects = Projects.Where(p => p.CustomerId == customer.CustomerId).ToList();

            var customerItem = new DraggableSchedulingItem
            {
                DisplayName = customer.AccountName,
                Subtitle = $"{customerProjects.Count} project(s)",
                ItemType = "Customer",
                CustomerId = customer.CustomerId,
                StatusColor = Color.FromArgb("#5B4DFF"),
                IsExpandable = customerProjects.Count > 0,
                IsExpanded = false,
                Children = new List<DraggableSchedulingItem>()
            };

            foreach (var project in customerProjects)
            {
                customerItem.Children.Add(new DraggableSchedulingItem
                {
                    DisplayName = project.ProjectName,
                    Subtitle = $"{project.StartDate:MMM dd} - {project.EndDate:MMM dd}",
                    ItemType = "Project",
                    CustomerId = customer.CustomerId,
                    ProjectId = project.ProjectId,
                    StatusColor = Color.FromArgb("#0EA5E9"),
                    IsExpandable = false
                });
            }

            items.Add(customerItem);
        }

        CustomerProjectItems = items;
        FilteredCustomerProjectItems = items;
    }

    private void LoadResourceItems()
    {
        var items = new List<DraggableSchedulingItem>();

        // Load categories with their assets
        var categories = _dataStore.GetAll<AssetCategory>()
            .Where(c => c.ParentCategoryId == null)
            .OrderBy(c => c.Name)
            .ToList();

        var allAssets = _dataStore.GetAll<SerializedAsset>()
            .Where(a => a.CurrentStatus == AssetStatus.Available)
            .OrderBy(a => a.SerialNumber)
            .ToList();

        foreach (var cat in categories)
        {
            var catAssets = allAssets.Where(a => a.Sku == cat.Name || true).ToList(); // Simplified - in real app, use proper FK

            var catItem = new DraggableSchedulingItem
            {
                DisplayName = cat.Name,
                Subtitle = "Category",
                ItemType = "Category",
                CategoryId = cat.CategoryId,
                StatusColor = Color.FromArgb("#5B4DFF"),
                IsExpandable = catAssets.Count > 0,
                IsExpanded = false,
                Children = new List<DraggableSchedulingItem>()
            };

            foreach (var asset in catAssets)
            {
                catItem.Children.Add(new DraggableSchedulingItem
                {
                    DisplayName = asset.SerialNumber,
                    Subtitle = asset.Sku,
                    ItemType = "Asset",
                    AssetId = asset.AssetId,
                    Sku = asset.Sku,
                    Status = "Available",
                    StatusColor = Color.FromArgb("#22C55E"),
                    IsExpandable = false
                });
            }

            items.Add(catItem);
        }

        // Add labor items
        foreach (var tech in Technicians)
        {
            items.Add(new DraggableSchedulingItem
            {
                DisplayName = tech.DisplayName,
                Subtitle = $"${tech.InternalHourlyRate ?? 0:F2}/hr",
                ItemType = "Labor",
                TechnicianId = tech.UserId,
                HourlyRate = tech.InternalHourlyRate,
                StatusColor = Color.FromArgb("#F59E0B"),
                IsExpandable = false
            });
        }

        ResourceItems = items;
        FilteredResourceItems = items;
    }

    [RelayCommand]
    private void FilterCustomerProjects()
    {
        if (string.IsNullOrWhiteSpace(CustomerSearchText))
        {
            FilteredCustomerProjectItems = CustomerProjectItems;
            return;
        }

        var query = CustomerSearchText.ToLower();
        FilteredCustomerProjectItems = CustomerProjectItems
            .Where(c => c.DisplayName.ToLower().Contains(query) ||
                        c.Children.Any(p => p.DisplayName.ToLower().Contains(query)))
            .ToList();

        // Also filter children
        foreach (var item in FilteredCustomerProjectItems)
        {
            item.Children = item.Children
                .Where(p => p.DisplayName.ToLower().Contains(query))
                .ToList();
        }
    }

    [RelayCommand]
    private void FilterResources()
    {
        if (string.IsNullOrWhiteSpace(ResourceSearchText))
        {
            FilteredResourceItems = ResourceItems;
            return;
        }

        var query = ResourceSearchText.ToLower();
        FilteredResourceItems = ResourceItems
            .Where(r => r.DisplayName.ToLower().Contains(query) ||
                        r.Subtitle.ToLower().Contains(query) ||
                        r.Children.Any(c => c.DisplayName.ToLower().Contains(query)))
            .ToList();

        foreach (var item in FilteredResourceItems)
        {
            item.Children = item.Children
                .Where(c => c.DisplayName.ToLower().Contains(query))
                .ToList();
        }
    }

    [RelayCommand]
    private void ToggleCustomerProjectExpand(DraggableSchedulingItem item)
    {
        item.IsExpanded = !item.IsExpanded;
        // Force UI refresh
        var list = FilteredCustomerProjectItems;
        FilteredCustomerProjectItems = new List<DraggableSchedulingItem>(list);
    }

    [RelayCommand]
    private void ToggleResourceExpand(DraggableSchedulingItem item)
    {
        item.IsExpanded = !item.IsExpanded;
        var list = FilteredResourceItems;
        FilteredResourceItems = new List<DraggableSchedulingItem>(list);
    }

    // ─── Calendar Tree Building ───

    private void BuildCalendarTree()
    {
        var projectDict = Projects.ToDictionary(p => p.ProjectId, p => p);
        var customerDict = Customers.ToDictionary(c => c.CustomerId, c => c);
        var categories = _dataStore.GetAll<AssetCategory>().ToDictionary(c => c.CategoryId, c => c.Name);
        var assets = _dataStore.GetAll<SerializedAsset>().ToDictionary(a => a.AssetId, a => a.SerialNumber);
        var techDict = Technicians.ToDictionary(t => t.UserId, t => t.DisplayName);

        var nodes = new List<CalendarTreeNode>();

        if (CalendarViewMode == CalendarViewMode.GroupByCustomer)
        {
            // Group allocations by Customer → Project → Resource
            var customerGroups = FilteredAllocations
                .GroupBy(a =>
                {
                    projectDict.TryGetValue(a.ProjectId, out var proj);
                    return proj?.CustomerId ?? Guid.Empty;
                })
                .OrderBy(g =>
                {
                    customerDict.TryGetValue(g.Key, out var cust);
                    return cust?.AccountName ?? "Unknown";
                });

            foreach (var custGroup in customerGroups)
            {
                if (custGroup.Key == Guid.Empty) continue;
                customerDict.TryGetValue(custGroup.Key, out var customer);

                var custNode = new CalendarTreeNode
                {
                    Id = $"cust-{custGroup.Key}",
                    Label = customer?.AccountName ?? "Unknown Customer",
                    Icon = "🏢",
                    Depth = 0,
                    IsExpandable = true,
                    IsExpanded = true,
                    NodeType = CalendarEventType.Project,
                    CustomerId = custGroup.Key
                };

                var projectGroups = custGroup.GroupBy(a => a.ProjectId);
                foreach (var projGroup in projectGroups)
                {
                    projectDict.TryGetValue(projGroup.Key, out var project);

                    var projNode = new CalendarTreeNode
                    {
                        Id = $"proj-{projGroup.Key}",
                        Label = project?.ProjectName ?? "Unknown Project",
                        Icon = "📋",
                        Depth = 1,
                        IsExpandable = true,
                        IsExpanded = true,
                        NodeType = CalendarEventType.Project,
                        CustomerId = custGroup.Key,
                        ProjectId = projGroup.Key
                    };

                    foreach (var alloc in projGroup)
                    {
                        var categoryName = alloc.CategoryId.HasValue ? categories.GetValueOrDefault(alloc.CategoryId.Value) : null;
                        var assetSerial = alloc.SerializedAssetId.HasValue ? assets.GetValueOrDefault(alloc.SerializedAssetId.Value) : null;

                        var title = assetSerial ?? categoryName ?? "Allocation";
                        var eventType = CalendarEventType.Category;
                        if (assetSerial != null)
                            eventType = CalendarEventType.Asset;
                        if (alloc.CategoryId == null && alloc.SerializedAssetId == null)
                            eventType = CalendarEventType.Labor;

                        var calendarEvent = new CalendarEvent
                        {
                            EventId = Guid.NewGuid(),
                            EventType = eventType,
                            AllocationId = alloc.AllocationId,
                            ProjectId = alloc.ProjectId,
                            ProjectName = project?.ProjectName ?? "Unknown",
                            Title = title,
                            StartDate = alloc.StartDate,
                            EndDate = alloc.EndDate,
                            Tier = alloc.AllocationTier,
                            CategoryName = categoryName,
                            AssetSerial = assetSerial,
                            BulkQuantity = alloc.BulkQuantity,
                            Notes = alloc.Notes,
                            Details = alloc.Details
                        };

                        var resNode = new CalendarTreeNode
                        {
                            Id = $"res-{alloc.AllocationId}",
                            Label = title,
                            Icon = eventType switch
                            {
                                CalendarEventType.Asset => "🔧",
                                CalendarEventType.Category => "📦",
                                CalendarEventType.Labor => "👤",
                                _ => "📌"
                            },
                            Depth = 2,
                            IsExpandable = false,
                            NodeType = eventType,
                            CustomerId = custGroup.Key,
                            ProjectId = projGroup.Key,
                            CalendarEvent = calendarEvent
                        };

                        projNode.Children.Add(resNode);
                    }

                    if (projNode.Children.Count > 0)
                        custNode.Children.Add(projNode);
                }

                if (custNode.Children.Count > 0)
                    nodes.Add(custNode);
            }
        }
        else // GroupByResource
        {
            // Group allocations by Category → Asset → Customer/Project
            var categoryGroups = FilteredAllocations
                .Where(a => a.CategoryId.HasValue)
                .GroupBy(a => a.CategoryId!.Value);

            foreach (var catGroup in categoryGroups)
            {
                var catName = categories.GetValueOrDefault(catGroup.Key, "Unknown Category");

                var catNode = new CalendarTreeNode
                {
                    Id = $"cat-{catGroup.Key}",
                    Label = catName,
                    Icon = "📦",
                    Depth = 0,
                    IsExpandable = true,
                    IsExpanded = true,
                    NodeType = CalendarEventType.Category,
                    CategoryId = catGroup.Key
                };

                var assetGroups = catGroup.GroupBy(a => a.SerializedAssetId);
                foreach (var assetGroup in assetGroups)
                {
                    var assetSerial = assetGroup.Key.HasValue ? assets.GetValueOrDefault(assetGroup.Key.Value) : "Bulk";

                    var assetNode = new CalendarTreeNode
                    {
                        Id = $"asset-{assetGroup.Key?.ToString() ?? "bulk"}",
                        Label = assetSerial ?? "Bulk Quantity",
                        Icon = "🔧",
                        Depth = 1,
                        IsExpandable = true,
                        IsExpanded = true,
                        NodeType = CalendarEventType.Asset,
                        CategoryId = catGroup.Key,
                        AssetId = assetGroup.Key
                    };

                    foreach (var alloc in assetGroup)
                    {
                        projectDict.TryGetValue(alloc.ProjectId, out var project);
                        customerDict.TryGetValue(project?.CustomerId ?? Guid.Empty, out var customer);

                        var label = customer != null
                            ? $"{customer.AccountName}: {project?.ProjectName ?? "Unknown"}"
                            : project?.ProjectName ?? "Unknown Project";

                        var calendarEvent = new CalendarEvent
                        {
                            EventId = Guid.NewGuid(),
                            EventType = CalendarEventType.Asset,
                            AllocationId = alloc.AllocationId,
                            ProjectId = alloc.ProjectId,
                            ProjectName = project?.ProjectName ?? "Unknown",
                            Title = label,
                            StartDate = alloc.StartDate,
                            EndDate = alloc.EndDate,
                            Tier = alloc.AllocationTier,
                            CategoryName = catName,
                            AssetSerial = assetSerial,
                            BulkQuantity = alloc.BulkQuantity,
                            Notes = alloc.Notes,
                            Details = alloc.Details
                        };

                        var custProjNode = new CalendarTreeNode
                        {
                            Id = $"cp-{alloc.AllocationId}",
                            Label = label,
                            Icon = "🏢",
                            Depth = 2,
                            IsExpandable = false,
                            NodeType = CalendarEventType.Project,
                            CustomerId = project?.CustomerId,
                            ProjectId = alloc.ProjectId,
                            CalendarEvent = calendarEvent
                        };

                        assetNode.Children.Add(custProjNode);
                    }

                    if (assetNode.Children.Count > 0)
                        catNode.Children.Add(assetNode);
                }

                if (catNode.Children.Count > 0)
                    nodes.Add(catNode);
            }

            // Add labor allocations
            var laborAllocs = FilteredAllocations.Where(a => a.CategoryId == null && a.SerializedAssetId == null);
            if (laborAllocs.Any())
            {
                var laborNode = new CalendarTreeNode
                {
                    Id = "labor-group",
                    Label = "Labor",
                    Icon = "👤",
                    Depth = 0,
                    IsExpandable = true,
                    IsExpanded = true,
                    NodeType = CalendarEventType.Labor
                };

                foreach (var alloc in laborAllocs)
                {
                    projectDict.TryGetValue(alloc.ProjectId, out var project);
                    customerDict.TryGetValue(project?.CustomerId ?? Guid.Empty, out var customer);

                    var label = customer != null
                        ? $"{customer.AccountName}: {project?.ProjectName ?? "Unknown"}"
                        : project?.ProjectName ?? "Unknown Project";

                    var calendarEvent = new CalendarEvent
                    {
                        EventId = Guid.NewGuid(),
                        EventType = CalendarEventType.Labor,
                        AllocationId = alloc.AllocationId,
                        ProjectId = alloc.ProjectId,
                        ProjectName = project?.ProjectName ?? "Unknown",
                        Title = label,
                        StartDate = alloc.StartDate,
                        EndDate = alloc.EndDate,
                        Tier = alloc.AllocationTier,
                        Notes = alloc.Notes,
                        Details = alloc.Details
                    };

                    var custProjNode = new CalendarTreeNode
                    {
                        Id = $"cp-labor-{alloc.AllocationId}",
                        Label = label,
                        Icon = "🏢",
                        Depth = 1,
                        IsExpandable = false,
                        NodeType = CalendarEventType.Project,
                        CustomerId = project?.CustomerId,
                        ProjectId = alloc.ProjectId,
                        CalendarEvent = calendarEvent
                    };

                    laborNode.Children.Add(custProjNode);
                }

                if (laborNode.Children.Count > 0)
                    nodes.Add(laborNode);
            }
        }

        CalendarTreeNodes = nodes;
    }

    // ─── Drag & Drop Handling ───

    public void HandleDrop(string dragData, DateTime dropDate)
    {
        var parts = dragData.Split(':');
        if (parts.Length < 2) return;

        var itemType = parts[0];

        switch (itemType)
        {
            case "Customer":
                HandleCustomerDrop(parts, dropDate);
                break;
            case "Project":
                HandleProjectDrop(parts, dropDate);
                break;
            case "Category":
                HandleCategoryDrop(parts, dropDate);
                break;
            case "Asset":
                HandleAssetDrop(parts, dropDate);
                break;
            case "Labor":
                HandleLaborDrop(parts, dropDate);
                break;
        }

        Refresh();
    }

    private void HandleCustomerDrop(string[] parts, DateTime dropDate)
    {
        if (parts.Length < 2) return;
        if (!Guid.TryParse(parts[1], out var customerId)) return;

        // Create a placeholder allocation for the customer
        // This creates a project-level allocation that can be refined later
        var customer = Customers.FirstOrDefault(c => c.CustomerId == customerId);
        if (customer == null) return;

        // Find or create a project for this customer
        var existingProject = Projects.FirstOrDefault(p => p.CustomerId == customerId);
        if (existingProject != null)
        {
            // Use the first project found
            var allocation = new ScheduleAllocation
            {
                AllocationId = Guid.NewGuid(),
                ProjectId = existingProject.ProjectId,
                AllocationTier = AllocationTier.Soft_Hold,
                StartDate = dropDate,
                EndDate = dropDate.AddDays(14),
                BulkQuantity = 0
            };
            _fakeData.ScheduleAllocations.Add(allocation);
        }
    }

    private void HandleProjectDrop(string[] parts, DateTime dropDate)
    {
        if (parts.Length < 3) return;
        if (!Guid.TryParse(parts[2], out var projectId)) return;

        var project = Projects.FirstOrDefault(p => p.ProjectId == projectId);
        if (project == null) return;

        var allocation = new ScheduleAllocation
        {
            AllocationId = Guid.NewGuid(),
            ProjectId = projectId,
            AllocationTier = AllocationTier.Soft_Hold,
            StartDate = dropDate,
            EndDate = dropDate.AddDays(14),
            BulkQuantity = 0
        };
        _fakeData.ScheduleAllocations.Add(allocation);
    }

    private void HandleCategoryDrop(string[] parts, DateTime dropDate)
    {
        if (SelectedProject == null) return;
        if (parts.Length < 2) return;
        if (!Guid.TryParse(parts[1], out var categoryId)) return;

        var allocation = new ScheduleAllocation
        {
            AllocationId = Guid.NewGuid(),
            ProjectId = SelectedProject.ProjectId,
            AllocationTier = AllocationTier.Soft_Hold,
            StartDate = dropDate,
            EndDate = dropDate.AddDays(14),
            CategoryId = categoryId,
            BulkQuantity = 1
        };
        _fakeData.ScheduleAllocations.Add(allocation);
    }

    private void HandleAssetDrop(string[] parts, DateTime dropDate)
    {
        if (SelectedProject == null) return;
        if (parts.Length < 5) return;
        if (!Guid.TryParse(parts[4], out var assetId)) return;

        var allocation = new ScheduleAllocation
        {
            AllocationId = Guid.NewGuid(),
            ProjectId = SelectedProject.ProjectId,
            AllocationTier = AllocationTier.Soft_Hold,
            StartDate = dropDate,
            EndDate = dropDate.AddDays(14),
            SerializedAssetId = assetId,
            BulkQuantity = 0
        };
        _fakeData.ScheduleAllocations.Add(allocation);
    }

    private void HandleLaborDrop(string[] parts, DateTime dropDate)
    {
        if (SelectedProject == null) return;
        if (parts.Length < 7) return;
        if (!Guid.TryParse(parts[6], out var techId)) return;

        decimal? hourlyRate = null;
        if (parts.Length > 7 && decimal.TryParse(parts[7], out var parsedRate))
            hourlyRate = parsedRate;

        var allocation = new ScheduleAllocation
        {
            AllocationId = Guid.NewGuid(),
            ProjectId = SelectedProject.ProjectId,
            AllocationTier = AllocationTier.Soft_Hold,
            StartDate = dropDate,
            EndDate = dropDate.AddDays(1),
            SerializedAssetId = null,
            BulkQuantity = 0
        };
        _fakeData.ScheduleAllocations.Add(allocation);
    }

    // ─── View Mode Toggle ───

    [RelayCommand]
    private void ToggleViewMode()
    {
        CalendarViewMode = CalendarViewMode == CalendarViewMode.GroupByCustomer
            ? CalendarViewMode.GroupByResource
            : CalendarViewMode.GroupByCustomer;
    }

    // ─── Event Selection & Detail Editing ───

    [RelayCommand]
    private void SelectEvent(CalendarEvent calendarEvent)
    {
        SelectedEvent = calendarEvent;
        IsEventSelected = true;

        var project = Projects.FirstOrDefault(p => p.ProjectId == calendarEvent.ProjectId);
        var customer = project != null ? Customers.FirstOrDefault(c => c.CustomerId == project.CustomerId) : null;

        EditingDetail = new EditableEventDetail
        {
            EventId = calendarEvent.EventId,
            Title = calendarEvent.Title,
            EventType = calendarEvent.EventType.ToString(),
            ProjectName = calendarEvent.ProjectName,
            CustomerName = customer?.AccountName ?? string.Empty,
            StartDate = calendarEvent.StartDate,
            EndDate = calendarEvent.EndDate,
            Tier = calendarEvent.Tier.ToString(),
            Notes = calendarEvent.Notes ?? string.Empty,
            Details = calendarEvent.Details ?? string.Empty,
            TechnicianName = calendarEvent.TechnicianName ?? string.Empty,
            EstimatedHours = calendarEvent.EstimatedHours ?? 0,
            HourlyRate = calendarEvent.HourlyRate ?? 0,
            HasConflict = calendarEvent.HasConflict,
            ConflictDescription = calendarEvent.ConflictDescription ?? string.Empty
        };
        DetailPanelTitle = $"Edit: {calendarEvent.Title}";
        IsDetailPanelVisible = true;
    }

    [RelayCommand]
    private void ClearSelection()
    {
        SelectedEvent = null;
        IsEventSelected = false;
        IsDetailPanelVisible = false;
        EditingDetail = null;
    }

    [RelayCommand]
    private void SaveEventDetails()
    {
        if (EditingDetail == null || SelectedEvent == null) return;

        SelectedEvent.Title = EditingDetail.Title;
        SelectedEvent.StartDate = EditingDetail.StartDate;
        SelectedEvent.EndDate = EditingDetail.EndDate;
        SelectedEvent.Notes = EditingDetail.Notes;
        SelectedEvent.Details = EditingDetail.Details;
        SelectedEvent.EstimatedHours = EditingDetail.EstimatedHours;
        SelectedEvent.HourlyRate = EditingDetail.HourlyRate;

        var allocation = _fakeData.ScheduleAllocations
            .FirstOrDefault(a => a.AllocationId == SelectedEvent.AllocationId);
        if (allocation != null)
        {
            allocation.StartDate = EditingDetail.StartDate;
            allocation.EndDate = EditingDetail.EndDate;
        }

        UpdateCalendarEvents();
        IsDetailPanelVisible = false;
    }

    [RelayCommand]
    private void DeleteEvent()
    {
        if (SelectedEvent == null) return;

        var allocation = _fakeData.ScheduleAllocations
            .FirstOrDefault(a => a.AllocationId == SelectedEvent.AllocationId);
        if (allocation != null)
        {
            _fakeData.ScheduleAllocations.Remove(allocation);
        }

        ClearSelection();
        Refresh();
    }

    // ─── Event Resize Handling ───

    public void HandleEventResized(CalendarEvent evt, DateTime newStart, DateTime newEnd)
    {
        var allocation = _fakeData.ScheduleAllocations
            .FirstOrDefault(a => a.AllocationId == evt.AllocationId);
        if (allocation != null)
        {
            allocation.StartDate = newStart;
            allocation.EndDate = newEnd;
        }

        Refresh();
    }

    // ─── Project Selection ───

    [RelayCommand]
    private void SelectProject(Project project)
    {
        SelectedProject = project;
        FilteredAllocations = Allocations.Where(a => a.ProjectId == project.ProjectId).ToList();
        UpdateCalendarEvents();
        UpdateStats();
    }

    [RelayCommand]
    private void ClearProject()
    {
        SelectedProject = null;
        FilteredAllocations = Allocations;
        UpdateCalendarEvents();
        UpdateStats();
    }

    [RelayCommand]
    private void FilterByDateRange()
    {
        FilteredAllocations = Allocations
            .Where(a => a.StartDate >= ViewStartDate && a.EndDate <= ViewEndDate)
            .ToList();
        UpdateCalendarEvents();
        UpdateStats();
    }

    [RelayCommand]
    private void Search()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredAllocations = Allocations;
            UpdateCalendarEvents();
            UpdateStats();
            return;
        }

        var query = SearchText.ToLower();
        var matchingProjectIds = Projects
            .Where(p => p.ProjectName.ToLower().Contains(query))
            .Select(p => p.ProjectId)
            .ToHashSet();

        FilteredAllocations = Allocations
            .Where(a => matchingProjectIds.Contains(a.ProjectId))
            .ToList();
        UpdateCalendarEvents();
        UpdateStats();
    }

    [RelayCommand]
    private void ZoomIn()
    {
        var mid = ViewStartDate.AddTicks((ViewEndDate - ViewStartDate).Ticks / 2);
        var range = (ViewEndDate - ViewStartDate).TotalDays;
        var newRange = Math.Max(30, range / 2);
        ViewStartDate = mid.AddDays(-newRange / 2);
        ViewEndDate = mid.AddDays(newRange / 2);
        UpdateCalendarEvents();
    }

    [RelayCommand]
    private void ZoomOut()
    {
        var mid = ViewStartDate.AddTicks((ViewEndDate - ViewStartDate).Ticks / 2);
        var range = (ViewEndDate - ViewStartDate).TotalDays;
        var newRange = Math.Min(365, range * 2);
        ViewStartDate = mid.AddDays(-newRange / 2);
        ViewEndDate = mid.AddDays(newRange / 2);
        UpdateCalendarEvents();
    }

    [RelayCommand]
    private void GoToToday()
    {
        ViewStartDate = DateTime.Today.AddMonths(-1);
        ViewEndDate = DateTime.Today.AddMonths(3);
        UpdateCalendarEvents();
    }

    private void UpdateCalendarEvents()
    {
        var projectDict = Projects.ToDictionary(p => p.ProjectId, p => p.ProjectName);
        var categories = _dataStore.GetAll<AssetCategory>().ToDictionary(c => c.CategoryId, c => c.Name);
        var assets = _dataStore.GetAll<SerializedAsset>().ToDictionary(a => a.AssetId, a => a.SerialNumber);
        var techDict = Technicians.ToDictionary(t => t.UserId, t => t.DisplayName);

        var events = new List<CalendarEvent>();

        var projectGroups = FilteredAllocations.GroupBy(a => a.ProjectId);

        foreach (var group in projectGroups)
        {
            var projectName = projectDict.GetValueOrDefault(group.Key, "Unknown Project") ?? "Unknown Project";

            foreach (var alloc in group)
            {
                var categoryName = alloc.CategoryId.HasValue ? categories.GetValueOrDefault(alloc.CategoryId.Value) : null;
                var assetSerial = alloc.SerializedAssetId.HasValue ? assets.GetValueOrDefault(alloc.SerializedAssetId.Value) : null;

                var title = assetSerial ?? categoryName ?? "Allocation";
                var eventType = CalendarEventType.Category;
                if (assetSerial != null)
                    eventType = CalendarEventType.Asset;
                if (alloc.CategoryId == null && alloc.SerializedAssetId == null)
                    eventType = CalendarEventType.Labor;

                var calendarEvent = new CalendarEvent
                {
                    EventId = Guid.NewGuid(),
                    EventType = eventType,
                    AllocationId = alloc.AllocationId,
                    ProjectId = alloc.ProjectId,
                    ProjectName = projectName,
                    Title = title,
                    StartDate = alloc.StartDate,
                    EndDate = alloc.EndDate,
                    Tier = alloc.AllocationTier,
                    CategoryName = categoryName,
                    AssetSerial = assetSerial,
                    BulkQuantity = alloc.BulkQuantity,
                    Notes = alloc.Notes,
                    Details = alloc.Details
                };

                events.Add(calendarEvent);
            }
        }

        CalendarEvents = events;
        DateRangeText = $"{ViewStartDate:MMM dd, yyyy} — {ViewEndDate:MMM dd, yyyy}";
    }

    private void UpdateStats()
    {
        TotalAllocations = FilteredAllocations.Count;
        TotalConflicts = CalendarEvents.Count(e => e.HasConflict);
    }
}
