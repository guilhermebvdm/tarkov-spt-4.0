# 057 — Identidade de classe per-player em coop (Fika) · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** [057-class-identity-coop-01-spec.md](057-class-identity-coop-01-spec.md)
**Criado:** 2026-07-03

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Este item toca sobretudo **FIKA** (fonte: `references/fika-plugin/`, 🥇 #3 da hierarquia) e **server SPT** (`references/spt-source/`, 🥇 #2) — o EFT client não é patcheado além do que o 055 já faz.

## 1. Estratégia

**Decisão de mecanismo (delegada pela 01-spec, agora resolvida): resolução 100% server-side por nickname.**
O recon confirmou que na tela de loading do FIKA o cliente só tem `netId + nickname` de cada player
(`LoadingScreenUI.AddPlayer(int, string)` — ref: fika-plugin/Fika.Core/UI/Custom/LoadingScreenUI.cs:97); os
perfis remotos NÃO estão disponíveis nesse momento, o que **descarta** a hipótese do recon do backlog (ler
`Profile.Info.GameVersion` dos remotos no client). Em contrapartida, no server a resolução perfil→classe é trivial
e idioma-independente: `profile.ProfileInfo.Edition` é **exatamente a chave** do `ClassVisualRegistry._byEdition`
(ref: modded/Server/SkillMultipliersRouter.cs:33-35 — mesmo caminho da rota existente). Logo:

- **Server:** nova rota estática `/customclasses/class-identities` (mesmo padrão `[Injectable] StaticRouter` da
  `SkillMultipliersRouter`) que enumera `saveServer.GetProfiles()` e devolve, por perfil com classe do mod:
  `nickname + classNameEn/Pt + iconFile + nameColor`. O matching en+pt da 01-spec vira não-problema: a Edition
  do perfil é a chave do registry, seja ela displayName.pt ou name en (ambas as formas de registro caem no mesmo
  dicionário).
- **Client:** cache estático `ClassIdentities` (lazy, molde do `SkillMultipliers.EnsureLoaded`) com o mapa
  `nickname → identidade`. O patch existente do 055 (`ClassDetailLoadingPatch`, Postfix em `AddPlayer`) deixa de
  gatear por `nickname == local` e passa a: resolver a identidade de **cada** linha → tingir o nome da linha
  (cor da classe + brasão) → anexar o hover com o popover parametrizado pela classe **daquele** player.
- **View:** `PerksPanelView.Refresh` e `PerksCatalog.LocalGroups` são parametrizados por classe (hoje leem
  `SkillMultipliers.*` = sempre o local). O estado de idempotência `_lastPanelClass` (estático) vira **per-panel**
  — no loading agora coexistem N painéis (um por linha, lazy).

Alternativas descartadas: (a) GameVersion no client — perfis remotos indisponíveis no loading (acima);
(b) painel único compartilhado entre linhas — economiza build, mas complica lifecycle (dono do Destroy) e o
per-row lazy já é o padrão validado do 055; (c) rota dinâmica por nickname — N roundtrips, sem ganho (o payload
completo é pequeno: dezenas de perfis num server privado).

## 2. Pontos de patch

| Alvo | Tipo | Motivo |
|---|---|---|
| `LoadingScreenUI.AddPlayer(int, string)` — [fika-plugin LoadingScreenUI.cs:97](../../../../references/fika-plugin/Fika.Core/UI/Custom/LoadingScreenUI.cs#L97) | Postfix (**já existe** — `ClassDetailLoadingPatch`) | Única mudança de alvo: nenhum. O Postfix atual é generalizado para todas as linhas. Alvo resolvido por `AccessTools.TypeByName("LoadingScreenUI")` (nenhum tipo FIKA no IL — padrão do 055, [ClassDetailLoadingPatch.cs:24](../../modded/Client/Patches/ClassDetailLoadingPatch.cs#L24)). |

Não há ponto de patch novo. Referências de leitura (não-patch):

- `LoadingScreenUI._loadingPlayers` (dict netId→row) — [LoadingScreenUI.cs:14](../../../../references/fika-plugin/Fika.Core/UI/Custom/LoadingScreenUI.cs#L14); já lido via reflection no 055.
- `LoadingScreenPlayer.Nickname` (`TMP_Text`, campo público) — [LoadingScreenPlayer.cs:7](../../../../references/fika-plugin/Fika.Core/UI/Custom/LoadingScreenPlayer.cs#L7); tingir via reflection (`AccessTools.Field(row.GetType(), "Nickname")`) para manter zero tipos FIKA no IL.
- `LoadingScreenUI.ReInitAfterTransit` → chama `AddPlayer` de novo — [LoadingScreenUI.cs:21](../../../../references/fika-plugin/Fika.Core/UI/Custom/LoadingScreenUI.cs#L21); trânsito re-passa pelo mesmo Postfix (corner da 01-spec coberto por construção).

## 3. Novas propriedades F12 (BepInEx)

> Nenhum `ConfigEntry` novo. O gate existente `Perks — UI / Class Detail on Loading Screen`
> (`PerksConfig.ClassDetailOnLoading`, [PerksConfig.cs](../../modded/Client/PerksConfig.cs)) passa a gatear a
> feature inteira (identidade + popover, local e remoto) — semântica documentada no tooltip atual permanece válida.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Server/ClassIdentitiesRouter.cs` | CRIAR | Rota estática `/customclasses/class-identities`: enumera perfis, filtra os com Edition no `ClassVisualRegistry`, responde lista `{nickname, classNameEn, classNamePt, iconFile, nameColor}`. |
| `modded/Server/ClassIdentitiesResponse.cs` | CRIAR | DTOs (`record` + `[JsonPropertyName]`), molde da `SkillMultipliersResponse`. |
| `modded/Client/ClassIdentities.cs` | CRIAR | Cache estático client: fetch lazy 1× da rota, mapa `nickname → Identity` (Ordinal, primeira ocorrência vence), `TryResolve(nickname)`, `WarnedOnce` p/ degradação. |
| `modded/Client/PerksCatalog.cs` | MODIFICAR | Extrair `GroupsFor(string nameEn)` de `LocalGroups()` ([PerksCatalog.cs:177](../../modded/Client/PerksCatalog.cs#L177)); `LocalGroups` delega. |
| `modded/Client/PerksPanelView.cs` | MODIFICAR | `Refresh(panel)` ganha overload `Refresh(panel, ClassIdentities.Identity)`; `_lastPanelClass` estático ([PerksPanelView.cs:22](../../modded/Client/PerksPanelView.cs#L22)) vira estado per-panel (componente `PanelState`). |
| `modded/Client/Patches/ClassDetailLoadingPatch.cs` | MODIFICAR | Postfix: remove gate `nickname == local` ([ClassDetailLoadingPatch.cs:50](../../modded/Client/Patches/ClassDetailLoadingPatch.cs#L50)); resolve identidade por linha; tinge `Nickname` TMP + brasão; `LoadingClassHover` parametrizado pela identidade. |

## 5. Stubs de código

### 5.1 Server — rota nova

```csharp
// modded/Server/ClassIdentitiesResponse.cs
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CustomClasses.Server;

/// <summary>057 — payload da rota /customclasses/class-identities (molde: SkillMultipliersResponse.cs:9).</summary>
public sealed record ClassIdentitiesResponse
{
    [JsonPropertyName("players")]
    public List<PlayerClassIdentity> Players { get; init; } = new();
}

public sealed record PlayerClassIdentity
{
    [JsonPropertyName("nickname")]    public string? Nickname { get; init; }
    [JsonPropertyName("classNameEn")] public string? ClassNameEn { get; init; }
    [JsonPropertyName("classNamePt")] public string? ClassNamePt { get; init; }
    [JsonPropertyName("iconFile")]    public string? IconFile { get; init; }
    [JsonPropertyName("nameColor")]   public string? NameColor { get; init; }
}
```

```csharp
// modded/Server/ClassIdentitiesRouter.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Utils;   // usings reais espelhados da SkillMultipliersRouter.cs:1-14
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace CustomClasses.Server;

/// <summary>
///     057 — rota estática com o mapa nickname → identidade de classe de TODOS os perfis do server.
///     Molde: SkillMultipliersRouter.cs:16-54. Read-only; computada por request (perfis já estão em memória —
///     ref: spt-source SaveServer.cs:147 GetProfiles() devolve o dicionário vivo/cópia).
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
                    foreach (var profile in saveServer.GetProfiles().Values)   // ref: spt-source SaveServer.cs:147
                    {
                        var edition = profile?.ProfileInfo?.Edition;           // ref: spt-source SptProfile.cs:99
                        var nickname = profile?.CharacterData?.PmcData?.Info?.Nickname;
                        if (string.IsNullOrEmpty(edition) || string.IsNullOrEmpty(nickname))
                        {
                            continue;
                        }

                        var visual = visualRegistry.Get(edition!);             // ref: ClassVisualRegistry.cs:29
                        if (visual == null || !seen.Add(nickname!))
                        {
                            continue;   // vanilla OU nickname duplicado (primeira ocorrência vence — corner da 01-spec)
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

                    return new ValueTask<string>(jsonUtil.Serialize(response) ?? "{}");   // ref: SkillMultipliersRouter.cs:50
                })
        ];
    }
}
```

> **TODO confirmar no code-mod:** usings/assinatura exata do `RouteAction<EmptyRequestData>` — copiar 1:1 da
> `SkillMultipliersRouter.cs` (mesmo arquivo-fonte de verdade; o stub acima espelha a estrutura reportada no recon).

### 5.2 Client — cache de identidades

```csharp
// modded/Client/ClassIdentities.cs
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SPT.Common.Http;   // RequestHandler — molde: SkillMultipliers.cs:5

namespace CustomClasses.Client;

/// <summary>
///     057 — mapa nickname → identidade de classe de TODOS os players do server (rota
///     /customclasses/class-identities). Fetch LAZY 1× (molde: SkillMultipliers.EnsureLoaded,
///     SkillMultipliers.cs:63). Rota ausente/erro → mapa vazio + 1 aviso (degrada p/ comportamento 055).
/// </summary>
internal static class ClassIdentities
{
    internal sealed class Identity
    {
        public string? NameEn, NamePt, IconFile, NameColor;
        public string? DisplayName => GameLocale.IsPortuguese ? (NamePt ?? NameEn) : (NameEn ?? NamePt);
    }

    private static readonly Dictionary<string, Identity> ByNickname = new(StringComparer.Ordinal);
    private static bool _loaded;

    public static bool TryResolve(string? nickname, out Identity identity)
    {
        EnsureLoaded();
        identity = null!;
        return nickname != null && ByNickname.TryGetValue(nickname, out identity!);
    }

    public static void Reset() { ByNickname.Clear(); _loaded = false; }

    private static void EnsureLoaded()
    {
        if (_loaded) { return; }
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
                    { NameEn = p.ClassNameEn, NamePt = p.ClassNamePt, IconFile = p.IconFile, NameColor = p.NameColor };
                }
            }
        }
        catch (Exception ex)
        {
            // rota ausente (mod server antigo) ou erro de rede → degrada; 1 aviso só (critério da 01-spec).
            Plugin.Log?.LogWarning($"[CustomClasses] (057) class-identities indisponível — identidade só local: {ex.Message}");
        }
    }

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
```

### 5.3 Client — catálogo/painel parametrizados

```csharp
// modded/Client/PerksCatalog.cs — extração (LocalGroups delega)
/// <summary>057 — grupos de QUALQUER classe pela chave EN estável (ByClass). Null se desconhecida.</summary>
internal static PerkGroup[]? GroupsFor(string? classNameEn)
{
    ValidateOnce();
    if (classNameEn == null || !ByClass.TryGetValue(classNameEn, out var keys)) { return null; }
    return keys.Select(k => Library.TryGetValue(k, out var g) ? g : null).Where(g => g != null).ToArray()!;
}

