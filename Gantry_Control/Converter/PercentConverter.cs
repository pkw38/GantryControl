using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Gantry_Control.Converter
{
    public class PercentConverter : IValueConverter
    {
        public object Convert(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            // XAML의 ConverterParameter("0.15")는 OS 로케일과 무관하게 '.' 소수점으로 해석
            if (value is double height &&
                parameter != null &&
                double.TryParse(parameter.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double percent))
            {
                return height * percent;
            }

            return 0d;
        }

        public object ConvertBack(
            object? value,
            Type targetType,
            object? parameter,
            CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
