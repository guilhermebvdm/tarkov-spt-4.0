# Memória de Sessões — Project FIKA

## Estado atual

> **Delta 2026-09-06 (Sessão 3):** Corrigida a rejeição `GClass1561` (`PlayerIsBusyError`) no swap de magazine 1-para-1 em arma empunhada via drag-and-drop em partidas coop com Headless dedicado (item de backlog `003-magazine-swap-inplace-fix`). Causa raiz: a própria troca de carregador colide consigo mesma no motor do EFT (não é concorrência real nem stub incompleto). Fix cirúrgico em `ObservedInventoryController.CheckItemAction` + novo patch de correlação `InOutHandsProcessTimestampPatch`. `Fika.Core.dll` v2.3.14, validado in-game pelo usuário — bug alvo não reaparece.
>
> **Delta 2026-09-03 (Sessão 2):** Implementação e validação do item `001-drop-backpack-sync-fix` (v2.3.11). Correção da trava e desync de descarte rápido de mochila ("ZZ" / `DropBackpack`) em instâncias de Headless/Host coop via patch Harmony `ObservedPlayer_DropBackpackSafety_Patch` em `Player.TryRemoveFromHands`. Atualização de `Fika.Core.dll` para v2.3.11 e recompilação de `Fika.Headless.dll` com 0 erros/avisos.
>
> **Delta 2026-09-02 (Sessão 1):** FIKA modded compilado com 0 Erros em todos os 4 módulos do ecossistema ([`mods/FIKA/modded/`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded/)). Concluído o ciclo de engenharia composto por: (1) Auditoria Diagnóstica da base original; (2) Fase B de Correções Cirúrgicas com integração dos patches do `TRL-Fixes` (#1 a #6); (3) Re-Auditoria Técnica Profunda; (4) Aplicação da 2ª Rodada de Refino; (5) Planejamento arquitetural no `docs/ROADMAP.md`; (6) Especificação de infraestrutura dedicada Headless; (7) 100% de contratos públicos preservados.

- **Módulos Compilados e Versionados:**
  - `Fika.Core.dll` (v2.3.11 — .NET Standard 2.1) $\rightarrow$ 🟢 0 erros / 0 avisos
  - `FikaServer.dll` (v2.3.6 — .NET 9.0) $\rightarrow$ 🟢 0 erros
  - `Fika.Headless.dll` (v1.4.16 — .NET Standard 2.1) $\rightarrow$ 🟢 0 erros / 0 avisos
  - `Fika.Headless.AssetNuker.dll` (v1.4.16 — .NET 9.0 win-x64) $\rightarrow$ 🟢 0 erros / 0 avisos
- **Documentação Técnica Integral:** Backlog formal iniciado (`001-drop-backpack-sync-fix`), 24 relatórios modulares e Roadmap preservados.

---

## Pendências

- [P-1.1] (aberta 2026-09-02) **VALIDAR IN-GAME a suite completa modded do FIKA** — Cenários a testar em sessão multiplayer: **(1)** Conexão cliente-servidor e movimentação sem jitter; **(2)** Mecânica de reviver verificando hitboxes pós-revive (TRL-Fixes #1); **(3)** Movimentação rápida de inventário com `Ctrl+Click` para validar auto-recuperação (TRL-Fixes #2); **(4)** Equipar arma com trilhos múltiplos tácticos (TRL-Fixes #4); **(5)** Entrada de bots em metralhadoras/lança-granadas montadas (TRL-Fixes #6); **(6)** Transição e retorno ao menu principal monitorando descarte de memória RAM; **(7)** [NOVO] Descarte rápido de mochila com "ZZ" e re-coleta no chão em raid multiplayer com Headless (Item 001). 🟡 Validação in-game.
- [P-1.2] (aberta 2026-09-02) **Implementação da Correção de Desync no Reconect (Ghost Body)** — Re-binding atômico de `ObservedPlayer` e reset de interpolação no Host conforme [`docs/ROADMAP.md`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/docs/ROADMAP.md) §1. 🟢 Feature / Fix.
- [P-1.3] (aberta 2026-09-02) **Implementação do Sistema de Senha Temporária para Raids** — Integração de validação de hash de senha no `FikaServer` e modal de input no `MatchMakerUIScript.cs` conforme [`docs/ROADMAP.md`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/docs/ROADMAP.md) §3. 🟢 Feature.
- [P-3.1] (aberta 2026-09-06) **Calibrar `GraceWindowSeconds`** — hoje fixo em `0.35f` (marcado `TODO confirmar` no código) em `ObservedInventoryController.cs`. Precisa de instrumentação temporária (log de `elapsedSeconds` real) em sessão Headless de verdade antes de considerar definitivo. 🟡 Débito técnico.
- [P-3.2] (aberta 2026-09-06) **Trilha B do item 003** — limpar `FikaActiveWeaponMagSwapPatch` em `mods/UIFixes/modded/src/Patches/SwapPatches.cs:891-924` (patch no alvo errado — `TraderControllerClass.CheckItemAction` do lado cliente — hoje inofensivo mas inútil pro Headless). Cross-ref: `mods/UIFixes/memory/sessions.md` P-8.1. 🟢 Ideia / limpeza.

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
