using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;                  // StaticRouter, RouteAction
using SPTarkov.Server.Core.Models.Eft.Common;   // EmptyRequestData
using SPTarkov.Server.Core.Utils;               // JsonUtil

namespace CustomClasses;

/// <summary>
///     Item 058: <c>GET /customclasses/classes</c> — lista PÚBLICA (sem sessionId; pré-registro) das
///     classes registradas, no contrato SP0 do launcher (004-classes-dados-reais, congelado 2026-07-03).
///     Fontes: <see cref="ClassEditorService.GetCachedEntries"/> (cache por (mtime,length) do item 037 —
///     zero dry-run no hot path), <see cref="ClassEditionKeyRegistry"/> (chave EFETIVA gravada no
///     <see cref="ClassRegistrar.Commit"/> — nunca re-derivar a língua aqui) e
///     <see cref="SkillMultiplierRegistry"/> (fatores normalizados em vigor).
///     Filtro: Enabled &amp;&amp; Registered — mapeamento presente no key registry + ownership no
///     <see cref="ClassVisualRegistry"/> (mesmo critério do SkillMultipliersRouter).
///     Padrão StaticRouter/RouteAction: modelo em <see cref="SkillMultipliersRouter"/>.
/// </summary>
[Injectable]
public class ClassListRouter : StaticRouter
{
    public ClassListRouter(
        JsonUtil jsonUtil,
        ClassEditorService editorService,
        ClassEditionKeyRegistry keyRegistry,
        ClassVisualRegistry visualRegistry,
        SkillMultiplierRegistry multiplierRegistry)
        : base(jsonUtil, GetRoutes(jsonUtil, editorService, keyRegistry, visualRegistry, multiplierRegistry))
    {
    }

    private static List<RouteAction> GetRoutes(
        JsonUtil jsonUtil,
        ClassEditorService editorService,
        ClassEditionKeyRegistry keyRegistry,
        ClassVisualRegistry visualRegistry,
        SkillMultiplierRegistry multiplierRegistry)
    {
        return
        [
            new RouteAction<EmptyRequestData>(
                "/customclasses/classes",
                (url, info, sessionId, output) =>
                {
                    // sessionId ignorado de propósito: rota pré-registro (launcher ainda não tem perfil).
                    var items = new List<ClassListItem>();
                    foreach (var entry in editorService.GetCachedEntries())
                    {
                        var def = entry.Definition;
                        if (def is null || !entry.Enabled)
                        {
                            continue;   // unparseable ou enabled:false — nunca servida
                        }

                        // Chave EFETIVA registrada a partir DESTE arquivo (Commit grava; language=pt
                        // re-chaveia p/ displayName.pt). null → nunca registrou (skip no boot por
                        // colisão/erro) ou já foi removida. NÃO usar entry.Registered: ele checa
                        // def.Name cru e é falso-negativo sob language=pt (spec 058 §Resolução).
                        var editionKey = keyRegistry.GetEditionKey(entry.FileName);
                        if (editionKey is null || !visualRegistry.Contains(editionKey))
                        {
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
                            Skills = def.Skills ?? [],
                            SkillMultipliers = multiplierRegistry.Get(editionKey),
                        });
                    }

                    var json = jsonUtil.Serialize(items) ?? "[]";
                    return new ValueTask<string>(json);
                }),
        ];
    }
}
