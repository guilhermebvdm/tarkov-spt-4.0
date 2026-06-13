using System;

namespace CustomClasses.Client;

/// <summary>
///     Item 008 (i18n): idioma atual do EFT (cliente). O EFT guarda o código do idioma escolhido no
///     menu em <c>LocaleManagerClass</c> (singleton) — o mesmo que <c>"key".Localized()</c> usa para
///     resolver textos. Usamos esse código para mostrar o nome da classe no idioma do jogo.
///     "po" = Português no EFT/SPT (há <c>locales/global/po.json</c>); demais idiomas → inglês (fallback).
///     ref: Assembly-CSharp.dll → (global) LocaleManagerClass.LocaleManagerClass (singleton) .String_0 ("en"/"po"/"ru"/…).
/// </summary>
internal static class GameLocale
{
    /// <summary>Código do idioma do EFT (ex.: "en", "po", "ru"); vazio se indisponível.</summary>
    public static string Current
    {
        get
        {
            try
            {
                return global::LocaleManagerClass.LocaleManagerClass.String_0 ?? string.Empty;
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogWarning($"[CustomClasses] idioma do EFT indisponível ({ex.Message}) — usando inglês.");
                return string.Empty;
            }
        }
    }

    /// <summary>True se o jogo está em Português ("po" no EFT/SPT; aceita "pt*" por segurança).</summary>
    public static bool IsPortuguese
    {
        get
        {
            var l = Current;
            return l.StartsWith("po", StringComparison.OrdinalIgnoreCase)
                || l.StartsWith("pt", StringComparison.OrdinalIgnoreCase);
        }
    }
}
