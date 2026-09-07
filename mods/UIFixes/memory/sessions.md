# UIFixes — Memória de Sessões

## Snapshot Delta
- **Versão:** 5.3.23 (SPT 4.0 / EFT 0.16.9)
- **Estado:** Dois fixes independentes nesta sessão: (1) item de backlog `001-reload-swap-sem-espaco` — `SwapIfNoSpacePatch` ganhou fallback pra considerar a mochila (via `magAddress` direto) quando a busca padrão (`GetPrioritizedGridsForUnloadedObject(false)`) não acha vaga pro carregador antigo ao recarregar com "R"; (2) fix pontual (fora do ciclo formal) em `EmptySlotMenuTrigger.cs` — removida uma lógica `enabled = true/false` que causava deadlock e quebrava por completo a busca por slot vazio (`EnableSlotSearch`). A rejeição `GClass1561` no swap por drag-and-drop (sessões anteriores, v5.3.14-5.3.21) foi **efetivamente resolvida no lado do FIKA**, não aqui — ver `mods/FIKA/memory/sessions.md` Sessão 3; o patch `FikaActiveWeaponMagSwapPatch` deste mod está no alvo errado e virou uma limpeza pendente (P-8.1).
- **Pendências:** ver bloco abaixo.

## Pendências

- [P-8.1] (aberta 2026-09-06) **Limpar `FikaActiveWeaponMagSwapPatch`** em `SwapPatches.cs:891-924` — intercepta `TraderControllerClass.CheckItemAction` do lado cliente, mas a rejeição real acontece em `ObservedInventoryController.CheckItemAction` no Headless (override que nunca chama `base`, despacho virtual não alcança este patch). Hoje inofensivo (não conflita com nada) porém inútil pro caso Headless, que já foi resolvido do lado do FIKA (`mods/FIKA/backlog/003-magazine-swap-inplace-fix/`). Cross-ref: `mods/FIKA/memory/sessions.md` P-3.2. 🟢 Ideia / limpeza.
- [P-8.2] (aberta 2026-09-06) **Validar in-game o fix do `EmptySlotMenuTrigger`** — corrigido nesta sessão, ainda não testado pelo usuário em jogo. 🟡 Débito técnico (validação pendente, AP-06).

---

## 2026-09-06 23:34 (GMT-3) — Sessão 8: Fix reload-swap sem espaço (item 001) + fix pontual do EmptySlotMenuTrigger e Release v5.3.23

**Tema central:** Dois problemas distintos resolvidos na mesma sessão: (1) troca de magazine no mesmo slot ao recarregar com "R" não funcionava quando o colete/bolsos estavam sem espaço livre (item de backlog `001-reload-swap-sem-espaco`); (2) busca por slot vazio (`EnableSlotSearch`) parou de responder ao botão direito, bug isolado sem relação com o item 001.

**Decisões-chave:**
- [Causa raiz item 001]: `GetPrioritizedGridsForUnloadedObject(false)` (`GClass3372.cs:136-152`, motor do EFT) **nunca considera a mochila** na busca de vaga pro carregador antigo. Se o carregador novo veio da mochila, ou colete+bolsos já estão sem espaço, a vaga que ele acabou de deixar livre nunca é vista pela busca — `SwapIfNoSpacePatch` (`ReloadInPlacePatches.cs`) cai no comportamento padrão do jogo (derruba no chão).
- [Fix item 001]: adicionado fallback checando `magAddress` (endereço exato de onde o carregador novo saiu) diretamente via `GridItemAddress.Grid.FindLocationForItem`, **só quando a busca ampla já falhou** — não muda a prioridade do caso que já funcionava (`AlwaysSwapMags` desligado + vaga livre em outro lugar). Ref: `ReloadInPlacePatches.cs:192-206`.
- [Confirmado com evidência — double-R preservado]: `Class1730.TranslateCommand` (`Class1730.cs:430-442`) despacha `ECommand.ReloadWeapon` (recarga simples, "R") e `ECommand.QuickReloadWeapon` (recarga rápida, double-R) para dois métodos completamente distintos (`ReloadMag` vs `QuickReloadMag`) — decidido pelo próprio sistema de input do jogo, antes de qualquer patch nosso rodar. `SwapIfNoSpacePatch` só intercepta `ReloadMag`; o double-R (derruba o carregador de propósito, pra reload mais rápido) continua garantidamente intocado.
- [Bug separado — `EmptySlotMenuTrigger`]: encontrado numa sessão anterior sem registro em memória, um código adicionando `enabled = false`/`enabled = true` ao componente (`Awake`/`Init` desligam, só `OnPointerEnter` religa). No Unity, um `MonoBehaviour` desabilitado não recebe `OnPointerEnter`/`OnPointerClick` do EventSystem — o componente nasce desligado e nunca mais liga, porque o único jeito de religar dependia do evento que só chega se já estiver ligado (deadlock circular). Corrigido revertendo pro comportamento do `original/` (sem a lógica de `enabled`).
- [Investigação de mods de terceiros — descartados como causa]: usuário achou que o bug do slot vazio era do mod `LetMeRightClick` (recém-adicionado ao repo). Investigação de código mostrou que **não é** — `LetMeRightClick` só remove a trava "can't open context menu while searching" via transpiler em `ItemUiContext.ShowContextMenu` (conferido IL a IL contra o assembly atual — bate exato, nada desatualizado). `WeaponCamoAndStickers` (também adicionado) só adiciona uma entrada "APPLY PAINT" via Postfix aditivo em `GetItemContextInteractions` — sem conflito com os outros mods que tocam o mesmo método.

