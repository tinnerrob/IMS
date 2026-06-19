using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IMS.Controls;
using IMS.Models;
using IMS.Models.Enums;
using IMS.Services;

namespace IMS.ViewModels;

/// <summary>
/// Represents a draggable item in the asset/labor list panel.
/// </summary>
public partial class DraggableSchedulingItem : ObservableObject
{
    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _subtitle = string.Empty;

    [ObservableProperty]
    private string _itemType = "Category"; // "Category", "Asset", "Labor"

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private Color _statusColor = Colors.Transparent;

    [ObservableProperty]
    private Guid _categoryId;

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

    public string DragData => $"{ItemType}:{CategoryId}:{AssetId}:{Sku}:{TechnicianId}:{HourlyRate}";
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
    private CalendarEvent? _selectedEvent;

    [ObservableProperty]
    private bool _isEventSelected;

    [ObservableProperty]
    private int _totalConflicts;

    [ObservableProperty]
    private int _totalAllocations;

    [ObservableProperty]
    private string _dateRangeText = string.Empty;

    // ─── Asset/Labor List for Drag & Drop ───
    [ObservableProperty]
    private List<DraggableSchedulingItem> _schedulingItems = new();

    [ObservableProperty]
    private List<DraggableSchedulingItem> _filteredSchedulingItems = new();

    [ObservableProperty]
    private string _schedulingTab = "Categories"; // "Categories", "Assets", "Labor"

