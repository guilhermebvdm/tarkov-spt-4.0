# UIFixes — Memória de Sessões

## Snapshot Delta
- **Versão:** 5.3.26 (SPT 4.0 / EFT 0.16.9)
- **Estado:** Item `001-reload-swap-sem-espaco` fechado de fato — o fallback CR-01-01 (v5.3.22) não cobria o cenário real reportado ("0% de espaço livre em qualquer lugar"); causa raiz e fix definitivo ver Sessão 9 abaixo. `EmptySlotMenuTrigger` (fix de sessão anterior) ainda sem confirmação explícita do usuário. Logs `[UIFixes-Diag]` de diagnóstico **mantidos deliberadamente** em `ReloadInPlacePatches.cs` (usuário está coletando mais relatos de terceiros antes de pedir a remoção). Novo achado (Sessão 9b): o drag-and-drop de carregador em arma equipada (`WeaponApplyPatch`, `SwapPatches.cs:840`) pode ser rejeitado pelo Host quando a arma acabou de ser reequipada por OUTRA ação (ex.: curar um aliado via `TRL-ImmersiveCombatMedicine`) — gap no fix `003` do FIKA, não em código deste mod. Rastreado como item `004` no backlog do FIKA.
- **Pendências:** ver bloco abaixo.

## Pendências

- [P-8.1] (aberta 2026-09-06) **Limpar `FikaActiveWeaponMagSwapPatch`** em `SwapPatches.cs:891-924` — intercepta `TraderControllerClass.CheckItemAction` do lado cliente, mas a rejeição real acontece em `ObservedInventoryController.CheckItemAction` no Headless (override que nunca chama `base`, despacho virtual não alcança este patch). Hoje inofensivo (não conflita com nada) porém inútil pro caso Headless, que já foi resolvido do lado do FIKA (`mods/FIKA/backlog/003-magazine-swap-inplace-fix/`). Cross-ref: `mods/FIKA/memory/sessions.md` P-3.2. 🟢 Ideia / limpeza.
- [P-8.2] (aberta 2026-09-06) **Validar in-game o fix do `EmptySlotMenuTrigger`** — corrigido em sessão anterior, ainda não testado pelo usuário em jogo. 🟡 Débito técnico (validação pendente, AP-06).
- [P-9.1] (aberta 2026-09-08) **Remover os logs `[UIFixes-Diag]` de `SwapIfNoSpacePatch.Prefix`** (`ReloadInPlacePatches.cs`) quando o usuário confirmar que não precisa mais coletar relatos — são diagnóstico temporário, não devem sobreviver a uma versão de produção. 🟡 Débito técnico (mantido por pedido explícito do usuário nesta sessão).
- [P-9.2] (aberta 2026-09-08) **Validar `001-reload-swap-sem-espaco` (Fix 01) em raid1→raid2 e alt-F4/morte/MIA** — só testado dentro de uma única raid Fika Host+Headless até agora. 🟡 Débito técnico (checklist do `06-fix-01.md` incompleto).
- [P-9.3] (aberta 2026-09-08) **Acompanhar `mods/FIKA/backlog/004-colisao-cura-swap-magazine`** — causa raiz do "mão travada após curar aliado + arrastar carregador" já diagnosticada (ver Sessão 9b), fix ainda não implementado (usuário pediu pra parquear "pra ver mais tarde"). O código deste mod (`WeaponApplyPatch`) não precisa mudar — o fix é inteiramente do lado do FIKA. 🟡 Débito técnico / cross-mod.

---

## 2026-09-08 03:00 (GMT-3) — Sessão 9: Causa raiz real do swap-sem-espaço (0% de vaga) + fix atômico via Swap e Release v5.3.26

**Tema central:** O fallback CR-01-01 (Sessão 8, v5.3.22) resolvia "colete/bolsos cheios, mochila com vaga", mas o usuário reportou que "0% de espaço livre em qualquer lugar" continuava sem funcionar. Investigação revelou uma limitação de ordenação no próprio motor nativo do EFT, não coberta pela spec técnica original.

