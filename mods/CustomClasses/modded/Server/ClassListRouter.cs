using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;                  // StaticRouter, RouteAction
using SPTarkov.Server.Core.Models.Eft.Common;   // EmptyRequestData
using SPTarkov.Server.Core.Models.Enums;        // SkillTypes (CR-01-05: normalização de skills)
using SPTarkov.Server.Core.Models.Utils;        // ISptLogger
using SPTarkov.Server.Core.Utils;               // JsonUtil

namespace CustomClasses;

/// <summary>
///     Item 058: <c>GET /customclasses/classes</c> — lista PÚBLICA (sem sessionId; pré-registro) das
///     classes registradas, no contrato SP0 do launcher (004-classes-dados-reais, congelado 2026-07-03).
///     Fontes: <see cref="ClassEditorService.GetCachedEntries"/> (cache por (mtime,length) do item 037 —
///     zero dry-run no hot path), <see cref="ClassEditionKeyRegistry"/> (chave EFETIVA gravada no
///     <see cref="ClassRegistrar.Commit"/>; a resolução de língua vive no pipeline — ref: CR-01-01) e
///     <see cref="SkillMultiplierRegistry"/> (fatores normalizados em vigor).
///     Filtro: Enabled &amp;&amp; Registered — mapeamento presente no key registry + ownership no
///     <see cref="ClassVisualRegistry"/> (mesmo critério do SkillMultipliersRouter) + dedupe por
///     editionKey (ref: CR-01-03 — contrato assume chave única no array).
///     Padrão StaticRouter/RouteAction: modelo em <see cref="SkillMultipliersRouter"/>.
///     Proveniência (ref: CR-01-05): displayName/description/skills refletem o ARQUIVO atual (cache por
///     mtime); editionKey/skillMultipliers refletem o último Commit. Edit externo sem hot-apply pode
///     divergir do template registrado até restart — aceito e documentado (as-built §Decisões).
/// </summary>
[Injectable]
public class ClassListRouter : StaticRouter
{
    public ClassListRouter(
        JsonUtil jsonUtil,
        ClassEditorService editorService,
        ClassEditionKeyRegistry keyRegistry,
        ClassVisualRegistry visualRegistry,
        SkillMultiplierRegistry multiplierRegistry,
        ISptLogger<ClassListRouter> logger)
        : base(jsonUtil, GetRoutes(jsonUtil, editorService, keyRegistry, visualRegistry, multiplierRegistry, logger))
    {
    }

