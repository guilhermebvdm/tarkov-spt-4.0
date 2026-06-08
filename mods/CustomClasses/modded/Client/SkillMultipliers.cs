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

    /// <summary>Item 010: nome da classe/edition do perfil atual (p/ o tooltip da UI). Null se edition vanilla.</summary>
    public static string? ClassName { get; private set; }

    /// <summary>Reseta o cache (ex.: troca de perfil) — força novo fetch.</summary>
    public static void Reset()
    {
        Factors.Clear();
        ClassName = null;
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

            // Item 010: payload é { className, multipliers } (antes era o Dictionary na raiz).
            var payload = JsonConvert.DeserializeObject<Payload>(json);
            if (payload?.Multipliers is null)
            {
                return;
            }

            ClassName = payload.ClassName;

            foreach (var kv in payload.Multipliers)
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

    /// <summary>Espelho do payload server (SkillMultipliersResponse) — Item 010. Props (não campos) p/ o Newtonsoft preencher sem CS0649.</summary>
    private sealed class Payload
    {
        [JsonProperty("className")] public string? ClassName { get; set; }
        [JsonProperty("multipliers")] public Dictionary<string, double>? Multipliers { get; set; }
    }
}
