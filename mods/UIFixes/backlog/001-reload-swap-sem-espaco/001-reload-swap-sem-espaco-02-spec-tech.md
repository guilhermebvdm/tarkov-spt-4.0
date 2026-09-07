# 001 — Swap de magazine no mesmo slot ao recarregar com R sem espaço · Spec Técnica

**Mod:** UIFixes
**Spec funcional:** [001-reload-swap-sem-espaco-01-spec.md](001-reload-swap-sem-espaco-01-spec.md)
**Criado:** 2026-09-06

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

**Memória consultada:** sem memória prévia relevante ao item (memória do UIFixes é narrativa por sessão, sem pendências 🔴 registradas no formato P-N.M).
**Docs técnicos lidos (gatilho disparado):** `spt-antipatterns.md` (sempre) · `spt4-vs-spt41-gclass-deobfuscation.md` (spec cita `GClass3372`, `GClass3393` — aliases abaixo). `fika-packet-desync-prevention-plan.md` não disparado (nenhum `INetSerializable` novo).

## 0. Aliases 4.1 (deofuscação — rótulo, não pinado — AP-09)

| Nome 4.0 | Alias 4.1 (`consolidated-mappings.txt`) |
|---|---|
| `GClass3372` | (sem entrada no mapa — ausência não é prova de que não existe; conceito confirmado por leitura: extension methods de `InventoryEquipment`, ex. `GetPrioritizedGridsForUnloadedObject`) |
| `GClass3393` | (sem entrada no mapa; conceito confirmado por leitura: endereço de item dentro de uma `StashGridClass`, retornado por `FindLocationForItem` — mesmo papel de `StashGridItemAddress` já referenciado em `mods/UIFixes/modded/src/Patches/SwapPatches.cs`) |

## 1. Estratégia

### 1.1 Causa raiz confirmada (evidência lida diretamente nesta sessão)

