# 001 — Auto-desdobrar fone só no próprio jogador · Spec Técnica

**Mod:** SPT-Foldables
**Spec funcional:** [001-desdobrar-fone-proprio-jogador-01-spec.md](001-desdobrar-fone-proprio-jogador-01-spec.md)
**Criado:** 2026-09-24

> Fonte primária de verdade para qualquer assinatura citada do EFT: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). O alvo do patch em si é código do próprio mod Foldables (`mods/SPT-Foldables/original/` → `modded/`), não o Assembly do EFT — não há patch Harmony novo sobre o EFT aqui, é edição direta do source forkado.

## 1. Estratégia

**Revisão de abordagem (review 01, PA-01-01):** a primeira versão desta spec técnica propunha checar se `inventoryController` pertencia ao próprio jogador local (`Player.PlayerInventoryController`/`IsYourPlayer`). A review encontrou que essa premissa não se sustenta: lendo o corpo real de `EFT.UI.ItemsPanel.Show` ([`ItemsPanel.cs:219-292`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.UI/ItemsPanel.cs#L219-L292)), `inventoryController` é **sempre** o controller do jogador local — usado só pra processar as operações de move/drag (`StopProcesses()`, repassado como o controller que executa as ações). Quem representa a entidade sendo lootada (corpo, bot, outro jogador, baú) é um parâmetro **diferente**, `lootItem` (um `CompoundItem`), que a versão anterior desta spec nem capturava. Checar o dono de `inventoryController` nunca distinguiria "olhando só meu inventário" de "olhando meu inventário + o de um corpo" — o guard proposto originalmente sempre passaria.

**Abordagem revisada:** o `Postfix` já existente (Harmony postfix em `ItemsPanel.Show`, `async void`) ganha um guard de early-return logo após o guard atual (`!inRaid || currentTab != Gear`): só prossegue se **nenhum container externo** estiver sendo mostrado junto — ou seja, se `lootItem == null`.

Verificado no Assembly (fonte primária, `EFT.UI/ItemsPanel.cs`):
- `EFT.UI.ItemsPanel.Show(ItemContextAbstractClass sourceContext, CompoundItem lootItem, ISession session, InventoryController inventoryController, ..., EItemsTab currentTab, bool inRaid, ...)` — assinatura completa em [`ItemsPanel.cs:219`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.UI/ItemsPanel.cs#L219). `lootItem` é `null` quando a tela mostra só o próprio inventário do jogador (nenhum container externo aberto).
- [`ItemsPanel.cs:273`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.UI/ItemsPanel.cs#L273): `await method_0(equipment ?? inventory_0.Equipment, sourceContext, inRaid, SplitInFrames);` — a aba de equipamento do **próprio jogador** é sempre processada aqui, independente de `lootItem`.
- [`ItemsPanel.cs:282-287`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.UI/ItemsPanel.cs#L282-L287): `if (lootItem is InventoryEquipment inventoryEquipment) { _complexStashPanel.Show(inventoryController_0, ..., inventoryEquipment, ...); ... }` — é **este** caminho que processa o corpo/bot/outro jogador sendo lootado (quando o item lootado é, ele mesmo, um conjunto de equipamento — o caso de looter uma entidade viva/morta, não um baú simples). A pilha de exceção real reportada pelo usuário (`ArgumentException: TacticalVest`) cita exatamente `ComplexStashPanel.Show` → `ContainersPanel.Show`, confirmando que o crash acontece processando o **corpo** via este caminho, não o jogador.
- [`ItemsPanel.cs:288-292`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.UI/ItemsPanel.cs#L288-L292): `else if (lootItem != null) { _simpleStashPanel.Show(lootItem, inventoryController_0, ...); ... }` — caminho pra container simples (baú, mochila no chão) que não é um conjunto de equipamento completo; `lootItem` também não é nulo aqui.

Ou seja: `lootItem != null` cobre **todo** caso de "há algo externo sendo lootado junto" (corpo, bot, outro jogador vivo, baú, mochila no chão) — exatamente o critério que a spec funcional revisada (`01-spec.md`) descreve.

**Alternativas descartadas:**
- Checar o dono de `inventoryController` (`Player.PlayerInventoryController`/`IsYourPlayer`) — descartada nesta revisão (PA-01-01): não distingue os dois cenários, já que `inventoryController` é sempre do jogador local.
- Reescrever o patch como um Prefix (bloquear o `Show()` original) — descartado porque o problema não é o `Show()` em si, é a lógica de auto-desdobrar que o Foldables anexa depois; um Prefix mudaria o comportamento do inventário em si (fora do escopo deste item).

## 2. Pontos de patch

| Alvo | Tipo | Motivo |
|---|---|---|
| [`InventoryScreenShowPatch.cs`](../../../SPT-Foldables/modded/Foldables/Patches/Operations/InRaid/InventoryScreenShowPatch.cs) (mod Foldables, `modded/`) | Edição direta | Adiciona early-return quando há um container externo (`lootItem`) sendo mostrado junto. |

Referências ao Assembly do EFT usadas pra validar a checagem (não são pontos de patch, são evidência): `ItemsPanel.cs:219, 273, 282-287, 288-292` (ver §1).

## 3. Novas propriedades F12 (BepInEx)

N/A — não introduz `ConfigEntry` nova. O comportamento corrigido é estritamente um bug fix (a intenção original — desdobrar o fone do próprio jogador — continua idêntica); não há trade-off de UX que justifique um toggle novo.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `Foldables/Patches/Operations/InRaid/InventoryScreenShowPatch.cs` | MODIFICAR | Adiciona guard `lootItem != null` (captura o parâmetro `CompoundItem lootItem` de `ItemsPanel.Show`) logo após o guard existente de `inRaid`/`currentTab`. |
| `Foldables.csproj` (ou equivalente) | MODIFICAR | Bump de versão (patch — fix de bug), se o build system do Foldables usar versionamento por `<Version>`/`AssemblyInfo`. |

## 5. Stubs de código

```csharp
// Foldables/Patches/Operations/InRaid/InventoryScreenShowPatch.cs
using System.Reflection;
using System.Threading.Tasks;
using EFT;
using EFT.InventoryLogic;
using Foldables.Utils;
using SPT.Reflection.Patching;

#pragma warning disable VSTHRD003
#pragma warning disable VSTHRD100
// ReSharper disable AsyncVoidMethod

namespace Foldables.Patches.Operations.InRaid;

/// <summary>
/// Force unfold headphones if folded
/// </summary>
public class InventoryScreenShowPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ItemsPanel).GetMethod(nameof(ItemsPanel.Show));
    }

    [PatchPostfix]
    protected static async void Postfix(InventoryController inventoryController, ItemsPanel.EItemsTab currentTab, bool inRaid, Task __result, CompoundItem lootItem)
    {
        if (!inRaid || currentTab != ItemsPanel.EItemsTab.Gear) return;

        // ref: Assembly-CSharp/EFT.UI/ItemsPanel.cs:219 (assinatura de Show, parâmetro lootItem),
        // :282-287 (lootItem is InventoryEquipment -> ComplexStashPanel.Show, caminho que processa
        // corpo/bot/outro jogador -- é exatamente aqui que o ArgumentException real acontece),
        // :288-292 (lootItem != null, caminho de container simples -- baú/mochila no chão).
        // lootItem só é null quando a tela mostra exclusivamente o próprio inventário do jogador
        // (inventoryController, usado abaixo, é sempre o do jogador local -- nunca muda; backlog
        // 001, PA-01-01). Sem esta checagem, abrir QUALQUER container externo dispara a espera de
        // dobra abaixo, que pode reentrar em ItemsPanel.Show() depois que o jogador já fechou
        // aquele painel -- causa raiz do bug de painel duplicado/corrompido em coop.
        if (lootItem != null) return;

        await __result;

        var headphoneSlot = inventoryController.Inventory.Equipment.GetSlot(EquipmentSlot.Earpiece);
        if (headphoneSlot.ContainedItem.IsFoldableFolded())
        {
            headphoneSlot.ContainedItem.FoldItemWithDelay(force: true);
        }
    }
}
```

## 6. Fluxo de dados

```
Cenário A (jogador abre só o próprio inventário, TAB, com fone dobrado — deve continuar desdobrando):
[Jogador] pressiona TAB, sem nenhum container externo aberto → ItemsPanel.Show(lootItem = null) → Postfix roda
  → lootItem == null (ItemsPanel.cs:219) → guard passa → await __result → fone dobrado → FoldItemWithDelay como hoje.
  Sem mudança de comportamento.

Cenário B (jogador loota corpo/bot com o PRÓPRIO fone dobrado — bug relatado, corrigido):
[Jogador] abre painel de loot do corpo → ItemsPanel.Show(inventoryController = do jogador, lootItem = equipamento do corpo) → Postfix roda
  → lootItem != null (é o corpo, via ItemsPanel.cs:282-287 ComplexStashPanel/ContainersPanel — caminho onde o
    ArgumentException real acontece) → guard falha → return imediato, sem await, sem FoldItemWithDelay,
    sem reentrância futura em ItemsPanel.Show()/ContainersPanel.Show() sobre o painel do corpo.
```

## 7. Riscos e dependências

- **Nenhum patch de `modded/` próprio deste repo é afetado** — a mudança é isolada dentro do fork do Foldables; não há interação direta com FIKA/VisceralCombat/etc. no código em si (a interação observada foi só a EXPOSIÇÃO do bug pela latência de rede do FIKA, não uma dependência de código).
- **Atualização futura do Foldables (upstream):** se uma nova versão do Foldables reescrever `InventoryScreenShowPatch.cs`, o guard precisa ser reaplicado manualmente ao atualizar o fork (`mods/SPT-Foldables/original/` vs `modded/` — `diff -r` pra localizar). Registrar isso no README do mod.
- **Fallback seguro (corner case da spec funcional):** `lootItem` é o próprio parâmetro que `ItemsPanel.Show` já recebe — não há caminho conhecido em que ele exista mas não possa ser lido pelo `Postfix` (é injetado pelo Harmony por nome, igual aos demais parâmetros já capturados). Se, por algum motivo, o parâmetro não puder ser resolvido, o comportamento padrão do Harmony é omiti-lo da assinatura do Postfix — não há um estado "indeterminado" a tratar em runtime além disso.

## 8. Checklist de implementação

- [x] Adicionar o guard em `InventoryScreenShowPatch.cs` (`modded/`) logo após o guard existente de `inRaid`/`currentTab`, conforme stub §5.
- [ ] Confirmar que o guard não quebra o caso do próprio jogador (checar visualmente: abrir o próprio inventário com fone dobrado ainda desdobra).
- [x] Bump de versão do Foldables (patch) — 1.0.3 → 1.0.4 (`Foldables.cs` `BepInPlugin` + `Foldables.csproj` `<Version>`).
- [ ] Build Release do fork `mods/SPT-Foldables/modded/` — pendente `/compile-mod`.
- [ ] Validar em raid real: abrir corpo/bot com fone dobrado várias vezes seguidas não gera `ArgumentException` nem painel duplicado.
- [ ] Validar em raid real: abrir o próprio inventário (TAB) depois de lootear um corpo não mostra painel/rig residual.
- [ ] Validar em raid real (Fika/multiplayer): host e convidado, olhando corpo/bot e um jogador humano vivo (ex.: revive), confirmando que a rotina de auto-desdobrar não dispara pra nenhuma entidade que não seja o próprio jogador de cada cliente.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes (`GameWorld.OnDestroy` + `BaseLocalGame.Stop`) — AP-01 | N/A | Não há hook de raid-start/stop próprio; o guard é avaliado a cada chamada do `Postfix`, sem estado persistido entre raids. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | É o núcleo da correção: `lootItem != null` (§1, §5, `ItemsPanel.cs:219/282-287/288-292`) distingue "só o próprio inventário" de "há um container externo (corpo/bot/outro jogador) aberto junto" — o filtro que faltava e causava a reentrância em coop. Revisado em PA-01-01 (review 01) após a primeira versão (checar dono de `inventoryController`) se mostrar ineficaz, já que esse parâmetro é sempre do jogador local. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | N/A | Não há patch novo sobre método virtual/ofuscado do EFT; o alvo (`ItemsPanel.Show`) já é patcheado pelo Foldables via `GetMethod(nameof(...))` por nome público, sem mudança nesta correção. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | N/A | A correção só adiciona um `return` condicional antes da lógica existente — não muta nenhum estado novo do EFT. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | N/A | `IsYourPlayer` é lido do `Player` corrente a cada chamada, sem cache; nenhum estado é retido entre raids por esta correção. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | N/A | Nenhuma `ConfigEntry` nova (§3). |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` (sem recursão infinita) — AP-07 | ✅ | O guard adicionado é exatamente o que **elimina** a reentrância existente (o `Postfix`, ao rodar pra uma entidade observada, podia acabar re-invocando `ItemsPanel.Show()` de forma atrasada/fora de contexto — ver spec funcional, Visão geral). Após a correção, esse caminho nunca é alcançado pra entidades que não são o próprio jogador. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca (arma/operação/tela) — AP-08 | ✅ com ressalva | O guard em si não tem cache (é reavaliado por chamada), mas o mecanismo de delay/coroutine dentro de `FoldItemWithDelay` (não modificado por este item) continua sem revalidar se o painel ainda é o mesmo quando a espera termina — isso deixa de ser alcançável para entidades observadas (o caso que causava o bug), mas **continua existindo, sem correção, para o próprio jogador** trocando de tela rapidamente com o fone dobrado. Fora do escopo deste item (spec funcional não pede isso); registrar como risco conhecido residual, não bloqueador. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump (não só no recon); "não existe" conferido no `types-index.json`, nunca num grep vazio — AP-09 | ✅ | `ItemsPanel.Show` (assinatura completa e corpo do método) foi lido diretamente em `references/eft-decompiled/Assembly-CSharp/EFT.UI/ItemsPanel.cs` nesta sessão (linhas citadas em §1), inclusive na review 01 que refutou a primeira versão da spec. |
| 10 | Skill EFT usada como lever confirmada não-inerte (`SkillsSettings` ≠ `[]`) — AP-10 | N/A | Não usa skill do EFT como mecanismo. |
| 11 | Pacote FIKA próprio: envelope + `TryGet*` + `Valid` + campos resetados + envio só main thread + registro por instância + zero `UnregisterPacket` + airbag com throttle — AP-11 | N/A | Este mod não declara nenhum pacote de rede próprio; não interage com o transporte de rede do FIKA em nenhum ponto. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-24 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-24 | Revisão `/review-technical-spec` 01 (PA-01-01, 🔴) encontrou erro de lógica: `inventoryController` é sempre o do jogador local em `ItemsPanel.Show` (confirmado em `ItemsPanel.cs:219-292`), então o guard original (`Player.PlayerInventoryController`/`IsYourPlayer`) nunca distinguiria "olhando meu inventário" de "olhando meu inventário + um corpo". Reescrita pra usar o parâmetro `lootItem` (não nulo quando há container externo aberto) — §1, §2, §4, §5, §6, §7, §9 atualizados |
