/* ChamferClipConverter.cs
 * License: NCSA Open Source License
 *
 * Copyright: SPT / TRL
 *
 * Item 003 (visual batch): mirrors the DS `--trl-chamfer` clip-path
 * (design-system/tokens.css) as an Avalonia geometry. Cuts the top-left and
 * bottom-right corners (2 opposite corners) with an 8px chamfer — solid button
 * variants only (.primary / .danger), where the fill/border share one color so
 * clipping the 1px border is invisible.
 *
 * Bound to a control's Bounds (RelativeSource Self); re-runs on every layout
 * pass so the clip tracks resize.
 */

using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace SPT.Launcher.Converters
{
    public class ChamferClipConverter : IValueConverter
    {
        private const double ChamferSize = 8.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Rect bounds)
            {
                return null;
            }

            double w = bounds.Width;
            double h = bounds.Height;

            if (w <= 0 || h <= 0)
            {
                return null;
            }

            // Clamp so tiny controls never invert the polygon.
            double c = Math.Min(ChamferSize, Math.Min(w, h) / 2.0);

            // 6 points, origin (0,0), cutting top-left + bottom-right:
            //   (c,0) (w,0) (w,h-c) (w-c,h) (0,h) (0,c)
            var figure = new PathFigure
            {
                StartPoint = new Point(c, 0),
                IsClosed = true,
                IsFilled = true,
                Segments = new PathSegments
                {
                    new LineSegment { Point = new Point(w, 0) },
                    new LineSegment { Point = new Point(w, h - c) },
                    new LineSegment { Point = new Point(w - c, h) },
                    new LineSegment { Point = new Point(0, h) },
                    new LineSegment { Point = new Point(0, c) },
                }
            };

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            return geometry;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
