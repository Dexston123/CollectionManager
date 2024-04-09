using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace CollectionManager
{
    public class OwnershipStatusToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OwnershipStatus status)
            {
                return status == OwnershipStatus.Sold ? 0.5 : 1.0;
            }
            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
