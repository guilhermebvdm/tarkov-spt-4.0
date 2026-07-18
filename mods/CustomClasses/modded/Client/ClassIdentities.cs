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
        public string? NameEn, NamePt, IconFile;
        public string? DisplayName => GameLocale.IsPortuguese ? (NamePt ?? NameEn) : (NameEn ?? NamePt);

        // item 068: description localizada (en/pt), resolvida pelo idioma do EFT — aba CLASS + tooltip.
        public string? DescriptionEn, DescriptionPt;
        public string? Description => DescriptionEn == null && DescriptionPt == null
            ? null
            : (GameLocale.IsPortuguese ? (DescriptionPt ?? DescriptionEn) : (DescriptionEn ?? DescriptionPt));

        // 067: NameColor resolve o override do F12 (por classe, keyed no NameEn) → fallback da cor CRUA do
        // server (o setter grava _serverNameColor). Assim todo consumidor de Identity.NameColor pega o override
        // sem tocar em patch nenhum. O getter é lazy (relido a cada acesso) — mudança no F12 vale na hora.
        private string? _serverNameColor;
        public string? NameColor
        {
            get => ClassColorOverride.Resolve(NameEn) ?? _serverNameColor;
            set => _serverNameColor = value;
        }
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
        // ⚠️ NÃO é Reset() + EnsureLoaded() (code-review B20, F1 — regressão pega antes de rodar).
        // Aquela forma DESTRUÍA o mapa bom antes de tentar buscar o novo, sem rollback: um único GET falho no
        // raid-start (server remoto ocupado gerando a raid, hiccup de rede) deixava o mapa VAZIO e marcado como
        // carregado, PELA RAID INTEIRA — e como o hot path não busca mais sozinho, não havia retry. Todo perk de
        // som morreria em silêncio (o 2º aviso em diante é LogDebug). Agora: busca primeiro, troca só em sucesso.
        if (TryFetch(out var fresh))
        {
            Commit(fresh);
            _loaded = true;
            return;
        }

        // Falhou. Se havia mapa da sessão anterior, PRESERVA (stale > morto — nicknames e classes mudam pouco).
        // Se não havia nada a preservar, deixa destravado para o próximo consumidor LAZY (fora do hot path) tentar.
        if (ByNickname.Count == 0)
        {
            _loaded = false;
        }
    }

    /// <summary>
    ///     <b>B14 (coop) + B20 (rolloff)</b> — nome EN da classe de QUALQUER player, local ou remoto.
    ///     Null = vanilla/bot/desconhecido.
    ///     <para>
    ///     É a peça que destrava os perks de som em coop, em DOIS canais distintos:
    ///     (1) <b>percepção da IA</b> — os bots vivem no processo do HOST, então quem decide o que eles ouvem é o
    ///     host, inclusive para o barulho de um peer Fika; (2) <b>áudio que um humano ouve</b> — o
    ///     <c>Player.method_67</c> de um peer roda NO CLIENTE DE QUEM OUVE. Os patches de som gateavam em "player
    ///     local", o que tornava Ghost Step/Stalker/Loud Operator um PLACEBO nos dois canais. Com isto, cada
    ///     pipeline resolve a classe de QUEM EMITIU o som e aplica o multiplicador DELA.
    ///     </para>
    ///     <para>
    ///     Player local → <see cref="SkillMultipliers"/> (fonte autoritativa da própria classe).
    ///     Peer remoto → mapa nickname→classe da rota 057 (já existente; sem protocolo novo).
    ///     </para>
    ///     <para>
    ///     ⚠️ <b>O VALOR do multiplicador vem do F12 de QUEM ESTÁ RODANDO ISTO</b>, e NÃO há sync de config entre
    ///     peers: o host é a autoridade da percepção da IA (a IA é dele) e cada cliente é a autoridade do que ELE
    ///     ouve. Logo, um peer com F12 divergente percebe o som de forma divergente — o perk não "vaza" errado, mas
    ///     também não é imposto. <b>Decisão do projeto (2026-07-12): a config é distribuída IDÊNTICA para todos</b>,
    ///     o que torna o sync desnecessário. Se um dia essa premissa cair (config por jogador), aí sim seria preciso
    ///     um protocolo — o server teria de ser a fonte dos valores, não o F12.
    ///     </para>
    /// </summary>
    public static string? ClassNameEnOf(EFT.Player? player)
    {
        // ⚠️ BOTS FORA — gate crítico, não é defensivo à toa: bots também emitem passo por
        // `BotEventHandler.PlaySound` (MovementContext.cs:1629 passa o Player do bot como `person`), e o EFT
        // gera nome de bot a partir de uma LISTA DE NICKNAMES REAIS. Sem este gate, um bot cujo nickname
        // COLIDISSE com o de um jogador no mapa herdaria a classe dele e teria o próprio som alterado.
        // (É também o gate mais barato: bot é a MAIORIA das chamadas deste hot path → sai na 1ª linha.)
        if (player is null || player.IsAI)   // ref: Player.cs:25135
        {
            return null;
        }

        if (player.IsYourPlayer)
        {
            return SkillMultipliers.ClassNameEn;
        }

        // ⚠️ HOT PATH — lookup CRU no dict, sem EnsureLoaded (code-review B14, achado 4). Este método roda a cada
        // passo de CADA player (BotEventHandler.PlaySound) e, desde o B20, também no rolloff. Passar pelo
        // TryResolve() faria um `RequestHandler.GetJson` SÍNCRONO na main thread se o mapa estivesse frio —
        // freeze no meio da raid. Quem garante o mapa quente é o Prefetch() do raid-start; frio aqui = sem perk
        // (degradação silenciosa), nunca um hitch.
        var nickname = player.Profile?.Nickname;
        return nickname != null && ByNickname.TryGetValue(nickname, out var identity) ? identity.NameEn : null;
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
                // 067: cor CRUA do server (não a já resolvida) → o getter da Identity resolve o override 1× por
                // cima, sem dupla resolução nem risco de fixar um hex de override obsoleto no fallback.
                NameColor = SkillMultipliers.ServerNameColor,
                DescriptionEn = SkillMultipliers.DescriptionEn,   // item 068
                DescriptionPt = SkillMultipliers.DescriptionPt,   // item 068
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

        if (TryFetch(out var fresh))
        {
            Commit(fresh);
        }
    }

    /// <summary>
    ///     Baixa o mapa para um dict NOVO. Não toca no estado vigente — é o que permite ao <see cref="Prefetch"/>
    ///     ser não-destrutivo (code-review B20, F1). False = rota ausente/erro de rede.
    /// </summary>
    private static bool TryFetch(out Dictionary<string, Identity> fresh)
    {
        fresh = new Dictionary<string, Identity>(StringComparer.Ordinal);

        try
        {
            var json = RequestHandler.GetJson("/customclasses/class-identities");
            var payload = JsonConvert.DeserializeObject<Payload>(json);
            foreach (var p in payload?.Players ?? new List<PlayerEntry>())
            {
                if (!string.IsNullOrEmpty(p.Nickname) && !fresh.ContainsKey(p.Nickname!))
                {
                    fresh[p.Nickname!] = new Identity
                    {
                        NameEn = p.ClassNameEn,
                        NamePt = p.ClassNamePt,
                        IconFile = p.IconFile,
                        NameColor = p.NameColor,
                        DescriptionEn = p.DescriptionEn,   // item 068
                        DescriptionPt = p.DescriptionPt,   // item 068
                    };
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            // rota ausente (mod server antigo) ou erro de rede → degrada p/ identidade só do local (critério da 01-spec).
            // ref: CR-01-01 — o WARNING sai 1× por sessão (resto em Debug).
            if (!_warnedUnavailable)
            {
                _warnedUnavailable = true;
                Plugin.Log?.LogWarning($"[CustomClasses] (057) class-identities indisponível — identidade só local: {ex.Message}");
            }
            else
            {
                Plugin.Log?.LogDebug($"[CustomClasses] (057) class-identities ainda indisponível: {ex.Message}");
            }

            return false;
        }
    }

    private static void Commit(Dictionary<string, Identity> fresh)
    {
        ByNickname.Clear();
        foreach (var kv in fresh)
        {
            ByNickname[kv.Key] = kv.Value;
        }

        Plugin.Log?.LogInfo($"[CustomClasses] (057) {ByNickname.Count} identidade(s) de classe carregada(s).");
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
        [JsonProperty("descriptionEn")] public string? DescriptionEn { get; set; }   // item 068
        [JsonProperty("descriptionPt")] public string? DescriptionPt { get; set; }   // item 068
    }
}
