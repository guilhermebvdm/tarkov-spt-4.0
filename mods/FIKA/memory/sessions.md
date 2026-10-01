# Memória de Sessões — Project FIKA

## Estado atual

> **Delta 2026-09-14 (Sessão 10):** Item `010-join-in-progress-senha-lobby-headless` totalmente implementado, compilado em Release (0 erros) e registrado no fork `mods/FIKA/modded-V2/` (`Fika.Core` v2.4.0, `FikaServer` v2.4.0, `Fika.Headless` v1.5.0). Criados `04-code-review-01.md` e `05-asbuild.md`. Conclui a pendência histórica `P-1.3`.
>
> **Delta 2026-09-13 (Sessão 9):** Item `010-join-in-progress-senha-lobby-headless` criado e formalizado no backlog (`01-spec`, `02-spec-tech`, `03-spec-tech-review-01`). Criado fork `mods/FIKA/modded-V2/` a partir de `modded/` para implementar a feature com isolamento total. Todos os arquivos de documentação e specs foram reendereçados para `modded-V2`. Endereça a pendência histórica `P-1.3`.
>
> **Delta 2026-09-13 (Sessão 8):** Item `009-reconnect-ressincronizacao-corpo-inplace` implementado, compilado (v2.3.21) e validado em raid real com 3 jogadores. Resolve em definitivo a pendência `P-1.2`: corpo congelado no ponto da queda, invisibilidade por oclusão do EFT e descarte de snapshots de movimento pós-crash. Ressincronização autoritativa in-place via `ForceTeleport`, `ApplyVisibleState` e broadcast de `ClearSnapshotterPacket`. Fundação para a entrada em raid em andamento.
>
> **Delta 2026-09-11 (Sessão 7):** Item `006-colisao-maos-item-nao-carregador` despausado, reinvestigado e implementado.
>
## Pendências

- [P-10.1] **Validar in-game no fork `modded-V2` o Item 010 (Join In Progress e Senha)** — Testar criação de lobby com e sem senha (Host e Headless), invasão de raid pelo MatchMaker e pela lista de amigos do menu.
- [P-1.3] (resolvida 2026-09-14 na Sessão 10) **Implementação de Join In Progress (Invasão) e Sistema de Senha Opcional (Host & Headless)** — Implementação completa no fork `mods/FIKA/modded-V2/` com os 3 módulos (`FikaServer` v2.4.0, `Fika.Core` v2.4.0, `Fika.Headless` v1.5.0). ✅ Resolvido.
- [P-8.1] (resolvida 2026-09-13 na Sessão 8) **VALIDAR IN-GAME o item `009-reconnect-ressincronizacao-corpo-inplace` (v2.3.21)** — Validado com sucesso em raid real de Factory com 3 jogadores (Host Sivan, Convidado reconectado UmbigoPreto, Convidado observador Cherno). Re-ancoragem física e visibilidade sem corpo congelado nem invisibilidade confirmadas nos 3 logs. ✅ Resolvido.
- [P-1.2] (resolvida 2026-09-13 na Sessão 8) **Implementação da Correção de Desync no Reconect (Ghost Body)** — Resolvido pelo item 009 (v2.3.21) via ressincronização in-place (`ForceTeleport`), reset defensivo de timestamp em `PlayerSnapshotter` e broadcast de `ClearSnapshotterPacket`. ✅ Resolvido.

---

## 2026-09-14 02:45 (GMT-3) — Sessão 10: Implementação e Compilação de Join In Progress e Senha no fork `modded-V2` (Item 010)

**Tema central:** Implementação de código nos 3 submódulos do fork `modded-V2` (`Fika-Server-CSharp`, `Fika-Plugin/Fika.Core` e `Fika-Headless`), validação das compilações Release (0 erros) e formalização do as-built do Item 010.

**Decisões e entregas:**
- `FikaServer` v2.4.0: suporte a `Password` em DTOs, validação no `RaidController.HandleRaidJoin`, persistência em `MatchService` e exposição de `HasPassword` em `LocationController`.
- `Fika.Headless` v1.5.0: consumo de `request.Password` via WebSocket e repasse a `FikaBackendUtils.CreateMatch`.
- `Fika.Core` v2.4.0: campo de senha injetado em "CONFIGURAÇÕES DE SESSÃO" (`DediSelection`), criação do modal nativo reutilizável `JoinSessionModal`, desbloqueio do botão de invasão de raid em `MatchMakerUIScript` e `MainMenuUIScript`, liberação do pinger UDP e listener de rede em `FikaServer.cs`.
- Todos os binários gerados exclusivamente dentro do fork `mods/FIKA/modded-V2/` sem cópia automática para o jogo.

