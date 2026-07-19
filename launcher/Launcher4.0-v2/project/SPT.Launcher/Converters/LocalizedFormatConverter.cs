/* LocalizedFormatConverter.cs
 * Formata um valor com um TEMPLATE localizado vindo do LocalizationProvider, de forma reativa à troca
 * de idioma. Uso via MultiBinding: values[0] = template (ex.: "Nível: {0}" / "Level: {0}"),
 * values[1..] = os valores. O agrupamento numérico segue o idioma (pt-BR → "5.245", en-US → "5,245").
 * Substitui os StringFormat/ConverterParameter hardcoded do painel de perfil (que não eram traduzíveis).
 */

using Avalonia.Data.Converters;
using SPT.Launcher.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SPT.Launcher.Converters
{
    public class LocalizedFormatConverter : IMultiValueConverter
    {
        private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");
        private static readonly CultureInfo EnUs = CultureInfo.GetCultureInfo("en-US");

        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values == null || values.Count == 0 || values[0] is not string template)
            {
                return null;
            }

            var args = new object[values.Count - 1];
            for (int i = 1; i < values.Count; i++)
            {
                args[i - 1] = values[i];
            }

            CultureInfo fmtCulture =
                string.Equals(LocalizationProvider.Instance?.ietf_tag, "en", StringComparison.OrdinalIgnoreCase)
                    ? EnUs
                    : PtBr;

            try
            {
                return string.Format(fmtCulture, template, args);
            }
            catch
            {
                return template;
            }
        }
    }
}