**Lições / hipóteses descartadas:**
- Hipótese "checagem prioritária por `magAddress` antes da busca ampla" (1ª versão do fix do item 001) descartada na review técnica (`PA-01-01`) — mudaria a prioridade do caso que já funciona hoje. Corrigido pra fallback (só age quando a busca ampla já falhou).
- Hipótese "conflito entre os 3 mods que patcham `ItemUiContext.GetItemContextInteractions`" (LetMeRightClick, WeaponCamoAndStickers, MaterialEditor) descartada como causa do bug do slot vazio — nenhum deles é responsável; causa real era um bug isolado, não relacionado, dentro do próprio UIFixes modded.
- `graphify` (ferramenta de grafo de código) estava com um install órfão quebrado nesta máquina (executáveis presentes mas não registrados em `uv tool list`) — resolvido com `uv tool install graphifyy --force`. Não é um problema do repo, é ambiente local.

**Atividade cronológica:**
1. Ciclo completo de backlog (spec → spec técnica → review → `/code-mod` → `/code-review` → `/apply-code-review`) pro item `001-reload-swap-sem-espaco`.
2. Investigação e correção direta (fora do ciclo formal) do bug do `EmptySlotMenuTrigger`.
3. Adicionados ao repo 2 mods de terceiros (`LetMeRightClick`, `WeaponCamoAndStickers`) via `/add-mod-repo-for-modding`, usados na investigação do bug de botão direito.
4. `Tyfon.UIFixes.dll` v5.3.23 compilado localmente (`mods/UIFixes/builds/`), sem instalação automática no jogo — por pedido explícito do usuário, builds ficam só locais a partir de agora.

**Pendências abertas nesta sessão:**
- [P-8.1] Limpar `FikaActiveWeaponMagSwapPatch` (patch no alvo errado, hoje inútil). Categoria: 🟢 ideia.
- [P-8.2] Validar in-game o fix do `EmptySlotMenuTrigger`. Categoria: 🟡 débito técnico.

**Cross-refs:**
- Trabalho paralelo no mesmo dia: ver `mods/FIKA/memory/sessions.md` 2026-09-06 Sessão 3 (item `003-magazine-swap-inplace-fix` — causa raiz relacionada, mesma investigação de `GClass1561`/`inOutHandsProcess`, resolvida do lado do FIKA).

---

## 2026-09-06 — Sessão: Swap In-Place 1-para-1 Seguro com FIKA Headless e Release v5.3.21

**Tema central:** Viabilização do drag-and-drop de carregadores ocupando o mesmo slot de inventário (Swap 1-para-1) em armas empunhadas em partidas Coop com FIKA Headless sem causar rejeição de inventário ou travamento do gatilho.