internal static PerkGroup[]? LocalGroups() => GroupsFor(SkillMultipliers.ClassNameEn);   // ref: PerksCatalog.cs:177
```

```csharp
// modded/Client/PerksPanelView.cs — idempotência per-panel + Refresh parametrizado
internal sealed class PanelState : MonoBehaviour { public string? LastClass; }   // substitui _lastPanelClass estático (PerksPanelView.cs:22)

// Refresh(panel) atual → vira wrapper do local:
internal static void Refresh(GameObject panel) => Refresh(panel, LocalIdentity());
// Nova assinatura: Refresh(GameObject panel, string? nameEn, string? displayName, string? iconFile, string? nameColor)
// (corpo atual, trocando SkillMultipliers.ClassNameEn/ClassName/IconFile/NameColor pelos parâmetros e o
//  guard `_lastPanelClass == cls` por `panel.GetComponent<PanelState>().LastClass == nameEn`.)
```

### 5.4 Client — patch do loading generalizado

```csharp
// modded/Client/Patches/ClassDetailLoadingPatch.cs — Postfix generalizado (esqueleto do diff)
[PatchPostfix]
private static void Postfix(object __instance, int netId, string nickname)
{
    if (PerksConfig.ClassDetailOnLoading?.Value != true) { return; }

    // 057: resolve a identidade DESTE nickname (local incluso — o mapa cobre todos os perfis do server).
    // Fallback: mapa vazio (rota ausente) + nickname local → identidade local via SkillMultipliers (055).
    SkillMultipliers.EnsureLoaded();
    ClassIdentities.Identity? id = null;
    if (ClassIdentities.TryResolve(nickname, out var resolved)) { id = resolved; }
    else if (string.Equals(nickname, SkillMultipliers.Nickname, StringComparison.Ordinal)
             && SkillMultipliers.ClassNameEn != null)
    {
        id = new ClassIdentities.Identity { NameEn = SkillMultipliers.ClassNameEn, /* … demais campos */ };
    }
    if (id == null) { return; }   // vanilla/desconhecido → linha intocada (critério da 01-spec)

    if (PlayersField?.GetValue(__instance) is not IDictionary dict || dict[netId] is not Component row) { return; }

    // (a) identidade na linha: tinge o TMP `Nickname` (campo público do LoadingScreenPlayer —
    //     ref: fika-plugin/LoadingScreenPlayer.cs:7) via reflection (zero tipos FIKA no IL).
    if (AccessTools.Field(row.GetType(), "Nickname")?.GetValue(row) is TMP_Text nickTmp)
    {
        ClassIdentityView.ApplyGradient(nickTmp as TextMeshProUGUI, id.NameColor, Color.white);   // ref: ClassIdentityView.cs:39
    }

    // (b) popover: hover parametrizado pela identidade DESTE player.
    var hover = row.GetComponent<LoadingClassHover>() ?? row.gameObject.AddComponent<LoadingClassHover>();
    hover.Identity = id;   // campo novo; Show() → PerksPanelView.Refresh(_panel, Identity)
}
```

## 6. Fluxo de dados

```
[server boot/editor] ClassRegistrar.Commit registra edition → ClassVisualRegistry._byEdition
        (ref: ClassRegistrar.cs:277 · ClassVisualRegistry.cs:11-36)
                │
