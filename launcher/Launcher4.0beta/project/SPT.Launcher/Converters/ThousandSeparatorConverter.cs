/* ThousandSeparatorConverter.cs
 * License: NCSA Open Source License
 *
 * Copyright: SPT / TRL
 *
 * Item 012 (visual batch): formats a numeric value with pt-BR grouping so the
 * XP counter reads "234.324" regardless of the OS current culture (StringFormat
 * on a binding would otherwise follow CultureInfo.CurrentCulture). The optional
 * ConverterParameter is a composite format string (e.g. "Faltam: {0:N0} xp").
 */

using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace SPT.Launcher.Converters
{
    public class ThousandSeparatorConverter : IValueConverter
    {
        private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            string format = parameter as string;
            if (string.IsNullOrEmpty(format))
            {
                format = "{0:N0}";
            }

            try
            {
                return string.Format(PtBr, format, value);
            }
            catch
            {
                return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
