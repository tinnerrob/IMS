using IMS.Models;
using IMS.Models.Enums;

namespace IMS.Controls;

/// <summary>
/// Represents the type of a calendar event in the multi-tier scheduling system.
/// </summary>
public enum CalendarEventType
{
    Project,    // Top-level project/customer row
    Category,   // Asset category allocation
    Asset,      // Specific serialized asset
    Labor       // Technician/labor allocation
}

/// <summary>
/// Represents a calendar event for display in the scheduling calendar.
/// Supports multi-tier hierarchy: Project > Category/Asset/Labor.
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
    public int IndentLevel => EventType switch
    {
        CalendarEventType.Project => 0,
        CalendarEventType.Category => 1,
        CalendarEventType.Asset => 2,
        CalendarEventType.Labor => 1,
        _ => 0
    };

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
/// as events on a timeline grid with conflict detection, drag-to-resize, and click-to-edit.
/// </summary>
public class SchedulingCalendar : ContentView
{
    private readonly Grid _calendarGrid;
    private readonly ScrollView _scrollView;
    private DateTime _viewStartDate;
    private DateTime _viewEndDate;
    private List<CalendarEvent> _events = new();
    private const int DayColumnWidth = 40;
    private const int RowHeight = 48;
    private const int ProjectRowHeight = 52;
    private const int HeaderHeight = 44;
    private const int TimelineWidth = 220;

    // Drag resize state
    private CalendarEvent? _resizingEvent;
    private DateTime _resizeStartDate;

    /// <summary>
    /// Fired when an asset/category/labor is dropped onto a specific date.
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

