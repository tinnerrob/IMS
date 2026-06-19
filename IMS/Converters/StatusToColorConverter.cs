using System.Globalization;
using IMS.Models.Enums;

namespace IMS.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is AssetStatus status)
        {
            return status switch
            {
                AssetStatus.Available => Color.FromArgb("#4CAF50"),
                AssetStatus.In_Use => Color.FromArgb("#2196F3"),
                AssetStatus.Damaged_Cosmetic => Color.FromArgb("#FF9800"),
                AssetStatus.In_Repair => Color.FromArgb("#F44336"),
                AssetStatus.Retired => Color.FromArgb("#9E9E9E"),
                _ => Color.FromArgb("#9E9E9E")
            };
        }
        return Color.FromArgb("#9E9E9E");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class StatusToTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is AssetStatus status)
        {
            return status switch
            {
                AssetStatus.Available => "Available",
                AssetStatus.In_Use => "In Use",
                AssetStatus.Damaged_Cosmetic => "Damaged",
                AssetStatus.In_Repair => "In Repair",
                AssetStatus.Retired => "Retired",
                _ => "Unknown"
            };
        }
        return "Unknown";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b;
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class CurrencyFormatConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal d)
            return $"${d:N2}";
        if (value is double db)
            return $"${db:N2}";
        return "$0.00";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class InverseBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return !b;
        return true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class FirstLetterConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s && !string.IsNullOrEmpty(s))
            return s[0].ToString().ToUpper();
        return "?";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class AllocationTierToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Models.Enums.AllocationTier tier)
        {
            return tier switch
            {
                Models.Enums.AllocationTier.Soft_Hold => Color.FromArgb("#FFB74D"),
                Models.Enums.AllocationTier.Hard_Locked => Color.FromArgb("#6750A4"),
                _ => Color.FromArgb("#9E9E9E")
            };
        }
        return Color.FromArgb("#9E9E9E");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class TabActiveConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string currentTab && parameter is string tab)
        {
            return currentTab == tab ? Color.FromArgb("#EEEAFF") : Colors.Transparent;
        }
        return Colors.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class TabTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string currentTab && parameter is string tab)
        {
            return currentTab == tab ? Color.FromArgb("#5B4DFF") : Color.FromArgb("#5F6368");
        }
        return Color.FromArgb("#5F6368");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class IsLaborConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string eventType)
        {
            return eventType == "Labor";
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class ExpandCollapseConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isExpanded)
        {
            return isExpanded ? "▼" : "▶";
        }
        return "▶";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class CreditStatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Models.Enums.CreditStatus status)
        {
            return status switch
            {
                Models.Enums.CreditStatus.Approved => Color.FromArgb("#4CAF50"),
                Models.Enums.CreditStatus.Hold => Color.FromArgb("#F44336"),
                Models.Enums.CreditStatus.Cash_Only => Color.FromArgb("#FF9800"),
                _ => Color.FromArgb("#9E9E9E")
            };
        }
        return Color.FromArgb("#9E9E9E");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
