using IMS.Controls;
using IMS.Helpers;
using IMS.ViewModels;

namespace IMS.Views;

public partial class SchedulingPage : ContentPage, IInitializablePage
{
    private readonly SchedulingViewModel _viewModel;
    private bool _isInitialized;

    public SchedulingPage(SchedulingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        InitializeCalendar();
    }

    public void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        try
        {
            // Initialize the ViewModel data
            _viewModel.Initialize();

            // Initialize calendar with current data
            CalendarControl.SetDateRange(_viewModel.ViewStartDate, _viewModel.ViewEndDate);
            CalendarControl.SetEvents(_viewModel.CalendarEvents);
            CalendarControl.SetTreeNodes(_viewModel.CalendarTreeNodes);

            // Listen for property changes to update calendar
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            // Handle event taps from calendar (click to edit)
            CalendarControl.EventTapped += OnCalendarEventTapped;

            // Handle drops from resource list onto calendar
            CalendarControl.ItemDropped += OnCalendarItemDropped;

            // Handle event resize (drag edges to expand/contract)
            CalendarControl.EventResized += OnCalendarEventResized;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SchedulingPage] InitializeCalendar error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[SchedulingPage] StackTrace: {ex.StackTrace}");
        }
    }

    private void InitializeCalendar()
    {
        // This is now handled by the Initialize() method
        // Kept for backward compatibility with OnAppearing
        Initialize();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args)
    {
        try
        {
            switch (args.PropertyName)
            {
                case nameof(SchedulingViewModel.CalendarEvents):
                    CalendarControl.SetEvents(_viewModel.CalendarEvents);
                    break;
                case nameof(SchedulingViewModel.CalendarTreeNodes):
                    CalendarControl.SetTreeNodes(_viewModel.CalendarTreeNodes);
                    break;
                case nameof(SchedulingViewModel.ViewStartDate):
                case nameof(SchedulingViewModel.ViewEndDate):
                    CalendarControl.SetDateRange(_viewModel.ViewStartDate, _viewModel.ViewEndDate);
                    break;
                case nameof(SchedulingViewModel.CalendarViewMode):
                    CalendarControl.ViewMode = _viewModel.CalendarViewMode;
                    break;
            }
        }
        catch
        {
            // Silently handle calendar update errors
        }
    }

    private void OnCalendarEventTapped(object? sender, CalendarEvent calendarEvent)
    {
        _viewModel.SelectEventCommand.Execute(calendarEvent);
    }

    private void OnCalendarItemDropped(object? sender, (string DragData, DateTime DropDate) dropInfo)
    {
        _viewModel.HandleDrop(dropInfo.DragData, dropInfo.DropDate);
    }

    private void OnCalendarEventResized(object? sender, (CalendarEvent Event, DateTime NewStart, DateTime NewEnd) resizeInfo)
    {
        _viewModel.HandleEventResized(resizeInfo.Event, resizeInfo.NewStart, resizeInfo.NewEnd);
    }

    /// <summary>
    /// Called when a drag starts from the resource list.
    /// Uses the BindingContext of the item to get drag data.
    /// </summary>
    private void OnDragStarting(object? sender, DragStartingEventArgs e)
    {
        try
        {
            // Walk up the visual tree to find the item's BindingContext
            if (sender is GestureRecognizer gesture)
            {
                var element = gesture.Parent;
                while (element != null)
                {
                    if (element.BindingContext is DraggableSchedulingItem item)
                    {
                        e.Data.Text = item.DragData;
                        e.Data.Properties["SchedulingItem"] = item;
                        return;
                    }
                    element = element.Parent;
                }
            }
        }
        catch
        {
            // Silently handle drag errors
        }
    }

    /// <summary>
    /// Called when the customer/project search text changes.
    /// </summary>
    private void OnCustomerSearchChanged(object? sender, TextChangedEventArgs e)
    {
        _viewModel.CustomerSearchText = e.NewTextValue;
        _viewModel.FilterCustomerProjectsCommand.Execute(null);
    }

    /// <summary>
    /// Called when the resource search text changes.
    /// </summary>
    private void OnResourceSearchChanged(object? sender, TextChangedEventArgs e)
    {
        _viewModel.ResourceSearchText = e.NewTextValue;
        _viewModel.FilterResourcesCommand.Execute(null);
    }

    /// <summary>
    /// Called when the project search text changes.
    /// </summary>
    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        _viewModel.SearchText = e.NewTextValue;
        _viewModel.SearchCommand.Execute(null);
    }
}
