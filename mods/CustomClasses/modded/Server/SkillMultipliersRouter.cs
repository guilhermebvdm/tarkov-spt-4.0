using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;                  // StaticRouter, RouteAction
using SPTarkov.Server.Core.Models.Eft.Common;   // EmptyRequestData
using SPTarkov.Server.Core.Servers;             // SaveServer
using SPTarkov.Server.Core.Utils;               // JsonUtil

namespace CustomClasses;

/// <summary>
///     Item 005/010/011: serve ao client a IDENTIDADE da classe (nome/ícone/cor) + os multiplicadores de XP
///     da edition do session atual. Resolve sessionId → profile.ProfileInfo.Edition.
///     Item 011: devolve className/iconFile/nameColor sempre que a edition for classe do mod
///     (visualRegistry.Contains), independente de ter skillMultipliers; edition vanilla → tudo null/vazio.
///     Padrão StaticRouter/RouteAction confirmado em mods/SkillDistribution + spt-source DI/Router.cs.
/// </summary>
[Injectable]
public class SkillMultipliersRouter : StaticRouter
{
    public SkillMultipliersRouter(JsonUtil jsonUtil, SkillMultiplierRegistry registry, ClassVisualRegistry visualRegistry, SaveServer saveServer)
        : base(jsonUtil, GetRoutes(jsonUtil, registry, visualRegistry, saveServer))
    {
    }

    private static List<RouteAction> GetRoutes(JsonUtil jsonUtil, SkillMultiplierRegistry registry, ClassVisualRegistry visualRegistry, SaveServer saveServer)
    {
        return
        [
            new RouteAction<EmptyRequestData>(
                "/customclasses/skill-multipliers",
                (url, info, sessionId, output) =>
                {
                    // ref: SaveServer.cs:118 (GetProfile(MongoId)); Edition usada em CreateProfileService.cs:44
                    // (fix 2026-07-04) server reiniciado com o client ABERTO → sessionId vazio e GetProfile
                    // lança ("did you restart the server while the game was running?"), sujando o console com
                    // stack — cenário conhecido/benigno. Sem sessão resolvível → payload vazio (o client
                    // degrada: sem identidade até reiniciar o EFT, com 1 log próprio).
                    SPTarkov.Server.Core.Models.Eft.Profile.SptProfile? profile;
                    try
                    {
                        profile = saveServer.GetProfile(sessionId);
                    }
                    catch
                    {
                        return new ValueTask<string>("{}");
                    }
                    var edition = profile?.ProfileInfo?.Edition ?? string.Empty;
                    var isClass = visualRegistry.Contains(edition);   // item 011: é classe do mod?
                    var visual = visualRegistry.Get(edition);
                    var dto = new SkillMultipliersResponse
                    {
                        // identidade exposta p/ qualquer classe do mod (mesmo sem multiplicadores). edition == nome da classe.
                        ClassName = isClass ? edition : null,
                        // item 008 (i18n): nome localizado en/pt (fallback ao edition/PT) — client resolve pelo idioma do EFT.
                        ClassNameEn = isClass ? (visual?.DisplayNameEn ?? edition) : null,
                        ClassNamePt = isClass ? (visual?.DisplayNamePt ?? edition) : null,
                        // item 015: nickname do PMC (CharacterData.PmcData.Info.Nickname) p/ o client casar o ChatSpecialIcon do jogador local.
                        Nickname = profile?.CharacterData?.PmcData?.Info?.Nickname,
                        IconFile = visual?.IconFile,
                        NameColor = visual?.NameColor,
                        Multipliers = registry.Get(edition),
                    };
                    var json = jsonUtil.Serialize(dto) ?? "{}";
                    return new ValueTask<string>(json);
                }),
        ];
    }
}
