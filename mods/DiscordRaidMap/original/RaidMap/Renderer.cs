using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace DiscordRaidMap.RaidMap
{
    internal sealed class Renderer : IDisposable
    {
        private static readonly Color32 PlayerName = new(255, 255, 255, 255);
        private static readonly Color32 PlayerNameShadow = new(0, 0, 0, 220);
        private static readonly Color32 CornerText = new(255, 255, 255, 255);
        private static readonly Color32 CornerTextShadow = new(0, 0, 0, 235);
        private const string DefaultFont = "DelaGothicOne-Regular.ttf";

        private readonly string _mapAssetPath;
        private readonly string _markerAssetPath;
        private readonly string _fontAssetPath;
        private readonly string _mapTextFont;
        private readonly int _mapTextFontSize;
        private readonly int _markerDisplaySize;
        private readonly Dictionary<string, CpuImage> _backgroundCache = [];
        private readonly Dictionary<RaidMarkerType, CpuImage> _markerCache = [];
        private PrivateFontCollection _fontCollection;

        public Renderer(string mapAssetPath, string markerAssetPath, string fontAssetPath, string mapTextFont, int mapTextFontSize, int markerDisplaySize)
        {
            _mapAssetPath = mapAssetPath;
            _markerAssetPath = markerAssetPath;
            _fontAssetPath = fontAssetPath;
            _mapTextFont = string.IsNullOrWhiteSpace(mapTextFont) ? DefaultFont : mapTextFont;
            _mapTextFontSize = Math.Max(1, mapTextFontSize);
            _markerDisplaySize = Math.Max(1, markerDisplaySize);
        }

        public byte[] Render(RaidSnapshot snapshot)
        {
            var background = LoadBackground(snapshot.Map);
            var width = background.Width;
            var height = background.Height;
            var pixels = CopyBackground(background);

            foreach (var marker in snapshot.Markers)
            {
                if (marker.Type == RaidMarkerType.Player)
                    continue;

                DrawRaidMarker(snapshot, pixels, width, height, marker);
            }

            foreach (var marker in snapshot.Markers)
            {
                if (marker.Type != RaidMarkerType.Player)
                    continue;

                DrawRaidMarker(snapshot, pixels, width, height, marker);
            }

            if (!string.IsNullOrWhiteSpace(snapshot.TimeRemaining))
            {
                DrawLabel(pixels, width, height, $"TIME LEFT {snapshot.TimeRemaining}", 12, 12, CornerText, CornerTextShadow);
            }

            return EncodePng(pixels, width, height);
        }

        private void DrawRaidMarker(RaidSnapshot snapshot, Color32[] pixels, int width, int height, RaidMarker marker)
        {
            var point = Project(snapshot.Map, marker.MapPosition, width, height);

            switch (marker.Type)
            {
                case RaidMarkerType.KilledEnemy:
                case RaidMarkerType.KilledBoss:
                case RaidMarkerType.Airdrop:
                case RaidMarkerType.Extract:
                case RaidMarkerType.ExtractRequirements:
                    DrawMarkerIcon(pixels, width, height, point, marker.Type);
                    break;

                case RaidMarkerType.DeadPlayer:
                    DrawDeadPlayer(pixels, width, height, point, marker.Label);
                    break;

                case RaidMarkerType.Player:
                    DrawPlayer(
                        pixels,
                        width,
                        height,
                        point,
                        marker.Label,
                        marker.RotationDegrees + snapshot.Map.Rotation);
                    break;
            }
        }

        private CpuImage LoadBackground(MapDefinition map)
        {
            if (_backgroundCache.TryGetValue(map.ImageFile, out var cached))
            {
                return cached;
            }

            var path = Path.Combine(_mapAssetPath, map.ImageFile);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Map image not found: {path}");
            }

            var image = LoadPng(path);
            _backgroundCache[map.ImageFile] = image;
            return image;
        }

        private static Color32[] CopyBackground(CpuImage source)
        {
            var dst = new Color32[source.Pixels.Length];
            Array.Copy(source.Pixels, dst, source.Pixels.Length);
            return dst;
        }

        private CpuImage LoadMarker(RaidMarkerType markerType)
        {
            if (_markerCache.TryGetValue(markerType, out var cached))
            {
                return cached;
            }

            var fileName = GetMarkerFileName(markerType);
            var path = Path.Combine(_markerAssetPath, fileName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Marker image not found: {path}");
            }

            var image = LoadPng(path);
            _markerCache[markerType] = image;
            return image;
        }

        private static string GetMarkerFileName(RaidMarkerType markerType)
        {
            return markerType switch
            {
                RaidMarkerType.Player => "player.png",
                RaidMarkerType.DeadPlayer => "dead_player.png",
                RaidMarkerType.KilledEnemy => "dead_enemy.png",
                RaidMarkerType.KilledBoss => "dead_boss.png",
                RaidMarkerType.Airdrop => "airdrop.png",
                RaidMarkerType.Extract => "extract.png",
                RaidMarkerType.ExtractRequirements => "extract_requirements.png",
                _ => throw new ArgumentOutOfRangeException(nameof(markerType), markerType, null)
            };
        }

        private Vector2Int Project(MapDefinition map, Vector3 mapPosition, int width, int height)
        {
            var point = new Vector2(mapPosition.x, mapPosition.y);
            var center = (map.Min + map.Max) * 0.5f;
            var rotated = Rotate(point - center, map.Rotation);

            var corners = new[]
            {
                Rotate(map.Min - center, map.Rotation),
                Rotate(new Vector2(map.Min.x, map.Max.y) - center, map.Rotation),
                Rotate(map.Max - center, map.Rotation),
                Rotate(new Vector2(map.Max.x, map.Min.y) - center, map.Rotation)
            };

            var min = corners[0];
            var max = corners[0];
            foreach (var corner in corners)
            {
                min = Vector2.Min(min, corner);
                max = Vector2.Max(max, corner);
            }

            var normalized = new Vector2(
                Mathf.InverseLerp(min.x, max.x, rotated.x),
                Mathf.InverseLerp(min.y, max.y, rotated.y));

            return new Vector2Int(
                Mathf.RoundToInt(normalized.x * (width - 1)),
                Mathf.RoundToInt((1f - normalized.y) * (height - 1)));
        }

        private static Vector2 Rotate(Vector2 value, float degrees)
        {
            var radians = degrees * Mathf.Deg2Rad;
            var sin = Mathf.Sin(radians);
            var cos = Mathf.Cos(radians);
            return new Vector2(value.x * cos - value.y * sin, value.x * sin + value.y * cos);
        }

        private void DrawPlayer(Color32[] pixels, int width, int height, Vector2Int center, string name, float rotationDegrees)
        {
            DrawMarkerIcon(pixels, width, height, center, RaidMarkerType.Player, rotationDegrees);
            DrawLabelCentered(pixels, width, height, FormatPlayerName(name), center.x, center.y + _markerDisplaySize / 2 + 3, PlayerName, PlayerNameShadow);
        }

        private void DrawDeadPlayer(Color32[] pixels, int width, int height, Vector2Int center, string name)
        {
            DrawMarkerIcon(pixels, width, height, center, RaidMarkerType.DeadPlayer);
            DrawLabelCentered(pixels, width, height, FormatPlayerName(name), center.x, center.y + _markerDisplaySize / 2 + 3, PlayerName, PlayerNameShadow);
        }

        private void DrawMarkerIcon(Color32[] pixels, int width, int height, Vector2Int center, RaidMarkerType markerType, float rotationDegrees = 0f)
        {
            var icon = LoadMarker(markerType);
            var iconPixels = icon.Pixels;
            var sourceWidth = icon.Width;
            var sourceHeight = icon.Height;
            var outputSize = Mathf.CeilToInt(Mathf.Sqrt(_markerDisplaySize * _markerDisplaySize * 2));
            var left = center.x - outputSize / 2;
            var top = center.y - outputSize / 2;
            var angle = rotationDegrees * Mathf.Deg2Rad;
            var sin = Mathf.Sin(angle);
            var cos = Mathf.Cos(angle);
            var displayCenter = (_markerDisplaySize - 1) * 0.5f;
            var outputCenter = (outputSize - 1) * 0.5f;

            for (var y = 0; y < outputSize; y++)
            {
                for (var x = 0; x < outputSize; x++)
                {
                    var dx = x - outputCenter;
                    var dy = y - outputCenter;
                    var displayX = dx * cos - dy * sin + displayCenter;
                    var displayY = dx * sin + dy * cos + displayCenter;
                    if (displayX < 0f || displayY < 0f || displayX > _markerDisplaySize - 1 || displayY > _markerDisplaySize - 1)
                    {
                        continue;
                    }

                    var sourceX = _markerDisplaySize == 1 ? 0f : displayX * (sourceWidth - 1) / (_markerDisplaySize - 1);
                    var sourceY = _markerDisplaySize == 1 ? 0f : displayY * (sourceHeight - 1) / (_markerDisplaySize - 1);
                    var src = SampleBilinear(iconPixels, sourceWidth, sourceHeight, sourceX, sourceY);
                    if (src.a == 0)
                    {
                        continue;
                    }

                    SetPixel(pixels, width, height, left + x, top + y, src);
                }
            }
        }

        private static Color32 SampleBilinear(Color32[] pixels, int width, int height, float x, float y)
        {
            var x0 = Mathf.Clamp(Mathf.FloorToInt(x), 0, width - 1);
            var y0 = Mathf.Clamp(Mathf.FloorToInt(y), 0, height - 1);
            var x1 = Mathf.Clamp(x0 + 1, 0, width - 1);
            var y1 = Mathf.Clamp(y0 + 1, 0, height - 1);
            var tx = x - x0;
            var ty = y - y0;

            var top = Lerp(pixels[(height - 1 - y0) * width + x0], pixels[(height - 1 - y0) * width + x1], tx);
            var bottom = Lerp(pixels[(height - 1 - y1) * width + x0], pixels[(height - 1 - y1) * width + x1], tx);
            return Lerp(top, bottom, ty);
        }

        private static Color32 Lerp(Color32 a, Color32 b, float t)
        {
            return new Color32(
                (byte)Mathf.RoundToInt(Mathf.Lerp(a.r, b.r, t)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(a.g, b.g, t)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(a.b, b.b, t)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(a.a, b.a, t)));
        }

        private void DrawLabelCentered(Color32[] pixels, int width, int height, string text, int centerX, int y, Color32 color, Color32 shadow)
        {
            var textWidth = MeasureText(text);
            DrawLabel(pixels, width, height, text, centerX - textWidth / 2, y, color, shadow);
        }

        private void DrawLabel(Color32[] pixels, int width, int height, string text, int x, int y, Color32 color, Color32 shadow)
        {
            DrawTrueTypeText(pixels, width, height, text, x + 2, y + 2, shadow);
            DrawTrueTypeText(pixels, width, height, text, x, y, color);
        }

        private static string FormatPlayerName(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "Player" : value;
        }

        private int MeasureText(string text)
        {
            using var bitmap = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
            using var graphics = System.Drawing.Graphics.FromImage(bitmap);
            using var font = CreateConfiguredFont();
            return Mathf.CeilToInt(graphics.MeasureString(text ?? "", font, int.MaxValue, StringFormat.GenericTypographic).Width);
        }

        private void DrawTrueTypeText(Color32[] pixels, int width, int height, string text, int x, int y, Color32 color)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            using var font = CreateConfiguredFont();
            using var measureBitmap = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
            using var measureGraphics = System.Drawing.Graphics.FromImage(measureBitmap);
            var size = measureGraphics.MeasureString(text, font, int.MaxValue, StringFormat.GenericTypographic);
            var textWidth = Math.Max(1, Mathf.CeilToInt(size.Width) + 2);
            var textHeight = Math.Max(1, Mathf.CeilToInt(size.Height) + 2);
            using var textBitmap = new Bitmap(textWidth, textHeight, PixelFormat.Format32bppArgb);
            using (var graphics = System.Drawing.Graphics.FromImage(textBitmap))
            using (var brush = new SolidBrush(System.Drawing.Color.FromArgb(color.a, color.r, color.g, color.b)))
            {
                graphics.Clear(System.Drawing.Color.Transparent);
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                graphics.DrawString(text, font, brush, 0f, 0f, StringFormat.GenericTypographic);
            }

            BlendTextBitmap(pixels, width, height, textBitmap, x, y);
        }

        private System.Drawing.Font CreateConfiguredFont()
        {
            var fontPath = Path.Combine(_fontAssetPath, _mapTextFont);
            if (!File.Exists(fontPath))
            {
                throw new FileNotFoundException($"Map text font not found: {fontPath}");
            }

            _fontCollection ??= new PrivateFontCollection();
            if (_fontCollection.Families.Length == 0)
            {
                _fontCollection.AddFontFile(fontPath);
            }

            return new System.Drawing.Font(_fontCollection.Families[0], _mapTextFontSize, FontStyle.Regular, GraphicsUnit.Pixel);
        }

        private static void BlendTextBitmap(Color32[] pixels, int width, int height, Bitmap textBitmap, int x, int y)
        {
            var rect = new Rectangle(0, 0, textBitmap.Width, textBitmap.Height);
            var data = textBitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                var stride = Math.Abs(data.Stride);
                var bytes = new byte[stride * textBitmap.Height];
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

                for (var py = 0; py < textBitmap.Height; py++)
                {
                    var dstY = y + py;
                    if ((uint)dstY >= height)
                    {
                        continue;
                    }

                    var row = py * stride;
                    for (var px = 0; px < textBitmap.Width; px++)
                    {
                        var dstX = x + px;
                        if ((uint)dstX >= width)
                        {
                            continue;
                        }

                        var offset = row + px * 4;
                        var src = new Color32(bytes[offset + 2], bytes[offset + 1], bytes[offset], bytes[offset + 3]);
                        if (src.a == 0)
                        {
                            continue;
                        }

                        SetPixel(pixels, width, height, dstX, dstY, src);
                    }
                }
            }
            finally
            {
                textBitmap.UnlockBits(data);
            }
        }

        private static void SetPixel(Color32[] pixels, int width, int height, int x, int y, Color32 color)
        {
            if ((uint)x >= width || (uint)y >= height)
            {
                return;
            }

            var index = (height - 1 - y) * width + x;
            pixels[index] = Blend(pixels[index], color);
        }

        private static Color32 Blend(Color32 dst, Color32 src)
        {
            if (src.a == 255)
            {
                return src;
            }

            var a = src.a / 255f;
            return new Color32(
                (byte)(src.r * a + dst.r * (1f - a)),
                (byte)(src.g * a + dst.g * (1f - a)),
                (byte)(src.b * a + dst.b * (1f - a)),
                255);
        }

        private static CpuImage LoadPng(string path)
        {
            using var source = new Bitmap(path);
            using var bitmap = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
            using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
            {
                graphics.DrawImage(source, 0, 0, source.Width, source.Height);
            }

            var width = bitmap.Width;
            var height = bitmap.Height;
            var rect = new Rectangle(0, 0, width, height);
            var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                var stride = Math.Abs(data.Stride);
                var bytes = new byte[stride * height];
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);

                var pixels = new Color32[width * height];
                for (var y = 0; y < height; y++)
                {
                    var row = y * stride;
                    for (var x = 0; x < width; x++)
                    {
                        var offset = row + x * 4;
                        pixels[(height - 1 - y) * width + x] = new Color32(
                            bytes[offset + 2],
                            bytes[offset + 1],
                            bytes[offset],
                            bytes[offset + 3]);
                    }
                }

                return new CpuImage(width, height, pixels);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }

        private static byte[] EncodePng(Color32[] pixels, int width, int height)
        {
            using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var rect = new Rectangle(0, 0, width, height);
            var data = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            try
            {
                var stride = Math.Abs(data.Stride);
                var bytes = new byte[stride * height];
                for (var y = 0; y < height; y++)
                {
                    var row = y * stride;
                    for (var x = 0; x < width; x++)
                    {
                        var pixel = pixels[(height - 1 - y) * width + x];
                        var offset = row + x * 4;
                        bytes[offset] = pixel.b;
                        bytes[offset + 1] = pixel.g;
                        bytes[offset + 2] = pixel.r;
                        bytes[offset + 3] = pixel.a;
                    }
                }

                Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            return stream.ToArray();
        }

        public void Dispose()
        {
            _backgroundCache.Clear();
            _markerCache.Clear();
            _fontCollection?.Dispose();
            _fontCollection = null;
        }

        private sealed class CpuImage
        {
            public CpuImage(int width, int height, Color32[] pixels)
            {
                Width = width;
                Height = height;
                Pixels = pixels;
            }

            public int Width { get; }
            public int Height { get; }
            public Color32[] Pixels { get; }
        }
    }
}