    private static List<RouteAction> GetRoutes(
        JsonUtil jsonUtil,
        ClassEditorService editorService,
        ClassEditionKeyRegistry keyRegistry,
        ClassVisualRegistry visualRegistry,
        SkillMultiplierRegistry multiplierRegistry,
        ISptLogger<ClassListRouter> logger)
    {
        return
        [
            new RouteAction<EmptyRequestData>(
                "/customclasses/classes",
                (url, info, sessionId, output) =>
                {
                    // sessionId ignorado de propósito: rota pré-registro (launcher ainda não tem perfil).
                    var items = new List<ClassListItem>();
                    var seenKeys = new HashSet<string>(StringComparer.Ordinal);   // ref: CR-01-03
                    foreach (var entry in editorService.GetCachedEntries())
                    {
                        var def = entry.Definition;
                        if (def is null || !entry.Enabled)
                        {
                            continue;   // unparseable ou enabled:false — nunca servida
                        }

                        // Chave EFETIVA registrada a partir DESTE arquivo (Commit grava; a resolução de
                        // língua vive no pipeline compartilhado — ref: CR-01-01). null → nunca registrou
                        // (skip no boot por colisão/erro) ou já foi removida. Mapeamento por arquivo é
                        // gate mais preciso que entry.Registered (distingue o arquivo dono na colisão).
                        var editionKey = keyRegistry.GetEditionKey(entry.FileName);
                        if (editionKey is null || !visualRegistry.Contains(editionKey))
                        {
                            continue;
                        }

                        // ref: CR-01-03: dois arquivos mapeados p/ a mesma edition (Save de arquivo
                        // colidido/rename cross-file) → dedupe, primeiro vence (ordem determinística
                        // por fileName). Contrato SP0 assume editionKey única no array.
                        if (!seenKeys.Add(editionKey))
                        {
                            logger.Warning(
                                $"[CustomClasses] /customclasses/classes: '{entry.FileName}' mapeia p/ editionKey "
                                + $"'{editionKey}' já servida por outro arquivo — item pulado (dedupe). "
                                + "Corrija a colisão de nomes nos arquivos de classe.");
                            continue;
                        }

                        var name = def.Name?.Trim();
                        items.Add(new ClassListItem
                        {
                            EditionKey = editionKey,
                            DisplayName = new LocalizedPair
                            {
                                En = def.DisplayName?.En ?? name,
                                Pt = def.DisplayName?.Pt ?? name,
                            },
                            Description = new LocalizedPair
                            {
                                En = def.Description?.En,
                                Pt = def.Description?.Pt,
                            },
                            // Mesma montagem das páginas web (Classes.razor:250) — sem URL-encode
                            // (ícones são slugs ASCII; consistência com o editor).
                            IconUrl = def.IconFile is { Length: > 0 } icon
                                ? $"/CustomClasses-Server/icons/{icon}"
                                : null,
                            NameColor = def.NameColor,
                            Skills = NormalizeSkills(def.Skills),
                            SkillMultipliers = multiplierRegistry.Get(editionKey),
                            // Item 029: perks/drawbacks (snapshot nominal). Join por DisplayName.En (chave do
                            // ByClass), NÃO por editionKey (ref: handoff 029-01 §3). null quando a classe não
                            // tem entrada no catálogo (ex.: Peladão) — WhenWritingNull omite do JSON.
                            Effects = BuildEffects(def.DisplayName?.En ?? name),
                        });
                    }

                    var json = jsonUtil.Serialize(items) ?? "[]";
                    return new ValueTask<string>(json);
                }),
        ];
    }

    /// <summary>
    ///     Item 029: monta os <c>effects</c> da classe a partir do <see cref="PerksCatalogData"/> (snapshot
    ///     nominal), achatando os efeitos na ordem de exibição. null quando a classe não está no catálogo
    ///     (sem perks) — o WhenWritingNull omite o campo. O token/isPerk são derivados aqui (mesma lógica do
    ///     MultiplierFormat/PerkLine do client).
    /// </summary>
    private static IReadOnlyList<ClassEffect>? BuildEffects(string? classNameEn)
    {
        if (classNameEn is null || !PerksCatalogData.ByClass.TryGetValue(classNameEn, out var lines) || lines.Length == 0)
        {
            return null;
        }

        var effects = new List<ClassEffect>(lines.Length);
        foreach (var line in lines)
        {
            effects.Add(new ClassEffect
            {
                IsPerk = PerksCatalogData.IsPerk(line),
                Pending = line.Pending,
                Title = new LocalizedPair { En = line.TitleEn, Pt = line.TitlePt },
                Label = new LocalizedPair { En = line.LabelEn, Pt = line.LabelPt },
                ValueToken = PerksCatalogData.ValueToken(line),
            });
        }

        return effects;
    }

    /// <summary>
    ///     ref: CR-01-05: espelha <c>ClassRegistrar.ApplySkills</c> — nome normalizado pelo enum
    ///     (TryParse ignoreCase + IsDefined; desconhecidas fora) e nível clamped 0..51. O launcher
    ///     nunca vê uma skill que o pipeline não aplicaria, nem nível que não existe.
    /// </summary>
    private static Dictionary<string, int> NormalizeSkills(Dictionary<string, int>? skills)
    {
        var normalized = new Dictionary<string, int>(StringComparer.Ordinal);
        if (skills is null or { Count: 0 })
        {
            return normalized;
        }

        foreach (var (skillName, level) in skills)
        {
            if (Enum.TryParse<SkillTypes>(skillName, ignoreCase: true, out var skill)
                && Enum.IsDefined(typeof(SkillTypes), skill))
            {
                normalized[skill.ToString()] = Math.Clamp(level, 0, 51);
            }
        }

        return normalized;
    }
}