[client, 1º hover/AddPlayer] ClassIdentities.EnsureLoaded()
        → GET /customclasses/class-identities
        → server: saveServer.GetProfiles() → por perfil: ProfileInfo.Edition → visualRegistry.Get(edition)
          (refs: spt-source SaveServer.cs:147 · SptProfile.cs:99 · ClassVisualRegistry.cs:29)
        → payload [{nickname, classNameEn/Pt, iconFile, nameColor}] → mapa ByNickname (client)
                │
[FIKA loading] LoadingScreenUI.AddPlayer(netId, nickname)  (ref: LoadingScreenUI.cs:97; trânsito re-entra por :21)
        → Postfix ClassDetailLoadingPatch: TryResolve(nickname)
        → linha: tinge Nickname TMP (LoadingScreenPlayer.cs:7) na cor da classe
        → hover: LoadingClassHover.Identity = id
                │
[hover] Show() → PerksPanelView.Refresh(panel, id)
        → header/brasão/cor de id · cards de PerksCatalog.GroupsFor(id.NameEn)
        → ícone local: ClassIconCache.Get(id.IconFile)  (ref: ClassIconCache.cs:25 — icons/ é deployado inteiro)
```

## 7. Riscos e dependências

- **`modded/Server` compartilhado com a sessão paralela do editor web** — usuário liberou em 2026-07-03; ainda
  assim, tocar só arquivos NOVOS (`ClassIdentitiesRouter/Response`) minimiza chance de conflito de merge.
- **Staleness do mapa client (AP-08):** o fetch é 1× por sessão de jogo; classe editada/deletada no editor web
  com o jogo aberto → identidade desatualizada até reiniciar o client. Aceito (dados de exibição; mesmo trade-off
  do `SkillMultipliers`). `Reset()` existe para futuro hook de troca de perfil.
- **Perfis sem PMC** (recém-criados/corrompidos): `CharacterData?.PmcData?.Info?.Nickname` null-safe → pulados.
- **Privacidade:** a rota expõe nickname+classe de todos os perfis a qualquer client do server — servidor privado
  de coop (aceito; sem dados sensíveis).
- **Patches existentes que tocam o mesmo alvo:** só o próprio `ClassDetailLoadingPatch` (055) — é modificado, não
  duplicado. `PlayerNamePanelPatch`/`RaidReadyPlayerPanelPatch`/`ChatSpecialIconPatch` continuam local-only
  (fora de escopo por decisão do usuário).
- **Ordem de init:** rota client chamada lazy no primeiro `AddPlayer` — sessão já existe (mesma garantia do
  `SkillMultipliers`, PA-01-04). `ClassVisualRegistry` é Singleton populado no boot (CustomClassesMod.OnLoad:84).
- **Headless/dedicated FIKA:** `AddPlayer` roda com nickname "Headless" (LoadingScreenUI.cs:24) → sem match no
  mapa → no-op limpo.

## 8. Checklist de implementação

- [ ] Server: `ClassIdentitiesResponse.cs` (DTOs) — molde `SkillMultipliersResponse.cs`.
- [ ] Server: `ClassIdentitiesRouter.cs` — usings/`RouteAction` copiados 1:1 da `SkillMultipliersRouter.cs`; dedup de nickname; null-safety de perfil.
- [ ] Client: `ClassIdentities.cs` (cache lazy + `TryResolve` + aviso 1×).
- [ ] Client: `PerksCatalog.GroupsFor(nameEn)` + `LocalGroups` delegando.
- [ ] Client: `PerksPanelView` — `PanelState` per-panel substitui `_lastPanelClass`; `Refresh` parametrizado (wrapper local mantém call-sites do 053/059 intactos).
- [ ] Client: `ClassDetailLoadingPatch` — remover gate local; resolver por nickname c/ fallback local; tinge `Nickname` TMP; `LoadingClassHover.Identity`.
- [ ] Client: `LoadingClassHover.Show()` usa `Refresh(_panel, Identity)`; `Ensure()` inalterado (zoom 0.75 do 055 preservado).
- [ ] `/compile-mod CustomClasses` (client + server buildam e instalam).
- [ ] Atualizar `HANDOFF.md` (pendência #6) + backlog status 057 → 🟡 (aguardando gate in-game).

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | ✅ | Sem estado de raid: painéis são filhos do canvas da tela de loading (destruídos com ela — `LoadingClassHover.OnDestroy`, ClassDetailLoadingPatch.cs:174); caches estáticos (`ClassIdentities`) são dados de exibição imutáveis por sessão (§7 staleness). |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | Inverso proposital: a feature É per-player (todas as linhas). Nenhum efeito de gameplay — só UI. Gate único `ClassDetailOnLoading` (§3). |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | ✅ | `LoadingScreenUI.AddPlayer` é público, não-virtual, classe MonoBehaviour concreta sem herdeiros (fonte FIKA disponível — LoadingScreenUI.cs:97); resolvido por `TypeByName` + parâmetros explícitos (padrão 055 já validado in-game). |
| 4 | Mudança de estado via API canônica; side-effects mapeados — AP-04 | ✅ | Nenhum estado do EFT é alterado — só TMP color/UI própria. Server: rota read-only (`GetProfiles` sem mutação — SaveServer.cs:147). |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | Painéis morrem com a tela (item 1); mapa estático sobrevive e é reutilizado (dados imutáveis); trânsito re-entra pelo mesmo Postfix (`ReInitAfterTransit` → `AddPlayer`, LoadingScreenUI.cs:21). |
| 6 | Semântica/defaults/faixas de ConfigEntry sem ambiguidade — AP-05 | N/A | Nenhum ConfigEntry novo (§3 — reusa gate existente com semântica inalterada). |
| 7 | Re-invocação de método patcheado tem reentry-guard — AP-07 | ✅ | Postfix não re-invoca `AddPlayer` nem métodos patcheados; idempotência por `GetComponent<LoadingClassHover>()` (reuso, não duplicação). |
| 8 | Flags/caches validados contra o contexto atual após troca — AP-08 | ✅ | Mapa por nickname é imutável por sessão (§7 — staleness aceito e documentado); `PanelState.LastClass` compara contra a identidade da linha ATUAL a cada `Show` (multi-painel, §5.3). |

## Histórico

| Data | Evento |
|---|---|
| 2026-07-03 | Spec técnica criada via `/create-technical-spec` — mecanismo resolvido: rota server por nickname (hipótese GameVersion-no-client descartada com evidência FIKA) |
