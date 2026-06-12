using System;
using System.Reflection;
using BepInEx.Bootstrap;
using UnityEngine;

namespace CustomClasses.Client;

/// <summary>
///     (015 polish) Ponte para o Menu-Overhaul: seta o <c>Settings.AccentColor</c> dele (a cor usada no
///     número do EXP, top glow, botões e luzes do menu) = cor da classe, via reflection. O MO dispara
///     <c>AccentColor.SettingChanged</c> e recolore tudo sozinho. Guarda/restaura o valor original (perfil
///     vanilla). No-op se o Menu-Overhaul não estiver instalado.
///     ref: mods/SPT-Menu-Overhaul/modded/Utils/Settings.cs → public static ConfigEntry&lt;Color&gt; AccentColor
///          (aplicado em PlayerProfileFeaturesPatch:690 EXP, MenuOverhaulPatch:217 glow, TweenButtonPatch:55 botões).
/// </summary>
internal static class MenuOverhaulBridge
{
    private const string MoGuid = "com.moxopixel.menuoverhaul";
    private const string SettingsTypeName = "MoxoPixel.MenuOverhaul.Utils.Settings";

    private static bool _resolved;
    private static object? _accentEntry;          // ConfigEntry<Color>
    private static PropertyInfo? _valueProp;       // ConfigEntry<Color>.Value
    private static Color _original;
    private static bool _hasOriginal;

    private static void Resolve()
    {
        if (_resolved)
        {
            return;
        }

        _resolved = true;
        try
        {
            if (!Chainloader.PluginInfos.TryGetValue(MoGuid, out var info) || info?.Instance == null)
            {
                return;   // Menu-Overhaul ausente
            }

            var settingsType = info.Instance.GetType().Assembly.GetType(SettingsTypeName);
            var field = settingsType?.GetField("AccentColor", BindingFlags.Static | BindingFlags.Public);
            _accentEntry = field?.GetValue(null);   // ConfigEntry<Color>
            _valueProp = _accentEntry?.GetType().GetProperty("Value");
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogWarning($"[CustomClasses] Menu-Overhaul bridge indisponível: {ex.Message}");
        }
    }

    internal static bool Available
    {
        get
        {
            Resolve();
            return _accentEntry != null && _valueProp != null;
        }
    }

    /// <summary>True se o Menu-Overhaul está carregado (independe do AccentColor). Usado pelo botão SKILLS (013).</summary>
    internal static bool IsPresent => Chainloader.PluginInfos.ContainsKey(MoGuid);

    /// <summary>Seta o AccentColor do MO = cor da classe (dispara o recolorir do MO). Guarda o original 1x.</summary>
    internal static void SetAccent(Color color)
    {
        if (!Available)
        {
            return;
        }

        try
        {
            if (!_hasOriginal)
            {
                _original = (Color)_valueProp!.GetValue(_accentEntry);
                _hasOriginal = true;
            }

            var current = (Color)_valueProp!.GetValue(_accentEntry);
            if (current != color)
            {
                _valueProp.SetValue(_accentEntry, color);   // SettingChanged → MO recolore EXP/glow/botões
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] MO SetAccent falhou: {ex.Message}");
        }
    }

    /// <summary>Restaura o AccentColor original do MO (perfil vanilla / mod desligado).</summary>
    internal static void RestoreAccent()
    {
        if (!Available || !_hasOriginal)
        {
            return;
        }

        try
        {
            var current = (Color)_valueProp!.GetValue(_accentEntry);
            if (current != _original)
            {
                _valueProp.SetValue(_accentEntry, _original);
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] MO RestoreAccent falhou: {ex.Message}");
        }
    }
}
