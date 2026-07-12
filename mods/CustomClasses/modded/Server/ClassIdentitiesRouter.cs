using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;                  // StaticRouter, RouteAction
using SPTarkov.Server.Core.Models.Eft.Common;    // EmptyRequestData
using SPTarkov.Server.Core.Models.Eft.Profile;   // SptProfile (B20/F2 — tipo do helper Add)
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
                    // ref: spt-source SaveServer.cs:147
                    var profiles = saveServer.GetProfiles()
                        .OrderBy(kv => kv.Key.ToString(), StringComparer.Ordinal)
                        .Select(kv => kv.Value)
                        .Where(p => !string.IsNullOrEmpty(p?.ProfileInfo?.Edition)
                                    && !string.IsNullOrEmpty(p?.CharacterData?.PmcData?.Info?.Nickname))   // sem PMC = perfil recém-criado/corrompido
                        .Select(p => (Profile: p, Visual: visualRegistry.Get(p!.ProfileInfo!.Edition!)))   // ref: ClassVisualRegistry.cs:29
                        // ref: CR-01-02/03 — edition vanilla ou órfã → sem identidade, SEM log (o server não
                        // distingue as duas; comportamento seguro; corner da 01-spec emendado nesta decisão).
                        .Where(x => x.Visual != null)
                        .ToList();

                    // ⚠️ DOIS PASSES, e a ordem importa (code-review B20, F2). O nickname de SCAV vem do pool de
                    // nomes REAIS do EFT, então o scav do perfil A pode colidir com o PMC do perfil B. Num passe só
                    // (PMC+scav intercalados por perfil), o scav de A — que ordena antes — roubaria a entrada do PMC
                    // de B, e um jogador real passaria a resolver para a CLASSE ERRADA. Pior: o nickname de scav é
                    // regerado a cada raid de scav, então o sintoma apareceria e sumiria sozinho (heisenbug).
                    // PMC é identidade estável e sempre ganha; scav só entra no que sobrou.
                    foreach (var (profile, visual) in profiles)
                    {
                        Add(profile!.CharacterData!.PmcData!.Info!.Nickname, profile, visual!);
                    }

                    // B14/B20 (achado 5): quem entra de SCAV carrega o nickname do SCAV no `Profile.Nickname` que o
                    // client consulta — sem isto, o peer scav não resolvia e os perks de som dele viravam placebo
                    // (o player LOCAL escapava por curto-circuito em IsYourPlayer, o que mascarava o furo em solo).
                    // Colisão scav↔bot-scav é esperada (mesmo pool) e inofensiva: o gate `IsAI` do
                    // ClassIdentities.ClassNameEnOf nunca deixa um bot resolver.
                    foreach (var (profile, visual) in profiles)
                    {
                        Add(profile?.CharacterData?.ScavData?.Info?.Nickname, profile!, visual!);
                    }

                    return new ValueTask<string>(jsonUtil.Serialize(response) ?? "{}");

                    void Add(string? nickname, SptProfile profile, ClassVisualRegistry.Visual visual)
                    {
                        if (string.IsNullOrEmpty(nickname) || !seen.Add(nickname!))
                        {
                            return;   // vazio, ou já mapeado — 1ª ocorrência (ordem estável) vence (corner da 01-spec)
                        }

                        var edition = profile.ProfileInfo!.Edition;
                        response.Players.Add(new PlayerClassIdentity
                        {
                            Nickname = nickname,
                            ClassNameEn = visual.DisplayNameEn ?? edition,
                            ClassNamePt = visual.DisplayNamePt ?? edition,
                            IconFile = visual.IconFile,
                            NameColor = visual.NameColor,
                        });
                    }
                }),
        ];
    }
}
