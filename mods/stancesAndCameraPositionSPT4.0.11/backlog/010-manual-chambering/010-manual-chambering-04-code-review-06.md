# 010 — Manual Chambering & Mecanim Smooth Reload Sync · Code Review 06

**Mod:** stancesAndCameraPositionSPT4.0.11  
**Spec funcional:** [010-manual-chambering-01-spec.md](010-manual-chambering-01-spec.md)  
**Spec técnica:** [010-manual-chambering-02-spec-tech.md](010-manual-chambering-02-spec-tech.md)  
**Review anterior:** [010-manual-chambering-04-code-review-05.md](010-manual-chambering-04-code-review-05.md)  
**Data:** 2026-09-05  

> Análise crítica do código implementado na transição suave via Mecanim (v2.19.8–v2.19.9) e na sincronização de recarga suave em 3ª pessoa (`ObservedPlayer`) com isolamento inteligente de jogadores humanos vs. bots de IA no FIKA coop (v2.19.15).

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 7 · Total: 7

## Resoluções de Reviews Anteriores

- ✅ **CR-05-04 resolvido fora do fluxo automatizado — fechado:** `InstallMagChamberPatch` teve a guarda legada `IsFikaGuestClient()` removida na v2.19.15, sendo unificada com o helper `IsHumanInventory` para paridade de UX no drag-and-drop de inventário.
- ⏸️ **CR-05-01, CR-05-02, CR-05-03 e CR-05-05:** Mantidos pendentes em `FikaSyncManager.cs` para a rodada de sincronização autoritativa via `/apply-code-review`.

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| **CR-06-01** | B — Bug latente | 🟠 Forte | Fast-path ausente em `IsHumanInventory`: lookup O(1) via `GetAlivePlayerByProfileID` antes do loop O(N) | `[x]` Aceitar sugestão (✅ Aplicado) |
| **CR-06-02** | D — Arquitetura | 🟡 Médio | Ausência de cache estático para o `FieldInfo` de `IsObservedAI` em `IsHumanPlayer` | `[x]` Aceitar sugestão (✅ Aplicado) |
| **CR-06-03** | B — Bug latente | 🟡 Médio | Verificação de consistência entre `FirearmsAnimator` e arma ativa em `IdleStartEventPatch` na troca rápida de mãos | `[x]` Aceitar sugestão (✅ Aplicado) |
| **CR-06-04** | C — Gap vs. spec | 🟡 Médio | Ausência de validação de `isLocalPlayer` ao manipular `ManualChamberingState` em `ReloadResetPatch` | `[x]` Aceitar sugestão (✅ Aplicado) |
| **CR-06-05** | E — Legibilidade | 🟢 Menor | Comentários semânticos de classes ofuscadas sem referência explícita à tabela do SPT 4.1 (AP-09) | `[x]` Aceitar sugestão (✅ Aplicado) |
| **CR-06-06** | F — Melhoria opcional | 🟢 Menor | Unificação dos logs de salvaguarda de inventário (`SwitchToIdlingState` e `method_5`) com rate-limiting | `[x]` Aceitar sugestão (✅ Aplicado) |

---

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### CR-06-01 · Cat B — Bug latente · 🟠 Forte · ✅ Aplicado em 2026-09-05

