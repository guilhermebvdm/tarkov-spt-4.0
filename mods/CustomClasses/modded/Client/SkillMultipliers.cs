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

    /// <summary>Item 050: nome EN estável da classe (= campo `name` do config) — chave de gating idioma-independente. Null se vanilla.</summary>
    public static string? ClassNameEn => _classNameEn;

    /// <summary>Item 057 (PA-01-08): nome PT da classe local — p/ o fallback <c>ClassIdentities.Local()</c> não degradar pt→EN.</summary>
    public static string? ClassNamePt => _classNamePt;

    /// <summary>Item 050: true se a classe do perfil local é <paramref name="nameEn"/> (compara o nome EN estável, case-insensitive).</summary>
    public static bool IsLocalClass(string nameEn)
    {
        EnsureLoaded();
        return IsClass(_classNameEn, nameEn);
    }

    /// <summary>
    ///     B14 (coop): compara DUAS classes pelo nome EN estável (case-insensitive), sem assumir que uma delas é
    ///     a local. Necessário porque o HOST precisa avaliar a classe de um peer Fika (resolvida via
    ///     <see cref="ClassIdentities.ClassNameEnOf"/>), não só a sua. Null = vanilla/desconhecida → false.
    /// </summary>
    public static bool IsClass(string? classNameEn, string nameEn)
    {
        return classNameEn != null && string.Equals(classNameEn, nameEn, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Item 015: nickname do perfil local (p/ casar o ChatSpecialIcon do jogador local).</summary>
    public static string? Nickname { get; private set; }

    /// <summary>Item 011: nome do PNG do ícone da classe (null se ausente). Carregado via ClassIconCache.</summary>
    public static string? IconFile { get; private set; }

    /// <summary>Item 011: cor do nome da classe (hex #RRGGBB; null = cor default).</summary>
    public static string? NameColor { get; private set; }

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
