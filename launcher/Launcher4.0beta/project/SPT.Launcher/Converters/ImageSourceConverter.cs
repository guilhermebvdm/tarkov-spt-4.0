/* ImageSourceConverter.cs
 * License: NCSA Open Source License
 *
 * Copyright: SPT
 * AUTHORS:
 * waffle.lord
 */

using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Concurrent;
using System.Globalization;

namespace SPT.Launcher.Converters
{
    public class ImageSourceConverter : IValueConverter
    {
        // Item 025 (CC-3): memoiza o decode por path. O converter é chamado por várias
        // telas vivas (ClassSelection/Login/Register/Settings/Profile/MainWindow); o
        // conjunto de paths é limitado (facção/ícones/poucos fundos), então o cache é
        // limitado por construção. 1 decode por path → sem churn ao re-avaliar o binding.
        private static readonly ConcurrentDictionary<string, Bitmap> _cache = new();

        // Seam de teste: fábrica do Bitmap por path (default decodifica do disco).
        // Permite cobrir a memoização/os ramos null sem um PNG real ou Avalonia headless.
        internal static Func<string, Bitmap> BitmapFactory { get; set; } = path => new Bitmap(path);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string rawUri))
            {
                return null;
            }

            try
            {
                if (targetType.IsAssignableFrom(typeof(Bitmap)))
                {
                    // GetOrAdd propaga a exceção da fábrica (path inválido) sem cachear nada;
                    // o catch abaixo devolve null, então caminhos ruins não poluem o cache.
                    return _cache.GetOrAdd(rawUri, BitmapFactory);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
