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
/// <summary>
///     ref: AUD-01-02 — id numérico da classe. A classe é IMUTÁVEL durante a raid; resolvê-la 1× no fetch
///     transforma todo gate (42+ call-sites, vários per-frame) numa comparação de inteiros, em vez de
///     <c>string.Equals(..., OrdinalIgnoreCase)</c>, que faz dobra de caixa caractere a caractere.
///     <para>
///     ⚠️ PA-01-06 — <c>internal</c>: casa com <c>ClassIdentities.Identity</c> (internal sealed) e evita
///     expor um tipo novo na superfície pública que o TRL-ImmersiveCombatMedicine consome por reflexão.
///     </para>
///     <para>
///     ⚠️ PA-04-02 — este enum é o <b>eixo canônico</b> de TRÊS listas paralelas de classe:
///     <see cref="SkillMultipliers.Parse"/> (gates), <c>PerksConfig.ClassColors</c> (cor do F12) e
///     <c>PerksCatalog.ByClass</c> (composição de perks). A checagem de boot em <c>PerksConfig.Bind</c>
///     é o que impede as três de divergirem em silêncio.
///     </para>
/// </summary>
internal enum EClassId
{
    None = 0,
    CombatMedic,
    Rifleman,
    Hunter,
    Stealth,
    Scavenger,
    Naked,
    Tank,
}

internal static class SkillMultipliers
{
    private static readonly Dictionary<ESkillId, float> Factors = new();
    private static bool _loaded;

    /// <summary>ref: AUD-01-02 — resolvido em <see cref="Apply"/>, zerado em <see cref="Reset"/>.</summary>
    private static EClassId _classId;

    private static bool _warnedUnknownClass;

    /// <summary>
    ///     ref: AUD-01-02 — id da classe local, CRU.
    ///     <para>
    ///     ⚠️ PA-04-05 — <b>NÃO chama <c>EnsureLoaded</c></b>, ao contrário do <see cref="IsLocalClass"/>,
    ///     e a diferença é DELIBERADA. Este acessor é o que o <c>ClassIdentities.ClassIdOf</c> usa no ramo
    ///     <c>IsYourPlayer</c>, e o <c>ClassIdOf</c> roda a cada passo de cada player/bot
    ///     (<c>BotEventHandler.PlaySound</c>): um GET síncrono ali seria freeze no meio da raid — foi
    ///     exatamente o que o achado 4 do code-review B14 tirou do hot path (ClassIdentities.cs:131-135).
    ///     Não "uniformizar" com o <see cref="IsLocalClass"/> em nenhuma das duas direções.
    ///     </para>
    /// </summary>
    internal static EClassId LocalClassId => _classId;

    /// <summary>
    ///     ref: PA-01-03 · PA-04-03 — disparado sempre que a classe/idioma resolvidos mudam (fim de
    ///     <see cref="Apply"/> e de <see cref="Reset"/>). Quem cacheia algo derivado da classe assina isto;
    ///     o <c>SkillMultipliers</c> não conhece consumidor nenhum. Molde: <c>PerksConfig.ClassColorsChanged</c>
    ///     (item 067). Assinatura estática↔estática, 1× no <c>Awake</c>, sem <c>-=</c> — mesma vida do plugin.
    /// </summary>
    internal static event Action? ClassChanged;

    /// <summary>
    ///     ref: AUD-01-02 — nome EN → id. Fonte única do mapeamento (os literais de classe existiam
    ///     espalhados em 42+ call-sites; agora existem só aqui, e o compilador cuida do resto).
    ///     <para>
    ///     ref: PA-03-06 — <paramref name="warnUnknown"/> existe para a checagem de boot (PA-02-03/PA-04-02)
    ///     não consumir o warn-once: ela emite o próprio erro, mais específico, e o warn-once fica
    ///     reservado ao caminho de runtime (fetch de peer, troca de perfil).
    ///     </para>
    /// </summary>
    internal static EClassId Parse(string? nameEn, bool warnUnknown = true)
    {
        if (string.IsNullOrEmpty(nameEn))
        {
            return EClassId.None;
        }

        // OrdinalIgnoreCase preservado (mesma semântica do IsClass antigo) — roda 1× por fetch, não por frame.
        if (string.Equals(nameEn, "Combat Medic", StringComparison.OrdinalIgnoreCase)) return EClassId.CombatMedic;
        if (string.Equals(nameEn, "Rifleman", StringComparison.OrdinalIgnoreCase)) return EClassId.Rifleman;
        if (string.Equals(nameEn, "Hunter", StringComparison.OrdinalIgnoreCase)) return EClassId.Hunter;
        if (string.Equals(nameEn, "Stealth", StringComparison.OrdinalIgnoreCase)) return EClassId.Stealth;
        if (string.Equals(nameEn, "Scavenger", StringComparison.OrdinalIgnoreCase)) return EClassId.Scavenger;
        if (string.Equals(nameEn, "Naked", StringComparison.OrdinalIgnoreCase)) return EClassId.Naked;
        if (string.Equals(nameEn, "Tank", StringComparison.OrdinalIgnoreCase)) return EClassId.Tank;

        // Corner case da 01-spec: edition órfã, ou classe nova criada no editor web. Degrada para None
        // (nenhum perk dispara) com 1 aviso por sessão — NUNCA casa com a classe errada.
        if (warnUnknown && !_warnedUnknownClass)
        {
            _warnedUnknownClass = true;
            Plugin.Log?.LogWarning($"[CustomClasses] (AUD-01-02) classe desconhecida '{nameEn}' — perks desligados p/ ela.");
        }

        return EClassId.None;
    }

