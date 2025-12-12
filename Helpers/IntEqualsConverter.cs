using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Simulator.Helpers
{
    public class IntEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            if (!int.TryParse(parameter.ToString(), out var target))
                return false;

            return value is int i && i == target;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultrue)
        {
            if (value is bool b && b && int.TryParse(parameter.ToString(), out var target))
                return target;

            return Binding.DoNothing;
        }
    }
}
