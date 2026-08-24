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

    // ─────────────────────────────────────────────────────────────────────────────────────────────
    // ref: AUD-01-08 — LIMITE do TintedCache.
    //
    // A chave inclui a COR, e a cor é um ConfigEntry<Color> do F12 (item 067). Cada chave nova custa uma
    // Texture2D 256×256 RGBA32 (256 KB de VRAM) + um Color32[65536] (256 KB gerenciados, acima do limiar de
    // 85 KB → vai para o Large Object Heap) + 65.536 operações de pixel + upload à GPU. E NADA era liberado:
    // o DestroySprite só rodava no Dispose (fechar o jogo). Arrastar o picker de cor de uma classe gerava uma
    // entrada permanente por evento de mudança, em DOIS consumidores (menu via ClassColorsChanged→ApplyToMenu
    // e aba CLASS via SkillsClassTabPatch.OnColorsChanged).
    //
    // Cap 4 por ícone, e não 1: o MESMO iconFile precisa de DUAS variantes vivas ao mesmo tempo — o brasão
    // com gradiente (top != bottom, ClassIdentityView.cs:134) e a marca d'água chapada (top == bottom,
    // PerksPanelView.cs:242), ambas visíveis juntas na aba CLASS. 4 = as 2 formas + 1 geração de folga.
    private const int MaxVariantsPerIcon = 4;
    private const int ColorQuantum = 8;   // arredonda cada canal p/ múltiplo de 8 → ~32× menos chaves

    /// <summary>Ordem de recência das chaves de cada ícone (índice 0 = menos recente). Ver <see cref="Touch"/>.</summary>
    private static readonly Dictionary<string, List<string>> VariantsByIcon = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Frame em que cada chave nasceu — guard anti-eviction no mesmo frame (ver <see cref="EvictIfNeeded"/>).</summary>
    private static readonly Dictionary<string, int> CreatedFrame = new(StringComparer.Ordinal);

    /// <summary>
    ///     Quantiza a cor da chave em múltiplos de <see cref="ColorQuantum"/> por canal.
    ///     <para>
    ///     ⚠️ Exceção declarada na 01-spec: muda a cor renderizada em até 3/255 por canal (~1,2%),
    ///     imperceptível, em troca de ~32× menos chaves.
    ///     </para>
    ///     <para>
    ///     ⚠️ ref: PA-01-02 — o clamp vem DEPOIS da quantização. Com v=1.0: round(1.0×255/8) = round(31.875)
    ///     = 32, e 32×8 = <b>256</b>, que em <c>(byte)</c> unchecked (o default do C#) vira <b>0</b> — o topo
    ///     do gradiente de uma classe clara (ex.: Saqueador #c4ad45) viraria PRETO, por canal, de forma
    ///     intermitente. Clampar antes não resolve: o estouro nasce na multiplicação.
    ///     </para>
    /// </summary>
    private static Color32 Quantize(Color c)
    {
        static byte Q(float v)
        {
            var q = Mathf.RoundToInt(Mathf.Clamp01(v) * 255f / ColorQuantum) * ColorQuantum;
            return (byte)Mathf.Min(q, 255);
        }

        return new Color32(Q(c.r), Q(c.g), Q(c.b), 255);
    }

    /// <summary>
    ///     ref: AUD-01-08 · PA-03-03 — LRU <b>de verdade</b>: usar uma variante a manda para o FIM da fila.
    ///     Sem o move-to-end isto degenera em FIFO, e aí o brasão em uso (redesenhado a cada <c>Show</c>)
    ///     seria evicto antes de uma variante velha e parada — exatamente o sprite que não pode morrer.
    ///     É esta função que decide qual textura é destruída.
    /// </summary>
    private static void Touch(string name, string key)
    {
        if (!VariantsByIcon.TryGetValue(name, out var keys))
        {
            keys = new List<string>(MaxVariantsPerIcon + 1);
            VariantsByIcon[name] = keys;
        }

        var at = keys.IndexOf(key);   // O(n) com n <= 5 — irrelevante
        if (at >= 0)
        {
            keys.RemoveAt(at);
        }

        keys.Add(key);
    }

    /// <summary>
    ///     Destrói as variantes mais antigas do ícone até caber no cap.
    ///     <para>
    ///     Guard de MESMO FRAME: nunca destruir algo criado neste frame — dentro de um frame todos os
    ///     consumidores do <c>ClassColorsChanged</c> (menu + aba CLASS) já se re-apontaram para o sprite
    ///     novo. Se TODAS forem do frame atual, o cache excede o cap temporariamente (intencional); a
    ///     próxima inserção resolve.
    ///     </para>
    /// </summary>
    private static void EvictIfNeeded(string name)
    {
        if (!VariantsByIcon.TryGetValue(name, out var keys))
        {
            return;
        }

        var i = 0;
        while (i < keys.Count && keys.Count > MaxVariantsPerIcon)
        {
            var k = keys[i];
            if (CreatedFrame.TryGetValue(k, out var f) && f == Time.frameCount)
            {
                i++;
                continue;
            }

            if (TintedCache.TryGetValue(k, out var old))
            {
                DestroySprite(old);   // libera Texture2D + Sprite
            }

            TintedCache.Remove(k);
            CreatedFrame.Remove(k);
            keys.RemoveAt(i);         // não incrementa i — o próximo desliza para esta posição
        }
    }

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

        // ref: AUD-01-08 — cor QUANTIZADA na chave (~32× menos entradas).
        var qTop = Quantize(top);
        var qBottom = Quantize(bottom);
        var key = $"{name}|{qTop.r:X2}{qTop.g:X2}{qTop.b:X2}|{qBottom.r:X2}{qBottom.g:X2}{qBottom.b:X2}";

        if (TintedCache.TryGetValue(key, out var cached))
        {
            Touch(name, key);   // LRU: recém-usado vai para o fim da fila
            return cached;
        }

        var sprite = BuildTinted(name, qTop, qBottom);

        TintedCache[key] = sprite;
        CreatedFrame[key] = Time.frameCount;
        Touch(name, key);
        EvictIfNeeded(name);

        // PERF-INSTR AUD-01-08 — temporary, remove after validation
        // Responde a única pergunta que dimensiona o achado: quantas entradas um arrasto do picker gera?
        // Logado só na INSERÇÃO (o cache é o que impede o flood) e só com o diagnóstico ligado.
        if (PerkDiag.Enabled)
        {
            Plugin.Log?.LogInfo($"[CustomClasses][perf/AUD-01-08] tintedCache={TintedCache.Count} (~{TintedCache.Count * 256} KB VRAM) +{key}");
        }

        return sprite;
    }

    /// <summary>
    ///     (06-fix-02) Constrói a textura tingida. ref: PA-01-09 — extraído do corpo inline do
    ///     <c>GetTinted</c>, preservando integralmente o <c>try/catch</c>, o aviso de arquivo ausente e o
    ///     <c>Destroy(tex)</c> do ramo em que <c>LoadImage</c> falha (é ele que evita vazar uma textura
    ///     quando o PNG está corrompido).
    /// </summary>
    private static Sprite? BuildTinted(string name, Color top, Color bottom)
    {
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
        VariantsByIcon.Clear();   // ref: AUD-01-08
        CreatedFrame.Clear();     // ref: AUD-01-08
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
