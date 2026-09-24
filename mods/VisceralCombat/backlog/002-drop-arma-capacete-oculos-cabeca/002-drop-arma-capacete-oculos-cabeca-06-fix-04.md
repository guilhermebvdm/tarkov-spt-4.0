# 002 — Fix 04 · Investigação do Ícone 2D Bugado de Capacete (❌ NÃO ERA BUG DO VISCERALCOMBAT — Revertido)

> **Status final:** este "fix" foi **revertido em 2026-09-20**, na mesma sessão. A investigação avançou por 4 rounds até o usuário confirmar, por fora deste mod, que o bug reproduz **comprando um capacete no mercado, fora de raid, sem qualquer capacete ter passado pelo VisceralCombat** — prova definitiva de que a causa era um conflito de OUTRO mod no pacote do usuário. Resolvido pelo usuário revertendo o próprio pacote de mods (mantendo a build atual do VisceralCombat) — confirmado funcionando tanto no menu quanto em raid. O código abaixo documenta a investigação (útil como referência de como NÃO confundir "correlação" com "causa", e como as APIs internas de item/viseira funcionam), mas o contorno aplicado (`RaiseHelmetVisorIfPresent`) foi **removido** do código nos 3 pontos onde tinha sido inserido — build final limpa em `3.11.7`.

**Mod:** VisceralCombat
**Item raiz:** [002-drop-arma-capacete-oculos-cabeca-01-spec.md](002-drop-arma-capacete-oculos-cabeca-01-spec.md)
**Asbuild:** [002-drop-arma-capacete-oculos-cabeca-05-asbuild.md](002-drop-arma-capacete-oculos-cabeca-05-asbuild.md)
**Criado:** 2026-09-20
**Disparado por:** Usuário reportou: capacete com viseira, ao ser lootado (drop na morte, não post-mortem), aparece com ícone 2D mostrando só a viseira no inventário — 3D na cabeça e equipar funcionam normal.

## Contexto

Usuário notou que capacetes com viseira, dropados via `DeathInventoryDropPatch` (caminho "na morte"), ficam com o ícone 2D do inventário mostrando só a viseira em vez do capacete completo. O modelo 3D (na cabeça de quem equipa) e a funcionalidade (proteção, equipar) continuam normais — só o ícone renderiza errado.

## Causa raiz

**Não é um bug introduzido pelo VisceralCombat.** Usuário confirmou experimentalmente: no próprio inventário, ativar "levantar viseira" corrige o ícone; abaixar de novo quebra o ícone de novo. Isso prova que o bug depende só do **estado atual** do componente de viseira (`TogglableComponent.On`, `references/eft-decompiled/Assembly-CSharp/EFT.InventoryLogic/TogglableComponent.cs`) — não de como ou onde o item foi criado. É uma limitação pré-existente do jogo/SPT na renderização de ícone pra viseiras abaixadas, que provavelmente já existiria pra qualquer capacete de viseira abaixada, com ou sem este mod (bots ficam com a viseira abaixada em combate com frequência — por isso passamos a notar isso agora, com muito mais capacetes se tornando lootáveis).

Como a renderização de ícone é responsabilidade do cliente do jogo/SPT (fora do alcance de um mod cliente como este), não há como corrigir a causa raiz. A correção possível é um **contorno**: já que o mod controla o momento exato do drop, é possível forçar a viseira pra posição levantada antes de soltar o capacete, evitando o estado que dispara o bug de ícone.

## Mudanças aplicadas

Novo método público `DeathInventoryDropPatch.RaiseHelmetVisorIfPresent(Item helmet)`, chamado nos dois pontos que derrubam capacete (na morte e post-mortem):

```csharp
public static void RaiseHelmetVisorIfPresent(Item helmet)
{
    if (helmet == null) return;
    foreach (TogglableComponent togglable in helmet.GetItemComponentsInChildren<TogglableComponent>())
    {
        if (!togglable.On) togglable.Set(true, simulate: false, silent: true);
    }
}
```

`GetItemComponentsInChildren<T>()` é o mesmo método de extensão que o próprio jogo usa pra essa exata checagem (`Assembly-CSharp/EFT.InventoryLogic/Item.cs:569`) — não é uma API inventada. Capacetes sem viseira simplesmente não têm nenhum `TogglableComponent` nos filhos, então o `foreach` não encontra nada e a função não faz nada (sem risco pra capacetes comuns).