    private void DetectConflicts()
    {
        // Reset conflicts
        foreach (var evt in _events)
        {
            evt.HasConflict = false;
            evt.ConflictDescription = null;
        }

        // Group non-project events by resource (category or asset)
        var resourceGroups = new Dictionary<string, List<CalendarEvent>>();

        foreach (var evt in _events.Where(e => e.EventType != CalendarEventType.Project))
        {
            var key = evt.CategoryName ?? evt.AssetSerial ?? evt.TechnicianName ?? "unknown";
            if (!resourceGroups.ContainsKey(key))
                resourceGroups[key] = new List<CalendarEvent>();
            resourceGroups[key].Add(evt);
        }

        // Check for overlapping dates within each resource group
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

    private void RenderCalendar()
    {
        _calendarGrid.Children.Clear();
        _calendarGrid.RowDefinitions.Clear();
        _calendarGrid.ColumnDefinitions.Clear();

        var totalDays = (int)(_viewEndDate - _viewStartDate).TotalDays + 1;
        if (totalDays <= 0) totalDays = 1;

        // ─── Column Definitions ───
        _calendarGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(TimelineWidth)));
        for (int i = 0; i < totalDays; i++)
        {
            _calendarGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(DayColumnWidth)));
        }

        // ─── Row Definitions ───
        _calendarGrid.RowDefinitions.Add(new RowDefinition(new GridLength(HeaderHeight)));

        var visibleEvents = _events.Where(e => e.IsExpanded || e.EventType == CalendarEventType.Project).ToList();
        var rowMap = new List<CalendarEvent>();

        foreach (var evt in visibleEvents)
        {
            if (evt.EventType == CalendarEventType.Project)
            {
                rowMap.Add(evt);
            }
            else if (evt.ParentEventId.HasValue)
            {
                var parent = visibleEvents.FirstOrDefault(p => p.EventId == evt.ParentEventId.Value);
                if (parent != null && parent.IsExpanded)
                {
                    rowMap.Add(evt);
                }
            }
        }

        for (int i = 0; i < rowMap.Count; i++)
        {
            var isProject = rowMap[i].EventType == CalendarEventType.Project;
            _calendarGrid.RowDefinitions.Add(new RowDefinition(new GridLength(isProject ? ProjectRowHeight : RowHeight)));
        }

        // ─── Render Header Row ───
        var cornerLabel = new Label
        {
            Text = "Resource",
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
        for (int i = 0; i < totalDays; i++)
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
            var dropRecognizer = new DropGestureRecognizer();
            var capturedDate = date;
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

        // ─── Render Resource Rows ───
        for (int r = 0; r < rowMap.Count; r++)
        {
            var evt = rowMap[r];
            var rowIndex = r + 1;
            var isProject = evt.EventType == CalendarEventType.Project;

            // Row background
            var rowBg = new BoxView
            {
                Color = isProject ? Color.FromArgb("#F3F0FF") :
                        r % 2 == 0 ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#F8F9FA"),
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill
            };
            Grid.SetRow(rowBg, rowIndex);
            Grid.SetColumn(rowBg, 0);
            Grid.SetColumnSpan(rowBg, totalDays + 1);
            _calendarGrid.Children.Add(rowBg);

            // Resource label with indent
            var indent = evt.IndentLevel * 16;
            var resourceContent = new HorizontalStackLayout
            {
                Spacing = 6,
                Padding = new Thickness(12 + indent, 0, 4, 0),
                Children =
                {
                    new Label
                    {
                        Text = evt.TypeIcon,
                        FontSize = 14,
                        VerticalOptions = LayoutOptions.Center
                    },
                    new Label
                    {
                        Text = evt.Title,
                        FontSize = isProject ? 13 : 11,
                        FontAttributes = isProject ? FontAttributes.Bold : FontAttributes.None,
                        TextColor = Color.FromArgb("#1A1A2E"),
                        VerticalOptions = LayoutOptions.Center,
                        LineBreakMode = LineBreakMode.TailTruncation
                    }
                }
            };

            // Expand/collapse toggle for projects
            if (isProject)
            {
                var toggleBtn = new Label
                {
                    Text = evt.IsExpanded ? "▼" : "▶",
                    FontSize = 10,
                    TextColor = Color.FromArgb("#5F6368"),
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 0, 4, 0)
                };
                resourceContent.Children.Insert(0, toggleBtn);

                var tapToggle = new TapGestureRecognizer();
                tapToggle.Tapped += (s, e) =>
                {
                    evt.IsExpanded = !evt.IsExpanded;
                    RenderCalendar();
                };
                resourceContent.GestureRecognizers.Add(tapToggle);
            }

            var resourceFrame = new Frame
            {
                Padding = new Thickness(0),
                BackgroundColor = Colors.Transparent,
                CornerRadius = 0,
                HasShadow = false,
                BorderColor = Colors.Transparent,
                Content = resourceContent
            };
            Grid.SetRow(resourceFrame, rowIndex);
            Grid.SetColumn(resourceFrame, 0);
            _calendarGrid.Children.Add(resourceFrame);

            // Render event bar on the calendar
            RenderEventBar(evt, rowIndex, totalDays);

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
            Grid.SetColumnSpan(hBorder, totalDays + 1);
            _calendarGrid.Children.Add(hBorder);
        }

        // ─── Today indicator line ───
        var todayOffset = (int)(DateTime.Today - _viewStartDate).TotalDays + 1;
        if (todayOffset >= 1 && todayOffset <= totalDays)
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
            Grid.SetRowSpan(todayLine, rowMap.Count + 1);
            _calendarGrid.Children.Add(todayLine);
        }
    }

    private void RenderEventBar(CalendarEvent evt, int rowIndex, int totalDays)
    {
        if (evt.EventType == CalendarEventType.Project) return; // Projects are header rows, no bar

        var startCol = Math.Max(1, (int)(evt.StartDate - _viewStartDate).TotalDays + 1);
        var endCol = Math.Min(totalDays, (int)(evt.EndDate - _viewStartDate).TotalDays + 1);
        var span = Math.Max(1, endCol - startCol + 1);

        if (startCol > totalDays || endCol < 1) return;
        if (startCol < 1) startCol = 1;
        if (endCol > totalDays) endCol = totalDays;
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
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) => EventTapped?.Invoke(this, evt);
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
                _resizingEvent = evt;
                _resizeStartDate = evt.StartDate;
            }
            else if (e.StatusType == GestureStatus.Completed && _resizingEvent != null)
            {
                var daysDelta = (int)(e.TotalX / DayColumnWidth);
                var newStart = _resizeStartDate.AddDays(daysDelta);
                if (newStart < evt.EndDate)
                {
                    EventResized?.Invoke(this, (evt, newStart, evt.EndDate));
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
                _resizingEvent = evt;
                _resizeStartDate = evt.EndDate;
            }
            else if (e.StatusType == GestureStatus.Completed && _resizingEvent != null)
            {
                var daysDelta = (int)(e.TotalX / DayColumnWidth);
                var newEnd = _resizeStartDate.AddDays(daysDelta);
                if (newEnd > evt.StartDate)
                {
                    EventResized?.Invoke(this, (evt, evt.StartDate, newEnd));
                }
                _resizingEvent = null;
            }
        };
        rightHandle.GestureRecognizers.Add(rightPan);

        // Add resize handles as overlay
        var overlayGrid = new Grid
        {
            Children = { eventFrame, leftHandle, rightHandle }
        };

        Grid.SetRow(overlayGrid, rowIndex);
        Grid.SetColumn(overlayGrid, startCol);
        Grid.SetColumnSpan(overlayGrid, span);
        _calendarGrid.Children.Add(overlayGrid);
    }

    private List<string> GetUniqueResources()
    {
        var resources = new List<string>();
        foreach (var evt in _events.Where(e => e.EventType != CalendarEventType.Project))
        {
            var key = evt.CategoryName ?? evt.AssetSerial ?? evt.TechnicianName ?? "unknown";
            if (!resources.Contains(key))
                resources.Add(key);
        }
        return resources;
    }
}
