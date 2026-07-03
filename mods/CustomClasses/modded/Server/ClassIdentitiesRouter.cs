using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;                  // StaticRouter, RouteAction
using SPTarkov.Server.Core.Models.Eft.Common;   // EmptyRequestData
using SPTarkov.Server.Core.Servers;             // SaveServer
using SPTarkov.Server.Core.Utils;               // JsonUtil

namespace CustomClasses;

/// <summary>
///     Item 057: rota estática com o mapa nickname → identidade de classe de TODOS os perfis do server.
///     Read-only, computada por request (perfis já em memória — ref: spt-source SaveServer.cs:147, GetProfiles()
///     devolve cópia). A resolução perfil→classe é a MESMA da rota skill-multipliers: ProfileInfo.Edition é a
///     chave do ClassVisualRegistry (idioma-independente). Molde: SkillMultipliersRouter.
/// </summary>
[Injectable]
public class ClassIdentitiesRouter : StaticRouter
{
    // PA-01-09: editions fora do registry já vistas (vanilla legítima OU classe renomeada/apagada — indistinguíveis
    // aqui). Acumuladas 1× por edition, nunca por request; diagnóstico via inspeção, sem logger no molde do router.
    private static readonly HashSet<string> SeenUnknownEditions = new(StringComparer.Ordinal);

    public ClassIdentitiesRouter(JsonUtil jsonUtil, ClassVisualRegistry visualRegistry, SaveServer saveServer)
        : base(jsonUtil, GetRoutes(jsonUtil, visualRegistry, saveServer))
    {
    }

    private static List<RouteAction> GetRoutes(JsonUtil jsonUtil, ClassVisualRegistry visualRegistry, SaveServer saveServer)
    {
        return
        [
            new RouteAction<EmptyRequestData>(
                "/customclasses/class-identities",
                (url, info, sessionId, output) =>
                {
                    var response = new ClassIdentitiesResponse();
                    var seen = new HashSet<string>(StringComparer.Ordinal);
                    // PA-01-10: ordena pela chave → dedup de nickname determinístico entre restarts do server.
                    foreach (var kv in saveServer.GetProfiles().OrderBy(kv => kv.Key.ToString(), StringComparer.Ordinal))   // ref: spt-source SaveServer.cs:147
                    {
                        var profile = kv.Value;
                        var edition = profile?.ProfileInfo?.Edition;   // ref: spt-source SptProfile.cs:100; mesmo caminho da SkillMultipliersRouter.cs:34
                        var nickname = profile?.CharacterData?.PmcData?.Info?.Nickname;
                        if (string.IsNullOrEmpty(edition) || string.IsNullOrEmpty(nickname))
                        {
                            continue;   // perfil recém-criado/corrompido (sem PMC) → pulado
                        }

                        var visual = visualRegistry.Get(edition!);   // ref: ClassVisualRegistry.cs:29
                        if (visual == null)
                        {
                            SeenUnknownEditions.Add(edition!);   // PA-01-09
                            continue;   // edition vanilla ou órfã → sem identidade (corner da 01-spec)
                        }

                        if (!seen.Add(nickname!))
                        {
                            continue;   // nickname duplicado — primeira ocorrência (ordem estável) vence (corner da 01-spec)
                        }

                        response.Players.Add(new PlayerClassIdentity
                        {
                            Nickname = nickname,
                            ClassNameEn = visual.DisplayNameEn ?? edition,
                            ClassNamePt = visual.DisplayNamePt ?? edition,
                            IconFile = visual.IconFile,
                            NameColor = visual.NameColor,
                        });
                    }

                    return new ValueTask<string>(jsonUtil.Serialize(response) ?? "{}");
                }),
        ];
    }
}
