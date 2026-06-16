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
    // (06-fix-02) cache de ícones com gradiente EMBUTIDO na textura — chave inclui as cores top/bottom.
    private static readonly Dictionary<string, Sprite?> TintedCache = new(StringComparer.OrdinalIgnoreCase);
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

    /// <summary>
    ///     (06-fix-02) Sprite do ícone com GRADIENTE VERTICAL embutido na textura: a silhueta branca tem o
    ///     RGB de cada pixel multiplicado por <c>Lerp(bottom, top, y/altura)</c> (base = bottom, topo = top),
    ///     alpha preservado. Robusto onde o <c>BaseMeshEffect</c> falha (Image criada em runtime, ex.: o ícone
    ///     do menu). O chamador deve usar <c>icon.color = white</c> (a cor já está na textura).
    /// </summary>
    public static Sprite? GetTinted(string? iconFile, Color top, Color bottom)
    {
        if (string.IsNullOrWhiteSpace(iconFile))
        {
            return null;
        }

        var name = Path.GetFileName(iconFile);
        var key = $"{name}|{ColorUtility.ToHtmlStringRGBA(top)}|{ColorUtility.ToHtmlStringRGBA(bottom)}";
        if (TintedCache.TryGetValue(key, out var cached))
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
                if (tex.LoadImage(File.ReadAllBytes(path)))
                {
                    var px = tex.GetPixels32();
                    int w = tex.width, h = tex.height;
                    for (var y = 0; y < h; y++)
                    {
                        // y=0 = base (canto inferior no espaço de textura do Unity) → bottom; y=h-1 → top.
                        var t = h > 1 ? (float)y / (h - 1) : 0f;
                        var g = Color.Lerp(bottom, top, t);
                        var row = y * w;
                        for (var x = 0; x < w; x++)
                        {
                            var o = px[row + x];
                            px[row + x] = new Color32(
                                (byte)(o.r * g.r),
                                (byte)(o.g * g.g),
                                (byte)(o.b * g.b),
                                o.a);   // alpha (forma da silhueta) preservado
                        }
                    }

                    tex.SetPixels32(px);
                    tex.Apply(false);
                    sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
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
            Plugin.Log?.LogError($"[CustomClasses] falha ao tingir ícone '{name}': {ex.Message}");
        }

        TintedCache[key] = sprite;
        return sprite;
    }

    /// <summary>Destrói sprites/texturas (teardown do plugin) — evita leak de VRAM.</summary>
    public static void Dispose()
    {
        foreach (var s in Cache.Values)
        {
            DestroySprite(s);
        }

        foreach (var s in TintedCache.Values)
        {
            DestroySprite(s);
        }

        Cache.Clear();
        TintedCache.Clear();
    }

    private static void DestroySprite(Sprite? s)
    {
        if (s == null)
        {
            return;
        }

        if (s.texture != null)
        {
            UnityEngine.Object.Destroy(s.texture);
        }

        UnityEngine.Object.Destroy(s);
    }
}