**Decisões-chave:**
1. **Identificação da Causa Raiz no Headless:**
   - Descompilado `TraderControllerClass.CheckItemAction` e rastreado `GClass1561` ("Cannot apply {Item_0} because {Item_1} is currently being modified").
   - O Headless rejeitava `InventoryPacket` com `SwapOperationClass` porque o jogador estava com a arma ativa nas mãos (`inOutHandsProcess`).
2. **Patch Cirúrgico de Desbloqueio (`FikaActiveWeaponMagSwapPatch`):**
   - Intercepta `TraderControllerClass.CheckItemAction` em Postfix.
   - Quando `GClass1561` for acusado em uma operação envolvendo `MagazineItemClass` ou slot de arma, zera o erro (`__result = default`), permitindo que a transação de inventário seja executada e confirmada (`OperationCallbackPacket(Succeeded)`) no Headless e no Host.
3. **Pipeline Canônico de Swap no Mesmo Slot (`ActiveWeaponMagDropPatch`):**
   - Mantém o drop despachando através de `InteractionsHandlerClass.Swap`, garantindo que o carregador velho assuma com precisão o slot onde estava o carregador novo.
4. **SemVer & Build:**
   - Bump para `5.3.21` em `Shared.props`, `mod.json` e `sessions.md`.
   - Compilação de toda a solução (`UIFixes.sln`) em Release.

---

## 2026-09-06 — Sessão: Instrumentação Diagnóstica do Drag-and-Drop em Coop e Release v5.3.20

**Tema central:** Inclusão de logs diagnósticos detalhados para rastrear no console do BepInEx exatamente como o `ActiveWeaponMagDropPatch` está avaliando o estado das mãos e do controller no cliente FIKA durante partidas Coop com Headless.

**Decisões-chave:**
1. **Comparação Infalível por MongoID:**
   - Em vez de comparar ponteiros de instância (`!=`), utiliza `firearmController.Item.Id == targetWeapon.Id`, prevenindo falsos negativos decorrentes de clones de UI do Tarkov.
2. **Resolução de Endereço Seguro:**
   - `ItemAddress magAddress = magazine.CurrentAddress ?? magazine.Parent`.
3. **Delegação ao `ReloadMag` do Controller:**
   - Removido o `if (!firearmController.CanStartReload()) return false;` prévio, permitindo que o `firearmController.ReloadMag` execute o seu `ForceStopInteractions()` nativo e logando o estado de `CanStartReload` e `WaitingForCallback`.
4. **SemVer & Build:**
   - Bump para `5.3.20` em `Shared.props` e `mod.json`.
   - Compilação de toda a solução (`UIFixes.sln`) em Release.

---

## 2026-09-06 — Sessão: Correção de Desync no Drag-and-Drop de Magazine em Coop / Headless e Release v5.3.19

**Tema central:** Eliminação definitiva do desync silencioso de recarga por drag-and-drop de magazine em clientes conectados a servidores Headless / Coop no FIKA.

**Decisões-chave:**
1. **Causa Raiz Comprovada no FIKA:**
   - Ao executar `InteractionsHandlerClass.Swap` no drop em arma ativa nas mãos, o `SlotView.AcceptItem` chamava `ItemController.RunNetworkTransaction`, tentando desequipar/modificar a arma via pacote de inventário ao mesmo tempo que as mãos estavam ativas.
   - O servidor Headless travava o inventário com "Default Inventory is currently being modified". No cliente, o `FikaPlayer` ficava com `WaitingForCallback = true`, bloqueando o gatilho silenciosamente em `CanPressTrigger()`.
2. **Interceptação no Ponto de Drop (`ActiveWeaponMagDropPatch` em `SwapPatches.cs`):**
   - Intercepta `SlotView.AcceptItem` via Prefix.
   - Se o item for `MagazineItemClass` e o slot pertencer à arma ativa em mãos (`firearmController.Item == targetWeapon`):
     - Cancela o cursor visual de drag via `itemContext.DragCancelled()`.
     - Chama `firearmController.ReloadMag(magazine, magazine.CurrentAddress, null)` para iniciar a FSM canônica e transmitir o `ReloadMagPacket` oficial ao Headless.
     - Retorna `false` e define `__result = Task.CompletedTask`, suprimindo a chamada de `ItemController.RunNetworkTransaction`.
   - Se a arma for guardada (costas/coldre enquanto outra está em mãos) ou fora de raid no Stash: deixa o fluxo padrão rodar (`return true`), mantendo o swap de inventário instantâneo.
