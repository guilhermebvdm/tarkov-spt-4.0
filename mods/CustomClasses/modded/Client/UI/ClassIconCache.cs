using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CustomClasses.Client;

/// <summary>
///     Item 011: carrega o ícone da classe (PNG) → Sprite, com cache e null-safety.
///     Padrão de carga espelhado do Menu-Overhaul (File.ReadAllBytes → Texture2D.LoadImage → Sprite.Create).
///     Os PNGs ficam em BepInEx/plugins/CustomClasses/icons/ (instalados pelo compile-mod).
/// </summary>
internal static class ClassIconCache
{
    private static readonly Dictionary<string, Sprite?> Cache = new(StringComparer.OrdinalIgnoreCase);
    private static string? _iconsDir;

    private static string IconsDir =>
        _iconsDir ??= Path.Combine(
            Path.GetDirectoryName(typeof(ClassIconCache).Assembly.Location) ?? ".", "icons");

    /// <summary>Sprite do ícone (null se sem nome, arquivo ausente ou falha — o chamador degrada para só o nome).</summary>
    public static Sprite? Get(string? iconFile)
    {
        if (string.IsNullOrWhiteSpace(iconFile))
        {
            return null;
        }

        var name = Path.GetFileName(iconFile);   // sanitiza path traversal (../, /, \)
        if (Cache.TryGetValue(name, out var cached))
        {
            return cached;
        }

        Sprite? sprite = null;
        try
        {
            var path = Path.Combine(IconsDir, name);
            if (File.Exists(path))
            {
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (tex.LoadImage(File.ReadAllBytes(path)))   // redimensiona ao PNG
                {
                    sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                }
                else
                {
                    UnityEngine.Object.Destroy(tex);
                }
            }
            else
            {
                Plugin.Log?.LogWarning($"[CustomClasses] ícone não encontrado: {path}");
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] falha ao carregar ícone '{name}': {ex.Message}");
        }

        Cache[name] = sprite;   // cacheia inclusive null (não retentar PNG ausente/quebrado)
        return sprite;
    }

    /// <summary>Destrói sprites/texturas (teardown do plugin) — evita leak de VRAM.</summary>
    public static void Dispose()
    {
        foreach (var s in Cache.Values)
        {
            if (s == null)
            {
                continue;
            }

            if (s.texture != null)
            {
                UnityEngine.Object.Destroy(s.texture);
            }

            UnityEngine.Object.Destroy(s);
        }

        Cache.Clear();
    }
}