    [ObservableProperty]
    private bool _isDragging;

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
        FilteredAllocations = Allocations;
        LoadSchedulingItems();
        UpdateCalendarEvents();
        UpdateStats();
    }

    // ─── Scheduling Items Loading ───

    private void LoadSchedulingItems()
    {
        var items = new List<DraggableSchedulingItem>();

        // Load categories
        var categories = _dataStore.GetAll<AssetCategory>()
            .Where(c => c.ParentCategoryId == null)
            .OrderBy(c => c.Name)
            .ToList();

        foreach (var cat in categories)
        {
            items.Add(new DraggableSchedulingItem
            {
                DisplayName = cat.Name,
                Subtitle = "Category",
                ItemType = "Category",
                CategoryId = cat.CategoryId,
                StatusColor = Color.FromArgb("#5B4DFF")
            });
        }

        // Load serialized assets that are Available
        var assets = _dataStore.GetAll<SerializedAsset>()
            .Where(a => a.CurrentStatus == AssetStatus.Available)
            .OrderBy(a => a.SerialNumber)
            .ToList();

        foreach (var asset in assets)
        {
            items.Add(new DraggableSchedulingItem
            {
                DisplayName = asset.SerialNumber,
                Subtitle = asset.Sku,
                ItemType = "Asset",
                AssetId = asset.AssetId,
                Sku = asset.Sku,
                Status = "Available",
                StatusColor = Color.FromArgb("#22C55E")
            });
        }

        // Load technicians for labor scheduling
        foreach (var tech in Technicians)
        {
            items.Add(new DraggableSchedulingItem
            {
                DisplayName = tech.DisplayName,
                Subtitle = $"${tech.InternalHourlyRate ?? 0:F2}/hr",
                ItemType = "Labor",
                TechnicianId = tech.UserId,
                HourlyRate = tech.InternalHourlyRate,
                StatusColor = Color.FromArgb("#F59E0B")
            });
        }

        SchedulingItems = items;
        FilteredSchedulingItems = items;
    }

    [RelayCommand]
    private void FilterSchedulingItems()
    {
        if (string.IsNullOrWhiteSpace(AssetSearchText))
        {
            FilteredSchedulingItems = SchedulingItems;
            return;
        }

        var query = AssetSearchText.ToLower();
        FilteredSchedulingItems = SchedulingItems
            .Where(a => a.DisplayName.ToLower().Contains(query) ||
                        a.Subtitle.ToLower().Contains(query))
            .ToList();
    }

    [RelayCommand]
    private void SwitchSchedulingTab(string tab)
    {
        SchedulingTab = tab;
        switch (tab)
        {
            case "Categories":
                FilteredSchedulingItems = SchedulingItems.Where(a => a.ItemType == "Category").ToList();
                break;
            case "Assets":
                FilteredSchedulingItems = SchedulingItems.Where(a => a.ItemType == "Asset").ToList();
                break;
            case "Labor":
                FilteredSchedulingItems = SchedulingItems.Where(a => a.ItemType == "Labor").ToList();
                break;
            default:
                FilteredSchedulingItems = SchedulingItems;
                break;
        }
    }

    // ─── Drag & Drop Handling ───

    /// <summary>
    /// Called when an item is dropped onto a specific date on the calendar.
    /// Creates the appropriate allocation based on item type.
    /// </summary>
    public void HandleDrop(string dragData, DateTime dropDate)
    {
        if (SelectedProject == null) return;

        var parts = dragData.Split(':');
        if (parts.Length < 2) return;

        var itemType = parts[0];
        var categoryIdStr = parts[1];
        var assetIdStr = parts.Length > 2 ? parts[2] : null;
        var sku = parts.Length > 3 ? parts[3] : null;
        var techIdStr = parts.Length > 4 ? parts[4] : null;
        var hourlyRateStr = parts.Length > 5 ? parts[5] : null;

        if (!Guid.TryParse(categoryIdStr, out var categoryId)) return;

        Guid? assetId = null;
        if (!string.IsNullOrEmpty(assetIdStr) && Guid.TryParse(assetIdStr, out var parsedAssetId))
            assetId = parsedAssetId;

        Guid? techId = null;
        if (!string.IsNullOrEmpty(techIdStr) && Guid.TryParse(techIdStr, out var parsedTechId))
            techId = parsedTechId;

        decimal? hourlyRate = null;
        if (!string.IsNullOrEmpty(hourlyRateStr) && decimal.TryParse(hourlyRateStr, out var parsedRate))
            hourlyRate = parsedRate;

        switch (itemType)
        {
            case "Category":
                CreateCategoryAllocation(categoryId, dropDate, SelectedProject.ProjectId);
                break;
            case "Asset":
                CreateAssetAllocation(assetId, dropDate, SelectedProject.ProjectId);
                break;
            case "Labor":
                CreateLaborAllocation(techId, hourlyRate, dropDate, SelectedProject.ProjectId);
                break;
        }

        Refresh();
    }

    private void CreateCategoryAllocation(Guid categoryId, DateTime dropDate, Guid projectId)
    {
        var allocation = new ScheduleAllocation
        {
            AllocationId = Guid.NewGuid(),
            ProjectId = projectId,
            AllocationTier = AllocationTier.Soft_Hold,
            StartDate = dropDate,
            EndDate = dropDate.AddDays(14),
            CategoryId = categoryId,
            BulkQuantity = 1
        };
        _fakeData.ScheduleAllocations.Add(allocation);
    }

    private void CreateAssetAllocation(Guid? assetId, DateTime dropDate, Guid projectId)
    {
        if (assetId == null) return;
        var allocation = new ScheduleAllocation
        {
            AllocationId = Guid.NewGuid(),
            ProjectId = projectId,
            AllocationTier = AllocationTier.Soft_Hold,
            StartDate = dropDate,
            EndDate = dropDate.AddDays(14),
            SerializedAssetId = assetId,
            BulkQuantity = 0
        };
        _fakeData.ScheduleAllocations.Add(allocation);
    }

    private void CreateLaborAllocation(Guid? techId, decimal? hourlyRate, DateTime dropDate, Guid projectId)
    {
        if (techId == null) return;
        var allocation = new ScheduleAllocation
        {
            AllocationId = Guid.NewGuid(),
            ProjectId = projectId,
            AllocationTier = AllocationTier.Soft_Hold,
            StartDate = dropDate,
            EndDate = dropDate.AddDays(1), // Default 1 day for labor
            SerializedAssetId = null,
            BulkQuantity = 0
        };
        _fakeData.ScheduleAllocations.Add(allocation);
    }

    // ─── Event Selection & Detail Editing ───

    [RelayCommand]
    private void SelectEvent(CalendarEvent calendarEvent)
    {
        SelectedEvent = calendarEvent;
        IsEventSelected = true;

        // Populate detail panel
        EditingDetail = new EditableEventDetail
        {
            EventId = calendarEvent.EventId,
            Title = calendarEvent.Title,
            EventType = calendarEvent.EventType.ToString(),
            ProjectName = calendarEvent.ProjectName,
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

        // Update the calendar event
        SelectedEvent.Title = EditingDetail.Title;
        SelectedEvent.StartDate = EditingDetail.StartDate;
        SelectedEvent.EndDate = EditingDetail.EndDate;
        SelectedEvent.Notes = EditingDetail.Notes;
        SelectedEvent.Details = EditingDetail.Details;
        SelectedEvent.EstimatedHours = EditingDetail.EstimatedHours;
        SelectedEvent.HourlyRate = EditingDetail.HourlyRate;

        // Update the underlying allocation
        var allocation = _fakeData.ScheduleAllocations
            .FirstOrDefault(a => a.AllocationId == SelectedEvent.AllocationId);
        if (allocation != null)
        {
            allocation.StartDate = EditingDetail.StartDate;
            allocation.EndDate = EditingDetail.EndDate;
        }

        // Refresh display
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

        // Group allocations by project
        var projectGroups = FilteredAllocations.GroupBy(a => a.ProjectId);

        foreach (var group in projectGroups)
        {
            var projectName = projectDict.GetValueOrDefault(group.Key, "Unknown Project") ?? "Unknown Project";

            // Create a project-level event (header row)
            var projectEvent = new CalendarEvent
            {
                EventId = Guid.NewGuid(),
                EventType = CalendarEventType.Project,
                ProjectId = group.Key,
                ProjectName = projectName,
                Title = projectName,
                StartDate = group.Min(a => a.StartDate),
                EndDate = group.Max(a => a.EndDate),
                IsExpanded = true
            };
            events.Add(projectEvent);

            foreach (var alloc in group)
            {
                var categoryName = alloc.CategoryId.HasValue ? categories.GetValueOrDefault(alloc.CategoryId.Value) : null;
                var assetSerial = alloc.SerializedAssetId.HasValue ? assets.GetValueOrDefault(alloc.SerializedAssetId.Value) : null;

                var title = assetSerial ?? categoryName ?? "Allocation";
                var eventType = CalendarEventType.Category;
                if (assetSerial != null)
                    eventType = CalendarEventType.Asset;

                // Check if this might be a labor allocation (no category or asset)
                if (alloc.CategoryId == null && alloc.SerializedAssetId == null)
                    eventType = CalendarEventType.Labor;

                var calendarEvent = new CalendarEvent
                {
                    EventId = Guid.NewGuid(),
                    EventType = eventType,
                    ParentEventId = projectEvent.EventId,
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