3. **SemVer & Build:**
   - Bump para `5.3.19` em `Shared.props` e `mod.json`.
   - Compilação de toda a solução (`UIFixes.sln`) em Release.

---

## 2026-09-06 — Sessão: Drag-and-Drop de Magazine nos 3 Slots de Armas e Release v5.3.18

**Tema central:** Habilitação do drag-and-drop de carregadores diretamente no corpo da arma nos três slots de armas do personagem (Bandoleira, Costas e Coldre), replicando o funcionamento canônico com animação e swap 1-para-1 testado com sucesso no Inspecionar.

**Decisões-chave:**
1. **Unificação com o fluxo do Inspecionar (`WeaponApplyPatch` em `SwapPatches.cs`):**
   - Removida a `NoopOperation` temporária (que retornava `CanExecute => false` e fazia `SlotView.AcceptItem` abortar imediatamente no drop).
   - `WeaponApplyPatch.Postfix` agora retorna diretamente o resultado de `InteractionsHandlerClass.Swap(item, currentMagAddress, magazineSlot.ContainedItem, itemAddress, itemController, simulate)`.
   - O `SlotView.AcceptItem` do EFT recebe uma `SwapOperationClass` válida com `CanExecute = true` tanto na simulação (hover verde) quanto no drop, encaminhando para `ItemController.RunNetworkTransaction`.
2. **Suporte Universal aos 3 Slots (`FirstPrimaryWeapon`, `SecondPrimaryWeapon` e `Holster`):**
   - Todas as armas equipadas nesses 3 slots herdam da classe `Weapon`, de modo que o `Weapon.Apply` unifica o tratamento para qualquer uma delas.
   - Se a arma do slot for a arma empunhada no momento (mãos), o EFT despacha a FSM canônica `FirearmInsertedMagState` (`GClass2039`) realizando a animação e o swap no colete/bolso.
   - Se a arma for guardada (costas/coldre enquanto outra está em mãos), realiza o swap silencioso e instantâneo no inventário.
3. **SemVer & Build:**
   - Bump para `5.3.18` em `Shared.props` e `mod.json`.
   - Compilação de toda a solução (`UIFixes.sln`) em Release.

---

## 2026-09-06 — Sessão: Reload In-Place FIKA Safe e Release v5.3.17

**Tema central:** Eliminação do desync / carregador piscando no FIKA Headless ao arrastar carregador sobre arma ativa no inventário e correção da ordenação de slots no reload in-place.

**Decisões-chave:**
1. **Eliminação de Swap cru em arma ativa (`WeaponApplyPatch` em `SwapPatches.cs`):**
   - Ao arrastar um magazine sobre arma empunhada (`player.HandsController is Player.FirearmController firearmController && firearmController.Item == __instance`), não despacha mais `InteractionsHandlerClass.Swap`.
   - Em vez disso, no `simulate = false`, aciona `firearmController.ReloadMag(magazine, itemAddress, null)`, utilizando o canal de animação canônico do jogo e o pacote `ReloadMagPacket` do FIKA.
   - Retorna uma `NoopOperation : IRaiseEvents` com `CanExecute(controller) => false`. Isso permite que o UI do EFT trate a operação como aceita visualmente (sem snapback com erro), mas faz o `TryRunNetworkTransaction` ignorar a execução de rede de inventário crua (`Can not execute`), prevenindo transações rejeitadas pelo Headless.
2. **Correção de Double-Sort em LINQ (`SwapIfNoSpacePatch` em `ReloadInPlacePatches.cs`):**
   - O código anterior executava dois `OrderBy` sucessivos, onde o segundo sobrescrevia o primeiro.
   - Ajustado para `OrderByDescending(address => Settings.AlwaysSwapMags.Value && address.Equals(magAddress)).ThenBy(address => address.Grid.GridWidth * address.Grid.GridHeight)`.
   - Garante que a vaga de onde o carregador novo saiu seja prioritária absoluta quando `AlwaysSwapMags` estiver ativado.
