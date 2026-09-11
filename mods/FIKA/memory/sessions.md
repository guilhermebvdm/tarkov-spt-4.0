# Memória de Sessões — Project FIKA

## Estado atual

> **Delta 2026-09-10 (Sessão 6):** Auditoria de código 01 registrada em `docs/relatorio-auditoria-codigo-01.md` e elaboração de spec técnica para o item `006-colisao-maos-item-nao-carregador`. Item 006 está **PAUSADO** aguardando teste do usuário em raid real (com Debug+log) para confirmar se a causa assumida do bug `ContinuousLoadAmmo` está correta antes de implementar.
>
> **Delta 2026-09-09 (Sessão 5):** Item `005-hook-velocidade-cura-observada` implementado: criação de `ObservedMedsSpeedHook.cs` exportando o delegate público `Func<Player, Item, float> ExtraSpeedMultiplier` para consumo de outros mods (`CustomClasses/090`), e integração em `ObservedMedsController.cs`. Bump SemVer para v2.3.16 em `FikaPlugin.cs` e `mod.json`. Compilação/instalação no jogo não realizada a pedido do usuário (restrita ao repo). Validação in-game pendente (P-5.1).
>
> **Delta 2026-09-08 (Sessão 4):** Ciclo completo de backlog (spec → review-spec → spec técnica → review técnica → `/code-mod` → `/code-review` → `/apply-code-review`) para o item `004-colisao-cura-swap-magazine`. Causa raiz: `IsSelfReferentialMagazineSwap` (fix do item 003) só tolerava a colisão `inOutHandsProcess` quando o item que abriu o `Begin` pendente era outro `MagazineItemClass` — arma reequipada ao fim de uma cura (`TRL-ImmersiveCombatMedicine`, `method_9`/`ForceFinishAnimation`) abre esse `Begin` com o item de cura, não um carregador, então o swap de carregador seguinte era rejeitado com `GClass1561` de verdade, travando a mão do jogador. Fix: renomear para `IsSelfReferentialHandsTransition` e generalizar a condição de `movedItem is MagazineItemClass` para `movedItem != null && movedItem != weapon` — tolera qualquer transição de mãos recente do mesmo jogador na mesma arma (cura, granada, faca, reanimação), preservando o bloqueio de um saque/guarda real da própria arma. Nenhum novo Harmony patch: reaproveita 100% a infraestrutura `InOutHandsProcessTimestampPatch` do item 003. `Fika.Core.dll` v2.3.15 — **build e validação in-game ainda pendentes** (ver P-4.1).

## Pendências

