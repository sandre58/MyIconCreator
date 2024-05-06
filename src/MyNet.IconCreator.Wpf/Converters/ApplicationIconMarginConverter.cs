// Copyright (c) Stéphane ANDRE. All Right Reserved.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MyNet.IconCreator.Converters
{
    internal class ApplicationIconMarginConverter : IMultiValueConverter
    {
        private const double Offset = 5.0D;
        public static readonly ApplicationIconMarginConverter Default = new();

        public object? Convert(object?[]? values, Type targetType, object? parameter, CultureInfo culture)
            => values == null
                || values.Length != 2
                || Array.Exists(values, v => v == null)
                || !double.TryParse(values[0]?.ToString(), out var offsetX)
                || !double.TryParse(values[1]?.ToString(), out var offsetY)
                ? new Thickness(0)
                : new Thickness(Offset, Offset, Offset + Math.Max(0, offsetX), Offset + Math.Max(0, offsetY));

        public object?[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
