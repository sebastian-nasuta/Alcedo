using System.Globalization;

namespace Alcedo.Converters;

public class TagsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is List<string> tags)
        {
            return string.Join(" ", tags.Select(tag => tag));
        }
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string tagString)
        {
            var tags = tagString.Split(' ')
                                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                                .ToList();
            return tags;
        }
        return new List<string>();
    }
}