- [P-6.1] (aberta 2026-09-10) **Testar em raid real com Debug+log a causa raiz de mãos travadas no `ContinuousLoadAmmo` (item 006)** — Verificar se a transição de mãos recente no `inOutHandsProcess` realmente causa o erro `"Default Inventory is currently being modified"` ao equipar arma pós-carregamento contínuo. 🔴 Bloqueador (item pausado aguardando evidência empírica).
- [P-5.1] (aberta 2026-09-09) **Compilar e validar in-game o hook de velocidade de cura observada (item 005, v2.3.16)** — Testar em coop Fika com `CustomClasses` (item 090): confirmar que a velocidade extra de cirurgia/cura de aliado é repassada e renderizada suavemente para o jogador observador. 🟡 Validação in-game.
- [P-4.1] (aberta 2026-09-08) **VALIDAR IN-GAME o item `004-colisao-cura-swap-magazine`** — checklist completo em `004-colisao-cura-swap-magazine-02-spec-tech.md` §8: cenário principal (cura → swap de carregador), self-heal, sequência repetida/múltiplos aliados, checagem do observador (jogador B), e o teste **bloqueador** de que um saque de arma real continua sendo rejeitado (incerteza de análise estática documentada na spec técnica §1.3/§7 — só o teste in-game fecha com certeza). 🟡 Validação in-game (AP-06).
- [P-3.1] (aberta 2026-09-06) **Calibrar `GraceWindowSeconds`** — hoje fixo em `0.35f` (marcado `TODO confirmar` no código) em `ObservedInventoryController.cs`. Precisa de instrumentação temporária (log de `elapsedSeconds` real) em sessão Headless de verdade antes de considerar definitivo. Desde a Sessão 4, essa mesma constante é reaproveitada por `IsSelfReferentialHandsTransition` (item 004) — recalibrar afeta os dois. 🟡 Débito técnico.
- [P-3.2] (aberta 2026-09-06) **Trilha B do item 003** — limpar `FikaActiveWeaponMagSwapPatch` em `mods/UIFixes/modded/src/Patches/SwapPatches.cs:891-924` (patch no alvo errado — `TraderControllerClass.CheckItemAction` do lado cliente — hoje inofensivo mas inútil pro Headless). Cross-ref: `mods/UIFixes/memory/sessions.md` P-8.1. 🟢 Ideia / limpeza.
- [P-1.1] (aberta 2026-09-02) **VALIDAR IN-GAME a suite completa modded do FIKA** — Cenários a testar em sessão multiplayer: **(1)** Conexão cliente-servidor e movimentação sem jitter; **(2)** Mecânica de reviver verificando hitboxes pós-revive (TRL-Fixes #1); **(3)** Movimentação rápida de inventário com `Ctrl+Click` para validar auto-recuperação (TRL-Fixes #2); **(4)** Equipar arma com trilhos múltiplos tácticos (TRL-Fixes #4); **(5)** Entrada de bots em metralhadoras/lança-granadas montadas (TRL-Fixes #6); **(6)** Transição e retorno ao menu principal monitorando descarte de memória RAM; **(7)** [NOVO] Descarte rápido de mochila com "ZZ" e re-coleta no chão em raid multiplayer com Headless (Item 001). 🟡 Validação in-game.
- [P-1.2] (aberta 2026-09-02) **Implementação da Correção de Desync no Reconect (Ghost Body)** — Re-binding atômico de `ObservedPlayer` e reset de interpolação no Host conforme [`docs/ROADMAP.md`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/docs/ROADMAP.md) §1. 🟢 Feature / Fix.
- [P-1.3] (aberta 2026-09-02) **Implementação do Sistema de Senha Temporária para Raids** — Integração de validação de hash de senha no `FikaServer` e modal de input no `MatchMakerUIScript.cs` conforme [`docs/ROADMAP.md`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/docs/ROADMAP.md) §3. 🟢 Feature.

---

## 2026-09-10 20:15 (GMT-3) — Sessão 6: Auditoria de Código 01 e Especificação do Item 006 (PAUSADO para teste in-game)

**Tema central:** Auditoria estática de código no fork do Fika (`docs/relatorio-auditoria-codigo-01.md`), identificação de achados (AUD-01-01 a AUD-01-04) e ciclo de especificação até review técnica 01 do item 006 — porém trabalho colocado em **PAUSA** antes de codar para validação empírica.

**Decisões-chave:**
- **Auditoria de Código 01 formalizada (`docs/relatorio-auditoria-codigo-01.md`):** Mapeados 4 achados no Fika: AUD-01-01 (log silencioso de exceções de inventário), AUD-01-02 (tratamento de pacotes desconhecidos no NetPacketProcessor), AUD-01-03 (timeout de inventário), AUD-01-04 (catch vazio em ClientInventoryOperationHandler).
- **Item 006 pausado para validação em raid real:** O item 006 visa resolver a trava de mãos `"Default Inventory is currently being modified"` ao equipar arma/faca/granada logo após `SPT-ContinuousLoadAmmo` ou curar aliado. A spec técnica 01 foi elaborada e revisada (`03-spec-tech-review-01.md`), mas a decisão consciente é **NÃO codar nem assumir a causa raiz como confirmada** até que o usuário teste em raid real com Debug+log ativo para coletar o stack trace e o estado do `inOutHandsProcess`.

**Lições / hipóteses descartadas:**
- *Não antecipar código sem prova empírica:* A causa assumida para o conflito com `SPT-ContinuousLoadAmmo` é plausível em teoria, mas como envolve interação com outro mod em runtime de rede, implementar patches complexos sem telemetria real pode mascarar outro problema (AP-06 / AP-09).

**Atividade cronológica:**
1. Auditoria diagnóstica de código realizada e consolidada em `docs/relatorio-auditoria-codigo-01.md`.
2. Criação do item 006: spec funcional (`01-spec.md`) e spec técnica (`02-spec-tech.md`).
3. Review técnica 01 (`03-spec-tech-review-01.md`) realizada.
4. Item colocado em pausa deliberada aguardando dados de teste in-game do usuário.

**Pendências abertas nesta sessão:**
- [P-6.1] (aberta 2026-09-10) 🔴 Testar em raid real com Debug+log a causa raiz de mãos travadas no `ContinuousLoadAmmo` (item 006).

**Cross-refs:**
- Artefatos do item: `mods/FIKA/backlog/006-colisao-maos-item-nao-carregador/`.
- Relatório de auditoria: `mods/FIKA/docs/relatorio-auditoria-codigo-01.md`.

---

## 2026-09-09 00:30 (GMT-3) — Sessão 5: Hook Genérico de Velocidade de Cura Observada (item 005, v2.3.15 → v2.3.16)

**Tema central:** Ciclo completo de backlog (spec funcional → review-spec → spec técnica → review técnica 01 → `/code-mod` → `/code-review` 01 → `/apply-code-review`) do item 005 — criação de um ponto de extensão genérico e desacoplado no `ObservedMedsController` para permitir que mods externos (`CustomClasses`, item 090) multipliquem a velocidade da animação de cura replicada em peers remotos.

**Decisões-chave:**
- **Hook genérico e desacoplado (`ObservedMedsSpeedHook.cs`):** Criação da classe pública com `public static Func<Player, Item, float>? ExtraSpeedMultiplier`. O Fika não referencia nenhum mod externo; qualquer mod consumidor assina o delegate para compor multiplicadores de classe/perk (PA-01-01).
- **Tratamento defensivo (`ResolveExtra`):** Valida `player` e `item` não-nulos antes de invocar o delegate (PA-01-02), embrulha em try/catch fail-open (retorna 1.0f em caso de erro), e valida que o retorno é número finito e positivo.
- **Intervenção em dois pontos de `ObservedMedsController.cs`:** (1) `ObservedStart`: aplica o multiplicador na 1ª parte do corpo (antes ausente de multiplicador nativo); (2) `HealthController_EffectRemovedEvent`: compõe o multiplicador do hook sobre a fórmula vanilla da skill Cirurgia: `(1f + mult) * extra`.
- **Versionamento e alinhamento SemVer:** Versão do plugin elevada de `2.3.15` para `2.3.16` em `FikaPlugin.cs` e alinhada em `mod.json` (CR-01-01). Código compilável, sem `/compile-mod` para o jogo conforme diretriz do usuário.

**Lições / hipóteses descartadas:**
- *Impossibilidade de ajustar velocidade sem hook nativo:* No vanilla e no Fika upstream, `ObservedMedsController` é privado e recalcula a velocidade apenas pela skill de cirurgia; proxies de rede não têm `ActiveHealthController`. Um hook estático no próprio Fika é a solução mais elegante e de zero overhead (evita transpilers e reflexão por frame).

**Atividade cronológica:**
1. Mapeamento do problema a partir da demanda de replicação do `CustomClasses` (item 090).
2. Ciclo de spec funcional (`01-spec.md`), técnica (`02-spec-tech.md`) e review técnica (`03-spec-tech-review-01.md` com 4 achados PA-01-01 a PA-01-04 resolvidos).
3. Implementação via `/code-mod`: criação de `ObservedMedsSpeedHook.cs`, edição cirúrgica em `ObservedMedsController.cs` e bump para `2.3.16`.
4. Code review 01 identificou 2 achados (CR-01-01 alinhamento de `mod.json`, CR-01-02 aviso XML sobre main thread), aplicados via `/apply-code-review`.
5. As-built (`05-asbuild.md`) gerado. Build e validação in-game pendentes.

**Pendências abertas nesta sessão:**
- [P-5.1] (aberta 2026-09-09) 🟡 Compilar e validar in-game o hook de velocidade de cura observada (item 005, v2.3.16).

**Cross-refs:**
- Consumidor primário: `mods/CustomClasses/backlog/090-velocidade-cura-nao-replica/` (`ClassMedicReplicationHook.cs`).
- Artefatos do item: `mods/FIKA/backlog/005-hook-velocidade-cura-observada/`.

---

## 2026-09-08 19:28 (GMT-3) — Sessão 4: Generalização do fix 003 para colisão com transição de mãos não-magazine — item 004-colisao-cura-swap-magazine

**Tema central:** Fechar a lacuna do fix `003` (`IsSelfReferentialMagazineSwap`), que só tolerava a colisão `inOutHandsProcess` quando a metade "gêmea" era outro carregador — deixando qualquer OUTRA ação que reequipe a arma (cura, e por extensão granada/faca/reanimação) travar o swap de carregador seguinte.

**Decisões-chave:**
- [Causa raiz reconfirmada por leitura direta, não só herdada da memória cross-mod]: `TRLImmersiveCombatMedicine` finaliza toda cura chamando `MedicHealPatch.ForceFinishAnimation()` (`BandAidController.cs:625` do próprio mod), que invoca via reflexão `method_9` de `Player.MedsController.ObservedMedsControllerClass` (`Player.cs:19640-19660`) — o cleanup nativo que devolve a arma às mãos do médico. Esse reequipe abre um `Begin`/`GEventArgs17` na arma via `TrySetInHands`/`HandleInProcess`, com `movedItem` = o item de cura (não um carregador) — exatamente o caso que `IsSelfReferentialMagazineSwap` (item 003) não cobria. Ref: `004-colisao-cura-swap-magazine-02-spec-tech.md` §1.1.
- [Fix generaliza sem tocar a infraestrutura de correlação]: `InOutHandsProcessTimestampPatch` (item 003) já captura genericamente qualquer item que abriu o `Begin` — a lacuna estava só na condição consumidora. Renomeado `IsSelfReferentialMagazineSwap` → `IsSelfReferentialHandsTransition`; condição trocada de `movedItem is MagazineItemClass` para `movedItem != null && movedItem != weapon`. Nenhum novo Harmony patch, nenhuma mudança em `InOutHandsProcessTimestampPatch.cs`. Ref: `ObservedInventoryController.cs:211-237`.
- [Insight arquitetural sobre o "cenário protegido"]: leitura de `Player.TryRemoveFromHands` (`Player.cs:32223-32263`) mostrou que um saque/guarda REAL da arma já equipada nunca levanta `Begin`/`Succeed` — desvia para `SetControllerInsteadRemovedOne` (`Player.cs:32242`) antes disso. Ou seja, `movedItem == weapon` só pode acontecer (se acontecer) pelo lado "entrar nas mãos" (`TrySetInHands`/`HandleInProcess`), nunca por `TryRemoveFromHands`. Essa assimetria é o motivo de manter `movedItem != weapon` como critério de bloqueio residual, em vez de simplesmente remover a checagem inteira. Documentado como incerteza (não certeza) na spec técnica §1.3/§7, com item de validação in-game **bloqueador** dedicado no §8 — útil pra qualquer bug futuro nesse mesmo pipeline de mãos.
- [Proteção cross-player já é estrutural, não precisa de novo código]: `TryGetPendingBegin` é escopado por `TraderControllerClass` (uma instância por jogador) — generalizar `movedItem` aceito não introduz nenhum risco de tolerar colisão entre jogadores diferentes, essa proteção nunca dependeu do tipo de `movedItem`.

**Lições / hipóteses descartadas:**
- Hipótese "é preciso um bypass específico por tipo de item de cura" (replicar o padrão restrito do item 003 caso a caso) descartada — não escala e é o mesmo erro de design que originou este item; a spec funcional já generaliza o corner case para qualquer ação (granada, faca, reanimação).
- Hipótese "dá pra provar 100% por leitura estática que `movedItem == weapon` nunca ocorre" descartada como certeza — só é garantidamente falso pelo lado `TryRemoveFromHands` (comportamento hard-coded no método); pelo lado `TrySetInHands`/`HandleInProcess` não há como confirmar sem reproduzir em raid. Registrado como incerteza explícita em vez de afirmado sem prova (AP-09).

**Atividade cronológica:**
1. Ciclo completo de backlog (spec funcional → review-spec → spec técnica → review técnica 01 → `/code-mod` → `/code-review` 01 → `/apply-code-review`) pro item `004-colisao-cura-swap-magazine`.
2. Review técnica 01: 3 pontos (1🟡/2🟢) — 2 itens de validação in-game adicionados ao checklist (sequência repetida/múltiplos aliados; checagem do observador B) e 1 ajuste de convenção de comentário (ID curto em vez de path completo). Todos aceitos e resolvidos antes do `/code-mod`.
3. Code review 01: 1 achado (🟡) — a correção do ajuste de convenção do ponto anterior foi aplicada em 2 de 3 comentários no código real, não nos 3; aplicado via `/apply-code-review` (CR-01-01).
4. `Fika.Core.dll` bump de versão pra `2.3.15` (`FikaPlugin.cs` + `mod.json`) — **build efetivo (`dotnet build`) e validação in-game ainda não executados nesta sessão** (usuário instruiu não rodar `/compile-mod`, que instala automaticamente no jogo).

**Pendências abertas nesta sessão:**
- [P-4.1] Validar in-game o item 004 (checklist completo na spec técnica §8, incluindo o teste bloqueador do saque de arma real). Categoria: 🟡 validação in-game (AP-06).

**Cross-refs:**
- Diagnóstico original da causa raiz: `mods/UIFixes/memory/sessions.md` Sessão 9b (2026-09-08) — resolve/fecha a pendência `P-9.3` registrada lá ("acompanhar até ter spec/fix"); fix agora implementado, validação in-game rastreada aqui em `P-4.1`.
- Mesma família de proteção do item `003` (Sessão 3 acima) — `inOutHandsProcess`/`GClass1561`, generalização direta do mecanismo de correlação criado naquela sessão.

---

## 2026-09-06 23:34 (GMT-3) — Sessão 3: Fix da rejeição GClass1561 no swap de magazine in-place (Headless) — item 003-magazine-swap-inplace-fix

**Tema central:** Diagnóstico e correção da rejeição `GClass1561`/`PlayerIsBusyError` que o Headless dedicado emitia ao aplicar a troca 1-para-1 de magazine (drag-and-drop do UIFixes) em arma empunhada, travando o gatilho/mãos do jogador até o watchdog do item 002 drenar o callback.

**Decisões-chave:**
- [Causa raiz confirmada — despacho virtual, AP-03]: `ObservedInventoryController.CheckItemAction` (`Fika-Plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs:69-206`, **idêntico ao upstream original**, não alterado por nós) reimplementa a checagem inteira sem chamar `base.CheckItemAction()`. O patch do UIFixes (`FikaActiveWeaponMagSwapPatch`, fora deste mod) intercepta `TraderControllerClass.CheckItemAction` — a base — que nunca dispara pra jogadores observados por despacho virtual. Funciona no cliente local (`ClientInventoryController` não sobrescreve `CheckItemAction`) mas nunca chega no Headless.
- [Colisão é real, não bug de rede]: mover um item aninhado na arma empunhada (ex. magazine) dispara a mesma janela "entrando/saindo das mãos" (`Begin`/`Succeed`, `GEventArgs17`) que um saque de arma real — as duas metades do swap 1-para-1 colidem consigo mesmas. Ref: `Player.cs:1441-1495` (`method_33`/`34`/`35`), `Player.cs:32223-32337` (`TryRemoveFromHands`/`TrySetInHands`).
- [Fix cirúrgico]: novo `InOutHandsProcessTimestampPatch.cs` correlaciona qual item abriu cada `Begin` (a arma = saque real, item aninhado = swap) e `ObservedInventoryController.CheckItemAction` ganha uma exceção escopada (`IsSelfReferentialMagazineSwap`) que só libera a colisão quando é a própria troca de magazine — nunca um saque de arma real nem concorrência genuína de outro jogador. Ref: `mods/FIKA/backlog/003-magazine-swap-inplace-fix/`.
- [Desvio confirmado durante `/code-mod`]: `ObservedInventoryController.InProcess` é sobrescrito e nunca chama `Player.TrySetInHands` (a spec técnica original previa um 2º Harmony patch ali, que seria código morto) — substituído por uma chamada direta em `HandleInProcess`, sem Harmony.
- [CR-01-01, code-review]: os 2 novos `ModulePatch` são classes aninhadas dentro de `InOutHandsProcessTimestampPatch`; auto-discovery do `PatchManager` não pôde ser confirmado pra tipos aninhados (biblioteca externa, sem fonte no repo) — registrados explicitamente em `FikaPlugin.cs` por segurança (custo zero).
- [Validado in-game]: usuário confirmou que `GClass1561` não reaparece mais fazendo o swap repetidamente. Restou 1 timeout de rede isolado só na 1ª tentativa da raid, autorresolvido pelo watchdog do item 002 em 5s — não relacionado a este fix (ver lições).

**Lições / hipóteses descartadas:**
- Hipótese inicial "corrida de rede genérica" (Begin/Succeed atrasado por latência) descartada como causa raiz — o mecanismo é determinístico (self-collision estrutural do próprio swap), não uma questão de timing de rede.
- Hipótese "`ObservedFirearmController` é um stub incompleto" (mesmo padrão do bug DropBackpack, item 001) descartada — `ObservedFirearmController` estende `FirearmController` real e delega `ReloadMag`/`QuickReloadMag` pro `CurrentOperation` sem overrides de `CanExecute`/`Execute`.
- Retry automático no timeout de rede isolado (cogitado em discussão) descartado — reenviar uma operação sem confirmar se o servidor já a aplicou arrisca duplicar/desfazer o estado silenciosamente; o watchdog existente (item 002) já é uma rede de segurança mais segura que um retry ingênuo.

**Atividade cronológica:**
1. Ciclo completo de backlog (spec funcional → spec técnica → review técnica → `/code-mod` → `/code-review` → `/apply-code-review`) pro item `003-magazine-swap-inplace-fix`.
2. `Fika.Core.dll` v2.3.14 compilado e instalado (via `/compile-mod`, que auto-instala no jogo — usuário pediu pra não repetir isso; builds futuros ficam só em `mods/FIKA/builds/`).
3. Validação in-game pelo usuário — funciona, com 1 sintoma residual não relacionado (timeout isolado na largada, já mitigado pelo watchdog existente).

**Pendências abertas nesta sessão:**
- [P-3.1] Calibrar `GraceWindowSeconds` com instrumentação real. Categoria: 🟡 débito técnico.
- [P-3.2] Trilha B — limpar patch no alvo errado no UIFixes. Categoria: 🟢 ideia.

**Cross-refs:**
- Trabalho paralelo no mesmo dia: ver `mods/UIFixes/memory/sessions.md` 2026-09-06 (item `001-reload-swap-sem-espaco` + fix do `EmptySlotMenuTrigger`, mesma sessão de conversa).

## 2026-09-03 23:20 (GMT-3) — Sessão 2: Correção de Descarte de Mochila (DropBackpack "ZZ") no Headless/Host e Bump v2.3.11

**Tema central:** Diagnóstico e resolução do bug de rejeição de descarte rápido de mochila com duplo clique em Z ("ZZ") durante raids multiplayer conectadas ao servidor dedicado Headless (`hands controller can't perform this operation`). Estruturação formal do backlog (`001-drop-backpack-sync-fix`), implementação do patch Harmony no `Fika.Core`, sincronização com `Fika-Headless` e code-review.

**Decisões-chave:**
- [Causa Raiz Mapeada]: `EquipmentSlot.Backpack` é o único `AnimatedSlot` do EFT (`InventoryController.cs:147`). O método nativo `Player.OutProcess` $\rightarrow$ `method_34` $\rightarrow$ `TryRemoveFromHands` invoca `HandsController.CanExecute(abstractOperation)`. Em instâncias de rede `ObservedPlayer` no Headless, o `HandsController` não suporta operações de animação de FPS locais e retorna `false`, gerando `callback.Fail("hands controller can't perform this operation")`.
- [Implementação de Patch Cirúrgico]: Criado `ObservedPlayer_DropBackpackSafety_Patch : ModulePatch` interceptando `Player.TryRemoveFromHands`. Se `__instance is ObservedPlayer` e o item sendo removido não for a arma empunhada (`HandsController.Item != item`), chama `callback?.Succeed()` e retorna `false`, bypassando a máquina de estados de mãos do proxy remoto.
- [Bump SemVer e Isolamento de Build]: Versão incrementada de `2.3.10` para `2.3.11` em `FikaPlugin.cs` e `mod.json`. `Fika.Core.dll` compilado em Release com 0 erros/avisos. Binário copiado para `mods/FIKA/modded/Fika-Headless/References/Fika.Core.dll` e `Fika.Headless.dll` recompilado com 0 erros/avisos.
- [Code Review Realizada]: Gerado `001-drop-backpack-sync-fix-04-code-review-01.md` com 3 achados mapeados (0🔴 / 0🟠 / 1🟡 / 2🟢).

**Atividade cronológica:**
1. Análise diagnóstica nos fontes descompilados de `Assembly-CSharp` (`Player.cs`, `InventoryController.cs`, `TraderControllerClass.cs`).
2. Criação do patch `ObservedPlayer_DropBackpackSafety_Patch.cs`.
3. Bump de versão SemVer para `2.3.11` e compilação de `Fika.Core` e `Fika.Headless`.
4. Estruturação do backlog formal `mods/FIKA/backlog/001-drop-backpack-sync-fix/` (`01-spec`, `02-spec-tech`, `03-spec-tech-review-01`, `05-asbuild` e `04-code-review-01`).
5. Atualização da memória de sessões.

---

## 2026-09-02 04:15 (GMT-3) — Sessão 1: Auditoria Integral, Saneamento de Memória, TRL-Fixes, Re-Auditoria, Roadmap, Infraestrutura Headless e Builds v2.3.10 / v2.3.6 / v1.4.16

**Tema central:** Auditoria completa do ecossistema FIKA (Plugin, Servidor C#, Headless e Asset Nuker), aplicação cirúrgica de correções de memory leaks, integração de correções de estabilidade multiplayer, 2ª rodada de refinamento, consolidação do Roadmap de grandes features e diretrizes de infraestrutura para servidores dedicados.

**Decisões-chave:**
- [Auditoria Integral em 8 Partições]: Cobertura de 100% dos subsistemas de rede, replicação de jogadores, inventário estrito, bots, ciclo de vida de raid, HUD, servidor C# e cliente headless.
- [Eliminação Definitiva de Vazamentos de Memória]:
  - Teardown estruturado em `FikaServer.OnDestroy()` e `FikaClient.OnDestroy()`.
  - Descarte recursivo de instâncias em `PacketPool.Dispose()`.
  - Desinscrição de delegates de armadura em `FikaPlayer.OnDestroy()` e liberação de `VoipEftSource.Release()`.
  - Limpeza de referências estáticas de mundo em `FikaHostWorld.OnDestroy()` e `FikaClientWorld.OnDestroy()`.
- [Integração dos Patches do TRL-Fixes]:
  - **#1:** Restauração de Layer 12 (`HitCollider`) em corpos e placas de blindagem pós-revive em `ReviveInteractable.cs`.
  - **#2:** Auto-recuperação visual via `RaiseRefreshEvent` em operações de inventário rejeitadas (`ClientInventoryOperationHandler.cs`).
  - **#3:** Bypass para `ProceedType.EmptyHands` em `FikaServer.Callbacks.cs`.
  - **#4:** Suporte a armas multi-trilho em `ObservedPlayer.RefreshSlotViews()`.
  - **#5:** Despacho thread-safe de mensagens de UI via `AsyncWorker.RunInMainTread` em `FikaUIGlobals.cs`.
  - **#6:** Bypass de rede para bots de IA em armas montadas em `FikaPlayer.cs`.
- [Refino de 2ª Rodada]:
  - Proteção contra acessos diretos a Singletons (`AP-02`) na FreeCam e `SyncObjectProcessorFactory`.
  - Remoção da busca de cena `GameObject.Find("BattleUIScreen")` no loop da FreeCam.
  - Timeout de segurança com `CancellationTokenSource` em fechamento de WebSocket headless.
- [Infraestrutura Headless & Multi-Instância]:
  - `AssetNuker` homologado para redução drástica de pegada de RAM (~3 GB por instância dedicada).
  - Padrão de isolamento para 2 instâncias Headless no mesmo computador: pastas clonadas, portas UDP distintas (`25565` / `25566`), perfis SPT separados e isolamento de processo via usuários secundários do Windows (`runas`) ou Sandboxie-Plus.
- [Preservação Total de Contratos Públicos]: 100% das classes, métodos, enums e delegates originais preservados para garantir compatibilidade com todos os mods dependentes do ecossistema.
- [Roadmap de Novas Features]: Documentação técnica detalhada das 3 grandes iniciativas futuras no `docs/ROADMAP.md`.

**Atividade cronológica:**
1. Importação e sincronização das 3 fundações do FIKA (Plugin, Server C#, Headless).
2. Execução da Fase 1 de auditoria técnica gerando `docs/original/relatorio-auditoria-codigo-01.md` a `08.md`.
3. Aplicação das correções cirúrgicas particionadas gerando `docs/modded/relatorio-correcao-01.md` a `08.md`.
4. Execução da 2ª rodada de auditoria estática profunda gerando `docs/modded/relatorio-auditoria-codigo-01.md` a `08.md`.
5. Implementação da 2ª rodada de correções cirúrgicas (FreeCam, Singletons, WebSockets).
6. Compilação com 0 erros de todos os projetos em Release.
7. Estruturação e detalhamento do `docs/ROADMAP.md`.
8. Documentação das diretrizes de infraestrutura dedicada e criação da memória de sessões em `mods/FIKA/memory/sessions.md`.
