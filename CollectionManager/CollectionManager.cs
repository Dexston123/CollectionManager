using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace CollectionManager
{
    public class ItemsCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is List<Item> items)
            {
                return $"{items.Count} items";
            }

            return "0 items";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}