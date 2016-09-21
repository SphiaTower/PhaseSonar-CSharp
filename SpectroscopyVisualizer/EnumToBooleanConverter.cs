using System;
using System.Globalization;
using System.Windows.Data;
using JetBrains.Annotations;

namespace SpectroscopyVisualizer {
    public class EnumToBooleanConverter : IValueConverter {
        [NotNull]
        public object Convert([NotNull] object value, Type targetType, object parameter, CultureInfo culture) {
            return value.Equals(parameter);
        }

        public object ConvertBack([NotNull] object value, Type targetType, object parameter, CultureInfo culture) {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
}