using IMS.Models;
using IMS.Models.Enums;

namespace IMS.Controls;

/// <summary>
/// Represents the type of a calendar event in the multi-tier scheduling system.
/// </summary>
public enum CalendarEventType
{
    Project,
    Category,
    Asset,
    Labor
}

/// <summary>
/// View mode for the calendar tree.
/// </summary>
public enum CalendarViewMode
{
    /// <summary>Customer → Project → Resource hierarchy</summary>
    GroupByCustomer,
    /// <summary>Category → Asset → Customer/Project hierarchy</summary>
    GroupByResource
}

/// <summary>
/// A node in the calendar tree hierarchy.
/// </summary>
public class CalendarTreeNode
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Depth { get; set; }
    public bool IsExpanded { get; set; } = true;
    public bool IsExpandable { get; set; }
    public CalendarEventType NodeType { get; set; }
    public List<CalendarTreeNode> Children { get; set; } = new();

    // References back to source data
    public Guid? CustomerId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? AssetId { get; set; }
    public Guid? TechnicianId { get; set; }

    // The allocation event bar data (only on leaf nodes)
    public CalendarEvent? CalendarEvent { get; set; }
}

/// <summary>
/// Represents a calendar event for display in the scheduling calendar.
/// </summary>
public class CalendarEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public CalendarEventType EventType { get; set; }
    public Guid? ParentEventId { get; set; }

    // Core scheduling data
    public Guid AllocationId { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public AllocationTier Tier { get; set; } = AllocationTier.Soft_Hold;

    // Resource identifiers
    public string? CategoryName { get; set; }
    public string? AssetSerial { get; set; }
    public int BulkQuantity { get; set; }

    // Labor-specific
    public Guid? TechnicianId { get; set; }
    public string? TechnicianName { get; set; }
    public decimal? HourlyRate { get; set; }
    public decimal? EstimatedHours { get; set; }

    // Notes & details
    public string? Notes { get; set; }
    public string? Details { get; set; }

    // Conflict detection
    public bool HasConflict { get; set; }
    public string? ConflictDescription { get; set; }

    // Visual properties
    public bool IsExpanded { get; set; } = true;

    public Color EventColor => EventType switch
    {
        CalendarEventType.Project => Color.FromArgb("#5B4DFF"),
        CalendarEventType.Category => Color.FromArgb("#0EA5E9"),
        CalendarEventType.Asset => Color.FromArgb("#22C55E"),
        CalendarEventType.Labor => Color.FromArgb("#F59E0B"),
        _ => Color.FromArgb("#9CA3AF")
    };

    public Color EventBgColor => EventType switch
    {
        CalendarEventType.Project => Color.FromArgb("#EEEAFF"),
        CalendarEventType.Category => Color.FromArgb("#E0F2FE"),
        CalendarEventType.Asset => Color.FromArgb("#DCFCE7"),
        CalendarEventType.Labor => Color.FromArgb("#FEF3C7"),
        _ => Color.FromArgb("#F1F3F4")
    };

    public Color ConflictBgColor => Color.FromArgb("#FEE2E2");
    public Color ConflictBorderColor => Color.FromArgb("#DC2626");

    public string TypeIcon => EventType switch
    {
        CalendarEventType.Project => "📋",
        CalendarEventType.Category => "📦",
        CalendarEventType.Asset => "🔧",
        CalendarEventType.Labor => "👤",
        _ => "📌"
    };
}

/// <summary>
/// A custom calendar view for multi-tier scheduling that displays allocations
/// as events on a timeline grid with a hierarchical tree on the left.
/// Supports two view modes: GroupByCustomer and GroupByResource.
/// </summary>
public class SchedulingCalendar : ContentView
{
    private readonly Grid _calendarGrid;
    private readonly ScrollView _scrollView;
    private DateTime _viewStartDate;
    private DateTime _viewEndDate;
    private List<CalendarEvent> _events = new();
    private List<CalendarTreeNode> _treeNodes = new();
    private List<CalendarTreeNode> _visibleRows = new();
    private int _totalDays;
    private bool _isRendering;
    private bool _renderPending;

    private const int DayColumnWidth = 40;
    private const int RowHeight = 44;
    private const int HeaderHeight = 44;
    private const int TreeColumnWidth = 280;

