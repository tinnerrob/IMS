using IMS.Controls;
using IMS.ViewModels;

namespace IMS.Views;

public partial class SchedulingPage : ContentPage
{
    private readonly SchedulingViewModel _viewModel;
    private bool _isInitialized;

    public SchedulingPage(SchedulingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        // Wire up calendar events after the content is loaded
        // (The page itself may not be added to visual tree since MainLayout extracts Content)
        Loaded += OnPageLoaded;
        // Also try to initialize immediately as a fallback
        InitializeCalendar();
    }

    private void OnPageLoaded(object? sender, EventArgs e)
    {
        Loaded -= OnPageLoaded;
        InitializeCalendar();
    }

    private void InitializeCalendar()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        try
        {
            // Initialize calendar with current data
            CalendarControl.SetDateRange(_viewModel.ViewStartDate, _viewModel.ViewEndDate);
            CalendarControl.SetEvents(_viewModel.CalendarEvents);

            // Listen for property changes to update calendar
            _viewModel.PropertyChanged += (s, args) =>
            {
                try
                {
                    if (args.PropertyName == nameof(SchedulingViewModel.CalendarEvents))
                    {
                        CalendarControl.SetEvents(_viewModel.CalendarEvents);
                    }
                    if (args.PropertyName == nameof(SchedulingViewModel.ViewStartDate) ||
                        args.PropertyName == nameof(SchedulingViewModel.ViewEndDate))
                    {
                        CalendarControl.SetDateRange(_viewModel.ViewStartDate, _viewModel.ViewEndDate);
                    }
                }
                catch
                {
                    // Silently handle calendar update errors
                }
            };

            // Handle event taps from calendar (click to edit)
            CalendarControl.EventTapped += (s, calendarEvent) =>
            {
                _viewModel.SelectEventCommand.Execute(calendarEvent);
            };

            // Handle drops from resource list onto calendar
            CalendarControl.ItemDropped += (s, dropInfo) =>
            {
                _viewModel.HandleDrop(dropInfo.DragData, dropInfo.DropDate);
            };

            // Handle event resize (drag edges to expand/contract)
            CalendarControl.EventResized += (s, resizeInfo) =>
            {
                _viewModel.HandleEventResized(resizeInfo.Event, resizeInfo.NewStart, resizeInfo.NewEnd);
            };
        }
        catch
        {
            // Silently handle initialization errors
        }
    }

    /// <summary>
    /// Called when a drag starts from the resource list.
    /// Sets the drag data to the DragData property of the DraggableSchedulingItem.
    /// </summary>
    private void OnDragStarting(object? sender, DragStartingEventArgs e)
    {
        try
        {
            if (sender is GestureRecognizer gesture &&
                gesture.Parent is Label label &&
                label.Parent is Grid grid &&
                grid.Parent is Border border &&
                border.BindingContext is DraggableSchedulingItem item)
            {
                e.Data.Text = item.DragData;
                e.Data.Properties["SchedulingItem"] = item;
            }
        }
        catch
        {
            // Silently handle drag errors
        }
    }

    /// <summary>
    /// Called when the resource search text changes.
    /// </summary>
    private void OnAssetSearchChanged(object? sender, TextChangedEventArgs e)
    {
        _viewModel.AssetSearchText = e.NewTextValue;
        _viewModel.FilterSchedulingItemsCommand.Execute(null);
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