**Decisões-chave:**
- [Causa raiz real]: `Player.FirearmController.GClass2006.Run` (motor nativo de `ReloadMag`) move o carregador antigo pra `vestTargetAddress` **antes** de mover o carregador novo pra fora dali. Quando a única vaga possível é exatamente o espaço que o carregador novo ainda ocupa (inventário 100% cheio), esse `Move` colide consigo mesmo (`GridSpaceTakenError`) e o reload inteiro aborta silenciosamente — sem qualquer erro visível. Isso **refuta a premissa da spec técnica original** (`02-spec-tech.md`), que assumia que popular `itemAddress` bastava em qualquer cenário. Ref: `references/eft-decompiled` via `ilspycmd` (dump local não tinha o tipo nested; decompile direto autorizado) — `GClass2006.Run`, bloco `if (vestTargetAddress != null) { gStruct3 = InteractionsHandlerClass.Move(currentMagazine, vestTargetAddress, itemController); ... }` seguido do `Move(nextMagazine, magazineSlot.CreateItemAddress(), itemController)`.
- [Fix]: `SwapIfNoSpacePatch.Prefix` detecta quando `candidateAddress.Equals(magAddress)` (o caso "0% de espaço") e usa `InteractionsHandlerClass.Swap(magazine, weaponMagSlotAddress, currentMagazine, candidateAddress, controller, true)` + `controller.TryRunNetworkTransaction(swapResult, callback)` — troca atômica, sem o problema de ordenação sequencial — retornando `false` (pula o `ReloadMag` nativo só nesse caso específico). Para qualquer outro caso (vaga livre genuinamente diferente), mantém o `Move`/`return true` já existente. Ref: `ReloadInPlacePatches.cs:225-273` (ver `001-reload-swap-sem-espaco-06-fix-01.md` pro detalhamento completo).
- [Decisão de segurança Fika — por que Swap cru é seguro agora]: uma sessão anterior deste mod (v5.3.14-v5.3.21) havia **abandonado deliberadamente** o padrão "Swap cru numa arma empunhada" por causar `GClass1561`/travamento de mãos no FIKA Headless. Essa decisão é **anterior** ao fix `003-magazine-swap-inplace-fix` do FIKA (`ObservedInventoryController.IsSelfReferentialMagazineSwap`), que resolveu a causa raiz desse `GClass1561` de forma genérica (cobre qualquer troca entre dois `MagazineItemClass` na mesma arma, não importa qual UI disparou). Prova empírica: `WeaponApplyPatch` (`SwapPatches.cs:840`, drag-and-drop) já usa Swap cru com sucesso confirmado em Fika hoje. Testado in-game em sessão Fika Host + Headless real: log do Headless sem `GClass1561`/"currently being modified", `Swap result: Succeeded=True`. Cross-ref: `mods/FIKA/memory/sessions.md` Sessão 3.
- [Pacing da animação]: `ReloadMag` nativo chama `_player.RemoveLeftHandItem(3f)` + `AnimatedInteractions.ForceStopInteractions()` logo no início, antes de tocar em qualquer item — é essa chamada que dá a sensação de "recarga penalizada" (o motor real da animação, `GClass2016`, dispara eventos como `OnMagPuttedToRig` que incluem o passo extra de guardar o carregador velho no colete). Como nosso fix pula o `ReloadMag` nativo pra esse caso, replicamos essas duas chamadas manualmente antes do Swap — sem isso a troca acontecia instantânea, sem nenhum tempo/animação. Ref: decompile de `Player.FirearmController.ReloadMag` e `GClass2016.Start`/`OnMagPuttedToRig`.

**Lições / hipóteses descartadas:**
- Hipótese "o fallback CR-01-01 já cobre o caso de 0% de espaço" — descartada por evidência direta: os logs de diagnóstico mostraram `candidateAddress` sendo encontrado corretamente (a busca funciona), mas o motor nativo falhava silenciosamente ao tentar aplicá-lo. O gap não era na busca da vaga, era na aplicação.
- Hipótese "reescrever `GClass2006.Run` via transpiler pra manter a animação nativa 100% idêntica" — considerada e **rejeitada pelo usuário** por risco de regressão e fragilidade contra atualizações do jogo. Trade-off aceito: Swap atômico com pacing manual (`RemoveLeftHandItem`), sem os eventos intermediários do Animator (`OnMagPulledOutFromWeapon`/`OnMagAppeared`).
- Falso alarme do próprio investigador: achei inicialmente (antes de checar `original/`) que reverter pro padrão Swap cru reintroduziria a regressão do FIKA das sessões v5.3.14-21 — descartado ao confirmar que o fix `003` do FIKA (posterior àquelas sessões) já neutraliza a causa raiz de forma genérica.
- Usuário reportou inicialmente "recarga penalizada não voltou" após o fix de pacing — descartado como falso negativo (estava testando uma build antiga em cache); reteste confirmou que o pacing funciona corretamente.