    /// <summary>
    ///     ref: PA-03-01 — inverso do <see cref="Parse"/>: id → nome EN. <c>switch</c> puro, sem dicionário.
    ///     Existe SÓ para o diagnóstico (o <c>PerkDiag.LogPeer</c> precisa do nome legível) e para a checagem
    ///     de boot. <b>Chamar apenas de dentro de <c>if (PerkDiag.Enabled)</c></b> — nunca no caminho quente.
    /// </summary>
    internal static string? NameOf(EClassId id) => id switch
    {
        EClassId.CombatMedic => "Combat Medic",
        EClassId.Rifleman => "Rifleman",
        EClassId.Hunter => "Hunter",
        EClassId.Stealth => "Stealth",
        EClassId.Scavenger => "Scavenger",
        EClassId.Naked => "Naked",
        EClassId.Tank => "Tank",
        _ => null,
    };

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

    /// <summary>Item 050: nome EN estável da classe (= campo `name` do config) — chave de gating idioma-independente. Null se vanilla.</summary>
    public static string? ClassNameEn => _classNameEn;

    /// <summary>Item 057 (PA-01-08): nome PT da classe local — p/ o fallback <c>ClassIdentities.Local()</c> não degradar pt→EN.</summary>
    public static string? ClassNamePt => _classNamePt;

    /// <summary>Item 068: description localizada da classe (en/pt), resolvida pelo idioma do EFT. Null se ausente.</summary>
    public static string? Description => _descriptionEn == null && _descriptionPt == null
        ? null
        : (GameLocale.IsPortuguese ? (_descriptionPt ?? _descriptionEn) : (_descriptionEn ?? _descriptionPt));

    /// <summary>Item 068: description CRUA en/pt — p/ <c>ClassIdentities.Local()</c> propagar sem degradar idioma.</summary>
    public static string? DescriptionEn => _descriptionEn;
    public static string? DescriptionPt => _descriptionPt;
    private static string? _descriptionEn, _descriptionPt;

    /// <summary>
    ///     Item 050 · ref: AUD-01-02 — <b>o gate quente</b>. Comparação de int; sem alocação, sem string.
    ///     <para>
    ///     ⚠️ PA-02-08 — NÃO remover o <c>EnsureLoaded</c> por parecer redundante depois da migração. Ele é o
    ///     fetch PREGUIÇOSO para quando nenhum <c>Prefetch</c> rodou (menu, hideout, 1ª raid pós-restart do
    ///     server). Com o cache frio ele faz um GET HTTP SÍNCRONO — e é exatamente por isso que todo patch
    ///     que roda para bots/peers coloca o gate de INSTÂNCIA ANTES deste
    ///     (ref: CalmSightsPatch.cs:51-53, achado CR-F5).
    ///     </para>
    ///     <para>
    ///     Os overloads de <c>string</c> foram REMOVIDOS de propósito (PA-01-06): um wrapper de compatibilidade
    ///     deixaria call-sites antigos passarem despercebidos e anularia o ganho. Sem eles, o compilador aponta
    ///     cada call-site e um nome digitado errado vira erro de build em vez de perk silenciosamente morto.
    ///     </para>
    /// </summary>
    public static bool IsLocalClass(EClassId id)
    {
        EnsureLoaded();
        return id != EClassId.None && _classId == id;
    }

    /// <summary>
    ///     B14 (coop) · ref: AUD-01-02 — compara DUAS classes por id, sem assumir que uma delas é a local.
    ///     Necessário porque o HOST precisa avaliar a classe de um peer Fika (resolvida via
    ///     <see cref="ClassIdentities.ClassIdOf"/>), não só a sua. <c>None</c> = vanilla/desconhecida → false.
    /// </summary>
    public static bool IsClass(EClassId classId, EClassId id)
    {
        return id != EClassId.None && classId == id;
    }

    /// <summary>Item 015: nickname do perfil local (p/ casar o ChatSpecialIcon do jogador local).</summary>
    public static string? Nickname { get; private set; }

    /// <summary>Item 011: nome do PNG do ícone da classe (null se ausente). Carregado via ClassIconCache.</summary>
    public static string? IconFile { get; private set; }

    /// <summary>Item 011: cor do nome da classe (hex #RRGGBB; null = cor default).</summary>
    /// <remarks>067: resolve o override do F12 (por classe) → fallback da cor do server. Todos os consumidores
    /// leem isto, então pegam o override de graça. Para o valor CRU do server, use <see cref="ServerNameColor"/>.</remarks>
    public static string? NameColor => ClassColorOverride.Resolve(_classNameEn) ?? _serverNameColor;

