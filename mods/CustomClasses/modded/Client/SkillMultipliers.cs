using System;
using System.Collections.Generic;
using EFT;                 // ESkillId
using Newtonsoft.Json;
using SPT.Common.Http;     // RequestHandler

namespace CustomClasses.Client;

/// <summary>
///     Cache dos fatores de XP de skill (ESkillId → fator) da classe do perfil atual.
///     Carregamento LAZY na 1ª chamada (PA-01-04: evita depender de hook de seleção de perfil
///     ofuscado) — quando uma skill ganha XP, a sessão já existe e a rota responde.
/// </summary>
internal static class SkillMultipliers
{
    private static readonly Dictionary<ESkillId, float> Factors = new();
    private static bool _loaded;

    // Item 008 (i18n): nome localizado en/pt vindo do server (fallback ao className legado = edition/PT).
    private static string? _classNameEn;
    private static string? _classNamePt;

    /// <summary>
    ///     Item 010/008: nome da classe do perfil atual, resolvido pelo IDIOMA DO EFT (po → pt, senão en).
    ///     Null se a edition for vanilla (nenhum nome recebido). Usado no tooltip/menu/selo da UI.
    /// </summary>
    public static string? ClassName =>
        _classNameEn == null && _classNamePt == null
            ? null
            : (GameLocale.IsPortuguese ? (_classNamePt ?? _classNameEn) : (_classNameEn ?? _classNamePt));

    /// <summary>Item 015: nickname do perfil local (p/ casar o ChatSpecialIcon do jogador local).</summary>
    public static string? Nickname { get; private set; }

    /// <summary>Item 011: nome do PNG do ícone da classe (null se ausente). Carregado via ClassIconCache.</summary>
    public static string? IconFile { get; private set; }

    /// <summary>Item 011: cor do nome da classe (hex #RRGGBB; null = cor default).</summary>
    public static string? NameColor { get; private set; }

    /// <summary>Reseta o cache (ex.: troca de perfil) — força novo fetch.</summary>
    public static void Reset()
    {
        Factors.Clear();
        _classNameEn = null;
        _classNamePt = null;
        Nickname = null;
        IconFile = null;
        NameColor = null;
        _loaded = false;
    }

    public static void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;   // marca antes: se falhar, não retenta em loop a cada ganho de XP

        try
        {
            var json = RequestHandler.GetJson("/customclasses/skill-multipliers");
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            // Item 010/011: payload é { className, iconFile, nameColor, multipliers }.
            var payload = JsonConvert.DeserializeObject<Payload>(json);
            if (payload is null)
            {
                return;
            }

            // Item 011: identidade setada mesmo sem multiplicadores (classe do mod sem skillMultipliers).
            // Item 008 (i18n): guarda en/pt; o getter ClassName resolve pelo idioma do EFT. Fallback ao className legado.
            _classNameEn = payload.ClassNameEn ?? payload.ClassName;
            _classNamePt = payload.ClassNamePt ?? payload.ClassName;
            Nickname = payload.Nickname;
            IconFile = payload.IconFile;
            NameColor = payload.NameColor;

            foreach (var kv in payload.Multipliers ?? new Dictionary<string, double>())
            {
                // Casa ESkillId (client) com o nome do JSON (SkillTypes server), case-insensitive.
                if (Enum.TryParse<ESkillId>(kv.Key, ignoreCase: true, out var id) && Enum.IsDefined(typeof(ESkillId), id))
                {
                    Factors[id] = (float)kv.Value;
                }
                else
                {
                    Plugin.Log?.LogWarning($"[CustomClasses] multiplicador p/ skill desconhecida '{kv.Key}' — ignorado.");
                }
            }

            Plugin.Log?.LogInfo($"[CustomClasses] {Factors.Count} multiplicador(es) de skill carregado(s) (classe '{ClassName ?? "—"}').");
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] falha ao buscar multiplicadores de skill: {ex.Message}");
        }
    }

    public static bool TryGet(ESkillId id, out float factor) => Factors.TryGetValue(id, out factor);

    /// <summary>Pares (skill → fator) atuais — usado pelo patch do gym pra saber quais skills observar.</summary>
    public static IEnumerable<KeyValuePair<ESkillId, float>> Entries => Factors;

    /// <summary>Espelho do payload server (SkillMultipliersResponse) — Item 010/011. Props p/ o Newtonsoft preencher sem CS0649.</summary>
    private sealed class Payload
    {
        [JsonProperty("className")] public string? ClassName { get; set; }
        [JsonProperty("classNameEn")] public string? ClassNameEn { get; set; }   // item 008 (i18n)
        [JsonProperty("classNamePt")] public string? ClassNamePt { get; set; }   // item 008 (i18n)
        [JsonProperty("nickname")] public string? Nickname { get; set; }
        [JsonProperty("iconFile")] public string? IconFile { get; set; }
        [JsonProperty("nameColor")] public string? NameColor { get; set; }
        [JsonProperty("multipliers")] public Dictionary<string, double>? Multipliers { get; set; }
    }
}