---

## 2026-09-13 21:40 (GMT-3) — Sessão 9: Especificação de Join In Progress (Invasão) e Senha Opcional para Host & Headless (Item 010)

**Tema central:** Formalização no backlog do sistema de entrada em raids em andamento (Join In Progress / Invasão) integrado ao controle de acesso por senha opcional para partidas cooperativas e dedicadas (FIKA Headless), com criação do fork de código `modded-V2`.

**Decisões-chave:**
- [Criação do Fork `modded-V2`]: criado `mods/FIKA/modded-V2/` a partir de `mods/FIKA/modded/` para abrigar o desenvolvimento da feature 010, mantendo `modded/` estável com a v2.3.21 do Item 009.
- [Campo de Senha na Janela 'CONFIGURAÇÕES DE SESSÃO']: no `MatchMakerUIScript`, dentro do painel `DediSelection` (onde ficam "Usar Host Headless", a seleção de headless e o botão "INICIAR"), adicionado o campo de texto para senha opcional. Vale tanto para Host local quanto Headless.
- [Janela Modal de Entrada Idêntica para Invasão]: criada janela modal (`JoinSessionModal`) estilizada identicamente à de "CONFIGURAÇÕES DE SESSÃO" (moldura escura, cabeçalho, botão X vermelho no canto superior direito e botão largo ENTRAR/INVADIR), acionada de forma unificada tanto pela tela de incursões (`MatchMakerUIScript`) quanto pela lista de jogadores online do menu principal (`MainMenuUIScript`).
- [Propagação de Senha no Headless via WebSocket]: mapeado que `MatchMakerUIScript` envia `StartHeadlessRequest` via POST `/fika/raid/headless/start` -> backend empacota via WebSocket para `FikaHeadlessPlugin.cs:OnFikaStartRaid` -> `BeginFikaStartRaid` repassa para `FikaBackendUtils.CreateMatch(...)` -> partida registrada no `MatchService` como protegida, sem necessidade de arquivos externos ou configurações em disco.
- [Desbloqueio de Invasão de Raid]: partidas com status `IN_GAME` deixam de ter botão desabilitado na UI. Botão exibe "Invadir Raid" (`UI_INVADE_RAID`). O pinger UDP responde `"fika.hello"` e o `OnConnectionRequest` aceita novas conexões para a raid ativa.
- [Aproveitamento do Item 009]: toda a sincronização física, oclusão da malha 3D e buffers do snapshotter utilizam a infraestrutura autoritativa in-place validada no Item 009.
- [Ciclo formal de backlog]: criados e atualizados `010-join-in-progress-senha-lobby-headless-01-spec.md`, `010-join-in-progress-senha-lobby-headless-02-spec-tech.md` e `010-join-in-progress-senha-lobby-headless-03-spec-tech-review-01.md`, todos reendereçados para `modded-V2`.

---

## 2026-09-13 20:45 (GMT-3) — Sessão 8: Implementação da correção definitiva do Reconnect (item 009, v2.3.20/v2.3.21)

**Tema central:** Solução estrutural do dessync de Reconnect no FIKA (corpo congelado no ponto da queda e invisibilidade na nova posição), preservando integralmente o inventário e integridade do jogador em raid. Resolução da pendência histórica `P-1.2`.