    /// <summary>067: cor CRUA do server (sem o override do F12). Usada pela <c>ClassIdentities.Local()</c> como
    /// fallback, para o resolver da Identity não re-resolver por cima de um valor já resolvido.</summary>
    internal static string? ServerNameColor => _serverNameColor;
    private static string? _serverNameColor;

    /// <summary>
    ///     REFETCH forçado da classe local (code-review B14, achado 2). Simétrico ao
    ///     <see cref="ClassIdentities.Prefetch"/>, e pelo MESMO motivo: o mapa nickname→classe passou a vir fresco
    ///     todo raid-start, mas o único caller de <see cref="Reset"/> aqui é a tela de DEPLOY
    ///     (<c>PartyInfoPanelPrefetchPatch</c>) — que não roda em host headless nem quando o painel de grupo não é
    ///     renderizado. Sem isto, trocar de classe no editor web entre raids deixava o mapa com a classe NOVA e o
    ///     player local com a ANTIGA: perks locais errados a raid inteira, e incoerentes com o que o host aplica
    ///     aos peers. Chamado no <c>GameWorld.OnGameStarted</c> (tela de loading → GET síncrono é hitch invisível).
    /// </summary>
    public static void Prefetch()
    {
        // ⚠️ NÃO é Reset() + EnsureLoaded() (code-review B20, F1). Aquela forma zerava a classe e os fatores de XP
        // ANTES do GET, sem rollback: um GET falho no raid-start deixaria _classNameEn=null + Factors vazio
        // marcados como carregados pela raid inteira → perks de som E multiplicadores de XP mortos, em silêncio.
        // Busca primeiro, troca só em sucesso; em falha, preserva o que já estava carregado.
        if (TryFetch(out var payload))
        {
            Apply(payload);
            _loaded = true;
            return;
        }

        if (_classNameEn == null && _classNamePt == null && Factors.Count == 0)
        {
            _loaded = false;   // nada bom a preservar → destrava o retry lazy
        }
    }

    /// <summary>Reseta o cache (ex.: troca de perfil) — força novo fetch.</summary>
    public static void Reset()
    {
        Factors.Clear();
        _classNameEn = null;
        _classNamePt = null;
        Nickname = null;
        IconFile = null;
        _serverNameColor = null;   // 067: cor crua do server (o override vive no F12, não é resetado aqui)
        _descriptionEn = null;   // item 068
        _descriptionPt = null;   // item 068
        _classId = EClassId.None;   // ref: AUD-01-02
        _loaded = false;
        ClassChanged?.Invoke();     // ref: PA-04-03 — quem cacheia algo derivado da classe se invalida aqui
    }

    public static void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;   // marca antes: se falhar, não retenta em loop a cada ganho de XP

        if (TryFetch(out var payload))
        {
            Apply(payload);
        }
    }

    /// <summary>
    ///     Baixa o payload sem tocar no estado vigente — é o que torna o <see cref="Prefetch"/> não-destrutivo
    ///     (code-review B20, F1). False = rota fora do ar, JSON vazio ou payload nulo.
    /// </summary>
    private static bool TryFetch(out Payload payload)
    {
        payload = null!;

        try
        {
            var json = RequestHandler.GetJson("/customclasses/skill-multipliers");
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            // Item 010/011: payload é { className, iconFile, nameColor, multipliers }.
            payload = JsonConvert.DeserializeObject<Payload>(json)!;
            return payload is not null;
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] falha ao buscar multiplicadores de skill: {ex.Message}");
            return false;
        }
    }

    private static void Apply(Payload payload)
    {
        Factors.Clear();   // troca atômica: o estado antigo só cai agora, com o novo em mãos

        // Item 011: identidade setada mesmo sem multiplicadores (classe do mod sem skillMultipliers).
        // Item 008 (i18n): guarda en/pt; o getter ClassName resolve pelo idioma do EFT. Fallback ao className legado.
        _classNameEn = payload.ClassNameEn ?? payload.ClassName;
        _classNamePt = payload.ClassNamePt ?? payload.ClassName;
        _classId = Parse(_classNameEn);   // ref: AUD-01-02 — id resolvido junto com o nome (inclui o Prefetch)
        Nickname = payload.Nickname;
        IconFile = payload.IconFile;
        _serverNameColor = payload.NameColor;   // 067: guarda o valor CRU; NameColor resolve o override do F12
        _descriptionEn = payload.DescriptionEn;   // item 068
        _descriptionPt = payload.DescriptionPt;   // item 068

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

        ClassChanged?.Invoke();   // ref: PA-04-03 — invalida os caches derivados da classe (tooltip, grupos)
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
        [JsonProperty("descriptionEn")] public string? DescriptionEn { get; set; }   // item 068
        [JsonProperty("descriptionPt")] public string? DescriptionPt { get; set; }   // item 068
        [JsonProperty("multipliers")] public Dictionary<string, double>? Multipliers { get; set; }
    }
}
