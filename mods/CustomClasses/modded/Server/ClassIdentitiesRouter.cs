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
                        var pmcNickname = profile?.CharacterData?.PmcData?.Info?.Nickname;
                        if (string.IsNullOrEmpty(edition) || string.IsNullOrEmpty(pmcNickname))
                        {
                            continue;   // perfil recém-criado/corrompido (sem PMC) → pulado
                        }

                        var visual = visualRegistry.Get(edition!);   // ref: ClassVisualRegistry.cs:29
                        if (visual == null)
                        {
                            // ref: CR-01-02/03 — edition vanilla ou órfã → sem identidade, SEM log (server não
                            // distingue as duas; comportamento seguro; corner da 01-spec emendado nesta decisão).
                            continue;
                        }

                        // B14/B20 (code-review, achado 5): o mapa é consultado pelo `Profile.Nickname` do player
                        // RENDERIZADO no client. Quem entra de SCAV carrega o nickname do SCAV, não o do PMC — com
                        // só o PMC aqui, o peer scav não resolvia e os perks de som dele viravam placebo (o player
                        // LOCAL escapava por curto-circuito em IsYourPlayer, o que mascarava o furo em solo).
                        // ⚠️ O nickname de scav vem do MESMO pool de nomes dos bots-scav → colisão com bot é
                        // esperada; quem protege é o gate `IsAI` do ClassIdentities.ClassNameEnOf (bot nunca resolve).
                        var scavNickname = profile?.CharacterData?.ScavData?.Info?.Nickname;

                        foreach (var nickname in new[] { pmcNickname, scavNickname })
                        {
                            if (string.IsNullOrEmpty(nickname) || !seen.Add(nickname!))
                            {
                                continue;   // vazio, ou nickname duplicado — 1ª ocorrência (ordem estável) vence (corner da 01-spec)
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
                    }

                    return new ValueTask<string>(jsonUtil.Serialize(response) ?? "{}");
                }),
        ];
    }
}
