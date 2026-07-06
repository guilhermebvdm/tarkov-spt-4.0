using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SPT.Common.Http;     // RequestHandler — mesmo transporte do SkillMultipliers

namespace CustomClasses.Client;

/// <summary>
///     Item 057 — mapa nickname → identidade de classe de TODOS os players do server (rota
///     /customclasses/class-identities). Fetch LAZY (molde: <see cref="SkillMultipliers.EnsureLoaded"/>);
///     o <see cref="ClassDetailLoadingPatch"/> chama <see cref="Reset"/> quando a INSTÂNCIA da tela de loading
///     muda (PA-01-04 → 1 fetch por raid; perfis novos entram sem restart do client). Rota ausente/erro →
///     mapa vazio + 1 aviso por fetch (degrada pro comportamento 055 via <see cref="Local"/>).
/// </summary>
internal static class ClassIdentities
{
    /// <summary>Identidade de exibição de UMA classe (nome en/pt + ícone + cor). DisplayName segue o idioma do EFT.</summary>
    internal sealed class Identity
    {
        public string? NameEn, NamePt, IconFile, NameColor;
        public string? DisplayName => GameLocale.IsPortuguese ? (NamePt ?? NameEn) : (NameEn ?? NamePt);
    }

    private static readonly Dictionary<string, Identity> ByNickname = new(StringComparer.Ordinal);
    private static bool _loaded;
    private static bool _warnedUnavailable;   // ref: CR-01-01 — NÃO é limpa pelo Reset: 1 aviso por sessão (critério da 01-spec)

    /// <summary>Resolve a identidade da classe de um player pelo nickname (mapa do server). False se vanilla/desconhecido.</summary>
    public static bool TryResolve(string? nickname, out Identity identity)
    {
        EnsureLoaded();
        identity = null!;
        return nickname != null && ByNickname.TryGetValue(nickname, out identity!);
    }

    /// <summary>
    ///     PA-01-07/08 — identidade do player LOCAL via <see cref="SkillMultipliers"/> (fallback quando a rota
    ///     está ausente + caminho dos call-sites locais 053/059). Null se a classe local é vanilla.
    /// </summary>
    public static Identity? Local()
    {
        SkillMultipliers.EnsureLoaded();
        return SkillMultipliers.ClassNameEn == null
            ? null
            : new Identity
            {
                NameEn = SkillMultipliers.ClassNameEn,
                NamePt = SkillMultipliers.ClassNamePt,
                IconFile = SkillMultipliers.IconFile,
                NameColor = SkillMultipliers.NameColor,
            };
    }

    /// <summary>Limpa o mapa e força novo fetch no próximo TryResolve (PA-01-04: 1×/tela de loading).</summary>
    public static void Reset()
    {
        ByNickname.Clear();
        _loaded = false;
    }

    private static void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;   // marca antes: falha não retenta em loop (molde SkillMultipliers.cs:70)

        try
        {
            var json = RequestHandler.GetJson("/customclasses/class-identities");
            var payload = JsonConvert.DeserializeObject<Payload>(json);
            foreach (var p in payload?.Players ?? new List<PlayerEntry>())
            {
                if (!string.IsNullOrEmpty(p.Nickname) && !ByNickname.ContainsKey(p.Nickname!))
                {
                    ByNickname[p.Nickname!] = new Identity
                    {
                        NameEn = p.ClassNameEn,
                        NamePt = p.ClassNamePt,
                        IconFile = p.IconFile,
                        NameColor = p.NameColor,
                    };
                }
            }

            Plugin.Log?.LogInfo($"[CustomClasses] (057) {ByNickname.Count} identidade(s) de classe carregada(s).");
        }
        catch (Exception ex)
        {
            // rota ausente (mod server antigo) ou erro de rede → degrada p/ identidade só do local (critério da 01-spec).
            // ref: CR-01-01 — o Reset() por raid re-dispara o fetch; o WARNING sai 1× por sessão (resto em Debug).
            if (!_warnedUnavailable)
            {
                _warnedUnavailable = true;
                Plugin.Log?.LogWarning($"[CustomClasses] (057) class-identities indisponível — identidade só local: {ex.Message}");
            }
            else
            {
                Plugin.Log?.LogDebug($"[CustomClasses] (057) class-identities ainda indisponível: {ex.Message}");
            }
        }
    }

    /// <summary>Espelho do payload server (ClassIdentitiesResponse) — props p/ o Newtonsoft preencher sem CS0649.</summary>
    private sealed class Payload
    {
        [JsonProperty("players")] public List<PlayerEntry>? Players { get; set; }
    }

    private sealed class PlayerEntry
    {
        [JsonProperty("nickname")]    public string? Nickname { get; set; }
        [JsonProperty("classNameEn")] public string? ClassNameEn { get; set; }
        [JsonProperty("classNamePt")] public string? ClassNamePt { get; set; }
        [JsonProperty("iconFile")]    public string? IconFile { get; set; }
        [JsonProperty("nameColor")]   public string? NameColor { get; set; }
    }
}