**Fast-path ausente em `IsHumanInventory`: lookup O(1) via `GetAlivePlayerByProfileID` antes do loop O(N)**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/ManualChamberingPatches.cs:150-173`](../../modded-testchannel/Patches/ManualChamberingPatches.cs#L150-L173)

**Problema:**
O método `IsHumanInventory(TraderControllerClass controller)` realiza a busca do dono do inventário iterando sequencialmente sobre `gw.AllAlivePlayersList`:
```csharp
var players = gw.AllAlivePlayersList;
if (players != null)
{
    for (int i = 0; i < players.Count; i++)
    {
        var p = players[i];
        if (p != null && p.InventoryController == controller)
        {
            return IsHumanPlayer(p);
        }
    }
}
```
No EFT, a classe base `TraderControllerClass` expõe a propriedade pública `public string ID`, que armazena diretamente o `ProfileId` do jogador. Além disso, a classe `EFT.GameWorld` disponibiliza o método nativo público `public Player GetAlivePlayerByProfileID(string profileID)` (`GameWorld.cs:1238`), que realiza uma busca $O(1)$ por chave no dicionário interno `allAlivePlayersByID`.

**Por que importa:**
Em sessões de raid com muitos bots vivos (ex: 25 a 35 entidades entre Scavs, PMCs, Bosses e Rogues), iterar sobre a lista completa a cada cálculo de reload ou drag-and-drop adiciona overhead desnecessário e pode falhar caso um companheiro humano remoto esteja em transição de estado onde o seu `InventoryController` não corresponda exatamente ao ponteiro do avatar (ex: recém-extraído ou em transição de spawn).

**Sugestão:**
Adicionar o lookup direto via Profile ID como primeira opção (fast-path), mantendo a iteração como fallback defensivo:
```csharp
public static bool IsHumanInventory(TraderControllerClass controller)
{
    if (controller == null) return false;
    var gw = Singleton<GameWorld>.Instance;
    if (gw == null) return false;

    if (gw.MainPlayer != null && controller == gw.MainPlayer.InventoryController)
        return true;

    // Fast-path O(1) nativo do EFT via ProfileId
    if (!string.IsNullOrEmpty(controller.ID))
    {
        var player = gw.GetAlivePlayerByProfileID(controller.ID);
        if (player != null)
            return IsHumanPlayer(player);
    }

    // Fallback defensivo O(N)
    var players = gw.AllAlivePlayersList;
    if (players != null)
    {
        for (int i = 0; i < players.Count; i++)
        {
            var p = players[i];
            if (p != null && p.InventoryController == controller)
            {
                return IsHumanPlayer(p);
            }
        }
    }

    return false;
}
```

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.  
**Aplicação:** `ManualChamberingPatches.cs` adicionado lookup nativo O(1) por `controller.ID` via `gw.GetAlivePlayerByProfileID`.

---

### CR-06-02 · Cat D — Arquitetura · 🟡 Médio · ✅ Aplicado em 2026-09-05

**Ausência de cache estático para o `FieldInfo` de `IsObservedAI` em `IsHumanPlayer`**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/ManualChamberingPatches.cs:136-143`](../../modded-testchannel/Patches/ManualChamberingPatches.cs#L136-L143)

**Problema:**
Em `IsHumanPlayer(Player player)`:
```csharp
try
{
    var fikaField = AccessTools.Field(player.GetType(), "IsObservedAI");
    if (fikaField != null && (bool)fikaField.GetValue(player))
        return false;
}
catch { }
```
O método chama `AccessTools.Field(player.GetType(), "IsObservedAI")` a cada invocação para jogadores remotos.

**Por que importa:**
Mesmo com o cache do Harmony, a resolução dinâmica do tipo e a busca de membros por reflexão em caminhos frequentes viola a convenção do projeto (conforme diretriz `P-7.2` e boas práticas de C# para mods de SPT), que preconiza a resolução estática de campos via reflexão uma única vez na inicialização ou em propriedade com backing field estático.

**Sugestão:**
Implementar o cache estático do `FieldInfo` para o campo do FIKA:
```csharp
private static FieldInfo _fikaObservedAiField;
private static bool _fikaFieldResolved = false;

public static bool IsHumanPlayer(Player player)
{
    if (player == null) return false;
    if (player.IsYourPlayer) return true;
    if (player.IsAI) return false;

    // Fika compatibility: no cliente coop, bots replicados possuem IsObservedAI = true
    try
    {
        if (!_fikaFieldResolved)
        {
            _fikaObservedAiField = AccessTools.Field(player.GetType(), "IsObservedAI");
            _fikaFieldResolved = true;
        }

        if (_fikaObservedAiField != null && (bool)_fikaObservedAiField.GetValue(player))
            return false;
    }
    catch { }

    return true;
}
```

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.  
**Aplicação:** `ManualChamberingPatches.cs` adicionados campos estáticos `_fikaObservedAiField` e `_fikaFieldResolved` para resolução única de reflexão.

---

### CR-06-03 · Cat B — Bug latente · 🟡 Médio · ✅ Aplicado em 2026-09-05

**Verificação de consistência entre `FirearmsAnimator` e arma ativa em `IdleStartEventPatch` na troca rápida de mãos**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/ManualChamberingPatches.cs:683-690`](../../modded-testchannel/Patches/ManualChamberingPatches.cs#L683-L690)

**Problema:**
No patch `IdleStartEventPatch`:
```csharp
var weapon = __instance.Weapon_0;
var animator = __instance.FirearmsAnimator_0;
if (weapon != null && animator != null && weapon.ChamberAmmoCount == 0)
{
    animator.SetAmmoInChamber(0f);
    if (!weapon.MustBoltBeOpennedForExternalReload)
    {
        animator.SetBoltCatch(false);
    }
}
```
Se o jogador trocar de arma durante uma transição rápida (ou se a operação de Idle for disparada por um evento residual da arma que acabou de ser guardada no coldre), `__instance.FirearmsAnimator_0` pode aplicar `SetAmmoInChamber(0f)` no animator da arma que está sendo equipada caso ela ainda não tenha finalizado sua própria inicialização.

**Por que importa:**
Poderia sobrescrever inadvertidamente o parâmetro visual de uma arma secundária carregada se ela estiver no meio do seu spawn operation no mesmo instante em que a arma anterior encerra o evento de Idle.

**Sugestão:**
Garantir que a arma e o controller da operação atual sejam os mesmos ativos no `HandsController` do jogador:
```csharp
if (player.HandsController is Player.FirearmController fc && fc.Weapon == weapon && animator != null && weapon.ChamberAmmoCount == 0)
{
    animator.SetAmmoInChamber(0f);
    if (!weapon.MustBoltBeOpennedForExternalReload)
    {
        animator.SetBoltCatch(false);
    }
}
```

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.  
**Aplicação:** `ManualChamberingPatches.cs` em `IdleStartEventPatch.Postfix` adicionada verificação `player.HandsController is Player.FirearmController fc && fc.Weapon == weapon`.

---

### CR-06-04 · Cat C — Gap vs. spec · 🟡 Médio · ✅ Aplicado em 2026-09-05

**Ausência de validação de `isLocalPlayer` ao manipular `ManualChamberingState` em `ReloadResetPatch`**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/ManualChamberingPatches.cs:758-768`](../../modded-testchannel/Patches/ManualChamberingPatches.cs#L758-L768)

**Problema:**
No `ReloadResetPatch`, apenas os parâmetros visuais do animator são redefinidos:
```csharp
if (__instance.FirearmsAnimator_0 != null && __instance.Weapon_0 != null && __instance.Weapon_0.ChamberAmmoCount == 0)
{
    __instance.FirearmsAnimator_0.SetAmmoInChamber(0f);
    __instance.FirearmsAnimator_0.SetBoltCatch(false);
}
```
Contudo, se um reload do jogador local for interrompido pelo sprint no primeiro frame (logo após o `StartReloadResetPatch` ter setado `ManualChamberingState.BlockChambering = true`), o `ReloadResetPatch` não redefine o estado local para `BlockChambering = false` caso a câmara continue vazia.

**Por que importa:**
O estado `BlockChambering = true` pode permanecer travado na memória estática até a próxima recarga completa, fazendo com que inspeções subsequentes ou tentativas de carregar munição pelo inventário acreditem que uma recarga ainda está em curso.

**Sugestão:**
Adicionar o reset defensivo de estado se `player.IsYourPlayer`:
```csharp
if (player.IsYourPlayer)
{
    ManualChamberingState.BlockChambering = false;
}
```

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.  
**Aplicação:** `ManualChamberingPatches.cs` em `ReloadResetPatch.Prefix` adicionado reset defensivo de `ManualChamberingState.BlockChambering = false` para o jogador local.

---

### CR-06-05 · Cat E — Legibilidade · 🟢 Menor · ✅ Aplicado em 2026-09-05

**Comentários semânticos de classes ofuscadas sem referência explícita à tabela do SPT 4.1 (AP-09)**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/ManualChamberingPatches.cs:16-32`](../../modded-testchannel/Patches/ManualChamberingPatches.cs#L16-L32)

**Problema:**
As classes ofuscadas (`GClass2055`, `GClass2016`, `GClass2006`, `GClass2037`, `GClass2005`, `GClass2039`) estão documentadas com seus propósitos no EFT 0.16, mas não possuem nota de referência cruzada para a tabela de mapeamento semântico do SPT 4.1 (`references/eft-decompiled/types-index.json`).

**Por que importa:**
Facilita futuras migrações de versão (conforme regra `AP-09`), permitindo que desenvolvedores identifiquem instantaneamente o FQN desofuscado correspondente na versão 4.1 sem precisar refazer a engenharia reversa.

**Sugestão:**
Adicionar os nomes desofuscados canônicos nos comentários dos `using`:
- `GClass2016` $\rightarrow$ `ReloadExternalMagOperation`
- `GClass2006` $\rightarrow$ `ReloadExternalMagResult`
- `GClass2037` $\rightarrow$ `IdleWeaponOperation`
- `GClass2005` $\rightarrow$ `InstallMagResult`
- `GClass2039` $\rightarrow$ `InstallMagOperation`

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.  
**Aplicação:** `ManualChamberingPatches.cs` atualizados comentários dos `using` aliases com mapeamento semântico canônico do SPT 4.1.

---

### CR-06-06 · Cat F — Melhoria opcional · 🟢 Menor · ✅ Aplicado em 2026-09-05

**Unificação dos logs de salvaguarda de inventário (`SwitchToIdlingState` e `method_5`) com rate-limiting**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/ManualChamberingPatches.cs:733, 964, 1007`](../../modded-testchannel/Patches/ManualChamberingPatches.cs#L733)

**Problema:**
Os métodos de salvaguarda em `ReloadIdleStartEventPatch`, `InstallMagInsertedPatch` e `InstallMagResetPatch` usam `Plugin.Logger.LogWarning(...)` direto.

**Por que importa:**
Em situações anômalas repetitivas de cancelamento de ação rápida, múltiplos logs consecutivos de warning podem poluir o log do BepInEx.

**Sugestão:**
Utilizar o mecanismo existente de `ThrottledLog.Warning(...)` do mod para limitar a frequência de emissão a no máximo 1 log por segundo por contexto.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.  
**Aplicação:** `ThrottledLog.cs` expandido com método `Warning` rate-limited e integrado em `ReloadIdleStartEventPatch`, `InstallMagInsertedPatch` e `InstallMagResetPatch`.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-05 | Code review 06 criada via `/code-review` cobrindo transição suave Mecanim e sync 3ª pessoa FIKA (v2.19.15) |
| 2026-09-05 | Aplicação automática de 6 achados via `/apply-code-review` — IDs aplicados: CR-06-01, CR-06-02, CR-06-03, CR-06-04, CR-06-05, CR-06-06 (v2.19.16) |