3. **SemVer & Build:**
   - Bump para `5.3.17` em `Shared.props` e `mod.json`.
   - Compilação de toda a solução (`UIFixes.sln`) em Release gerando `Tyfon.UIFixes.dll`, `Tyfon.UIFixes.Net.dll` e `Tyfon.UIFixes.Server.dll` em `mods/UIFixes/builds/` com 0 erros e 0 avisos.

---

## 2026-09-05 — Sessão: Swap In-Place via SlotView/GridView Prefix e Release v5.3.15

**Tema central:** Correção do problema onde nada acontecia ao soltar o carregador sobre a arma no inventário durante a raid (após remoção da mensagem de erro de slot).

**Decisões-chave:**
1. **Causa Raiz Comprovada (`SlotView.AcceptItem`):**
   - No EFT (`SlotView.AcceptItem:811-817`), o método avaliava `ItemController.CanExecute(operation.Value)`. Com a simulação retornando `operation.Value == null`, o método abortava imediatamente.
2. **Interceptação via Prefix (`SlotViewMagReloadPatch` & `GridViewMagReloadPatch`):**
   - Intercepta `SlotView.AcceptItem` e `GridView.AcceptItem` antes da verificação vanilla de `CanExecute`.
   - Executa diretamente `firearmController.ReloadMag(mag, targetAddress, null)`.
   - Cancela o drag e retorna `false`, suprimindo o fluxo vanilla do jogo e prevenindo desyncs de inventário.
3. **Garantia de Swap In-Place (Sem cair no chão):**
   - `QuickReloadMag` eliminado. O slot de origem do novo carregador (`mag.Parent`) fica imediatamente vago e é garantido como destino do carregador antigo.
4. **SemVer & Build:** Bump para `5.3.15` em `Shared.props`. Compilação Release em `mods/UIFixes/builds/` gerada com 0 erros e 0 avisos.

---

## 2026-09-05 — Sessão: Recarga de Carregador em Arma Ativa via Drag-and-Drop em Raid e Release v5.3.14

**Tema central:** Correção da regressão e incompatibilidade de rede no FIKA Headless onde arrastar um carregador para uma arma equipada em raid ou falhava com "não tem slot disponível" (quando havia pente inserido) ou deixava o pente piscando eternamente com erro `Unable to process descriptor ... Cannot merge` (quando a arma estava sem pente).

**Decisões-chave:**
1. **Diferenciação Canônica de Armas em Mãos vs Inventário Puro (`SwapPatches.cs` - `WeaponApplyPatch`):**
   - Quando `Plugin.InRaid()` está ativo e o jogador arrasta um `MagazineItemClass` sobre a arma atualmente empunhada (`player.HandsController is IFirearmHandsController firearmController && firearmController.Item == __instance`), a operação **NÃO** pode ser executada como um `InteractionsHandlerClass.Move` ou `InteractionsHandlerClass.Swap` cru de inventário.
   - Operações cruas de inventário sobre armas empunhadas violam a FSM de mãos do EFT e geram descritores rejeitados pelo FIKA Headless.
2. **Integração com `FirearmController` / FIKA Network FSM:**
   - Em hover (`simulate == true`): Se `slot.CanAccept(mag)` e `firearmController.CanStartReload()`, retorna sucesso (`new ItemOperation((IRaiseEvents)null)`) permitindo feedback visual verde no inventário.
   - Em soltura (`simulate == false`): Calcula o endereço de retorno para o pente antigo (`GetPrioritizedGridsForUnloadedObject` ou reaproveitamento do slot de onde veio o novo pente com roll-back defensivo). Executa `firearmController.ReloadMag(mag, targetAddress, null)` (ou `QuickReloadMag` caso não haja espaço e a arma suporte reload rápido).
   - Sobrescreve `__result = new ItemOperation((IRaiseEvents)null)` para suprimir a operação crua do EFT nativo.
3. **Armas Guardadas / Stash:**
   - Armas guardadas nas costas/coldre (não ativas nas mãos) ou fora de raid no Stash continuam utilizando a lógica padrão do UIFixes (`Move` vanilla se vazio, `Swap` se ocupado).
4. **SemVer & Build:** Bump para `5.3.14` em `Shared.props`. Compilação Release em `mods/UIFixes/builds/` gerada com 0 erros e 0 avisos.