    // Drag resize state
    private CalendarEvent? _resizingEvent;
    private DateTime _resizeStartDate;

    private CalendarViewMode _viewMode = CalendarViewMode.GroupByCustomer;

    /// <summary>
    /// Gets or sets the current view mode.
    /// </summary>
    public CalendarViewMode ViewMode
    {
        get => _viewMode;
        set
        {
            if (_viewMode == value) return;
            _viewMode = value;
            RenderCalendar();
        }
    }

    /// <summary>
    /// Fired when an item is dropped onto a specific date.
    /// </summary>
    public event EventHandler<(string DragData, DateTime DropDate)>? ItemDropped;

    /// <summary>
    /// Fired when a calendar event is tapped (click to edit).
    /// </summary>
    public event EventHandler<CalendarEvent>? EventTapped;

    /// <summary>
    /// Fired when an event is resized by dragging its edge.
    /// </summary>
    public event EventHandler<(CalendarEvent Event, DateTime NewStart, DateTime NewEnd)>? EventResized;

    public SchedulingCalendar()
    {
        _viewStartDate = DateTime.Today.AddMonths(-1);
        _viewEndDate = DateTime.Today.AddMonths(3);

        _calendarGrid = new Grid
        {
            BackgroundColor = Color.FromArgb("#FFFFFF"),
            Padding = new Thickness(0)
        };

        _scrollView = new ScrollView
        {
            Orientation = ScrollOrientation.Vertical,
            Content = _calendarGrid,
            BackgroundColor = Color.FromArgb("#FFFFFF")
        };

        Content = _scrollView;
    }

    public void SetDateRange(DateTime start, DateTime end)
    {
        if (_viewStartDate == start && _viewEndDate == end) return;
        _viewStartDate = start;
        _viewEndDate = end;
        RenderCalendar();
    }

    public void SetEvents(List<CalendarEvent> events)
    {
        _events = events;
        DetectConflicts();
        RenderCalendar();
    }

    /// <summary>
    /// Sets the tree nodes for the hierarchical view.
    /// </summary>
    public void SetTreeNodes(List<CalendarTreeNode> nodes)
    {
        _treeNodes = nodes;
        RenderCalendar();
    }

    private void DetectConflicts()
    {
        foreach (var evt in _events)
        {
            evt.HasConflict = false;
            evt.ConflictDescription = null;
        }

        var resourceGroups = new Dictionary<string, List<CalendarEvent>>();

        foreach (var evt in _events.Where(e => e.EventType != CalendarEventType.Project))
        {
            var key = evt.CategoryName ?? evt.AssetSerial ?? evt.TechnicianName ?? "unknown";
            if (!resourceGroups.ContainsKey(key))
                resourceGroups[key] = new List<CalendarEvent>();
            resourceGroups[key].Add(evt);
        }

        foreach (var group in resourceGroups.Values)
        {
            for (int i = 0; i < group.Count; i++)
            {
                for (int j = i + 1; j < group.Count; j++)
                {
                    var a = group[i];
                    var b = group[j];

                    if (a.StartDate < b.EndDate && a.EndDate > b.StartDate)
                    {
                        a.HasConflict = true;
                        b.HasConflict = true;
                        a.ConflictDescription = $"Overlaps with: {b.ProjectName} - {b.Title}";
                        b.ConflictDescription = $"Overlaps with: {a.ProjectName} - {a.Title}";
                    }
                }
            }
        }
    }

    private void BuildVisibleRows()
    {
        _visibleRows = new List<CalendarTreeNode>();

        void Flatten(List<CalendarTreeNode> nodes)
        {
            foreach (var node in nodes)
            {
                _visibleRows.Add(node);
                if (node.IsExpanded && node.Children.Count > 0)
                {
                    Flatten(node.Children);
                }
            }
        }

        Flatten(_treeNodes);
    }

