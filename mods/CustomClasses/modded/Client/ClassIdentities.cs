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
    ///     <b>B14 (coop 2026-07-11)</b> — nome EN da classe de QUALQUER player, local ou remoto. Null = vanilla/desconhecido.
    ///     <para>
    ///     É a peça que destrava os perks de som em coop: os BOTS vivem no processo do HOST, então quem decide
    ///     o que a IA ouve é o host — inclusive para o barulho de um peer Fika. Os patches de som gateavam em
    ///     "player local", o que tornava Ghost Step/Stalker/Loud Operator um PLACEBO contra a IA para quem joga
    ///     como CLIENTE. Com isto, o host resolve a classe de QUEM EMITIU o som e aplica o multiplicador dela.
    ///     </para>
    ///     <para>
    ///     Player local → <see cref="SkillMultipliers"/> (a fonte autoritativa da própria classe).
    ///     Peer remoto → mapa nickname→classe da rota 057 (já existente; sem protocolo novo).
    ///     ⚠️ O VALOR do multiplicador vem do F12 de QUEM ESTÁ RODANDO ISTO (o host). Ou seja, o host é a
    ///     autoridade da percepção da IA — coerente, já que a IA é dele. Sem sync de config entre peers.
    ///     </para>
    /// </summary>
    /// <summary>
    ///     B14 — REFETCH forçado do mapa. Chamado no <c>GameWorld.OnGameStarted</c> (ainda na tela de loading,
    ///     então o GET síncrono é um hitch invisível).
    ///     <para>
    ///     É <c>Reset()</c> + <c>EnsureLoaded()</c>, e NÃO só um <c>EnsureLoaded()</c>, por dois motivos
    ///     (code-review 2026-07-11): (1) <c>_loaded</c> é marcado ANTES do GET e não há retry — se o PRIMEIRO
    ///     fetch da sessão falhar (server subindo, rota 404), o mapa fica VAZIO e marcado como carregado, e todo
    ///     perk de som de peer morre pela sessão inteira, em silêncio. (2) Os únicos outros pontos que invalidam
    ///     o mapa são de UI de MENU (<c>PartyPlayerItemPatch</c>, <c>ClassDetailLoadingPatch</c>) — num host
    ///     HEADLESS eles nunca rodam, e uma troca de classe no editor web nunca refletiria.
    ///     </para>
    ///     <para>
    ///     Também garante que a 1ª resolução de um peer NÃO caia dentro do <c>BotEventHandler.PlaySound</c>
    ///     (a cada passo!), onde o GET síncrono viraria hitch no meio da raid.
    ///     </para>
    /// </summary>
    public static void Prefetch()
    {
        Reset();
        EnsureLoaded();
    }

    public static string? ClassNameEnOf(EFT.Player? player)
    {
        // ⚠️ BOTS FORA — gate crítico, não é defensivo à toa: bots também emitem passo por
        // `BotEventHandler.PlaySound` (MovementContext.cs:1629 passa o Player do bot como `person`), e o EFT
        // gera nome de bot a partir de uma LISTA DE NICKNAMES REAIS. Sem este gate, um bot cujo nickname
        // COLIDISSE com o de um jogador no mapa herdaria a classe dele e teria o próprio som alterado.
        if (player is null || player.IsAI)   // ref: Player.cs:25135
        {
            return null;
        }

        if (player.IsYourPlayer)
        {
            SkillMultipliers.EnsureLoaded();
            return SkillMultipliers.ClassNameEn;
        }

        return TryResolve(player.Profile?.Nickname, out var identity) ? identity.NameEn : null;
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
