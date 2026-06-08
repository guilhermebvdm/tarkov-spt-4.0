using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine;
using ZGFueDkx.ZGCLib.helpers;

namespace ZGFueDkx.ZGCLib.Config
{
    internal static class ConfigColor
    {
        private static readonly Dictionary<ConfigEntryBase, ColorCacheItem> _colorCache = [];
        private static bool _useHex = false;


        public static ConfigEntry<Color> BindColor(this ConfigCategory category, string key, string defaultValue, string description, bool hide = false)
        {
            return category.Config.Bind(
                category.Name,
                key,
                ColorUtils.FromHex(defaultValue),
                new ConfigDescription(
                    description,
                    null,
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = hide,
                        Order = category.GetNextOrder(),
                        CustomDrawer = entry =>
                        {
                            Color color = (Color)entry.BoxedValue;

                            GUI.changed = false;

                            if (!_colorCache.TryGetValue(entry, out ColorCacheItem cache))
                            {
                                Texture2D tex = new(64, 16, TextureFormat.ARGB32, false);
                                cache = new ColorCacheItem(color, tex);
                                tex.FillColor(color);
                                _colorCache[entry] = cache;
                            }

                            GUILayout.BeginVertical();
                            {
                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Label(cache.Texture, GUILayout.ExpandWidth(false));
                                    string hex = _useHex ? $"#{ColorUtility.ToHtmlStringRGB(color)}" : $"{color.r * 255:F0},{color.g * 255:F0},{color.b * 255:F0}";
                                    string newHex = GUILayout.TextField(hex, GUILayout.ExpandWidth(true));
                                    if (GUI.changed && newHex != hex && TryParseColor(newHex, out Color c))
                                    {
                                        color = c;
                                    }
                                    _useHex = GUILayout.Toggle(_useHex, "Hex", GUILayout.ExpandWidth(false));
                                }
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                {
                                    GUILayout.Label("R", GUILayout.ExpandWidth(false));
                                    color.r = GUILayout.HorizontalSlider(color.r, 0f, 1f, GUILayout.ExpandWidth(true));
                                    GUILayout.Label("G", GUILayout.ExpandWidth(false));
                                    color.g = GUILayout.HorizontalSlider(color.g, 0f, 1f, GUILayout.ExpandWidth(true));
                                    GUILayout.Label("B", GUILayout.ExpandWidth(false));
                                    color.b = GUILayout.HorizontalSlider(color.b, 0f, 1f, GUILayout.ExpandWidth(true));
                                }
                                GUILayout.EndHorizontal();
                            }
                            GUILayout.EndVertical();

                            if (color != cache.Color)
                            {
                                entry.BoxedValue = color;

                                cache.Texture.FillColor(color);
                                cache.Color = color;
                            }
                        }
                    }
                )
            );
        }

        public static string GetHexColor(this ConfigEntry<Color> entry, bool useDefault = false)
        {
            return $"#{ColorUtility.ToHtmlStringRGB(entry.GetValue(useDefault))}";
        }

        private static bool TryParseColor(string str, out Color color)
        {
            string[] parts = str.Split(',');

            if (parts.Length == 3 &&
                byte.TryParse(parts[0].Trim(), out byte r) &&
                byte.TryParse(parts[1].Trim(), out byte g) &&
                byte.TryParse(parts[2].Trim(), out byte b))
            {
                color = ColorUtils.From32(r, g, b);
                return true;
            }

            if (ColorUtility.TryParseHtmlString(!str.StartsWith("#") ? $"#{str}" : str, out color))
            {
                return true;
            }

            color = Color.white;
            return false;
        }

        private static void FillColor(this Texture2D texture, Color color)
        {
            Color[] colors = new Color[texture.width * texture.height];

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }

            texture.SetPixels(colors);
            texture.Apply();
        }

        private sealed class ColorCacheItem(Color color, Texture2D texture)
        {
            public Color Color = color;
            public Texture2D Texture = texture;
        }
    }
}