**Decisões-chave:**
- [Abordagem In-Place obrigatória]: vetada qualquer destruição e recriação do `ObservedPlayer` na desconexão/reconexão. O `ObservedPlayer` no Host é a autoridade da raid para inventário coletado, danos em placas e saúde; destruí-lo causaria reset do perfil. A solução opera 100% in-place na entidade ativa.
- [Auto-recuperação de Snapshotter]: detectado que `NetworkTimeSync.NetworkTime` usa `Time.unscaledTimeAsDouble` (tempo desde boot do processo). Um crash/restart reinicia o relógio em `~0s`, fazendo com que `PlayerSnapshotter.AddSnapshot` descarte todos os pacotes seguintes por `snapshot.RemoteTime <= newestTime`. Implementada auto-recuperação defensiva quando `snapshot.RemoteTime < newestTime - 5.0d`, invocando `Clear()` antes de reinserir.
- [Fix da armadilha de Culling do EFT]: em `ObservedPlayer.ManualStateUpdate`, `!_cullingHandler.IsVisible` atualizava apenas `Position` (`PlayerBones.BodyTransform.position`), deixando a raiz `Transform.position` presa no ponto da queda e induzindo `LocalPlayerCullingHandlerClass` a forçar `forceRenderingOff = true`. Sincronizado `Transform.position = CurrentPlayerState.Position` sob oclusão e implementado `ForceTeleport` com `base.Teleport` e `ApplyVisibleState()`.
- [Broadcast do pacote]: `ClearSnapshotterPacket` expandido com `Position` e `Rotation`, com retransmissão via broadcast confiável (`ReliableOrdered`) do Host para os outros clientes.
- [Ciclo formal de backlog]: criados `01-spec.md`, `02-spec-tech.md`, `03-spec-tech-review-01.md`, `04-code-review-01.md` e `05-asbuild.md` em `mods/FIKA/backlog/009-reconnect-ressincronizacao-corpo-inplace/`. `Fika.Core.dll` bumpado para v2.3.20. Compilação Release validada com 0 erros.

**Atividade cronológica:**
1. Mapeamento das 3 causas raízes cruzando Assembly do EFT e código do FIKA.
2. Elaboração e revisão do plano e ciclo de backlog.
3. Implementação dos métodos e patches em `Fika.Core`.
4. Compilação Release local com 0 erros.
5. Formalização dos artefatos de backlog e atualização do índice canônico.

**Pendências abertas nesta sessão:**
- `P-8.1` aberta para validação in-game em sessão cooperativa.
- `P-1.2` marcada como resolvida.

---

## 2026-09-11 06:08 (GMT-3) — Sessão 7b: Validação in-game bloqueadora confirmada — item 006 fechado no cenário principal

**Tema central:** Fechamento do ciclo do item `006` — usuário testou o fix em raid Headless real com o build produzido na Sessão 7.

**Decisões-chave:**
- [Fix confirmado em condições reais]: usuário reproduziu o cenário exato do bug original (municiar vários carregadores em sequência via `SPT-ContinuousLoadAmmo`, incluindo cancelar no meio e esvaziar/reencher um por um) e **nenhuma trava ocorreu em nenhum ciclo**. `HandsBookkeepingGraceWindowSeconds` (2.5s) se mostrou suficiente nos testes feitos, embora continue não-calibrado por instrumentação (`P-7.2` permanece aberta).
- [Pendência rebaixada, não fechada por completo]: `P-7.1` (validações não-bloqueadoras — concorrência genuína entre jogadores, corner case de cura, cenário `GEventArgs9`) rebaixada de 🔴 bloqueador pra 🟢 ideia/validação opcional. O item já cumpre seu objetivo principal; o resto é reforço de cobertura, não condição pra considerar o item entregue.

**Lições / hipóteses descartadas:**
- Nenhuma lição nova — sessão de confirmação/fechamento, sem descoberta técnica adicional além do que a Sessão 7 já mapeou.

**Atividade cronológica:**
1. Usuário testou em raid real e reportou sucesso completo, sem reprodução do bug.
2. Checklist da spec técnica (`006-...-02-spec-tech.md` §8), `05-asbuild.md` e memória atualizados para refletir a validação.

**Pendências abertas nesta sessão:**
- Nenhuma nova — `P-7.1` rebaixada (ver acima), `P-7.2` (calibração da janela de graça) permanece como estava.

**Cross-refs:**
- Continuação direta da Sessão 7 (mesmo dia, mesmo item) — ver decisões-chave lá para o desenho completo do fix.

---

## 2026-09-11 05:45 (GMT-3) — Sessão 7: Despausa e implementação do item 006 — causa raiz real era GEventArgs9/10, não GEventArgs17

**Tema central:** Resolver a pausa deixada pela Sessão 6 (`P-6.1`): confirmar ou refutar, com evidência empírica, a causa raiz assumida do bug de trava de mãos no `SPT-ContinuousLoadAmmo` — e, a partir do resultado, redesenhar e implementar o fix do item `006-colisao-maos-item-nao-carregador`.

