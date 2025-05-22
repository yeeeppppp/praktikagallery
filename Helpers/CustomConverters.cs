using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ArtGalleryStore.Helpers
{
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isInverse = false;
            
            // Проверяем, нужно ли инвертировать значение
            if (parameter != null && parameter is bool)
            {
                isInverse = (bool)parameter;
            }
            else if (parameter != null && parameter is string)
            {
                bool.TryParse(parameter.ToString(), out isInverse);
            }
            
            bool boolValue = value != null && value is bool && (bool)value;
            
            // Если параметр True, инвертируем результат
            if (isInverse)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isInverse = false;
            
            if (parameter != null && parameter is bool)
            {
                isInverse = (bool)parameter;
            }
            else if (parameter != null && parameter is string)
            {
                bool.TryParse(parameter.ToString(), out isInverse);
            }
            
            Visibility visibility = value is Visibility ? (Visibility)value : Visibility.Visible;
            bool result = visibility == Visibility.Visible;
            
            if (isInverse)
            {
                return !result;
            }
            
            return result;
        }
    }
}