**Round 2 (mesma sessão) — `silent: true` de propósito:** usuário perguntou se levantar a viseira programaticamente não dispararia a animação de mão que acontece quando um jogador VIVO faz essa ação manualmente. Investigação achou os observadores de `TogglableComponent.OnChanged` mapeados (`Player.cs:27635`, `method_40`) — só mexem em respiração/fala abafada, nada de animação. Mesmo assim, trocado `Set(true)` por `Set(true, simulate: false, silent: true)` (a própria API já expõe esse parâmetro pra exatamente essa finalidade) — elimina `OnChanged.Invoke()` por completo, então nenhum observador (mapeado ou não) roda. Build `3.11.3`.

**Round 3 (mesma sessão) — achado o terceiro caminho de drop (`ShootOffHelmetPatch`):** usuário testou a 3.11.3 e reportou que o bug continuava E o capacete tinha dropado com a viseira **abaixada** (não levantada) — sinal de que nosso `RaiseHelmetVisorIfPresent` não rodou pra aquele item específico. Investigação achou `ShootOffHelmetPatch.cs` (sistema de "chance de arrancar capacete" em bot vivo, tiro não-fatal) — um caminho de drop completamente separado, nunca coberto pelos rounds 1/2. Corrigido chamando o mesmo helper lá também. Varredura confirmou que são só 3 pontos no mod inteiro que leem `EquipmentSlot.Headwear` pra derrubar item, todos cobertos agora. Build `3.11.4`.

**Investigação lateral (não resolvida, fora do alcance do mod):** durante o round 3, usuário forneceu informação nova sobre o SINTOMA em si: o modelo 3D do capacete no chão renderiza em tamanho normal (descartando a hipótese de que o encolhimento do bone da cabeça, usado pro efeito de "cabeça estourada", estivesse afetando o item já dropado — a ordem Prefix-antes-do-encolhimento já garante isso estruturalmente). Mas no inventário, o ícone 2D fica com um círculo de carregamento **travado** — indicando que o ícone desse tipo de item (capacete com peça visível/viseira) é renderizado **dinamicamente** pelo motor do jogo (não é um sprite fixo, ao contrário de capacetes sem viseira), e esse render dinâmico está travando — coincide com os erros de console `SkinnedMeshRenderer: ... Bear_Head_5.../ USEC_head_6...`. Avaliação: isso parece ser uma limitação/bug do **SPT** na renderização dinâmica de ícone pra itens com mods visíveis (fora do alcance de um mod cliente corrigir de verdade) — o contorno da viseira levantada (que descartamos como não-relacionado a esse render, mas relacionado ao ESTADO que dispara ele) continua sendo a mitigação prática disponível. Não foi possível confirmar 100% a causa exata do lado do motor gráfico nesta sessão.

| Arquivo | Mudança |
|---|---|
| `modded/VisceralCombat/VisceralCombat.Combined.Patches/DeathInventoryDropPatch.cs` | Novo método público `RaiseHelmetVisorIfPresent`; chamado em `ResolveAndDropHeadEquipment` antes do `ThrowItem` do capacete. |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs` | `DropCorpseHeadEquipment` chama `DeathInventoryDropPatch.RaiseHelmetVisorIfPresent(helmet)` (reusa o mesmo helper, sem duplicar lógica) antes do `ThrowFromCorpsePosition`. |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/ShootOffHelmetPatch.cs` | **Round 3** — achado um TERCEIRO caminho de drop de capacete que não tinha sido coberto: o sistema de "chance de arrancar capacete" em bot ainda VIVO (tiro não-fatal na cabeça, `ShootHelmetOff`/`HelmetShootOffChance`), completamente separado de `DeathInventoryDropPatch`/`LimbKillPatch`. Achado depois do usuário testar a 3.11.3 e reportar que um capacete ainda caiu com a viseira abaixada. Mesmo helper `DeathInventoryDropPatch.RaiseHelmetVisorIfPresent` chamado antes do `ThrowItem` aqui também. Varredura (`grep "EquipmentSlot.Headwear"` em todo `modded/`) confirmou que agora são só esses 3 pontos, todos cobertos. |
| `modded/VisceralCombat/VisceralCombat.csproj` | Nova referência `<Reference Include="ItemComponent.Types">` (necessária pra `IItemComponent`, interface que `TogglableComponent` implementa — `GetItemComponentsInChildren<T>` exige `where T : class, IItemComponent`). `<Version>` `3.11.1` → `3.11.4`. |
| `modded/VisceralCombat/VisceralCombat/VisceralEntry.cs` | `[BepInPlugin]` versão `3.11.1` → `3.11.4`. |