**Decisões-chave:**
- [Hipótese original refutada por evidência empírica, não por leitura estática apenas]: usuário compilou `Fika.Core` em Debug (build local, CRC32 replicado nos dois lados — cliente e Headless — pra passar da checagem de compatibilidade de versão do FIKA, `FikaBackendUtils.cs:205`), reproduziu o bug em raid Headless real municiando vários carregadores em sequência, e capturou o log do Headless no momento exato da rejeição: `"item was same as GEventArgs2.Item" / "Flag hit, gevent was GEventArgs10"`. O log `"failed inOutHandsProcess check"` (que confirmaria a hipótese da Sessão 6) **não apareceu em nenhuma tentativa**. Ref: `006-colisao-maos-item-nao-carregador-03-spec-tech-review-01.md` PA-01-01.
- [Causa raiz real mapeada]: `GEventArgs10` ("BeginRemoveFromHands", alias 4.1 `RemoveFromHandsEventArgs`) é levantado por `Class1312`/`method_138` (`Player.cs:32383-32393`). `FirearmController.Drop` (`Player.cs:13506-13524`) chama `method_138(weapon)` **antes** de `HideWeapon(...)` começar e só confirma **dentro** do callback de conclusão — existe uma janela real do tamanho da animação de guardar a arma. `SetEmptyHands` (usado pelo `ContinuousLoadAmmo` entre cada carregador) cai exatamente nesse caminho via `Proceed`/`Process<>`/`DropCurrentController`. `ObservedFirearmController` não sobrescreve `Drop` — o Headless roda o mesmo código, com seu próprio timer sujeito a latência de rede. Cliente local avança pro próximo carregador com base no seu próprio timing, sem saber se o Headless já confirmou o `GEventArgs10` anterior.
- [Fix generalizado em duas partes, sem tocar a infraestrutura do item 003]: **Fix 1a** — desenho original da Sessão 6, mantido (generaliza `IsSelfReferentialHandsTransition` removendo restrição a `MagazineItemClass`; cobre o corner case cura→faca/granada/troca-de-arma, que passa por `GEventArgs17`, não pelo `GEventArgs10`). **Fix 1b** (novo) — `HandsBookkeepingTimestampPatch.cs`: Postfix em `TraderControllerClass.method_19(GEventArgs1)` (hub confirmado de Add/Remove de `List_0` pra todos os tipos de evento — `RaiseInOutProcessEvents` e `RaiseEvent(GEventArgs13)` já delegam pra ele), filtrando `GEventArgs9`/`GEventArgs10`, correlaciona `(controller, item, timestamp)` num `ConditionalWeakTable` independente. `ObservedInventoryController.CheckItemAction` ganha tolerância no bloco genérico `item == geventArgs2.Item` quando o evento colidente é um `GEventArgs9`/`GEventArgs10` recente do mesmo jogador no mesmo item — sem risco de concorrência real (`List_0` é por jogador, mesma garantia estrutural do item 003/004).
- [3 erros auto-corrigidos durante a própria redação da spec técnica, antes de qualquer rodada de review]: (1) a janela de graça do Fix 1b foi inicialmente escrita reaproveitando `GraceWindowSeconds` (0.35f) do item 003/004 — errado, aquela constante limita a sobreposição quase instantânea de duas metades de UM swap de carregador, enquanto o Fix 1b precisa cobrir uma animação inteira de guardar arma (tipicamente > 1s); corrigido pra uma constante própria (`HandsBookkeepingGraceWindowSeconds`, 2.5f, `P-7.2`). (2) `using Fika.Core.Main.Utils;` faltando no novo arquivo de patch pro `FikaGlobals.LogError` compilar. (3) **achado mais importante**: `GetTargetMethod()` retornando `null` (o design defensivo previsto pra alvo obfuscado não encontrado) na verdade **lança uma exceção** dentro do `Enable()` do `ModulePatch` — verificado por investigação dedicada em `references/spt-source/Libraries/SPTarkov.Reflection/Patching/AbstractPatch.cs` (sucessor renomeado da mesma lib), não suposição. Registrar o patch via `_patchManager.EnablePatch(...)` sem `try/catch` no call site (o padrão usado pelos 2 patches do item 003) faria essa exceção abortar o `Awake()` inteiro do plugin. Corrigido: registro do Fix 1b em `FikaPlugin.cs` envolvido em `try/catch` próprio.
- [Ciclo formal completo, incluindo uma 2ª rodada de review pro desenho reescrito]: como a review 01 invalidou o desenho original, a spec técnica foi inteiramente reescrita e passou por uma **nova** rodada de review (02) antes do `/code-mod` — não se assumiu "já revisado" só porque o item já tinha uma review 01 (que validava um desenho já superado). Review 02 encontrou 2 pontos menores (extrapolação não-testada de `GEventArgs9` por simetria; checklist sem variação de ritmo) — ambos resolvidos com adição de itens de validação in-game, sem mudança de código.