**Atividade cronológica:**
1. Diagnóstico via logs `[UIFixes-Diag]` temporários adicionados em `SwapIfNoSpacePatch.Prefix` — confirmou que a busca de vaga funcionava mas o motor nativo falhava depois.
2. Decompile direto (`ilspycmd`, tipo nested ausente do dump local) de `GClass2006.Run` — confirmou a ordem de `Move`s e a causa raiz.
3. Comparação com `mods/UIFixes/original` a pedido do usuário — confirmou que o upstream usa Swap atômico pra esse exato cenário.
4. Cruzamento com `mods/UIFixes/memory/sessions.md` (sessões v5.3.14-21) — identificado o risco de regressão Fika do Swap cru, e depois descartado ao confirmar que o fix `003` do FIKA já resolve isso.
5. Implementado, compilado (v5.3.25) e validado in-game pelo usuário em raid Fika Host + Headless — sem `GClass1561`.
6. Usuário reportou perda da animação/tempo de "recarga penalizada" — decompile de `GClass2016` confirmou a causa; adicionado `RemoveLeftHandItem(3f)`/`ForceStopInteractions()`, recompilado (v5.3.26) e revalidado — confirmado funcionando.
7. Documentado em `001-reload-swap-sem-espaco-06-fix-01.md` (padrão `.agents/templates/fix.md.tmpl`) e atualizado o `05-asbuild.md`.

**Pendências abertas nesta sessão:**
- [P-9.1] Remover logs `[UIFixes-Diag]` quando o usuário não precisar mais deles (mantidos a pedido, coletando relatos de terceiros). Categoria: 🟡 débito técnico.
- [P-9.2] Validar o Fix 01 em raid1→raid2 e alt-F4/morte/MIA (só testado numa única raid até agora). Categoria: 🟡 débito técnico.

**Cross-refs:**
- Resolve, de fato, o item `001-reload-swap-sem-espaco` que a Sessão 8 havia marcado como fechado prematuramente (o fallback CR-01-01 não cobria o cenário real).
- Depende do fix `003-magazine-swap-inplace-fix` do FIKA (`mods/FIKA/memory/sessions.md` Sessão 3) para a segurança do Swap cru em arma empunhada — sem aquele fix, esta correção reintroduziria `GClass1561` no Headless.

---

## 2026-09-08 04:12 (GMT-3) — Sessão 9b: Diagnóstico de mão travada em terceiro (drag-and-drop de carregador colide com cura de aliado)

**Tema central:** Relato de um jogador da sessão do usuário (não o próprio usuário): depois de curar um aliado (`TRL-ImmersiveCombatMedicine`) e, em seguida, arrastar um carregador pra cima da própria arma equipada (`WeaponApplyPatch`, recurso já existente e confirmado funcionando em outros cenários), a arma "sumiu" e a mão ficou travada — nem trocar de slot nem o botão de emergência do `HandsAreNotBusy` (END) resolveram. Investigação por análise de código cruzada entre 3 mods (UIFixes, FIKA, TRL-ImmersiveCombatMedicine); log do lado do relator (cliente) disponível, log do lado do usuário (Host) **não continha essa raid** (mesmo o jogador aparecendo em outras raids do arquivo — sessão específica não capturada, possivelmente sobrescrita).