**Efeito colateral aceito (decidido com o usuário):** o capacete looteado via este mod sempre terá a viseira levantada, independente do estado que o bot/player tinha no momento da morte — troca aceitável pra evitar o ícone quebrado.

## Checklist de validação (obrigatório antes de marcar o fix como entregue)

- [x] Compila via `compile-mod.sh` sem erros em todos os rounds
- [x] ~~In-raid: capacete com viseira dropado (na morte) aparece com ícone correto~~ — N/A, contorno revertido (não era a causa)
- [x] ~~In-raid: capacete SEM viseira continua funcionando normal~~ — N/A, contorno revertido
- [x] **Causa raiz real confirmada e fora do mod:** usuário comprou capacete com viseira no MERCADO (fora de raid, sem nenhum capacete ter passado pelo VisceralCombat) e reproduziu o mesmo bug — prova definitiva. Resolvido revertendo o pacote de mods do usuário (mantendo o VisceralCombat atual). Confirmado funcionando no menu E em raid.
- [x] Memória do mod atualizada (`/update-memory`) com a lição do fix

## Histórico

| Data | Evento |
|---|---|
| 2026-09-20 | Fix criado e aplicado (round 1) — usuário reportou o sintoma junto com logs de erro (SkinnedMeshRenderer). Investigação identificou `FaceShieldComponent`/`TogglableComponent`; usuário confirmou experimentalmente (alternando o toggle manualmente) que o estado da viseira é a causa. Contorno aplicado via `GetItemComponentsInChildren<TogglableComponent>`. Build 3.11.2. |
| 2026-09-20 | Round 2 — `Set(true)` trocado por `Set(true, silent: true)` a pedido do usuário, pra garantir que nenhum efeito colateral (animação/som) dispare numa ação programática num personagem morrendo. Build 3.11.3. |
| 2026-09-20 | Round 3 — usuário testou a 3.11.3 em raid: bug persistiu, capacete dropou com viseira abaixada (sinal de que o fix não rodou). Achado um terceiro caminho de drop (`ShootOffHelmetPatch`, capacete arrancado de bot vivo) nunca coberto. Fix aplicado lá também, varredura confirmou que são só 3 pontos no mod. Build 3.11.4. |
| 2026-09-20 | Round 4 — usuário revelou que capacete SEM viseira também bugava (descartando a teoria da viseira como causa única). Investigação estrutural (`HeadwearItemClass`/`HelmetComponent`) não achou o código exato de renderização de ícone. Usuário propôs instrumentar (logar) o caminho real do drop manual via UI — 2 patches de diagnóstico criados (`HelmetThrowItemSpyPatch`/`HelmetRaiseRefreshEventSpyPatch`). Build 3.11.5-debug com um bug AUTOINFLIGIDO: versão `"3.11.5-debug"` no `[BepInPlugin]` tem sufixo não-numérico, que o BepInEx rejeita silenciosamente — **o mod inteiro não carregou**, invalidando o primeiro teste do usuário (comando "Descartar" sem nenhum log). Corrigido pra `"3.11.5"` puro, build 3.11.5 válida. |
| 2026-09-20 | **Resolução final — não era o VisceralCombat.** Antes de repetir o teste do Discard, usuário comprou um capacete com viseira no MERCADO (fora de raid, nenhuma relação com o mod) e reproduziu o MESMO bug de ícone — prova definitiva de que a causa é outro mod do pacote do usuário. Usuário já tinha removido VisceralCombat e TRL-CoreSight tentando isolar, sem sucesso; resolveu revertendo pro pacote de mods conhecido-bom + build atual do VisceralCombat. Confirmado funcionando no menu E validado em raid. **Ação de limpeza:** removidos os 2 patches de diagnóstico (`HelmetIconSpyPatch.cs` inteiro) e revertido o contorno "forçar viseira levantada" (`RaiseHelmetVisorIfPresent`) dos 3 pontos onde tinha sido aplicado (`DeathInventoryDropPatch`, `LimbKillPatch`, `ShootOffHelmetPatch`) — nenhum dos dois era necessário. Referência `ItemComponent.Types.dll` removida do `.csproj` (não usada mais). Build final limpa: `3.11.7`. |