**Lições / hipóteses descartadas:**
- Hipótese da Sessão 6 ("`SetEmptyHands`/`TrySetLastEquippedWeapon` colidem via `GEventArgs17`/`inOutHandsProcess`, mesmo mecanismo dos itens 003/004") — descartada por evidência empírica direta (log dirigido em raid real), não só por leitura estática. Reforça a disciplina da Sessão 6 de pausar antes de codar sem confirmação: implementar o fix original teria "fechado" o item sem resolver o bug relatado.
- Hipótese "retornar `null` de `GetTargetMethod()` é um design defensivo que falha graciosamente por si só" — descartada por investigação dedicada: falha, sim, mas lançando exceção — "gracioso" só se o call site do registro tiver `try/catch` próprio. Lição promovível: nenhum patch existente neste mod (nem em outros mods do repo, verificado por agente de busca) tinha precedente de `GetTargetMethod()` retornando `null` — é a primeira vez que esse padrão defensivo é usado no ecossistema FIKA, e ele exige proteção também no *call site* de registro, não só dentro do próprio método.

**Atividade cronológica:**
1. Usuário reportou erro real de jogo (`[HandleCallbackResponse]`) e log do Headless; investigação inicial (antes de qualquer teste dirigido) já suspeitou de conexão com o item 006 pausado.
2. Preparado build Debug local (`dotnet build -c Debug`, sem `/compile-mod`) e confirmado por leitura de bytes que a string de diagnóstico estava de fato compilada.
3. Usuário reportou erro de versão do FIKA ao tentar usar builds diferentes nas duas pontas (cliente Release, Headless Debug) — investigado e confirmado: checagem de CRC32 do arquivo inteiro (`FikaModHandler.cs:50`, `FikaBackendUtils.cs:205`), não a string de versão semântica. Resolvido copiando a mesma build Debug pros dois lados.
4. Usuário reproduziu o bug em raid real e capturou o log do Headless no momento da falha — log dirigido apontou `GEventArgs10`, não `inOutHandsProcess`.
5. Rastreamento completo do mecanismo real no Assembly (`Class1312`/`method_138`/`FirearmController.Drop`/`SetEmptyHands`/`DropCurrentController`), incluindo confirmação de que `ObservedFirearmController` não sobrescreve `Drop`.
6. Spec técnica do item 006 reescrita do zero (Fix 1 dividido em 1a+1b); review técnica 02 rodada e resolvida.
7. Investigação dedicada (agente em background) sobre comportamento de `ModulePatch.GetTargetMethod() == null` — achado incorporado à spec antes do `/code-mod`.
8. `/code-mod`, `/code-review` (1 achado cosmético, aplicado), build local verificado (0 erros), `check-packet-hashes.js` limpo. `Fika.Core.dll` v2.3.17 em `mods/FIKA/builds/`.

**Pendências abertas nesta sessão:**
- [P-7.1] Validar in-game o item 006 (checklist completo, incluindo teste bloqueador de ritmo variável). Categoria: 🔴 bloqueador (AP-06).
- [P-7.2] Calibrar `HandsBookkeepingGraceWindowSeconds` (hoje 2.5f, não medido). Categoria: 🟡 débito técnico.

**Cross-refs:**
- Resolve `P-6.1` (aberta Sessão 6, 2026-09-10).
- Trabalho paralelo relacionado no mesmo dia: ver `mods/SPT-ContinuousLoadAmmo/` (mod gatilho do bug, não modificado nesta sessão — o fix é inteiramente do lado FIKA).

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