    private void RenderCalendar()
    {
        System.Diagnostics.Debug.WriteLine($"[SchedulingCalendar] RenderCalendar called, _isRendering={_isRendering}, _renderPending={_renderPending}");

        // If a render is already in progress, mark that another is pending
        if (_isRendering)
        {
            _renderPending = true;
            System.Diagnostics.Debug.WriteLine("[SchedulingCalendar] RenderCalendar: already rendering, marking pending");
            return;
        }

        _isRendering = true;
        _renderPending = false;

        // Guard: don't render if the view hasn't been loaded yet
        if (_calendarGrid == null)
        {
            System.Diagnostics.Debug.WriteLine("[SchedulingCalendar] RenderCalendar: _calendarGrid is null, aborting");
            _isRendering = false;
            return;
        }

        _totalDays = (int)(_viewEndDate - _viewStartDate).TotalDays + 1;
        if (_totalDays <= 0) _totalDays = 1;

        BuildVisibleRows();
        System.Diagnostics.Debug.WriteLine($"[SchedulingCalendar] RenderCalendar: _totalDays={_totalDays}, _visibleRows.Count={_visibleRows.Count}");

        // Schedule the UI update on the main thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[SchedulingCalendar] BeginInvokeOnMainThread: starting render");
                _calendarGrid.Children.Clear();
                _calendarGrid.RowDefinitions.Clear();
                _calendarGrid.ColumnDefinitions.Clear();

                // Column definitions: Tree column + day columns
                _calendarGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(TreeColumnWidth)));
                for (int i = 0; i < _totalDays; i++)
                {
                    _calendarGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(DayColumnWidth)));
                }

                // Row definitions: Header + data rows
                _calendarGrid.RowDefinitions.Add(new RowDefinition(new GridLength(HeaderHeight)));

                for (int i = 0; i < _visibleRows.Count; i++)
                {
                    _calendarGrid.RowDefinitions.Add(new RowDefinition(new GridLength(RowHeight)));
                }

                // Render header
                RenderHeader();

                // Render tree rows + event bars
                for (int r = 0; r < _visibleRows.Count; r++)
                {
                    RenderTreeRow(r);
                }

                // Today indicator
                RenderTodayLine();
                System.Diagnostics.Debug.WriteLine("[SchedulingCalendar] BeginInvokeOnMainThread: render complete");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SchedulingCalendar] RenderCalendar error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[SchedulingCalendar] StackTrace: {ex.StackTrace}");
            }
            finally
            {
                _isRendering = false;

                // If another render was requested while we were rendering, do it now
                if (_renderPending)
                {
                    System.Diagnostics.Debug.WriteLine("[SchedulingCalendar] RenderCalendar: pending render detected, re-rendering");
                    MainThread.BeginInvokeOnMainThread(() => RenderCalendar());
                }
            }
        });
    }

    private void RenderHeader()
    {
        // Corner label
        var cornerLabel = new Label
        {
            Text = _viewMode == CalendarViewMode.GroupByCustomer ? "Customer / Project" : "Resource",
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#5F6368"),
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start,
            Padding = new Thickness(12, 0)
        };
        Grid.SetRow(cornerLabel, 0);
        Grid.SetColumn(cornerLabel, 0);
        _calendarGrid.Children.Add(cornerLabel);

        // Day headers
        for (int i = 0; i < _totalDays; i++)
        {
            var date = _viewStartDate.AddDays(i);
            var isToday = date.Date == DateTime.Today;
            var isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;

            var dayGrid = new Grid
            {
                BackgroundColor = isToday ? Color.FromArgb("#EEEAFF") :
                                  isWeekend ? Color.FromArgb("#F8F9FA") : Colors.Transparent
            };

            var dayLabel = new Label
            {
                Text = date.Day.ToString(),
                FontSize = 12,
                FontAttributes = isToday ? FontAttributes.Bold : FontAttributes.None,
                TextColor = isToday ? Color.FromArgb("#5B4DFF") : Color.FromArgb("#5F6368"),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            if (i == 0 || date.Day == 1)
            {
                var monthLabel = new Label
                {
                    Text = date.ToString("MMM"),
                    FontSize = 9,
                    TextColor = Color.FromArgb("#9CA3AF"),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.End,
                    Margin = new Thickness(0, 0, 0, 2)
                };
                dayGrid.Children.Add(monthLabel);
            }

            dayGrid.Children.Add(dayLabel);

            // Drop target
            var capturedDate = date;
            var dropRecognizer = new DropGestureRecognizer();
            dropRecognizer.DragOver += (s, e) =>
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
            };
            dropRecognizer.Drop += async (s, e) =>
            {
                if (e.Data != null)
                {
                    var dragData = await e.Data.GetTextAsync();
                    if (!string.IsNullOrEmpty(dragData))
                    {
                        ItemDropped?.Invoke(this, (dragData, capturedDate));
                    }
                }
            };
            dayGrid.GestureRecognizers.Add(dropRecognizer);

            var border = new BoxView
            {
                Color = Color.FromArgb("#F1F3F4"),
                WidthRequest = 1,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Fill
            };
            dayGrid.Children.Add(border);

            Grid.SetRow(dayGrid, 0);
            Grid.SetColumn(dayGrid, i + 1);
            _calendarGrid.Children.Add(dayGrid);
        }
    }

    private void RenderTreeRow(int r)
    {
        var node = _visibleRows[r];
        var rowIndex = r + 1;
        var isLeaf = node.CalendarEvent != null;

        // Row background
        var rowBg = new BoxView
        {
            Color = node.Depth == 0 ? Color.FromArgb("#F3F0FF") :
                    r % 2 == 0 ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#F8F9FA"),
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill
        };
        Grid.SetRow(rowBg, rowIndex);
        Grid.SetColumn(rowBg, 0);
        Grid.SetColumnSpan(rowBg, _totalDays + 1);
        _calendarGrid.Children.Add(rowBg);

        // Tree label with indent
        var indent = node.Depth * 20;
        var labelContent = new HorizontalStackLayout
        {
            Spacing = 4,
            Padding = new Thickness(8 + indent, 0, 4, 0),
            Children =
            {
                new Label
                {
                    Text = node.Icon,
                    FontSize = 12,
                    VerticalOptions = LayoutOptions.Center
                }
            }
        };

        // Expand/collapse toggle
        if (node.IsExpandable)
        {
            var toggleBtn = new Label
            {
                Text = node.IsExpanded ? "▼" : "▶",
                FontSize = 9,
                TextColor = Color.FromArgb("#5F6368"),
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 0, 2, 0)
            };
            labelContent.Children.Insert(0, toggleBtn);

            var capturedNode = node;
            var tapToggle = new TapGestureRecognizer();
            tapToggle.Tapped += (s, e) =>
            {
                capturedNode.IsExpanded = !capturedNode.IsExpanded;
                RenderCalendar();
            };
            labelContent.GestureRecognizers.Add(tapToggle);
        }

        // Label text
        var label = new Label
        {
            Text = node.Label,
            FontSize = node.Depth <= 1 ? 12 : 11,
            FontAttributes = node.Depth == 0 ? FontAttributes.Bold : FontAttributes.None,
            TextColor = Color.FromArgb("#1A1A2E"),
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.TailTruncation
        };
        labelContent.Children.Add(label);

        var resourceFrame = new Frame
        {
            Padding = new Thickness(0),
            BackgroundColor = Colors.Transparent,
            CornerRadius = 0,
            HasShadow = false,
            BorderColor = Colors.Transparent,
            Content = labelContent
        };
        Grid.SetRow(resourceFrame, rowIndex);
        Grid.SetColumn(resourceFrame, 0);
        _calendarGrid.Children.Add(resourceFrame);

        // Render event bar if this is a leaf node
        if (isLeaf && node.CalendarEvent != null)
        {
            RenderEventBar(node.CalendarEvent, rowIndex);
        }

        // Horizontal border
        var hBorder = new BoxView
        {
            Color = Color.FromArgb("#F1F3F4"),
            HeightRequest = 1,
            VerticalOptions = LayoutOptions.End,
            HorizontalOptions = LayoutOptions.Fill
        };
        Grid.SetRow(hBorder, rowIndex);
        Grid.SetColumn(hBorder, 0);
        Grid.SetColumnSpan(hBorder, _totalDays + 1);
        _calendarGrid.Children.Add(hBorder);
    }

    private void RenderEventBar(CalendarEvent evt, int rowIndex)
    {
        var startCol = Math.Max(1, (int)(evt.StartDate - _viewStartDate).TotalDays + 1);
        var endCol = Math.Min(_totalDays, (int)(evt.EndDate - _viewStartDate).TotalDays + 1);
        var span = Math.Max(1, endCol - startCol + 1);

        if (startCol > _totalDays || endCol < 1) return;
        if (startCol < 1) startCol = 1;
        if (endCol > _totalDays) endCol = _totalDays;
        span = endCol - startCol + 1;

        var eventBg = evt.HasConflict ? evt.ConflictBgColor : evt.EventBgColor;
        var eventBorder = evt.HasConflict ? evt.ConflictBorderColor : evt.EventColor;

        var eventFrame = new Frame
        {
            Padding = new Thickness(6, 2),
            BackgroundColor = eventBg,
            CornerRadius = 4,
            HasShadow = false,
            BorderColor = eventBorder,
            Margin = new Thickness(2, 2),
            Content = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                },
                Children =
                {
                    new Label
                    {
                        Text = $"{evt.TypeIcon} {evt.Title}",
                        FontSize = 10,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#1A1A2E"),
                        LineBreakMode = LineBreakMode.TailTruncation,
                        MaxLines = 1,
                        VerticalOptions = LayoutOptions.Center
                    },
                    new Label
                    {
                        Text = evt.HasConflict ? "⚠️" : "",
                        FontSize = 10,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.End
                    }
                }
            }
        };

        // Tap to edit
        var capturedEvent = evt;
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) => EventTapped?.Invoke(this, capturedEvent);
        eventFrame.GestureRecognizers.Add(tapGesture);

        // Drag handles on left and right edges for resize
        var leftHandle = new BoxView
        {
            Color = Colors.Transparent,
            WidthRequest = 6,
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Start
        };
        var leftPan = new PanGestureRecognizer();
        leftPan.PanUpdated += (s, e) =>
        {
            if (e.StatusType == GestureStatus.Started)
            {
                _resizingEvent = capturedEvent;
                _resizeStartDate = capturedEvent.StartDate;
            }
            else if (e.StatusType == GestureStatus.Completed && _resizingEvent != null)
            {
                var daysDelta = (int)(e.TotalX / DayColumnWidth);
                var newStart = _resizeStartDate.AddDays(daysDelta);
                if (newStart < capturedEvent.EndDate)
                {
                    EventResized?.Invoke(this, (capturedEvent, newStart, capturedEvent.EndDate));
                }
                _resizingEvent = null;
            }
        };
        leftHandle.GestureRecognizers.Add(leftPan);

        var rightHandle = new BoxView
        {
            Color = Colors.Transparent,
            WidthRequest = 6,
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.End
        };
        var rightPan = new PanGestureRecognizer();
        rightPan.PanUpdated += (s, e) =>
        {
            if (e.StatusType == GestureStatus.Started)
            {
                _resizingEvent = capturedEvent;
                _resizeStartDate = capturedEvent.EndDate;
            }
            else if (e.StatusType == GestureStatus.Completed && _resizingEvent != null)
            {
                var daysDelta = (int)(e.TotalX / DayColumnWidth);
                var newEnd = _resizeStartDate.AddDays(daysDelta);
                if (newEnd > capturedEvent.StartDate)
                {
                    EventResized?.Invoke(this, (capturedEvent, capturedEvent.StartDate, newEnd));
                }
                _resizingEvent = null;
            }
        };
        rightHandle.GestureRecognizers.Add(rightPan);

        var overlayGrid = new Grid
        {
            Children = { eventFrame, leftHandle, rightHandle }
        };

        Grid.SetRow(overlayGrid, rowIndex);
        Grid.SetColumn(overlayGrid, startCol);
        Grid.SetColumnSpan(overlayGrid, span);
        _calendarGrid.Children.Add(overlayGrid);
    }

    private void RenderTodayLine()
    {
        var todayOffset = (int)(DateTime.Today - _viewStartDate).TotalDays + 1;
        if (todayOffset >= 1 && todayOffset <= _totalDays)
        {
            var todayLine = new BoxView
            {
                Color = Color.FromArgb("#5B4DFF"),
                WidthRequest = 2,
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Center
            };
            Grid.SetRow(todayLine, 0);
            Grid.SetColumn(todayLine, todayOffset);
            Grid.SetRowSpan(todayLine, _visibleRows.Count + 1);
            _calendarGrid.Children.Add(todayLine);
        }
    }
}