**Decisões-chave:**
- [Causa raiz — gap no fix `003` do FIKA, não bug do UIFixes]: `TRL-ImmersiveCombatMedicine` finaliza uma cura chamando `ForceFinishAnimation()` → `method_9` do `ObservedMedsControllerClass` (`BandAidController.cs:625`) — a limpeza nativa do jogo que devolve a arma pras mãos do médico, disparando um `inOutHandsProcess` (Begin) pra arma no Host. O item que abriu esse Begin é o item de cura (bandagem/CMS), não um carregador. `ObservedInventoryController.IsSelfReferentialMagazineSwap` (fix `003` do FIKA) só tolera a colisão quando `movedItem is MagazineItemClass` — como quem abriu foi o item de cura, a condição falha e o Host rejeita o swap do carregador (que veio logo em seguida) com `GClass1561` de verdade. O cliente já tinha aplicado a troca localmente (Swap otimista), gerando a mesma divergência client/server (`server status: Failed / client status: Succeeded`) já vista antes — e travando o "Default Inventory" do jogador no Host, por isso nem troca de slot nem END resolveram (ambos dependem de operação aceita pelo Host).
- [Escopo do fix — não é aqui]: o código deste mod (`WeaponApplyPatch`, `SwapPatches.cs:840`) não precisa de nenhuma mudança — ele já delega corretamente pro pipeline nativo de Swap. O fix correto é ampliar `IsSelfReferentialMagazineSwap` no FIKA pra tolerar qualquer transição de mãos recente na MESMA arma do MESMO jogador (não só quando aberta por um carregador), preservando a proteção real (bloquear colisão entre jogadores DIFERENTES). Registrado como `mods/FIKA/backlog/004-colisao-cura-swap-magazine` — pasta criada, spec ainda não escrita (usuário pediu pra parquear).

**Lições / hipóteses descartadas:**
- Hipótese "é uma incompatibilidade direta entre UIFixes e TRL-ImmersiveCombatMedicine" (levantada pelo usuário) — refinada: os dois mods não conflitam entre si diretamente; o problema é que uma proteção do FIKA (`IsSelfReferentialMagazineSwap`) foi escrita estreita demais (só cobre magazine-vs-magazine) e qualquer AÇÃO NÃO RELACIONADA A CARREGADOR que reequipe a arma logo antes de um swap de carregador vai disparar o mesmo sintoma — cura é só o primeiro caso observado, não a causa raiz isolada.
- Tentativa de correlacionar via log do Host descartada como caminho principal — o log fornecido (21k linhas, cobrindo várias raids do mesmo jogador) não continha os identificadores exatos da raid do incidente; a causa raiz foi confirmada por leitura de código nos 3 mods, não por correlação de log.

**Atividade cronológica:**
1. Lido o log do cliente do relator — identificado `[ReceiveStatusFromServer]: ... Client operation rejected by server: 91 - OperationType: SwapOperationClass` e `status mismatch: server status: Failed client status: Succeeded`, com timing logo após `HealRoutine: UseTime TERMINOU`.
2. Buscado o log do Host (`E:\Tarkov Red Line\BepInEx\LogOutput.log`, 21988 linhas) pelos mesmos identificadores (item/arma/profile IDs, `SwapOperationClass`, `currently being modified`) — zero ocorrências; jogador aparece em outras raids do arquivo, mas não nessa específica.
3. Lido `TRL-ImmersiveCombatMedicine/modded-V3(review)/Patches/Medical/BandAidController.cs` — confirmado que `ForceFinishAnimation()` chama `method_9` (cleanup nativo) ao fim da cura, que reequipa a arma.
4. Relido `ObservedInventoryController.IsSelfReferentialMagazineSwap` (fix `003` do FIKA) — confirmado que a checagem `movedItem is MagazineItemClass` exclui exatamente esse caso.
5. Explicado o achado ao usuário (resumo técnico + resumo leigo) e criado `mods/FIKA/backlog/004-colisao-cura-swap-magazine` a pedido do usuário — pasta registrada, sem spec (parqueado pra retomar depois).

**Pendências abertas nesta sessão:**
- [P-9.3] Acompanhar `mods/FIKA/backlog/004-colisao-cura-swap-magazine` até ter spec/fix — ver bloco de pendências no topo. Categoria: 🟡 débito técnico / cross-mod.

**Cross-refs:**
- Gap no fix `003-magazine-swap-inplace-fix` do FIKA (`mods/FIKA/memory/sessions.md` Sessão 3) — mesma família de proteção (`inOutHandsProcess`/`GClass1561`), colisão não coberta.
- Fix propriamente dito vive inteiramente em `mods/FIKA/`, não em `mods/UIFixes/` — registrado aqui só porque a investigação partiu de um recurso deste mod (`WeaponApplyPatch`).

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