O patch `SwapIfNoSpacePatch` ([`ReloadInPlacePatches.cs:135-213`](../../modded/src/Patches/ReloadInPlacePatches.cs#L135)) intercepta `ReloadMag` e, quando a arma já tem um carregador (`currentMagazine`), tenta achar onde colocar esse carregador antigo:

1. Remove temporariamente o carregador **novo** (`magazine`) do seu endereço atual (`magAddress = magazine.Parent`, linha 183) via `InteractionsHandlerClass.Remove(magazine, controller, false)` (linha 186).
2. Procura uma vaga para o carregador **antigo** (`currentMagazine`) chamando `controller.Inventory.Equipment.GetPrioritizedGridsForUnloadedObject(false).Select(grid => grid.FindLocationForItem(currentMagazine))...FirstOrDefault()` (linhas 192-197).
3. Desfaz a remoção temporária (`operation.Value.RollBack()`, linha 199).
4. Se achou um candidato, usa esse endereço; senão, deixa `itemAddress` como veio (geralmente `null`), e o `ReloadMag` nativo/Fika cai no comportamento padrão (derruba o carregador antigo no chão quando não há vaga).

**A causa raiz do bug está na função chamada no passo 2.** `GetPrioritizedGridsForUnloadedObject(this InventoryEquipment equipment, bool backpackIncluded = false)` ([`GClass3372.cs:136-152`](../../../../references/eft-decompiled/Assembly-CSharp/GClass3372.cs#L136)):

```csharp
public static IEnumerable<StashGridClass> GetPrioritizedGridsForUnloadedObject(this InventoryEquipment equipment, bool backpackIncluded = false)
{
    Slot slot = equipment.GetSlot(EquipmentSlot.TacticalVest);
    Slot slot2 = equipment.GetSlot(EquipmentSlot.Pockets);
    Slot slot3 = equipment.GetSlot(EquipmentSlot.Backpack);
    ...
    StashGridClass[] first = (vest?.Grids ?? []);
    StashGridClass[] second = (pockets?.Grids ?? []);
    StashGridClass[] second2 = (backpack?.Grids ?? []);
    if (!backpackIncluded)
    {
        return first.Concat(second);           // <- SÓ colete + bolsos
    }
    return first.Concat(second).Concat(second2); // colete + bolsos + mochila
}
```

`SwapIfNoSpacePatch` chama essa função com `backpackIncluded = false` (linha 192: `GetPrioritizedGridsForUnloadedObject(false)`) — ou seja, **a busca por uma vaga para o carregador antigo nunca considera a mochila**. Isso é inofensivo quando o carregador novo (`magazine`) também vem do colete/bolsos e sobra espaço ali. Mas quando:

- o colete e os bolsos estão 100% cheios (o cenário relatado pelo usuário), **e/ou**
- o carregador novo (`magazine`) estava guardado na **mochila** (cenário comum: colete cheio, jogador pega carregador reserva da mochila),

a única vaga que deveria existir — o espaço exato que o carregador novo acabou de deixar livre (`magAddress`, dentro da mochila nesse segundo caso) — **nunca é vista pela busca**, porque `GetPrioritizedGridsForUnloadedObject(false)` já excluiu a mochila da lista de grades candidatas antes mesmo de chegar a `FindLocationForItem`. `candidateAddress` acaba `null`, `itemAddress` não é setado, e o `ReloadMag` nativo cai no comportamento padrão (derruba no chão).

Isso explica exatamente o sintoma relatado: falha específica quando o colete está sem espaço — é justamente quando o jogador mais provavelmente está puxando o carregador de outro lugar (bolsos cheios também, ou mochila), e é justamente aí que a mochila (o lugar mais provável de ter a vaga certa — a que o próprio carregador novo acabou de vagar) é ignorada pela busca.

### 1.2 Por que o caminho do inventário (drag-and-drop) não tem esse problema

O swap por arrastar (`SwapPatches.cs`, corrigido indiretamente pelo item `003-magazine-swap-inplace-fix` do FIKA) não depende de `GetPrioritizedGridsForUnloadedObject` — ele já sabe exatamente o endereço de destino porque o próprio drag-and-drop informa onde o carregador antigo deve ir (o endereço de onde o carregador novo foi arrastado). O caminho do "R" (`SwapIfNoSpacePatch`) é o único que precisa **procurar** uma vaga porque, ao apertar "R", o jogo escolhe automaticamente qual carregador usar — não há um "endereço de origem" explícito fornecido pelo jogador. Essa diferença estrutural é a razão de os dois caminhos terem causas de bug completamente diferentes, mesmo cuidando do mesmo tipo de funcionalidade.

### 1.3 Confirmação da preservação do double-R (`QuickReloadWeapon`)

Investigação já registrada nesta sessão: `Class1730.TranslateCommand` ([`Class1730.cs:430-442`](../../../../references/eft-decompiled/Assembly-CSharp/Class1730.cs#L430)) despacha `ECommand.ReloadWeapon` para `method_13()` (linha 435, que chama `ReloadMag` — o alvo do nosso patch) e `ECommand.QuickReloadWeapon` para `method_6()` (linha 440, que sempre chama `QuickReloadMag`, nunca `ReloadMag`). São dois comandos de input distintos, decididos pelo próprio sistema de input do jogo antes de qualquer código nosso rodar. `SwapIfNoSpacePatch` só intercepta `ReloadMag` — `QuickReloadMag` nunca é tocado, então a recarga rápida vanilla (derruba o carregador de propósito) continua garantidamente intocada por esta correção, sem necessidade de nenhuma lógica de detecção de duplo-toque da nossa parte.

### 1.4 Abordagem escolhida (revisada — PA-01-01)

**Importante, confirmado antes de desenhar a correção:** todo este trecho só executa quando o jogo **já** não encontrou uma vaga normal pro carregador antigo — o guard de entrada do próprio `Prefix` (`ReloadInPlacePatches.cs:155`: `if (!Settings.SwapMags.Value || (itemAddress != null && !Settings.AlwaysSwapMags.Value)) { return true; }`) já garante isso: se `itemAddress` chegou não-nulo (o jogo, em `Class1730.cs:1231-1235`, já achou uma vaga sozinho) e `AlwaysSwapMags` está desligado, o patch nem entra em ação — "R" age vanilla. A correção abaixo só afeta o caminho em que essa checagem inicial **já** determinou que não há vaga.

Adicionar uma checagem pelo endereço exato que o carregador novo está deixando (`magAddress`) como **fallback**, executada **somente se a busca ampla existente não encontrar nada** — nunca antes dela, para não alterar a prioridade que já existe hoje (`OrderByDescending(...Equals(magAddress))` só quando `AlwaysSwapMags` está ligado; sem essa configuração, hoje se prefere a menor vaga livre disponível — comportamento que não pode mudar). Como `magAddress` é o endereço de onde `magazine` acabou de ser removido (linha 186), ele é, por definição, um espaço livre válido para um item do mesmo formato — checar diretamente ali não depende de `GetPrioritizedGridsForUnloadedObject` nem da exclusão de mochila:

```csharp
ItemAddress candidateAddress = controller.Inventory.Equipment.GetPrioritizedGridsForUnloadedObject(false)
    .Select(grid => grid.FindLocationForItem(currentMagazine))
    .Where(address => address != null)
    .OrderByDescending(address => Settings.AlwaysSwapMags.Value && address.Equals(magAddress))
    .ThenBy(address => address.Grid.GridWidth * address.Grid.GridHeight)
    .FirstOrDefault();

// ref: PA-01-01 — fallback só quando a busca ampla acima não acha nada (colete/bolsos sem espaço,
// ou o carregador novo veio da mochila, que GetPrioritizedGridsForUnloadedObject(false) nunca
// considera). Não muda a prioridade da busca ampla — só age quando ela já falharia de qualquer forma.
if (candidateAddress == null && magAddress is GridItemAddress gridMagAddress)
{
    candidateAddress = gridMagAddress.Grid.FindLocationForItem(currentMagazine);
}
```

Isso garante o resultado prometido pela feature ("o carregador antigo ocupa o espaço de onde saiu o novo", quando não há outra vaga) **independente de o carregador novo vir do colete, bolsos ou mochila**, sem alterar em nada o comportamento do caso que já funciona hoje (busca ampla bem-sucedida) — zero mudança de prioridade, só fecha o buraco de quando ela falha.

**Limitação pré-existente, não introduzida por esta correção (PA-01-02):** tanto a busca ampla quanto o fallback dependem de `FindLocationForItem(currentMagazine)` encaixar o carregador antigo no espaço vago — só funciona garantidamente se `currentMagazine` tiver o mesmo tamanho/formato (ou menor) que `magazine`. Um carregador antigo fisicamente maior (ex.: estendido saindo, padrão entrando) pode não caber na vaga do que entrou; nesse caso o comportamento cai de volta ao padrão do jogo (derruba no chão), igual já acontece hoje com a busca ampla existente.

## 2. Pontos de patch

| Alvo (Assembly/mod) | Tipo | Motivo |
|---|---|---|
| [`ReloadInPlacePatches.cs:192-197`](../../modded/src/Patches/ReloadInPlacePatches.cs#L192) — busca de `candidateAddress` em `SwapIfNoSpacePatch.Prefix` | Edição direta (mod source) | Adicionar checagem prioritária pelo endereço exato vacado (`magAddress`) antes da busca ampla que exclui a mochila |
| [`GClass3372.cs:136-152`](../../../../references/eft-decompiled/Assembly-CSharp/GClass3372.cs#L136) — `GetPrioritizedGridsForUnloadedObject` | Leitura/evidência (não é ponto de patch — é o motor do jogo, não editável) | Confirma que `backpackIncluded=false` exclui a mochila da busca ampla — motivo da causa raiz |
| [`StashGridClass.cs:564`](../../../../references/eft-decompiled/Assembly-CSharp/StashGridClass.cs#L564) — `FindLocationForItem` | Leitura/evidência | Confirma que o método já é chamável em um único `StashGridClass` (usado tanto na busca ampla quanto na nova checagem prioritária) |
| [`Class1730.cs:430-442`](../../../../references/eft-decompiled/Assembly-CSharp/Class1730.cs#L430) — `TranslateCommand` | Leitura/evidência | Confirma que `QuickReloadWeapon`/double-R nunca chama `ReloadMag`, então esta correção não pode afetá-lo |

## 3. Novas propriedades F12 (BepInEx)

N/A — a correção usa a mesma `ConfigEntry` já existente (`Settings.SwapMags`/`Settings.AlwaysSwapMags`), sem adicionar nenhuma nova.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `mods/UIFixes/modded/src/Patches/ReloadInPlacePatches.cs` | MODIFICAR | `SwapIfNoSpacePatch.Prefix` ganha a checagem prioritária pelo endereço exato vacado, antes da busca ampla existente |

## 5. Stubs de código

```csharp
// Edição em mods/UIFixes/modded/src/Patches/ReloadInPlacePatches.cs
// dentro de SwapIfNoSpacePatch.Prefix (linhas 183-197 atuais).
// Trecho ANTES:
//
//     ItemAddress magAddress = magazine.Parent;
//
//     // Null address means it couldn't find a spot. Try to remove magazine (temporarily) and find where the old mag fits
//     var operation = InteractionsHandlerClass.Remove(magazine, controller, false);
//     if (operation.Failed)
//     {
//         return true;
//     }
//
//     ItemAddress candidateAddress = controller.Inventory.Equipment.GetPrioritizedGridsForUnloadedObject(false)
//         .Select(grid => grid.FindLocationForItem(currentMagazine))
//         .Where(address => address != null)
//         .OrderByDescending(address => Settings.AlwaysSwapMags.Value && address.Equals(magAddress))
//         .ThenBy(address => address.Grid.GridWidth * address.Grid.GridHeight)
//         .FirstOrDefault();
//
// Trecho DEPOIS:

ItemAddress magAddress = magazine.Parent;

// Null address means it couldn't find a spot. Try to remove magazine (temporarily) and find where the old mag fits
var operation = InteractionsHandlerClass.Remove(magazine, controller, false);
if (operation.Failed)
{
    return true;
}

ItemAddress candidateAddress = controller.Inventory.Equipment.GetPrioritizedGridsForUnloadedObject(false)
    .Select(grid => grid.FindLocationForItem(currentMagazine))
    .Where(address => address != null)
    .OrderByDescending(address => Settings.AlwaysSwapMags.Value && address.Equals(magAddress))
    .ThenBy(address => address.Grid.GridWidth * address.Grid.GridHeight)
    .FirstOrDefault();

// ref: PA-01-01 — GetPrioritizedGridsForUnloadedObject(false) exclui a mochila (GClass3372.cs:136-152:
// só retorna colete+bolsos quando backpackIncluded=false). Se o carregador novo veio da mochila, ou
// se colete/bolsos já estão sem espaço, a vaga que ele acabou de deixar livre nunca é vista pela busca
// acima. Fallback: só entra em ação quando a busca ampla já falhou — não muda a prioridade existente.
if (candidateAddress == null && magAddress is GridItemAddress gridMagAddress)
{
    candidateAddress = gridMagAddress.Grid.FindLocationForItem(currentMagazine);
}
```

## 6. Fluxo de dados

```
[A] Jogador aperta "R" com o colete/bolsos sem espaço livre, carregador novo vindo da mochila
      → Class1730.TranslateCommand(ECommand.ReloadWeapon) → method_13() → ReloadMag(magazine, itemAddress=null, ...)
[B] SwapIfNoSpacePatch.Prefix intercepta ReloadMag (ReloadInPlacePatches.cs:152)
      → magAddress = magazine.Parent (endereço na mochila)
      → Remove(magazine, controller, false) vacata esse endereço temporariamente
      → busca ampla existente (GetPrioritizedGridsForUnloadedObject(false)) roda primeiro, sem mudança
        → retorna null (colete/bolsos sem espaço, mochila fora da busca) — mesmo resultado de hoje
      → NOVO (fallback, só age porque a busca ampla retornou null): magAddress é GridItemAddress →
        gridMagAddress.Grid.FindLocationForItem(currentMagazine) encontra a vaga na própria mochila
        (recém-vacada) → candidateAddress preenchido
      → RollBack() desfaz a remoção temporária
      → itemAddress = candidateAddress
[C] Prefix retorna true → ReloadMag/FikaClientFirearmController.ReloadMag nativo roda com o itemAddress
    correto → animação canônica de recarga, carregador antigo vai para a mochila no lugar exato do novo
[D] Fika: ReloadMagPacket enviado ao Headless/Host com o itemAddress já resolvido — mesmo pipeline de
    rede já usado hoje para o caso "com espaço livre", sem qualquer pacote novo
```

## 7. Riscos e dependências

- **`AlwaysSwapMags` desligado, com espaço livre em outro lugar (caso que já funciona hoje):** resolvido por design (PA-01-01) — a checagem nova em `magAddress` só roda como fallback, depois da busca ampla, e só quando ela retorna `null`. Nenhuma mudança de prioridade nesse cenário.
- **Carregadores de tamanhos físicos diferentes (PA-01-02):** um carregador antigo maior que o novo (ex.: estendido saindo, padrão entrando) pode não caber no espaço vacado. Limitação pré-existente de `FindLocationForItem`, não introduzida por esta correção — nesses casos o comportamento cai de volta ao padrão do jogo (derruba no chão), igual já acontece hoje.
- **Nenhum conflito de patch:** `SwapIfNoSpacePatch` continua sendo o único patch em `ReloadMag`; a mudança é inteiramente interna ao seu `Prefix`.
- **Sem impacto em rede/FIKA novo:** `itemAddress` já era passado adiante para o `ReloadMag` nativo/Fika antes desta correção; só a lógica de **como encontrá-lo** muda. Nenhum pacote novo, nenhuma mudança no pipeline de rede.
- **Interação com o item 003 (FIKA):** nenhuma — os dois itens corrigem bugs em código totalmente diferente (`ObservedInventoryController.CheckItemAction` no FIKA vs. `GetPrioritizedGridsForUnloadedObject`/busca de vaga no UIFixes), sem sobreposição de arquivo ou método.

## 8. Checklist de implementação

- [x] Editar `SwapIfNoSpacePatch.Prefix` em `ReloadInPlacePatches.cs` conforme stub §5 (versão final pós-review: fallback via `magAddress`, não prioridade).
- [ ] Validar manualmente: colete 100% cheio, carregador novo vindo da mochila → troca no mesmo slot da mochila, sem derrubar no chão. **Pendente — in-game.**
- [ ] Validar manualmente: colete 100% cheio, carregador novo vindo do próprio colete (slot que ele mesmo ocupa) → troca no mesmo slot do colete. **Pendente — in-game.**
- [ ] Validar manualmente: colete com espaço livre (caso que já funcionava) → sem regressão. **Pendente — in-game.**
- [ ] Validar manualmente: double-R (`QuickReloadWeapon`) continua derrubando o carregador antigo normalmente, sem qualquer troca — confirma §1.3. **Pendente — in-game.**
- [ ] Validar manualmente em coop (Host e Headless dedicado): mesmo comportamento, sem travar gatilho/mãos. **Pendente — in-game.**
- [x] Bump de versão do UIFixes (`Shared.props`: `5.3.21` → `5.3.22`; `mod.json`: idem).

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | N/A | Sem estado por raid — a correção é uma checagem síncrona dentro de um `Prefix` já existente, sem alocar nada raid-scoped |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | `SwapIfNoSpacePatch.Prefix` já filtra `____player.IsAI` (`ReloadInPlacePatches.cs:160-163`, não alterado por esta correção) — bots continuam com o comportamento padrão |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | N/A | Nenhum novo ponto de patch — a correção edita a lógica interna de um `Prefix` já existente; `ReloadMag` (alvo do patch) já estava resolvido por assinatura antes desta correção, sem mudança |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | A correção só troca **como** `itemAddress` é calculado antes de repassar ao `ReloadMag` nativo/Fika (linha 211, `return true`) — a mutação de estado real continua 100% dentro do pipeline canônico do próprio jogo |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | N/A | Sem estado persistido — cada chamada de `ReloadMag` é independente |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | N/A | Nenhuma `ConfigEntry` nova; reusa `Settings.SwapMags`/`Settings.AlwaysSwapMags` já existentes e documentados em `Settings.cs` |
| 7 | Re-invocação de método patcheado tem reentry-guard — AP-07 | N/A | `Prefix` não re-invoca `ReloadMag` nem a si mesmo |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca — AP-08 | N/A | Sem cache/flag persistido entre chamadas — `magAddress`/`candidateAddress` são variáveis locais recalculadas a cada `Prefix` |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | `GetPrioritizedGridsForUnloadedObject` (`GClass3372.cs:136-152`), `FindLocationForItem` (`StashGridClass.cs:564`) e `TranslateCommand` (`Class1730.cs:430-442`) lidos diretamente nesta sessão |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Não usa skill do EFT como alavanca |
| 11 | Pacote FIKA próprio: envelope/`TryGet*`/`Valid`/main thread/registro por instância/zero `UnregisterPacket` — AP-11 | N/A | Não declara nenhum `INetSerializable` novo; reusa o `ReloadMagPacket` já existente do Fika sem alteração |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-06 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-06 | Revisão `/review-technical-spec` 01 — 1 forte (PA-01-01: ordem invertida, checagem nova vira fallback em vez de prioridade, preservando a busca ampla existente) + 1 menor (PA-01-02: limitação de tamanho de carregador documentada) resolvidos |
