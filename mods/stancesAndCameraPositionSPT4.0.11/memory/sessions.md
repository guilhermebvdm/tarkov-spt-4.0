# Memory — stancesAndCameraPositionSPT4.0.11

Memória cronológica de sessões de chat (timestamps em GMT-3, aproximados quando não puderem ser inferidos com precisão). Cada entrada resume o que foi feito, decisões-chave, bugs encontrados, e estado pendente. Atualizada ao fim de cada sessão de trabalho.

> Por que existe: o usuário trabalha múltiplos chats em paralelo. Este arquivo evita que cada chat reabra do zero — futuras sessões podem carregar contexto ao ler as últimas entradas. Entradas são ordenadas por timestamp GMT-3; chats paralelos podem aparecer interleaved.

## Estado atual (snapshot ao fim da última sessão)

- **Fork ativo = `modded` (CANÔNICO desde 2026-07-09).** Reorg: `git mv modded-beta → modded` e antigo `modded → modded-bak` (backup, não editar). Build **self-contained** (`/compile-mod` OU `dotnet build`; csproj puxa `Fika.Core` da raiz `references/`, sem `mods/references/` temp). **Deploy manual** do DLL em `D:/SPT/BepInEx/plugins/RealisticMobility/` (assets `.ogg`/`.png` ao lado; `/compile-mod` instala em `plugins/<AssemblyName>/`, então copiar à mão). **DLL atual: v2.17.0** — instalada em `D:/SPT` (pasta `TRL-StancesAndMobility/`; a antiga `RealisticMobility/` foi movida para `D:/SPT/_backup-RealisticMobility-2026-08-02/`) (build local; **não deployada** — o `modded/` mudou depois da última build, rebuildar antes de copiar). **Nem `D:/SPT` nem o servidor têm a 2.13.0/2.14.0** ([P-11.7]). Trilha desde a 2.5.0: 2.8.1 (`47d30935`, fix de colisão de `Order`) → 2.8.2 (`316b6581`, ADS Waypoint no rodapé) → 2.9.0 (`8c9ae609`, 30 configs promovidas a default + item 018 backlog) → 2.10.0 (`ed9cf500`, item 019 chamber-check ammo UI) → **2.11.0** (`22ae6176`, 26/07, envelope de comprimento no pacote de stance do FIKA — release lockstep) → **2.12.1** (`bada9d2a`, 27/07, chamber-check UI refeita via `EftBattleUIScreen` — a v1 resolvia o assinante sem erro e o painel nunca desenhava) → **2.13.0** e **2.14.0** (Sessão 12). ⚠️ **Não existiu v2.12.0**: o `PickupAimingSafetyPatch` entrou em 25/07 (`19aa6499`) sem release próprio e foi devolvido ao TRL-Fixes na 2.13.0. ⚠️ **As versões 2.11.0–2.12.1 não passaram por `/update-memory` em tempo real** — a Sessão 14 preencheu o vazio
retroativamente (gravada em 02/08, trabalho de 26-27/07): a narrativa completa da depuração do item 019
(por que a v1 do chamber-check-UI não desenhava, e o fix via `EftBattleUIScreen`) está lá. Ver Sessão 12, Sessão
14 e memória global `reference_stances_canonical_build`.
- ⚠️⚠️ **EIXOS DA ARMA SÃO LOCAIS, NÃO OS DO UNITY** (fix v2.2.0, commit `d9069fb`). A rotação é aplicada como `weapRotation * Quaternion.Euler(euler)` (`ApplyComplexRotationPatch:280`) → **espaço local da arma**: `X = lateral · Y = LONGITUDINAL (o cano) · Z = vertical`. Portanto **girar em torno de Y = TOMBAR (roll)** e **em torno de Z = APONTAR (yaw)** — o **contrário** da ordem canônica do Unity `(pitch, yaw, roll)`. A montagem correta é `new Vector3(pitch, roll, yaw)`. **Nunca presumir a convenção do Unity aqui.**
- **VERSÃO 2.5.0 (2026-07-14, commit `17f9d02`) — DLL `397b3c3`.** Trilha da sessão: 2.0.0 (`39e7a56`) → 2.1.0 (`ca9f868`) → 2.2.0 (`d9069fb`) → **2.2.1 hotfix** (`4936e8f`) → 2.3.0 (`b477c21`) → 2.4.0 (`8fc8f8e`) → 2.5.0 (`17f9d02`).
- ⚠️⚠️ **O BepInEx PROÍBE `=` no nome de uma key** (é o separador do `.cfg`) — `Config.Bind` **lança** e **aborta o `Awake`**. Também proibidos: `[ ] " ' \ tab`. Isto derrubou a v2.2.0 (ver Sessão 10).
- ⚠️ **ORDEM DO `Awake` (v2.3.0):** `BindAllConfig()` **ANTES** de `EnableEverything()`. Antes era o contrário e um bind que lançasse deixava os ~35 patches VIVOS com `ConfigEntry` null → NRE por frame na raid. Hoje um bind ruim → 1 log `[BOOT]`, `ConfigReady=false`, **nenhum** patch aplicado (jogo roda vanilla). `Plugin.Update` e `PassiveMountUI.Update` checam `ConfigReady` (o Unity chama MonoBehaviour mesmo com Awake abortado).
- **A ORDEM dos `Config.Bind` entre si é a ordem das SEÇÕES no F12** (o ConfigurationManager usa ordem de descoberta). Não reordenar sem querer. A Stance 0 é bindada cedo de propósito (`Plugin.cs:616`, antes da Stance 1).
- **F12 hoje: 19 seções · 123 opções** (apurado do código em 2026-08-02, v2.15.0 — método repetível descrito no cabeçalho do `PROPRIEDADES.md`). A `Action Stances` foi absorvida pelo rodapé de `Stance Cycle & Hotkeys` na 2.13.0 e a `Weapon Inspection` nasceu na 2.10.0: uma saiu, uma entrou. ⚠️ **As tabelas por seção do `PROPRIEDADES.md` continuam da v2.5.0** — cabeçalho e ordem estão corretos, as tabelas ainda precisam de regeneração ([P-12.3]).
- ⚠️ **`HandsContainer` É um `PlayerSpring`** (`ProceduralWeaponAnimation.cs:211` do decompilado) — os dois caminhos que o mod usa para o offset de câmera escrevem **o mesmo campo do mesmo objeto**, e **nada no EFT reescreve `CameraOffset`** (busca por escrita volta vazia no decompilado inteiro). Por isso o `PlayerSpringPatch` (Postfix de `PlayerSpring.Start`) cobre toda raid, e o cache `_cameraOffsetDirty` serve só para refletir mudanças do F12 ao vivo. **Não "consertar" isso** — foi a dúvida do `MP-02-05`, resolvida com o decompile completo que a review 02 não tinha.
- ⚠️ **Todo `float` com faixa exatamente `0–1` é renderizado como PORCENTAGEM** pelo ConfigurationManager, sem caixa de digitação — um tempo de `0.15 s` aparece como "15%". Alargar a faixa (ex.: `0–2`) devolve o valor real e a caixa. Corrigido em 2 props na 2.13.0 (`MP-02-06`).
- **`assets/config/com.shwng.fpscamerastances.cfg`** = a config CALIBRADA do servidor, versionada. **DLL e `.cfg` são um PAR** — desde a 2.0.0 as keys foram renomeadas, então DLL nova + cfg velho = o BepInEx não casa nada e reseta tudo. Distribuir via **`config-server`** do launcher (espelho, sobrescreve), **nunca** `config` (seed-if-missing: quem já tem o cfg antigo não receberia nada). Antes: 2.0.0 (commit `39e7a56`, bump `1.3.1 → 2.0.0`). A versão vive em **dois** lugares que precisam bater: `Plugin.cs` (`BepInPlugin` — é o que o F12 mostra) e `.csproj` (`Version`/`AssemblyVersion`/`FileVersion` — antes ausente, a DLL saía como `1.0.0.0`). Changelog do fork em **`modded/CHANGELOG.md`** (o `CHANGELOG_SIMPLIFIED.md` é do upstream e para na 1.1.4).
- **F12 hoje: 20 seções · 113 opções** (2.1.0). Era 21 · 120 na 2.0.0 — a review 02 achou e removeu **7 props fantasmas**.
- ⚠️ **A 2.0.0 reseta a config salva do usuário** (renome de seção/key na reorg do F12 — o BepInEx casa por `(seção, chave)` literal). A 2.1.0 **não** renomeia nada; só remove props (as entries removidas ficam órfãs no `.cfg` e são ignoradas).
- ⚠️ **A DLL instalada estava DEFASADA até 2026-07-11.** A instalada era de 11/07 00:53 (pré-reorg do F12); a build com a reorg é de 03:38. **Ou seja: a validação in-game da Sessão 7 rodou sobre código SEM a reorg do F12** — os 23 props mortos e 9 campos órfãos removidos **nunca rodaram no jogo**. O handoff de 2026-07-11 afirmava (errado) que a instalada era a `c83ed42`. Lição: **conferir o hash da DLL instalada contra a do repo**, não confiar no que a memória/handoff diz estar instalado.
- **BACKLOG INTEIRO VALIDADO IN-GAME (2026-07-11).** Todos os itens 🟢 no `mod-backlog.md` — 001/002/003/005/006/007/008/009/010/011/012/013/014/015. Único 🔴: **004** (mount próprio cancelado → substituído pelo 011). O 014 substitui o 006; o 011 substitui o 004.
- **014 (sync Fika) — VALIDADO:** o **fix-03** aplica o offset num **Postfix de `PlayerBones.ShiftWeaponRoot`** (janela **pré-IK**) → **braço E arma** acompanham juntos. Antes: fix-02 (Postfix de `ObservedVisualPass`) rodava **pós-IK** e movia **só a arma**.
- **015 (bloqueio de mount ativo) — VALIDADO:** `BlockActiveMountPatch` (Prefix de `Player.TryMountWeapon`) **impede** o mount vanilla em Stance 1/2/3; permitido em Stance 0, ADS e prone. **Bipé não é afetado** (usa `IsBipodUsed`, não `IsMountedState`). **Desmontar** ao trocar de stance seria **código morto** — o 013 já força Stance 0 enquanto montado (`StanceManager.Update` L169-180).
- **F12 reorganizado (2026-07-11):** **21 seções · 120 props** (eram 23 seções / 143 props — **23 propriedades mortas removidas**). Nomes de seção em inglês; **tooltips bilíngues** (inglês em cima, português embaixo, 1 linha em branco entre eles — `"<EN>\n\n<pt>"`); eixos Roll/Yaw (rótulos estavam trocados) corrigidos. **Renomes = breaking change**: a config salva do usuário reseta ao default. Relatório: `PROPRIEDADES-review-01.md`.
- **Stance layout:** 0 Vanilla · 1 High Ready (Pitch -15) · 2 Low Ready (Pitch +30) · 3 Custom (Yaw -30).

## Pendências / próximos passos conhecidos

- **[P-13.1] (aberta 2026-08-02) 🔴 Validar in-game a pilha 2.13.0→2.17.0** — cinco versões sem nenhuma raid, incluindo a troca de identidade. No F12: nome `TRL-StancesAndMobility` e **os valores calibrados do usuário**, não os defaults. Em raid: posturas, mira, mount, recarga, checagem de câmara, seção `Camera Position`. ⚠️ Erro novo no console provavelmente é **antigo** — o laço protegido (2.15.0) revela exceção que antes cancelava os subsistemas em silêncio.
- **[P-13.3] (aberta 2026-08-02) 🔴 O launcher precisa REMOVER `plugins/RealisticMobility/` ao atualizar**, não só criar `plugins/TRL-StancesAndMobility/`. Sem isso o jogador carrega **dois plugins de postura ao mesmo tempo** — é o único risco de dano real da renomeação. Vale para os beta testers e para o servidor. O `.cfg` novo tem que sair pelo canal `config-server` (sobrescreve), nunca pelo `config` (só cria quando falta).
- **[P-13.2] (aberta 2026-08-02) 🟡 Renomeação estrutural pendente:** namespace `CameraRotationMod` → `TarkovRedLine.StancesAndMobility` (39 arquivos) e pasta do repo `mods/stancesAndCameraPositionSPT4.0.11/` → `mods/TRL-StancesAndMobility/`. Invisível ao jogador; a pasta quebra caminhos de memória/backlog/grafos/harness e exige checkout sem sessão paralela. Ver `publish/RENAME.md`.
- **[P-12.1] (aberta 2026-08-01) 🟡 Calibrar a compressão de ADS-speed (item 017 F3) com o overlay novo.** A compressão **aplica** (mecanismo confirmado no decompilado), mas o **pivô default 1.5 está acima da faixa real das armas** — derivando de `globals.Aiming` (peso 0,6–9 kg; tempos 0,35–2,4 s), a velocidade interna vai de ~0,57 (LMG) a ~1,9 (pistola), fuzis em ~1,0. Com pivô 1,5 a compressão **acelera as pesadas** em vez de segurar as leves, e o usuário não sentia diferença. Centro real ≈ **1,0–1,1**. Ligar `Debug ADS Speed` (F12 → `Debug (Advanced)`), anotar os valores das armas usadas, escolher o pivô e **promover a default** (como foi feito na 2.9.0). Ver Sessão 12.
- **[P-12.2] (aberta 2026-08-01) 🔴 Preparação para publicação no SPT Forge — portão de elegibilidade.** (a) ✅ **Licença RESOLVIDA (2026-08-02): fica a CC BY-NC 4.0 do original** — decisão do usuário. A autorização do `shengzhanzhe` para publicar o fork está **registrada** (print de 2026-06-13 transcrito em `publish/PERMISSION.md`; PNG a anexar ao lado). Manter a mesma licença dispensa qualquer permissão adicional. ⚠️ Risco declarado: o Forge §6.1 situa CC como apropriada a doc/arte, não a código — não proíbe, mas a moderação pode questionar; a saída seria uma mensagem curta pedindo aval para licença OSI. **Não reabrir esta decisão sem motivo novo.** (b) **Política de IA**: declarar (o Forge recusa mod "substantially or entirely written by AI coding agents" e exige o flag "Contains AI Content" com qualquer uso de LLM). (c) **Origem dos assets** (5 `.png` + o `.ogg` do hold-breath). Rodar `/prepare-mod-for-publish` para a auditoria formal. Ver Sessão 12 e skill `trl-mod-publishing`.
- **[P-12.3] (aberta 2026-08-01) 🟡 Regenerar o `PROPRIEDADES.md` inteiro.** O documento se declara como da v2.5.0 e recebeu só remendos desde então; a contagem de seções/opções não bate com o jogo. Fazer junto do `/review-mod-properties` da preparação para publicação.
- **[P-11.7] (aberta 2026-07-25, atualizada 2026-08-01) 🟡 Subir a versão corrente ao servidor** via `config-server` do launcher (DLL + `.cfg`). O gap cresceu: servidor está na 2.8.0, o código na **2.17.0**. O `.cfg` versionado já tem o `Enable Action Stance Swap` na seção nova — distribuí-lo junto é o que impede o reset dessa opção para quem atualizar.
- **[P-11.1] (aberta 2026-07-15) 🟡 PARCIALMENTE RESOLVIDA (2026-08-01) — a velocidade fica presa devagar.** Sintoma: às vezes, andando normal (fora de postura), a velocidade **não volta ao máximo** e o personagem anda devagar sem motivo; **mirar (ADS)** ou trocar de postura e voltar destrava. ⚠️ **RELATO DE TERCEIRO — o dono do servidor NUNCA sofreu o sintoma** (esclarecido em 2026-08-01). As entradas anteriores desta memória atribuíam o relato, o workaround e o gatilho "voltar do agachado para em pé" ao usuário; **é repasse de outro jogador**, não observação direta. O requisito "cercar os dois regimes de caminhada (andar lento E andar normal)" veio da hipótese hoje refutada — tratar como pista, não como especificação.
  ⏸️ **Estado: aguardando reprodução.** O instrumento existe (v2.14.0, ver abaixo); **não investigar mais sem dado novo**. Quem reproduzir traz a leitura da tela/log e a pendência volta a andar. Para a publicação: **não é bloqueio** — é relato não confirmado, sem reprodução conhecida, com contorno trivial (mirar). Se for declarar na página do mod, declarar como possível limitação, não como bug confirmado.

  ❌ **A HIPÓTESE ORIGINAL ESTÁ REFUTADA (2026-08-01, Sessão 12).** Era: "o `target = fraction * mc.MaxSpeed` é calculado num instante e não é re-aplicado quando `MaxSpeed` muda; agachado o `MaxSpeed` é menor, ao levantar o cap fica preso no valor do agachado". **Três fatos do decompilado derrubam isso:**
  1. **`MaxSpeed` NÃO depende da pose** — é `Evaluate(BackendConfig.WalkSpeed, Strength/60)` (`MovementContext.cs:910`), função só do backend e da skill. Agachar/levantar não altera o valor de que o mod deriva o teto.
  2. **O recálculo não é preguiçoso** — `ProcessSpeedLimits` roda **todo frame** em `ManualUpdate` (`MovementContext.cs:2499`), e o mod ainda re-aplica por conta própria a cada tick (`StanceManager.EvaluateProneSuspensionTick`, com tolerância de 0.001).
  3. **Unidade:** os limites são **fatores normalizados** (o efetivo é o MENOR do dicionário `SpeedLimits`, default `1f` — `MovementContext.method_4`), não m/s. `WalkSpeed` no globals é `{x:0.625, y:0.717}`.

  🔍 **Hipótese vigente:** o EFT escolhe o teto pelo **menor** valor do dicionário `MovementContext.SpeedLimits` (causas: `BarbedWire`, `HealthCondition`, `Aiming`, `Weight`, `SurfaceNormal`, `Swamp`, `Shot`, `Armor`, `Fall`, + a do mod `9001`). O sintoma — devagar sem motivo, e **mirar destrava** — é compatível com **uma causa que continua registrada quando já deveria ter sido removida**; mirar/desmirar chama `SetAimingSlowdown` → `RemoveStateSpeedLimit`, o que forçaria a limpeza. Candidata a investigar: `Aiming` presa por um fluxo interrompido no meio (o `PickupAimingSafetyPatch`, hoje no TRL-Fixes, existe justamente porque o setter de `IsAiming` lança em certos caminhos).

  🛠️ **Instrumento entregue na v2.14.0:** opção `Debug Speed Limits` no F12 → mostra todas as causas ativas, qual vence, e **loga uma linha a cada troca de vencedora**. **O que fazer quando alguém reproduzir:** pedir a leitura da tela (ou a linha `[SpeedLimit]` do log em `user/logs/spt/`) no momento do sintoma — a causa com o menor valor é a resposta, e o fix decorre dela. Sem esse dado, qualquer correção é chute (já custou uma hipótese inteira). Relacionado: P-8.3 (o teto da Stance 0 é intencional — o alvo é a causa presa, não o cap).
- ✅ **[P-7.1] e [P-8.1] RESOLVIDAS (2026-08-01)** — o usuário confirmou que a revalidação em raid pós-reorg do F12 e os 4 fixes da 2.1.0 estão OK. Eram dívida de **registro**, não de teste: 8 versões se passaram desde então e o mod rodou em raid o tempo todo.
- ✅ **[P-8.2] parcialmente resolvida (2026-08-01)** — **MP-02-05 era FALSO POSITIVO** (prova abaixo), **MP-02-06 e MP-02-08 aplicados na 2.13.0**, MP-02-07 já era "manter". Sobra só o que está em [P-10.2]. Ver Sessão 12.
- **[P-8.3] ✅ RESOLVIDA (2026-07-12) — o cap de velocidade da Stance 0 é INTENCIONAL.** O usuário confirmou: *"tudo bem para a multiplicação de tudo do walk e das stances speed"*. A Stance 0 aplica cap de 90% fora de postura e **compõe** com o `Walk Speed Multiplier` (0.85) — é o comportamento desejado, não um bug. ⚠️ **Não "otimizar" isso.** (O comentário `// Stance 0: irrelevante` em `Plugin.cs:47` continua **falso** e ainda deve ser corrigido — parte do `MP-02-10`.)
- **[P-8.4] ✅ RESOLVIDA (2026-07-14) — eixos validados in-game.** `Yaw` aponta, `Roll` tomba, poses preservadas pela migração do `.cfg`. (Texto original abaixo.)
- **[P-10.1] (aberta 2026-07-14) 🟢 Achados do code-review NÃO aplicados** (`CODE-REVIEW-v2.2.1.md`): **CR-05** (o aparato de `Browsable`/ConfigurationManager virou no-op mas ainda força rebuild do F12 a cada mudança do scroll mode) e **CR-07** (`Plugin.Update` sem try/catch — uma exceção derruba o resto do tick, todo frame; se aplicar, com log **rate-limited**).
- **[P-10.2] (aberta 2026-07-14, reduzida 2026-08-01) 🟡 O que sobrou da review 02:** **MP-02-09** (código morto: `CameraBobbingScript` nunca instanciado, `PlayerSpringPatch._cameraOffsetField` resolvido e nunca usado, `FixedUpdate` vazio, `ApplySimpleRotationPatch` com `damping=12f` fixo ignorando a prop) e **MP-02-10** (o comentário `// Stance 0: irrelevante` em `Plugin.cs:47` é falso). Ambos entram na faxina de código da preparação para publicação — ver [P-12.2].
- ~~**[P-8.4]** (aberta 2026-07-12) Validar in-game a v2.2.0 (`a22d368`) — fix dos eixos:~~ `Yaw` deve **APONTAR** esq/dir e `Roll` deve **TOMBAR** a arma, nas 3 stances **e no ADS** (antes faziam o contrário). Conferir também que **as poses ficaram iguais às de antes** — o `.cfg` foi migrado (valores `Yaw`↔`Roll` trocados) justamente para preservar o visual; se alguma stance parecer diferente, a migração errou. Backup: `cfg.bak-pre-v220`.
- **[P-7.2] (aberta 2026-07-11) 🟢 Dívida técnica** (herda a antiga P-5.3): unificar a interpolação em `SpringMath.SpringDamp`, eliminar a reflection que roda a cada frame, `try/catch` nos ~19 patches restantes (só os 6 do Manual Chambering têm), auditar o reset de estado estático entre raids. Adiada porque mexe em código de câmera **já validado** — risco > valor até surgir bug real.
- **[P-7.3] (aberta 2026-07-11) 🟢 Dívida da revisão do F12** (achados adiados do `PROPRIEDADES-review-01.md`): reordenar as seções (**MP-01-03** — os binds de uma mesma seção estão espalhados pelo `Awake`, ex.: Stance 2 em L766 **e** L1184; reordenar arriscaria quebrar um arquivo de 1700 linhas já validado), rever onde ficam as opções de velocidade (**MP-01-08**) e se a seção da Stance 0 se justifica (**MP-01-10**).

## 2026-05-09 ~16:00 (GMT-3) — Sessão 1: item 002 backlog (criação + reviews)

Tarefas executadas neste dia (em ordem):

1. **Criação do item 002** via `/add-backlog-item stancesAndCameraPositionSPT4.0.11 "Ciclo linear, hotkeys e snap fogo"`. Item registrado em `mod-backlog.md`.
2. **Spec funcional** (`/create-spec`) com 5 features: F1 Include Stance 0 in Cycle, F2 Mouse Wheel Scroll Mode (Cycle/Linear), F3 hotkeys dedicadas por stance, F4 Snap to Stance 0 on Fire, F5 Start In Low Ready On Raid Begin.
3. **`/review-spec` rodada 1** — gaps corrigidos: critérios vagos reescritos, corner cases adicionados.
4. **`/review-spec` rodada 2** — refinamento dos ACs de F2 (enum), corner case de Stance 3 em Linear mode, AC F3 sobre ADS.
5. **`/review-spec` rodada 3** — †visibilidade condicional adicionada na tabela F12, contagem do delta corrigida (4→5), corner case de burst fire.
6. **Decisão hotkey + ADS:** ignorar silenciosamente quando em ADS (Opção A).
7. **Renomeação de stances pelos eixos:** spec funcional ganhou "Stance 1 - High Ready" / "Stance 2 - Custom" / "Stance 3 - Low Ready" baseado em Pitch/Yaw reais do código. Esta convenção depois foi alterada novamente no 06-fix-01 (Stance 2 ↔ Stance 3 swap).
8. **`/review-spec` rodada 4 + 5** — pontos restantes de gaps + 11 [NOVO] no delta.

**Trabalho paralelo nesta sessão:** o usuário também criou item 003 (Stamina Multiplier faixa até 10) — implementação trivial em 1 linha de código, sem passar pelas etapas formais de tech-spec/review (exceção documentada no próprio `003-…-01-spec.md`).

**Outro trabalho paralelo:** adição do mod `SPT-Realism-Mod-Client` via `/add-mod-repo-for-modding` (ver `mods/SPT-Realism-Mod-Client/memory/sessions.md`). Não impacta este mod.

**Debug session paralela:** usuário relatou bug "shoulder swap durante lean" no SPT — investigado e identificado culpado como mod externo `hazelify.StanceSync.dll`. Solução: desabilitar config `Sync leaning with shoulder swapping?` no F12 desse outro mod. **Não relacionado ao stances mod**.

## 2026-05-10 ~14:00 (GMT-3) — Sessão 2: implementação completa item 002 + code review + correções

Dia mais denso. Em ordem aproximada:

1. **`/create-technical-spec 002`** — spec técnica gerada. Estratégia inicial F4: patch em operation-base nested de `Player.FirearmController` via reflection (Estratégia A).
2. **`/review-technical-spec` rodadas 01-04** — 24 pontos PA-NN-MM levantados e aceitos pelo usuário:
   - **Round 01:** patch target via reflection da operation-base; race condition do timer; CM dependency; hotkey priority por menor índice; snap state leak; nullability; checklist refinements.
   - **Round 02:** `[ThreadStatic]` reentry guard contra recursão infinita; resolução `IsAbstract` + `GetBaseDefinition` fallback; defer 1-frame para resurrect; sem closure; Enable condicional; AC F5+F4 simultâneos.
   - **Round 03:** 2-frame pulse (synthetic false em N+2 para parar fullauto); validação `CurrentOperation` entre frames; AC ChangeFireMode mid-hold; SettingChanged unsubscribe; stub `BuildStanceConfig`.
   - **Round 04:** Order 59 collision fix (ScrollMode → 58); hotkeys antes de V no Update; natural-pressed guard no reset; HideoutPlayer guard em F5.
3. **`/code-mod 002`** — implementação. Arquivos modificados: `Plugin.cs`, `StanceConfig.cs`, `StanceManager.cs`, `Patches/RaidLifecyclePatches.cs`. Criado: `Patches/SnapFireTriggerPatch.cs`. PROPRIEDADES.md atualizado (89 props).
4. **`/compile-mod stancesAndCameraPositionSPT4.0.11 --flat`** — 1ª tentativa falhou (faltava `using CameraRotationMod.Patches;` em StanceManager.cs); corrigido; 2ª tentativa passou.
5. **`/code-review 002`** — 1ª code review. 6 achados:
   - **CR-01-01 (🟠):** F4 disparava em fogo de outros players em Fika multiplayer; faltava guard `__instance == MainPlayer.HandsController`.
   - **CR-01-02 (🟡):** Weapon swap entre button-down e button-up causava tiro espúrio; anti-swap via `_interceptOperationInstance` cacheado.
   - **CR-01-03 (🟢):** `TryInterceptTriggerDown` ignorava parâmetro — agora usado para anti-swap.
   - **CR-01-04 (🟢):** `IsHoldingFirearm()` redundante (caller já validou) — removido.
   - **CR-01-05 (🟢):** XMLDOC explícito do null sentinel em `SnapToStance0OnFire`.
   - **CR-01-06 (🟢):** `Snap Stale Timeout (s)` exposto como Advanced ConfigEntry (90ª prop).
6. **`/apply-code-review 002`** — todos os 6 achados aplicados; `05-asbuild.md` criado retroativamente (item 002 foi entregue antes de `/code-mod` passar a gerar asbuild automaticamente).
7. **`/compile-mod` 2ª vez** — sucesso, ~71KB de .dll.

**Teste in-raid pelo usuário** ao fim do dia. Feedback:
- F1 não testado.
- F2 ✓ funcionando.
- F3 ✓ funcionando.
- **F4 ❌ não funcionou**. Usuário cogitou abolir, depois optou por revisar.
- F5 ✓ funcionando.
- Reclamação de confusão entre Stance 2 (Custom) e Stance 3 (Low Ready) nos docs; quis trocar.

**Investigação de F4:** descobrimos pelo Assembly que dos 14 overrides de `SetTriggerPressed` aninhados em `FirearmController`, apenas **1** (linha 3184) chama `base.SetTriggerPressed()`. C# virtual dispatch executa o IL do override diretamente, então o Prefix patcheado na base virtual (3810) nunca disparava para 13 dos 14 caminhos. **Estratégia A do PA-01-01 review-01 estava errada** baseado em premissa incorreta sobre dispatch.

**Solução encontrada:** patchear `Player.FirearmController.SetTriggerPressed` na linha 13668 (método de roteamento da FC que chama `CurrentOperation.SetTriggerPressed(pressed && method_53())`). Captura todos os fire inputs ANTES da virtual dispatch.

8. **Plan + execução do 06-fix-01:**
   - **Phase A:** F4 patch target trocado. `ResolveFirearmOperationBase` → `ResolveFirearmControllerSetTrigger`. `SnapFireTriggerPatch` reescrito com `Player.FirearmController` tipado. Signatures de `StanceManager` simplificadas. `CurrentOperationGetter` removido (não precisa mais — staleness check vira `HandsController == fc` direto).
   - **Phase B:** Stance 2 ↔ Stance 3 swap completo (section constants, `_stanceDefaults`, hand rotation defaults Pitch/Yaw/Forward, F5 target). Após swap: Stance 2 = Low Ready, Stance 3 = Custom.
   - **Phase C:** F12 ordering alfabético aceito pelo usuário.
   - **Phase D:** `06-fix-01.md` criado com análise técnica completa do bug F4. `05-asbuild.md` atualizado. Entrada de meta-rastreabilidade no Histórico de `04-code-review-01.md` (CRs preservados; PA-01-01 reversal documentado).
9. **`/compile-mod` final do dia 10** — passou, ~70KB (~1.5KB menor; F4 simplificada).

## 2026-05-11 ~00:30 (GMT-3) — Sessão 3: meta-infraestrutura + debug ADS

Trabalho paralelo a sessões anteriores: o usuário criou infraestrutura de workflow geral. Embora não seja específico deste mod, este mod foi a cobaia.

**Mudanças repo-wide neste dia:**

- **Renomeação de convenção:** todos os artefatos de backlog passaram a usar prefixo numérico de ordem (`NNN-<slug>-01-spec.md`, `-02-spec-tech.md`, `-03-spec-tech-review-NN.md`, `-04-code-review-NN.md`, `-05-asbuild.md`, `-06-fix-NN.md`). Script `scripts/migrate-backlog-naming.sh` aplicado: 16 arquivos renomeados, 14 .md com refs atualizadas via sed.
- **Nova skill `repo-workflow-best-practices`** em `.claude/skills/repo-workflow-best-practices/SKILL.md` — formaliza convenção de naming, fluxo do ciclo, rastreabilidade PA-NN-MM/CR-NN-MM, imutabilidade de reviews.
- **Novo command `/code-review`** em `.claude/commands/code-review.md` + template `.agents/templates/code-review.md.tmpl`. 6 categorias × 4 impactos.
- **Novo command `/apply-code-review`** em `.claude/commands/apply-code-review.md`.
- **Novo template `asbuild.md.tmpl`** para `05-asbuild.md`.
- **`/code-mod` atualizado** — passa a gerar `05-asbuild.md` ao final (mudança comportamental).
- **Commands existentes atualizados** para nova convenção: `create-spec`, `review-spec`, `create-technical-spec`, `review-technical-spec`, `code-mod`.
- **Item 003 ganhou nota de "exceção documentada"** no `01-spec.md` (pulou etapas formais por trivialidade — não vira precedente).

**Debug ADS no fim da sessão:**

Usuário reportou "ADS lento" in-raid. Investigado:

- Nossas mudanças (002 + 06-fix-01) **não tocaram `_ADSTransitionSpeed`** ou caminhos de ADS speed.
- **Causa provável identificada:** `Stance 0 Stamina Multiplier = 0.5` por padrão (backlog 001) drena HandsStamina mesmo em hipfire vanilla → EFT aplica penalty de tired aim → percepção de "ADS lento". Workaround: setar `1.0` no F12.
- **Por que o slider `ADS Transition Speed` parecia "morto"** quando testado em Stance 0: `SpringGetPatch.cs:200-208` faz early-return quando NÃO há feature ativa (`isInAnyStance == false && !resetOnADSEnabled`). Em Stance 0 com flags Advanced desligadas, o slider nem é consultado. Slider só atua em Stance 1/2/3, ou quando `Reset Positions When Aiming = true`. Documentado mas sem fix de código.

**Sugestão pendente (não executada):** `06-fix-02` opcional para expor toggle "Aplicar ADS Speed Override mesmo em Stance 0".

**Aviso de drift no asbuild.md (linha 14):** existe uma referência a um `06-fix-02.md` ("Labels das hotkeys Stance 2/3 + ordem F12 via Order bump em BindStance") que **não corresponde a trabalho registrado** nesta sessão. Pode ter sido criado em chat paralelo. Investigar antes de criar novo fix-02 com numeração duplicada.

## 2026-06-11 ~madrugada (GMT-3) — Sessão 4a: backlog de ajustes (Fase 0 + itens 004/008/009/010 + F12)

Sessão autônoma noturna (usuário dormindo; sem testes in-game, sem pedidos de aprovação). Documento de produto do usuário definiu sintomas/critérios complementares. Plano aprovado em `~/.claude/plans/backlog-ajustes-de-kind-phoenix.md` (2 passadas de revisão crítica via `/g-review-content`). Referência decompilada usada: `mods/RealismMod/Client/DLL descompilada/`. APIs validadas contra Assembly 0.16 em `D:/SPT`.

**Commits (ordem):** `49d3cf7` Fase0 → `9c46bc6` 010 → `ad09bd7` 009 → `60de87a`+`98c3df3` 008 → `fa6dbd5` 004 → `aef05fe` F12 → `b905a7a` 004 fika-fix.

**Fase 0 — build destravado:** csproj absoluto→relativo; `.spt-path` gitignored + `.example`; `compile-mod.sh` ganhou IMGUIModule+Fika.Core no `resolve_references` e leitura do `.spt-path` (parse, não `source`). Smoke build OK. ⚠️ **As mudanças do `compile-mod.sh` NÃO foram commitadas** — o arquivo já tinha trabalho não-commitado da sessão CustomClasses (item 019/020 config-guards); precisam de commit separado (git add -p ou coordenar com a sessão CustomClasses). Estão no working tree, funcionando.

**Item 010 (Manual Chambering) — `06-fix-01`:** causa raiz = `CanLoadChamber` default `true` (Realism usa `false`). Corrigido + `PreChamberLoadPatch` só seta `BlockChambering` + `StartReloadMagBlockPatch`→`StartReloadResetPatch` (reset, anti-softlock) + discriminador `JustSpawned` (spawn vs equip mid-raid) + `Reset()` em raid start/end + configs `_ManualChamberingOnRaidStart`/`_ManualChamberingOnReload` + logs `[ManualChamber]`. **Maior incerteza do lote — risco de softlock**; master toggle é kill-switch vanilla.

**Item 009 (Wiggle) — `06-fix-01`:** disparava em colisão/mount porque o gatilho era `currentStance != _previousStance` e o mount força Stance 0. Trocado por request intencional: `StanceManager.RequestWiggle/ConsumeWiggleRequest` chamado só nos call-sites de input (V/scroll/hotkey via `ApplyUserStance`); `SpringGetPatch` consome com frame-guard, bloco movido p/ fora do `stateChanged`, direção por `from→to`. Gate ao MainPlayer já existia.

**Item 008 (Esvaziar câmara) — `06-fix-01`:** nova classe `ActionStanceUnloadChamberPatch` (Prefix em `GClass2046.Start()`), fim via `method_45` (OnIdle) existente. Guard `ChamberAmmoCount > 0` para disjunção com o 010. Reusa `_EnableActionStanceSwap`.

**Item 004 (Mount) — `06-fix-01`:** reescrita completa. `EMountState`; grude invertido (era no passivo→agora só Active); detecção unificada via Prefix em `method_11` (modelo Realism CollisionPatch); input ativo via `ECommand.WeaponMounting (140)` (suprime nativo exceto bipé); `ResetCollisionOffsets` ao sair; `TurnAwayEffector` cacheado/restaurado; stamina suspensa enquanto montado. Fix de code-review: SetMounted/Fika só em transições Active (evita spam None↔Passive).

**F12:** dedup do bind de mounting (1º bloco órfão removido, seção→"Weapon Mounting"); `4./8./9.` renomeadas; sway default 0.1→0.2.

**Premissas assumidas (validar in-game) — ver cada `06-fix-01.md`:** 010 default false + targets 0.16; 008 GClass2046 dispara com câmara cheia (log confirma); 004 suprimir nativo exceto bipé, `method_23` omitido, magnitudes do grude podem precisar re-tuning.

**Pendência de processo:** o pipeline SDD foi cumprido de forma pragmática — gerados `06-fix-01.md` por item (rastreabilidade) + implementação + compile + 1 code-review pass, em vez de invocar cada slash command isoladamente (eficiência na execução batch). Tech-specs formais (`02-spec-tech`) não regeradas para os fixes.

## 2026-06-11 21:52 (GMT-3) — Sessão 4b: code-review adversarial (2 rodadas) + push

Continuação direta da entrada de madrugada deste dia (Sessão 4a). Delta registrado após a gravação anterior do `sessions.md` (commit `6676a12`), que não incluía o code-review nem o push.

**Tema central:** endurecer (corretude) os 4 itens recém-implementados via code-review adversarial, já que nada foi testado in-game.

**Decisões-chave:**
- **2 rodadas de code-review por subagentes adversariais** (a 1ª caiu por API 529; re-rodada com 2 subagentes em paralelo: um em 004/009+infra, outro em 010/008). **8 findings de corretude aplicados.**
- **010 F2 (🔴):** guard do `ECommand.ChamberUnload` recuperou `!CanLoadChamber` (paridade com RealismMod `KeyInputPatch1`) — evita rechamber/consumo de munição espúrio. Ref: `ManualChamberingPatches.cs` (commit `57e54c4`).
- **010 F1 (🟡):** equip com câmara **cheia** agora libera `CanLoadChamber`/`BlockChambering` (antes ficava preso `false` → `SetAmmoCompatiblePatch` forçava `compatible=false` até o reload).
- **008/010 resiliência:** `.Enable()` do `ActionStanceUnloadChamberPatch` (GClass2046, volátil em 0.16) envolto em try/catch — degrada só a feature em vez de derrubar o mod inteiro.
- **004 hardening:** guards null nos `FieldInfo` do `TurnAwayEffector` e em `_firearmController`; `ForceNone` no `OnDestroy` + `ResetForRaid` no `OnGameStarted` (anti-resíduo de mount entre raids); `SetMounted`/Fika só em transições Active (evita spam None↔Passive).
- **Findings NÃO aplicados (documentados como validar-in-game):** F3 (`JustSpawned`), F5 (fim do unload-chamber depende de `method_45`), F7 (fallback Fika), F8 (guard `Stationary` nos animator-patches). Ref: `06-fix-01.md` de 008/010.

**Atividade cronológica:**
1. 1ª tentativa de subagente de review → API 529 (overload). 2 fixes já identificados manualmente aplicados (TurnAway guards, `_fcField`).
2. 2 subagentes adversariais em paralelo → relatórios consolidados; 4 findings novos aplicados (F1, F2, Enable try/catch, ResetForRaid). Build verde a cada passo.
3. Docs `06-fix-01.md` de 008/010 atualizados com findings remanescentes.
4. **Push** `584ca1b..57e54c4` para `origin/main` (aprovado pelo usuário).

**Cross-refs:**
- Complementa a Sessão 4a (madrugada) deste dia — implementação + Fase 0 + F12.
- Findings detalhados nos `backlog/{004,008,010}-…/…-06-fix-01.md`.

## 2026-06-21 00:08 (GMT-3) — Sessão 5: fix câmera (gimbal flip) + fix áudio hold-breath (fork modded)

**Tema central:** corrigir dois bugs críticos do refactor do dev rocket (pull `e8f706b`) na linha `modded`: câmera invertida ao aplicar stance e som de hold-breath que não tocava. + `/code-review` do fix de câmera.

**Decisões-chave:**
- **Câmera (gimbal flip):** `ApplyComplexRotationPatch`/`ApplySimpleRotationPatch` trocaram o `Quaternion.Slerp` do RealismMod por uma **mola Euler inline** que diverge/overshoota conforme o frame-timing e, operando em ângulos de Euler, cruza o gimbal (~180°) → câmera de cabeça pra baixo, só em alguns players (mesma DLL/config). Fix: **sub-stepping** (integração estável independente do `dt`) + **batente angular ±60°** (alvo legítimo é ±45°) + clamp de velocidade, idêntico nos dois patches. Preserva a "quicada". Validado in-game. Ref: `modded/Patches/ApplyComplexRotationPatch.cs`, `ApplySimpleRotationPatch.cs`.
- **Áudio hold-breath — dois bugs independentes:** (A) `.wav` em IEEE float 32-bit lidos pelo `WavUtility` como PCM 16-bit → ruído saturado; (B) `AudioClip` carregados no boot (cena de menu) e **descarregados na transição p/ o jogo** → `length 0` no play. Fix: assets p/ **OGG Vorbis mono** (heartbeat 23 MB→467 KB) + **decodificador nativo** `UnityWebRequestMultimedia`+`DownloadHandlerAudioClip` (`streamAudio=false` + cópia standalone) + **carregar em `GameWorld.OnGameStarted`** (não no boot). Validado in-game. Ref: `modded/Patches/HoldBreathPatch.cs`, `RaidLifecyclePatches.cs`, `Plugin.cs`.
- **Heartbeat órfão:** `HoldBreathPatch.OnRaidEnd()` para o loop e zera `IsHoldingBreath` — evita o batimento tocando no menu após morte/extração segurando a respiração.
- **Sequenciamento acordado:** corrigir features quebradas (som → mount) **antes** da refatoração grande. "Refatore código que funciona, não quebrado."

**Lições / hipóteses descartadas:**
- Câmera: a hipótese "mola diverge por config" foi enfraquecida pela análise de estabilidade — com `damping=12` (default) a mola é estável; o **batente ±60°** é a real garantia, não o sub-stepping. Causa determinística = frame-timing, não config.
- Áudio: gastei dois ciclos com `streamAudio=false` + cópia standalone achando que o `length 0` era o `Dispose` do `UnityWebRequest`; o sintoma persistia. Causa real = **descarregamento na troca de cena** (carregar no menu). Pista decisiva no log: "carrega 1.14s no boot, 0 no hideout, mesma DLL/objeto" → culpado é a transição.
- Launcher: o sync (Dev Mod off) revertia a DLL local pela do servidor a cada "Start" → testávamos a build antiga sem saber. **Confirmar a build via marcador de versão no log** antes de concluir que um fix "não funcionou". Ref: memória `feedback_server_launcher_sync_builds`.

**Atividade cronológica:**
1. `git pull` (`e8f706b`) — refactor de animação + hold-breath/oxigênio/FIKA sync do rocket.
2. Diagnóstico câmera — comparação com RealismMod (Slerp vs mola Euler) e decompilado (`GClass909-912`; `ProceduralWeaponAnimation.SetStrategy(pointOfView)`: 1ª/3ª pessoa = mesma PWA trocando estratégia).
3. Fix câmera (sub-step + clamp); `dotnet build`; instalado em `RealisticMobility/`; validado in-game.
4. `/code-review` do fix → `modded/code-review-camera-flip-fix-01.md` (8 achados CR-01-01..08, 0 🔴; hotfix fora do pipeline SDD).
5. `/g-diagnose` áudio — causa de formato provada offline (differential loop PCM16 vs float32).
6. Conversão OGG (ffmpeg) + reescrita do loader; vários ciclos até achar a 2ª causa (carregar no game start).
7. Fix final áudio + heartbeat órfão; validado in-game.
8. Memória `reference_spt_mod_audio_loading` criada.

**Pendências abertas nesta sessão:** P-5.1..P-5.5 (ver topo).

**Cross-refs:**
- Code-review: `modded/code-review-camera-flip-fix-01.md`.
- Memória: `reference_spt_mod_audio_loading` (pipeline de áudio), `feedback_server_launcher_sync_builds` (reversão de build pelo launcher).
- **Revisão de fato anterior:** as Sessões 1–4 tratavam o trabalho em `modded/`; a linha ativa agora é o fork `modded` (do rocket), buildado fora do `compile-mod.sh`. Histórico preservado.

## 2026-07-09 22:57 (GMT-3) — Sessão 6: code-review 02 do 014 + reorg de forks (modded canônico) + fix-03 (braço acompanha)

**Tema central:** validar/fechar o item 014 (sync visual de stances no Fika) sem poder testar de imediato, o que levou a: code-review por referências, reorganização dos forks para acabar com a confusão de build, e — após o teste do usuário — o diagnóstico e correção definitiva do braço que não acompanhava a arma.

**Decisões-chave:**
- **Code-review 02 do 014 por validação de referências** (2 sub-agents independentes confirmaram cada elo contra Assembly/Fika): hook roda todo frame, transform certo, coexistência aditiva. Veredito "deve funcionar". Aplicados **CR-02-01** (guard anti-acúmulo), **CR-02-02** (`TickAdsNetworkSync` reenvia stance ao mirar) e **CR-02-04** (remoção de `FikaNetworkSync.cs` + `PlayerStanceController.cs` mortos). CR-02-03/05/06 deferidos. Ref: [`04-code-review-02.md`](../backlog/014-sync-stances-fika/014-sync-stances-fika-04-code-review-02.md).
- **Reorganização dos forks:** `git mv modded-beta → modded` (canônico) e `modded → modded-bak`. Motivo: o `/compile-mod` resolvia `modded/` (antigo) e instalou um DLL errado por cima do bom; `modded-beta` já era o fork oficial. 128 refs `modded-beta`→`modded` nos docs. **csproj ajustado** para puxar `Fika.Core` da raiz `references/` → build **self-contained** (sem `mods/references/` temp). Ref: memória global `reference_stances_canonical_build`.
- **014 fix-03 — a correção que faltava:** aplicar o offset num **Postfix de `PlayerBones.ShiftWeaponRoot`** (janela **pré-IK**, linha ~1876), NÃO num Postfix de `ObservedVisualPass` (pós-IK). Como os markers de IK da arma são filhos do `Weapon_Root_Anim`, mover o root antes da IK faz o **braço** seguir (LimbIK) e o `Kinematics` cola a **arma** na mão. Ref: [`06-fix-03.md`](../backlog/014-sync-stances-fika/014-sync-stances-fika-06-fix-03.md), `modded/Patches/ObservedStanceShiftPatch.cs`.

**Lições / hipóteses descartadas:**
- **Armadilha de build:** `/compile-mod` compilava `modded/` (fork antigo, com `_wasSprinting`) em vez de `modded-beta` (ativo) → instalou DLL errado. Sintoma de detecção = warning `_wasSprinting`. Resolvido pela reorg (modded = canônico) + csproj self-contained.
- **014 — timing é tudo:** o fix-02 (Postfix de `ObservedVisualPass`) movia a arma mas **não o braço**, porque roda **depois** da IK das mãos (`method_19`, 1886) e do `Kinematics` (1889) — o braço já fora solveado na pose sem offset. Todas as tentativas anteriores erraram a **janela**, não o transform. A janela correta é **entre `ShiftWeaponRoot` (1876) e o alvo da IK `method_20` (1884)**. Confirmado por 2 sub-agents com refs primárias. Chave: a IK das mãos mira nos markers `weapon_L/R_IK_marker`, **filhos do `Weapon_Root_Anim`**.
- **Merge com auto-commit remoto:** o push falhou (remoto à frente com "Auto-commit" de `rockettechnology-dev` — launcher/TarkovIRL/Fika + refator no `modded` antigo do stances). Conflito porque o git casa por path (renomeei a pasta). Resolvido mantendo a reorg do stances (`git checkout HEAD -- stances/`) e integrando o resto. Refator do outro PC no fork aposentado fica só no histórico (`deb779e`).
- **Data:** o relógio do ambiente reportou `2026-06-23` no início da sessão (errado); a data real é **2026-07-09**. Artefatos criados com 06-23 foram corrigidos via sed. Não estimar data — reconferir o relógio.

**Atividade cronológica:**
1. `/code-review 014` (validação por referências, 2 sub-agents) → `04-code-review-02.md` (0🔴, 1🟠, 3🟡, 2🟢).
2. Aplicação CR-02-01/02/04; `/compile-mod` revelou a armadilha do fork errado; build manual do `modded-beta` correto reinstalado.
3. Reorg: `git mv` das pastas + sed 128 refs + csproj self-contained; build self-contained validado; commit `a29e241`.
4. Push falhou → merge `afffc77` com o auto-commit remoto (mantida a reorg do stances); push OK.
5. Usuário testou o 014: **arma move, braço não**. 2 sub-agents mapearam a cadeia → causa = hook pós-IK.
6. fix-03: novo `ObservedStanceShiftPatch` (Postfix de `ShiftWeaponRoot`), removido o `ObservedStanceVisualPatch`, animator simplificado. Build 0 erros; instalado (hash `972f5f8`).

**Pendências abertas nesta sessão:** P-6.1 (🔴 validar 014), P-6.2 (🟡 calibração eixo), P-6.3 (🟡 validar 011/013), P-6.4 (🟢 limpeza 014). Ver topo.

**Cross-refs:**
- ✅ **Resolve [P-5.4]** (2026-06-21, build do fork fora do pipeline) — reorg + csproj self-contained fazem o `/compile-mod` resolver o canônico.
- Pendências legadas P-4.2/4.3, P-4.6, P-5.1, P-5.2 movidas para a seção "legadas" do topo (provavelmente resolvidas/obsoletas após 011-014).
- Memória global nova: `reference_stances_canonical_build` (substituiu `reference_stances_build_modded_beta`).

## Arquivos-chave do mod (referência rápida)

- `modded/Plugin.cs` — Awake, ConfigEntries, helpers de F2 (CM cache) e F4 (`ResolveFirearmControllerSetTrigger`), section constants, `_stanceDefaults`, `BindStance`.
- `modded/StanceManager.cs` — Update tick, F1 IsStanceEnabled, F2 HandleLinearScroll, F3 HandleStanceHotkeys, F4 estado snap + 6 helpers, F5 QueueInitialStance/TryApplyPendingInitialStance.
- `modded/StanceConfig.cs` — StanceConfig record (ConfigEntries por stance, com SnapToStance0OnFire nullable para Stance 0).
- `modded/Patches/SnapFireTriggerPatch.cs` — F4 Prefix com `[ThreadStatic]` reentry guard, intercept-and-resurrect, 2-frame pulse.
- `modded/Patches/RaidLifecyclePatches.cs` — Postfix de `GameWorld.OnGameStarted` (StanceManager.OnRaidStart + F5 QueueInitialStance(Stance2)) e `GameWorld.OnDestroy` (OnRaidEnd).
- `modded/Patches/SpringGetPatch.cs` — pré-existente, controla transição de mãos via Spring; early-return quando nenhuma feature ativa (relevante para o slider `ADS Transition Speed`).
- `modded/Patches/StanceStaminaRecoveryPatch.cs` — pré-existente do backlog 001; controla drain/recovery de HandsStamina por stance.
- `PROPRIEDADES.md` — 90 props documentadas em pt-BR (era 79 antes do 002; 89 após CR-01-06 com Snap Stale Timeout).
- `backlog/` — todos os artefatos do ciclo (01-spec, 02-spec-tech, 03-spec-tech-review-NN, 04-code-review-NN, 05-asbuild, 06-fix-NN).

## 2026-07-04 — Sessão (CustomClasses/051): hook externo de dreno de braço

**Entrada de COORDENAÇÃO (escrita pela sessão do CustomClasses, worktree wt-057, branch feat/053-perks-property-model):**
o `StaminaController` ganhou um CONTRATO EXTERNO — `public static Func<float> ExternalHandsDrainMult` — composto
no Tick **só no ramo de dreno** (`delta < 0`): `delta *= Clamp(hook(), 0, 2)`. O CustomClasses o preenche por
reflection (Steady Arms do Caçador ×0.65 em ADS; Tireless Arms do Tanque ×0 com arma pesada). Null = comportamento
idêntico ao anterior (regressão zero). ⚠️ NÃO renomear `CameraRotationMod.StaminaController.ExternalHandsDrainMult`
sem coordenar. Artefatos: mods/CustomClasses/backlog/051-stances-zone-levers/ (spec + review técnica 01).

## 2026-07-11 00:50 (GMT-3) — Sessão 7: backlog fechado (validação in-game de tudo), item 015 + reorganização das propriedades F12

**Tema central:** validar in-game o que estava pendente (013/014 e depois 008/010/011/002/015), entregar o item 015 (bloquear o mount ativo nas stances), pagar o débito seguro e reorganizar as ~143 propriedades do F12.

**Decisões-chave:**

- **Item 015 — "travar em Stance 0" em vez de "desmontar".** A ideia inicial era: se o jogador montar a arma numa superfície e trocar de postura, desmontar sozinho. O code-review provou que esse tick seria **código morto**: o item 013 já força a Stance 0 enquanto a arma está montada (`StanceManager.Update` L169-180 — retorna cedo quando `isNativeMounting`), então a condição "montado E em Stance 1/2/3" nunca ocorre. Sobrou só o **bloqueio na entrada**: Prefix em `Player.TryMountWeapon` ([Player.cs:26218](../../../references/eft-decompiled/Assembly-CSharp/Player.cs#L26218)) que retorna `false` quando em Stance 1/2/3 fora da mira. O tick foi **removido** do `StanceManager`. Ref: `backlog/015-bloquear-mount-ativo-stances/…-04-code-review-01.md` (achado CR-01-01), decisão do usuário.
- **Bipé é exceção por construção.** O bloqueio testa `pwa.IsMountedState` (mount em superfície); o bipé usa outro estado (`IsBipodUsed`), então continua funcionando em qualquer postura. Não precisou de guard extra.
- **Débito técnico: só o seguro.** Aplicado `try/catch` nos **6 Prefixes** do Manual Chambering (item 010) — é o código com risco de **softlock** (arma travada sem munição na câmara): uma exceção num Prefix que retorna `bool` deixaria o jogador sem poder atirar. Prefix `bool` → `return true` (deixa o vanilla rodar); `void` → só loga. **Adiado:** unificar as molas (`SpringMath`) e matar a reflection por frame — mexem em código de câmera **já validado in-game**; risco > valor sem bug real motivando.
- **Padrão de tooltip bilíngue adotado** (decisão do usuário, virou regra do repo): inglês na 1ª linha, **linha em branco**, português na 3ª — no C#: `"<English>\n\n<Português>"`. Os dois idiomas precisam ficar intuitivos; as dicas que já existiam em português nos nomes das chaves (ex.: `Pitch (Cano Sobe/Desce)`) foram usadas para **melhorar o inglês**, não só traduzidas.
- **F12 de 143 → 120 propriedades.** **23 eram mortas** (bindadas, nunca lidas): a seção *Wiggle* inteira (animação orgânica removida numa refatoração anterior), 15 multiplicadores de ADS das Stances 0/1/2, o *shoulder-throw* e 2 opções de transição de ADS. Provado por grep de `_Xxx.Value` em `modded/`. Ref: `PROPRIEDADES-review-01.md`.
- **Novo command `/review-mod-properties`** (meta-repo): revisão de UX das propriedades F12 em 8 categorias — ordem/nome das seções, alocação, nome/tipo/tooltip das opções, propriedades mortas e uso do "Advanced". Registrado no `WORKFLOW.md` como comando **auxiliar** (fora do ciclo linear).

**Lições / hipóteses descartadas:**

- **"Desmontar ao trocar de postura" (item 015) era código morto** — não porque a lógica estivesse errada, mas porque **outro item entregue antes (013) já tornava o estado inalcançável**. Lição: antes de escrever um tick reativo, checar se um item anterior já **fecha o estado** que ele reagiria. O code-review pegou; a spec técnica não. Ref: item 015, achado CR-01-01.
- **Rótulo pode mentir sobre o eixo.** Em 8 propriedades (mira + Stances 1/2/3) as chaves `…HandsYawRotation` e `…HandsRollRotation` estavam **trocadas** em relação ao que o código aplica no `Vector3(pitch, yaw, roll)` — quem calibrasse pelo nome estaria mexendo no eixo errado o tempo todo. Mesmo tipo de erro no rótulo de "Start In Low Ready On Raid Begin", que dizia *Stance 3* mas o código aplica `Stance.Stance2` ([RaidLifecyclePatches.cs:37](../modded/Patches/RaidLifecyclePatches.cs#L37)). Lição: **o nome/tooltip é código também** — revisar contra o que o método faz, não contra o que a seção sugere.
- **Reordenar as seções do F12 com segurança é inviável hoje.** A ordem no ConfigurationManager é por **ordem de descoberta** (primeiro `Config.Bind` de cada seção) — mas os binds de uma mesma seção estão **fragmentados** no `Awake` (Stance 2 aparece em L766 **e** L1184). Mover blocos arriscaria quebrar um arquivo de 1700 linhas já validado. Adiado como dívida (P-7.3), não por preguiça — por risco.
- **Edição em massa por agente exige validação estrutural.** Os 102 tooltips restantes foram convertidos para bilíngue por um sub-agent; validei comparando as contagens estruturais entre `HEAD` e o work tree (`Config.Bind`=94, `AcceptableValueRange`=60, `ConfigurationManagerAttributes`=100, `Order`=96 — idênticas) e conferindo que o diff tocava **só** linhas de tooltip. Sem esse gate, um agente pode "arrumar" o código junto com o texto sem ninguém notar.

**Atividade cronológica:**

1. **Validação in-game** do 014 (sync das stances no Fika) e do 013 (refino das transições) → ambos ✅. O braço **e** a arma acompanham a postura no cliente remoto (era o sintoma do fix-03).
2. **Item 015** — ciclo SDD completo (backlog → spec → review → spec técnica → review técnica → código → code-review → aplicar). Entregue: `Patches/BlockActiveMountPatch.cs`; removido o tick morto do `StanceManager`.
3. **Débito técnico:** `try/catch` nos 6 Prefixes do Manual Chambering; limpeza dos logs de diagnóstico temporários (`[Spy]`, `[StanceSync-014]`, flags `_loggedApply`/`_loggedHook`).
4. **Criado `/review-mod-properties`** + template `.agents/templates/mod-properties-review.md.tmpl` + registro no `WORKFLOW.md`.
5. **Revisão do F12** (`PROPRIEDADES-review-01.md`, 12 achados). Aplicados: 23 props mortas + 9 campos órfãos removidos; eixos Roll/Yaw corrigidos; rótulos legados (Stance 2/3 e "Start In Low Ready"); nomes de seção em inglês; **109 tooltips bilíngues**. `PROPRIEDADES.md` regenerado (21 seções, 120 props, índice temático).
6. **Validação in-game final:** 008 (troca de postura ao recarregar), 010 (Manual Chambering), 011 (mount passivo), 015 (bloqueio do mount ativo) e as 2 funções do 002 (snap ao atirar + postura normal no ciclo do scroll) → **todos ✅**. **Backlog inteiro entregue.**

**Pendências abertas nesta sessão:**

- **[P-7.1]** (aberta 2026-07-11) Conferir o F12 in-game após a reorganização e **subir a versão do mod** no release (os renomes resetam a config salva). Categoria: 🟡 débito.
- **[P-7.2]** (aberta 2026-07-11) Dívida técnica adiada (molas, reflection por frame, `try/catch` nos patches restantes, reset estático). Categoria: 🟢 ideia. **Herda a P-5.3.**
- **[P-7.3]** (aberta 2026-07-11) Dívida da revisão do F12 (reordenar seções, alocação das opções de velocidade, seção da Stance 0). Categoria: 🟢 ideia.

**Cross-refs — pendências fechadas (todas ✅ em 2026-07-11):**

- **[P-6.1]** validar o 014 in-game → ✅ **passou** (fix-03).
- **[P-6.2]** calibração de eixo do `Weapon_Root_Anim` (1ª↔3ª pessoa) → ✅ **não foi necessária** — a pose ficou correta sem ajuste.
- **[P-6.3]** validar 011/013 → ✅ ambos passaram.
- **[P-6.4]** limpeza pós-014 (logs) → ✅ feita no passo 3.
- **[P-5.5]** limpar logs de diagnóstico temporários → ✅ feita no passo 3.
- **[P-4.1]** validar 008/010 → ✅ ambos passaram.
- **[P-4.4]** validar o snap ao atirar (F4) e a postura normal no ciclo (F1) do item 002 → ✅ ambos passaram.
- **Legadas descartadas por obsolescência** (P-4.2/P-4.3 infra de build, P-4.6 migração de `.cfg`, P-5.1 commit do fix de câmera, P-5.2 mount automático): superadas pela reorganização dos forks e pelo item 011. Removidas do topo.
- **Trabalho meta-repo nesta sessão** (`/review-mod-properties`, template, `WORKFLOW.md`): sem destino dedicado desde 2026-07-06 — `git log` é a fonte de verdade.


## 2026-07-11 ~21:30 (GMT-3) — Sessão 8: release 2.0.0 (versão + changelog + rebuild) e a DLL defasada

Sessão curta, aberta a partir do handoff `handoff-2026-07-11-stances-backlog-fechado.md` para fechar o lado
"release" da **P-7.1**. Nada de código funcional mudou.

1. **Bump `1.3.1 → 2.0.0`.** A 1.3.1 tinha sido escrita **uma única vez** (commit `a29e241`, 2026-06-23, promoção do
   `modded-beta`) e nunca mais subiu — os itens 014 fix-03 e 015, o `try/catch` do Manual Chambering e a reorg
   inteira do F12 entraram por cima dela. **Major** (não minor) porque a config salva do usuário **reseta**:
   o BepInEx casa cada entrada pelo par `(seção, chave)` literal e a reorg renomeou ambos.
2. **A versão agora vive em dois lugares** — `Plugin.cs` (`BepInPlugin`, que é o que o BepInEx mostra) **e** o
   `.csproj` (`Version`/`AssemblyVersion`/`FileVersion`). Antes o `.csproj` não tinha versão nenhuma: a DLL saía
   como `1.0.0.0` enquanto se anunciava 1.3.1. Manter os dois em sincronia.
3. **Changelog do fork em `modded/CHANGELOG.md`** (novo). O `CHANGELOG_SIMPLIFIED.md` é do **upstream** e para na
   v1.1.4 — foi deixado intacto como histórico. A entrada da 2.0.0 abre com o aviso de perda de config e explica
   que vale reconfigurar do zero (8 opções tinham Roll/Yaw trocados no rótulo, então quem calibrou pelo nome
   mexeu no eixo errado).
4. **Rebuild + deploy.** Build limpa (só o warning pré-existente `CS0618` no `FOVSliderPatch`). DLL **`f7752b6`**
   (v2.0.0) copiada para `D:/SPT/BepInEx/plugins/RealisticMobility/`, para o repo (`modded/`) e para `builds/`
   (que é **gitignored** — histórico local, não entra em commit).
5. **Commit `39e7a56`**, cirúrgico: só os 4 arquivos do stances. A árvore tinha trabalho não commitado de uma
   sessão paralela (TRL-ItemsManagement) e ele foi preservado.

**🔍 Achado que corrige o handoff — a DLL instalada estava defasada.**
O handoff afirmava que a DLL em `D:/SPT` era a `c83ed42` e "continha tudo". **Não continha.** Os hashes divergiam:
instalada `972f5f8` (11/07 **00:53**, 135,5 KB) × repo `c83ed42` (11/07 **03:38**, 151 KB). A build com a reorg do
F12 é a das 03:38 — **quase 3h depois** da que estava rodando. Consequência real: a **validação in-game da Sessão 7
rodou sobre código sem a reorg**, então os **23 props mortos e 9 campos órfãos removidos nunca rodaram no jogo**.
Por isso o teste que falta **não é cosmético** — é reconfirmar que os itens seguem funcionando sobre a 2.0.0.

**Lição:** conferir o **hash da DLL instalada contra a do repo** antes de confiar que "está deployado". Handoff e
memória registram a *intenção* do deploy; só o hash prova. (Reforça `feedback_spt_validation`: escrita em arquivo
SPT ≠ validação.)

**Pendências:**

- **[P-7.1]** → **parcial**. Lado release ✅ (2.0.0 + changelog + rebuild + deploy). **Falta o gate humano:** rodar
  o jogo com a `f7752b6`, conferir as 21 seções / 120 opções do F12 **e** reconfirmar os itens da Sessão 7.
- **[P-7.2]** e **[P-7.3]** — inalteradas.

## 2026-07-12 ~00:30 (GMT-3) — Sessão 9: review 02 de propriedades → 7 props fantasmas + regressão do FOV (v2.1.0)

Sessão disparada por uma **intuição do usuário**, depois de ele abrir o F12 da 2.0.0 in-game: *"a ordenação e os
tooltips deram certo, mas tenho impressão que continuamos com algumas ou várias configs fantasmas"*. **Ele estava
certo.** Rodado `/review-mod-properties` (round 02) → `PROPRIEDADES-review-02.md` (10 achados).

**Confirmado como CORRETO na 2.0.0** (o que os prints provaram): título `2.0.0` (deploy funcionou, o launcher não
reverteu), **21 seções na ordem certa**, e **95/95 tooltips** no padrão bilíngue. `PROPRIEDADES.md` estava
**sincronizado** com o código — zero divergência de key/default/faixa/seção.

**Por que a review 01 não pegou os fantasmas (a lição central):** ela caçava props **bindadas e nunca lidas** — e
removeu 23. **Todas as 120 sobreviventes SÃO lidas**, então `grep`/auditoria por leitura diz "está limpo". Os
fantasmas restantes são de outro tipo: **a prop é lida, mas o caminho onde ela é lida nunca executa.** Isso só sai
rastreando a cadeia do `.Value` até o efeito no jogo.

**Aplicado na v2.1.0 (commit `ca9f868`, DLL `0e622ba`) — F12: 120 → 113 props, 21 → 20 seções:**

1. **MP-02-01 — seção `Default Hands/Arms Positions` inteira REMOVIDA (4 props).** Alimentavam
   `_cachedDefaultPosition`, lido **só** no branch `_ =>` de `GetTargetPosition` (stance == Default). Mas os **3**
   call-sites são gated em `isInStance` (⇔ `CurrentStance != Default`) → **branch inalcançável**. Contradição
   estrutural: a prop dizia "posição quando NÃO está em postura" e só seria lida **estando** em postura.
2. **MP-02-02 — `Apply When Prone` agora só existe na Stance 0 (3 props removidas).** Ao deitar,
   `StanceManager.Update` força `SetStance(Default)` **antes** das leituras (`:165-176`) → a cfg consultada em prone
   é **sempre a da Stance 0**. As das Stances 1/2/3 eram lidas num caminho inalcançável. Implementado com **sentinel
   null** (espelhando o `SnapToStance0OnFire`, que faz o inverso); os 2 leitores usam `?.Value ?? false` e a
   assinatura do `SettingChanged` ganhou null-guard — **sem esse guard seria NRE no Awake**.
3. **MP-02-03 — REGRESSÃO: `FOVClampPatch` era órfão.** A classe existe e seu docstring diz servir "to allow FOV
   values outside the default 50-75 range", mas o **`.Enable()` nunca era chamado** — dos 35 patches, o único fora do
   `Awake`. `git log -S` prova: existia no commit inicial (`c078925`) e foi **apagado por arrasto** no `9816946`
   (commit de *mounting*, sem relação com FOV). Sobrava só o `FOVSliderPatch`, que alarga o **slider da UI** — o jogo
   re-clampava o valor. Reabilitado via `SafeEnable` (o patch já se auto-gateia em `_FOVExpandEnabled`).
4. **MP-02-04 — fantasma INVERSO: 4 props que funcionam e não apareciam.** `Include Stance 0 in Cycle` e
   `Enable Stance 1/2/3 in Cycle` tinham `Browsable = wheelEnabled && mode == Cycle` → **sumiam do F12** na config
   padrão (wheel off / modo `Linear` — exatamente o caso do usuário, visível nos prints). Mas elas governam **também
   o ciclo da tecla V**, em qualquer modo. Agora são **sempre visíveis**.

**🔴 Achado de maior impacto no jogo, que ninguém procurava (MP-02-10):** o comentário `// Stance 0: irrelevante`
(`Plugin.cs:47`) é **FALSO**. Com os defaults, a Stance 0 aplica um **cap de 90% na velocidade sempre que o jogador
está fora de postura** — a maior parte da partida — e isso **compõe** com o `Walk Speed Multiplier` (0.85). Responde
o MP-01-10 ("a seção Stance 0 se justifica?"): **sim, e é a mais impactante das quatro.** Virou a pendência **P-8.3**
(decisão de balance do usuário).

**Lições:**

- **"Prop é lida" ≠ "prop faz algo".** Auditoria de config precisa rastrear a cadeia `.Value → efeito no jogo`, não
  só a existência de uma leitura. As 3 categorias que apareceram: branch inalcançável, gate que muda o estado antes
  da leitura, e patch que não é habilitado.
- **Confiar na intuição do usuário sobre o que ele vê no jogo.** Duas auditorias anteriores disseram "limpo"; ele
  olhou o F12 e disse "tem fantasma". Tinha 7.
- **Patch órfão é invisível:** a classe compila, o arquivo existe, o code-review lê o patch e o dá por bom — mas ele
  nunca é habilitado. Vale um teste de sanidade: *todo `ModulePatch` do mod está em algum `Enable()`/`SafeEnable`?*
- **Contagem de props do F12 é um bom canário.** 120 → 113 sem perder função nenhuma.

**Pendências:** **P-7.1** (validar em raid os itens da Sessão 7 sobre a build nova) · **P-8.1** (validar os 4 fixes
da 2.1.0, sobretudo o FOV) · **P-8.2** (6 achados da review 02 não aplicados) · **P-8.3** (decisão de balance da
Stance 0). P-7.2 e P-7.3 inalteradas.

## 2026-07-12 ~02:00 (GMT-3) — Sessão 9 (cont.): Yaw e Roll estavam trocados (v2.2.0)

**Bug reportado pelo usuário jogando** (não por review): *"o Yaw está tombando a arma e o Roll está movendo para
esq/dir — em todas as stances"*. Confirmado nas 3 stances **e no ADS**.

**Causa raiz — a lição que vale para o mod inteiro.** A rotação é aplicada como
`weapRotation * Quaternion.Euler(euler)` (`ApplyComplexRotationPatch:280`), ou seja **no espaço local da arma**.
Nesse espaço — como os comentários de **posição** já registravam há tempos! — `Y = eixo LONGITUDINAL (o cano)` e
`Z = vertical`. Logo **girar em torno de Y tomba (roll)** e **em torno de Z aponta (yaw)**. O código montava
`new Vector3(pitch, yaw, roll)`, a ordem canônica do **Unity**, jogando cada valor no eixo do outro. Correto é
**`new Vector3(pitch, roll, yaw)`**. Corrigido nos **4** pontos de montagem (ADS + Stances 1/2/3) —
`ObservedStanceAnimator` (Fika) e `ApplySimpleRotationPatch` consomem `GetTargetRotation` e herdaram o fix.

**🔥 Era uma REGRESSÃO NOSSA.** O commit `261c069` (**MP-01-02**, review 01) presumiu a convenção do Unity, concluiu
que os rótulos estavam errados e **trocou os rótulos**. Os rótulos estavam **certos**; o **mapeamento** é que estava
errado. A troca inverteu os dois eixos para o usuário e mascarou a causa real por mais um ciclo — e o usuário chegou
a **compensar na mão** no `.cfg` (pôs o `-30` da Stance 3 no campo "Roll", que era o que de fato apontava).

**Por que nenhuma das 2 reviews de propriedades pegou:** ambas compararam **rótulo × nome do campo** — e esses
batiam (`_Stance1HandsYawRotation` ↔ key "Yaw"). O que não batia era **campo × eixo físico**, que só se enxerga
lendo como o `Vector3` é consumido. Virou o achado **MP-02-11**.

**Também aplicado (MP-02-07, decisão do usuário):** os sufixos didáticos das keys foram **traduzidos** para inglês,
não removidos — eles existem porque `Pitch`/`Yaw`/`Roll` é jargão: `(Cano Sobe/Desce)` → `(Muzzle Up/Down)`,
`(Apontar Esq/Dir)` → `(Point Left/Right)`, `(Tombar Arma)` → `(Cant Weapon)`, `(Coronha Sobe/Desce)` →
`(Stock Up/Down)`, `(Coronha Esq/Dir)` → `(Stock Left/Right)`, `(Contra o Peito)` → `(Toward the Chest)`,
`(Menos gera Mais Quicada)` → `(Lower Means More Bounce)` ⚠️ (a 1ª tentativa usou `(Lower = More Bounce)` e o **`=` derrubou o Awake** — o BepInEx proíbe `=` em nome de key; ver Sessão 10). `(Frente/Trás)` foi removido (redundante com
`Forward/Backward`). **Tooltips seguem bilíngues** (91 `ConfigDescription`, todos com `\n\n`).

**Config do usuário MIGRADA, não resetada.** O rename de key é breaking (o BepInEx casa por `(seção, chave)`), o que
apagaria a calibração dele. Em vez disso o `.cfg` foi reescrito: valores de `Yaw`↔`Roll` **trocados** + keys
renomeadas → **as poses ficam idênticas in-game**, mas cada nome passa a dizer a verdade. Backup em
`D:/SPT/BepInEx/config/com.shwng.fpscamerastances.cfg.bak-pre-v220`. Valores migrados:
Stance 1 (Yaw -5 / Roll 7) → (Yaw 7 / Roll -5) · Stance 2 (0 / -8) → (-8 / 0) · Stance 3 (0 / -30) → (-30 / 0).

**Lições:**

- **Propriedade de rotação valida-se contra o EIXO FÍSICO, nunca contra a convenção da engine.** Em espaço local de
  osso/arma os eixos não são os do mundo — e aqui a pista estava escrita no próprio arquivo, nos comentários de
  posição (`Y = Forward/Backward in Tarkov`). Ninguém ligou os pontos.
- **Quando o rótulo e o efeito divergem, suspeitar do MAPEAMENTO antes de renomear o rótulo.** Renomear é o conserto
  que parece mais barato — e é o que esconde o bug. Foi assim que a review 01 transformou um bug em regressão.
- **O usuário jogando acha o que 2 reviews não acharam.** Segunda vez nesta sessão (a 1ª foi a intuição das "configs
  fantasmas", que rendeu 7 props mortas).

**Pendências:** **P-8.4 (nova)** — validar in-game a v2.2.0: `Yaw` deve APONTAR e `Roll` deve TOMBAR, nas 3 stances
e no ADS; e conferir que as poses continuam como antes (a migração do `.cfg` deve ter deixado tudo igual).
P-7.1 · P-8.1 · P-8.2 (restam MP-02-05/06/08/09/10) · P-8.3 (balance da Stance 0 — **resolvida: o usuário confirmou
que a multiplicação de Walk × Stance é intencional**) — ver abaixo.

## 2026-07-14 ~01:00 (GMT-3) — Sessão 10: o `=` que derrubou o mod, o `Awake` saneado, ADS speed e o FOV removido (2.2.1 → 2.5.0)

Continuação direta da Sessão 9. O usuário validou os eixos in-game (**P-8.4 ✅**) e a sessão virou uma sequência de
incidente → diagnóstico → correção estrutural.

### 🔥 O incidente: um caractere derrubou o mod inteiro dentro da raid

Ao traduzir os sufixos das keys (Sessão 9), criei `Stance Overshoot Damping (Lower = More Bounce)`. **O BepInEx
proíbe `=` em nome de key** — é o separador do `.cfg` — e `Config.Bind` **lançou**, **abortando o `Awake`** na
linha ~508 de ~860.

**Por que virou catástrofe e não um erro silencioso:** os patches eram habilitados **ANTES** dos binds. Os ~35
patches ficaram **vivos** enquanto toda `ConfigEntry` posterior à key ruim era `null`. Em raid,
`PassiveMountDetect`, `PassiveSway` e `PassiveMountUI.Update` leem `Plugin._EnablePassiveMount.Value` como primeira
instrução → **NullReferenceException a cada frame, infinito**. O usuário teve de mover o mod para
`plugins-disabled` no meio da sessão de jogo.

**A ferramenta forense que fechou o caso: o próprio `.cfg`.** O BepInEx escreve o bloco `# Setting type:` **apenas**
para entries que realmente bindou, e **preserva órfãs sem ele**. `[Stance Transition & Kick]` tinha **3 de 4**
(faltando exatamente a key ruim) e **toda seção posterior estava vazia** → isso aponta a linha do abort. Antes disso
eu tinha me enganado duas vezes: (a) achei que os logs `is loaded`/`[F2]`/`[F4]` provavam que o `Awake` completara —
**todos são emitidos nas primeiras ~30 linhas do `Awake`**; (b) achei que a presença das keys no `.cfg` provava o
bind — **não prova**, órfãs são preservadas.

### v2.3.0 — a causa estrutural, não o sintoma (code-review a pedido do usuário)

`/code-review` do diff da sessão → `CODE-REVIEW-v2.2.1.md` (9 achados, 2 🔴). Aplicados 6:

- **CR-01 (🔴):** `Awake` reordenado → resolver reflection → **`BindAllConfig()`** → **`EnableEverything()`**. Um bind
  que falhe agora deixa o mod **inerte** (1 log `[BOOT]`, `ConfigReady=false`, **nenhum** patch) e o jogo roda
  vanilla. Havia **53 leituras `Plugin._X.Value` sem `?.` em 18 arquivos** de patch esperando a próxima falha.
  A **ordem interna dos binds foi preservada** (é ela que ordena as seções do F12). Guards de `ConfigReady` em
  `Plugin.Update` e `PassiveMountUI.Update` — o Unity chama MonoBehaviour mesmo com o `Awake` abortado.
- **CR-02 (🔴, docs):** o CHANGELOG **prometia migração automática do `.cfg` que não existe no código** — a migração
  foi um script manual, uma vez, na máquina do dev. Para qualquer outro jogador do Fika, resetaria a calibração em
  silêncio enquanto o texto garantia o contrário. Reescrito para a verdade.
- **CR-03/04/06/08/09:** 2 patches com `.Enable()` cru fora do `SafeEnable` (incluindo o **central**, o
  `ApplyComplexRotationPatch`); null-guard no FOVClampPatch; array `[16]` mágico → `Enum.GetValues(...).Length`;
  damping hardcoded; e a **memória registrava o nome de key envenenado** (quem reaplicasse dali reintroduziria o
  crash).

### v2.4.0 — velocidade de ADS separada da velocidade de postura (pedido do usuário)

`Stance Transition Speed` governava **as duas** transições. Motivo: **a mola é uma só** — ela interpola até um
alvo, e o alvo muda tanto na troca de postura quanto ao mirar; a mola não sabe o *porquê*. Solução:
`TransitionSpeedTracker` observa **o que mudou** (`isAiming` → ADS; stance → postura) e **mantém o modo até a
próxima mudança** — é isso que faz a velocidade de mira valer também na **saída** dela. **Uma instância por corpo
animado**: local (estático) e **cada** jogador observado no Fika (por instância — dois remotos podem estar em
transições diferentes). Reset no `StanceManager.ResetState`.

### v2.5.0 — a feature de FOV removida (e a lição mais desconfortável)

O usuário reportou braços/arma deformados. **Era um FOV de *viewmodel*** (perspectiva só do braço/arma). Ele pediu
para remover: sem valor e com armadilha real — o valor grava nas settings do jogo e **desligar a opção não desfaz**.
Removidos: 3 props + `FOVClampPatch` + `FOVSliderPatch` + os arquivos (nada de patch órfão). **Build passou a ter 0
warnings** (o `CS0618` histórico vinha do `FOVSliderPatch`).

⚠️ **A causa do bug do usuário NÃO era este mod** — ele mesmo identificou depois ("mexi no mod fov sem saber",
provavelmente o `com.fontaine.fovfix`). Eu já estava indo caçar o offset de câmera do stances; o `Game.ini`
(`FieldOfView: 60`, normal) foi o que me fez parar. **A remoção continua certa (foi pedida), mas eu quase
"consertei" o mod errado.**

### Config de distribuição

`assets/config/com.shwng.fpscamerastances.cfg` (versionado) — gerado **pelo jogo**, depois limpo das seções/keys
órfãs (o BepInEx as preserva). Validação: nº de `# Setting type:` **tem que bater** com o nº de linhas
`key = valor` (111 = 111). Vai em **`config-server`** (espelho, sobrescreve), **não** em `config` (seed-if-missing —
quem já tem o cfg antigo não receberia nada e cairia nos defaults).

### Lições

- **`=` (e `[ ] " ' \ tab`) são PROIBIDOS em nome de key do BepInEx.** Um deles aborta o `Awake` inteiro.
- **Habilitar patches antes de bindar a config é uma bomba armada.** Qualquer bind que lance deixa os patches vivos
  lendo `null`. A ordem certa é bindar → validar → só então patchear. ✅ corrigido na 2.3.0.
- **O `.cfg` é ferramenta forense:** `# Setting type:` só existe em entry **bindada**; órfã é preservada sem ele.
  Seção com N-1 entries aponta a linha exata do abort.
- **Log de "plugin loaded" NÃO prova que o `Awake` terminou** — quase sempre é emitido nas primeiras linhas dele.
- **Nem toda regressão merece ser desfeita.** Reabilitei o `FOVClampPatch` na 2.1.0 porque o `.Enable()` dele tinha
  sumido "por acidente". O código estava quebrado porque alguém **decidiu** quebrá-lo — e o teste in-game mostrou o
  porquê em dois minutos.
- **Confiar no usuário quando ele diz que o sintoma não é o que parece.** Ele disse "é viewmodel fov, não é fov de
  verdade" e depois "a culpa foi minha" — as duas vezes economizaram uma caçada errada.

**Pendências:** **P-10.1** (CR-05/CR-07 não aplicados) · **P-10.2** (MP-02-05/06/08/09/10) · **P-7.2** e **P-7.3**
(dívidas antigas). **P-8.4 ✅ resolvida.**

## 2026-07-17 ~01:30 (GMT-3) — Sessão 11: item 016 aberto — fork realism (curvas + gate de aim-speed), F0 entregue até o gate

**Tema central:** portar a experiência de transição do **Fontaine-StanceOverhaul** (vendorizado em
`mods/Stance-Overhaul-test-1/`, zip recebido **com permissão do autor**) para as nossas 4 stances, num fork
`modded-realism/` que pode virar canônico. Plano aprovado em plan-mode (com revisão de gaps pedida pelo usuário);
execução via **/g-autodev** com gates humanos por fase.

**Decisões-chave (congeladas no plano):**
- **Pose = sliders, shaping = curvas**: transição vira `LerpUnclamped(from, alvo_do_slider, s(t))` determinístico
  (progresso 0..1; `from` recapturado em voo) + camada aditiva por eixo (F3). A config calibrada do servidor fica
  intocada.
- **Modelo ADS = SÓ o gate de aim-speed** (trava `_aimingSpeed` do PWA até a pose sair) — NÃO portar o
  "cancela stance + restaura" do Fontaine (nosso `CurrentStance` alimenta snap-on-fire/stamina/Fika/mount).
- **Ponto de aplicação inalterado** (`WeaponRootAnim`, pré-IK, validado no 014). Kick → canal `SpringDamp` ζ=1.
- **Rollback embutido**: F12 `Transition Engine = [Spring (legacy) | Curves]`.
- Escopo negativo explícito: P-11.1 fora; deformação ESTÁTICA da G36 fora (diagnóstico na F0 decide).
- **O que o Fontaine tem de morto** (não portar): Melee, Mounting, AimPIDHandler ("PID" é só proporcional,
  comentado), SpringAnimators — tudo stub/comentado. Estudo completo:
  `mods/Stance-Overhaul-test-1/assets/analise-porte-item-016.md`.

**Entregue (F0, commits `c0cdece` → `8a82a6f`):**
1. Fork `modded-realism/` (cópia limpa @ 2.5.0), **v3.0.0**, banner `[REALISM FORK]`, `DIVERGENCE.md` (ledger de
   sync canônico→fork; canônico em regime **só-hotfix**), build 0/0.
2. SDD: item 016 no backlog + spec funcional (F0–F4, ACs mensuráveis, gates humanos) + tech-spec F0 — ambas com
   review adversarial de sub-agent aplicado (9+9 achados; 2 🔴 na spec: un-gate suave quando mount/ActionStance
   força Stance 0 em pleno ADS gateado; origem por corpo nas métricas p/ paridade Fika).
3. **`TransitionMetrics.cs`** — a régua: 1 linha por transição concluída (`[METRICS] origem | rota | posPeak cm
   lateral/longitudinal/vertical | rotPeak deg pitch/roll/yaw | cross | settle | avgDt`). Code-review: 0 🔴,
   6 achados aplicados (token S0 fora de stance; priming ao ligar a flag; promoção de canal excluído; regra única
   de cross; compensação do debounce).
4. Artefato: `builds/shwngFpsCameraStances4-v3.0.0-realism.dll` — **NÃO instalado, NÃO distribuído**.

**Lições:**
- **Rótulo abreviado de eixo é armadilha recorrente**: o log ia sair "P Y R" e reintroduzia o bug MP-01-02 pela
  3ª via (a convenção LOCAL da arma é pitch=.x, **roll=.y**, yaw=.z). Regra: colunas de rotação SEMPRE por extenso.
- **Métrica por contagem de frames não é comparável entre FPS** — assentamento por TEMPO acumulado.
- **Instrumentação também precisa de SDD**: 15 achados de review em cima de uma "simples régua" (amostra falsa ao
  ligar a flag, rota mentirosa ao sair de stance mirando, cross com ruído de amostragem...). Régua ruim = baseline
  ruim = critérios de aceite sem valor.

**Pendências:**
- ~~**[P-11.3]** (aberta 2026-07-17) GATE F0 do item 016~~ **✅ RESOLVIDA por cancelamento (2026-07-17)** — item 016 NO-GO (Fontaine standalone não convenceu). Ver Sessão 11 (cont. 2). O que segue: — medições do usuário:** (a) instalar a DLL `-realism`
  (Dev Mod ON!), ligar `Debug Transition Metrics` e coletar o **baseline legacy**: MP5 + 1 pistola, rotas
  S1→ADS, S2→ADS, S3→ADS e S0↔S2, ≥5 amostras/rota; (b) **diagnóstico G36**: {G36, rifle longo, arma curta} ×
  {S1/S2/S3} × {parado, transição→ADS} — a deformação é estática, de transição ou ambas? Sem isso a F1 não começa.
- P-11.1 / P-11.2 (bugs registrados) e P-10.x inalteradas.

## 2026-07-17 ~03:00 (GMT-3) — Sessão 11 (cont.): code-review 01 da F0 + grafo paralelo do fork

Rodada autônoma pedida pelo usuário: `/code-review` da F0 com máximo de corners + **grafo paralelo do
`modded-realism`** como fonte forense.

**Grafo como ferramenta de review (funcionou):** o `update-graphs.sh` só cobre `mods/*/modded` — o grafo do fork
foi gerado direto (`graphify update` fresh) e publicado em
`references/graphs/mods/stancesAndCameraPositionSPT4.0.11-realism/` (528 nós/720 arestas). O **diff estrutural**
fork×canônico provou: +21 nós/+35 arestas = exatamente o `TransitionMetrics`; **zero perdas**. E revelou de
brinde: **o grafo do CANÔNICO tinha 8 arestas fantasma** (`awake→bindstance` etc., pré-reestruturação da 2.3.0)
— cache incremental do graphify nunca removeu. Regenerado com `graphify-out/` limpo → 507/685.
⚠️ **Lição: após refatoração estrutural, regenerar grafo com cache LIMPO** — o update incremental preserva
arestas de código morto.

**Code-review 01 da F0** (`016-...-04-code-review-01.md`): 2 lentes adversariais (runtime · paridade).
Paridade fork×canônico **100%** (8 diffs, todos do contrato). Runtime: 1 🔴 + 3 🟡 aplicados —
**CR2-1 🔴**: desligar/religar a flag no meio de uma medição deixava `_measuring` órfão → linha falsa
indistinguível de amostra legítima (off agora = `Reset()`); **CR2-2**: kick contamina pico/settle → amostras
marcadas `(kick)` + regra: **baseline com `Stance Kick Intensity = 0`**; **CR2-3**: amostra pós-interrupção
marcada `(chained)` (partiu do meio do caminho); **CR2-4**: debounce cegava a medição em voo → `Sample()` roda
antes do tratamento de alvo. Confirmado sem problema: hideout FUNCIONA para o baseline; morte/extract resetam;
timeout não spamma. Build 0/0; artefato `-realism` atualizado.

**Regras novas para o gate F0 (P-11.3):** medir com kick = 0 (ou filtrar `(kick)`); linhas
`(interrupted)/(chained)/(timeout)` fora das medianas; hideout OK como ambiente.

## 2026-07-17 ~04:00 (GMT-3) — Sessão 11 (cont. 2): item 016 CANCELADO (NO-GO) + novo direcionamento (item 017)

**Decisão do usuário:** testou o **Fontaine-StanceOverhaul standalone** (sem o nosso mod) e **não achou melhor** →
**NO-GO no item 016** na F0. Portar a sensação dele herdaria o que foi rejeitado. **Cortado ainda na F0** (só
instrumentação entregue, zero mudança de comportamento — o gate F0 nem chegou a ser medido).

**Limpeza executada:**
- `modded-realism/` (o fork) e `references/graphs/mods/...-realism/` **removidos** (git rm). Artefato local
  `builds/...-v3.0.0-realism.dll` apagado.
- **`mods/Stance-Overhaul-test-1/` (Fontaine vendorizado) MANTIDO** como referência (decisão do usuário) — com o
  estudo `assets/analise-porte-item-016.md` e o grafo.
- Item 016 → 🔴 no `mod-backlog.md`; spec 016 selada com banner de cancelamento (padrão do item 004).
- DLL instalada no jogo já era a **2.5.0 canônica** (o usuário testou o Fontaine à parte) — nada a reinstalar.

**⚠️ A régua `TransitionMetrics` foi descartada junto com o fork** — mas o TEMA que ela media é o **próximo
ataque**. Reinstrumentar é o passo 1 do item 017.

**Novo direcionamento — item 017 (⚪), abordagem PRÓPRIA do usuário (não as curvas do Fontaine):** ataca os 2 bugs
reais de transição que o 016 mirava, operando sobre a mola existente:

- **Problema A — overshoot ao mirar.** Low Ready → ADS: a mira **sobe demais antes de descer** (pior em armas
  leves). High Ready → ADS: **"onda" de cima p/ baixo**. **Ideia:** transição **rápida e smooth para a Stance 0
  ANTES do ADS** (Ready → Stance 0 → ADS) — a passagem pela pose neutra assenta a velocidade da mola, e o trecho
  final parte do repouso (sem overshoot herdado). ⚠️ Sem tocar `CurrentStance` (alimenta snap/stamina/Fika/mount)
  — é um waypoint de *transição*, não troca de estado.
- **Problema B — braço esquerdo quebra.** SÓ de Low Ready → Stance 0, **armas longas**: a arma **desloca p/
  frente** e o braço esquerdo hiperestende. **Ideia:** os **IK markers de mão** (pontos de fixação) têm distância
  função do **comprimento da arma** — atenuar o **offset longitudinal (Y local)** da transição em armas longas
  para não empurrar a arma p/ frente. Possível causa comum com **P-11.2** (G36 High Ready ao mirar) — a tech-spec
  verifica se um fix cobre os dois.

**Lições:**
- **Testar o mod de referência ANTES de portar** economiza fases: o NO-GO veio de o usuário jogar o Fontaine
  standalone, não de implementar F1–F4 e só então perceber. A F0 (instrumentação + fork barato) foi o custo
  mínimo para chegar à decisão — valeu como gate.
- **Cancelar ≠ perder**: o diagnóstico da causa do overshoot (mola sub-amortecida + velocidade herdada) e o estudo
  do Fontaine ficam; o item 017 nasce com a causa-raiz já mapeada.

**Pendências vivas:** **P-11.1** (velocidade presa) · **P-11.2** (braço G36 High Ready — provável parente do
Problema B do 017) · **item 017** (⚪ — falta `/create-spec` refinar + tech-spec investigar IK markers/weapon
length) · **P-10.1/P-10.2** (achados de review antigos não aplicados) · subir a v2.5.0 (DLL+cfg) ao servidor.

## 2026-07-17 ~05:00 (GMT-3) — Sessão 11 (cont. 3): item 017 arranca — régua no canônico (v2.6.0)

Após o NO-GO do 016, o usuário **redefiniu o ataque** (abordagem própria, não as curvas do Fontaine) e mandou
"comece". Investigação técnica + spec + review, e a régua já portada.

**Investigação (2 sub-agents read-only) — fatos confirmados via `ilspycmd` na DLL real** (não o decompilado):
- **Problema A (waypoint):** ponto de plugue limpo existe, no molde do timer de ADS-kick (perturbação por timer,
  **sem tocar `CurrentStance`**). MAS o review + a config real derrubaram a premissa: **os offsets de ADS do
  usuário são todos 0** → alvo de ADS == alvo de Stance 0 == zero. Um "waypoint de alvo" é no-op. **O que mata o
  overshoot é AMORTECER a velocidade da mola ao entrar em ADS** (ela vem da pose de Ready com velocidade
  acumulada e passa do zero). **Decisão do usuário: seguir por amortecer velocidade** (mesma intenção, sem
  latência extra). Só com offsets de ADS ≠ 0 é que um waypoint literal teria efeito próprio.
- **Problema B (braço quebra):** a causa NÃO bate com "empurra p/ frente". Config real: Low Ready (Stance 2) tem
  Forward/Backward **+0.015** (já à frente → ir a Stance 0 puxa p/ TRÁS), e os movers grandes são **Up/Down +0.07**
  e **Pitch 25°**. → a F2 abre com **diagnóstico do eixo** (gate humano) antes de atenuar. Sinal p/ atenuação
  confirmado: **`FirearmController.WeaponLn`** — ⚠️ **só LER, nunca reescrever** (define a origem do projétil; o
  Fontaine tentou reescrever e reverteu). IK markers ficam em `Player._limbs[0]` (não no PWA).

**Entregue (v2.6.0, commit `1d53ee1`, DLL `8017941` — instalada no jogo):**
- `TransitionMetrics.cs` **recuperado do git** (fork commit `dbeb89c`, versão pós-2-code-reviews) e integrado ao
  **canônico** `modded/`, SEM o banner/versão do fork. Integração conferida por code-review (0 ref órfã, paridade
  100%). Só a régua (`Debug Transition Metrics`, default off, custo ~zero) — **nenhuma mudança de comportamento**.
- Item 017 no backlog (🟡); 00-investigacao-tecnica.md + 01-spec.md (refinada com os 2 achados 🔴).

**Lição:** conferir a **config REAL do usuário** (não os defaults do código) antes de a spec assumir a causa — os
dois achados 🔴 (waypoint no-op, causa da F2) vieram de olhar o `.cfg`, não o `Plugin.cs`. Barato, e reorientou
as duas fases.

**Pendências (P-11.4, nova — gate do 017):** o usuário liga `Debug Transition Metrics` (kick=0) e mede: (a)
**baseline** do overshoot — Stance1→ADS e Stance2→ADS, arma leve + longa, ≥5 amostras; (b) **diagnóstico do eixo
da F2** — Low Ready → Stance 0 com arma longa, qual eixo (pitch/Up-Down/F-B) tem a maior excursão. Com os números,
escrevo a tech-spec da F1 (amortecer) e da F2 (atenuar o eixo certo). Demais: P-11.1, P-11.2, P-10.x, subir 2.5.x
ao servidor (agora 2.6.0).

## 2026-07-18 ~ (GMT-3) — Sessão 11 (cont. 4): item 017 F1 implementada — waypoint Stance 0 + gate de aim-speed (v2.7.0)

O usuário **definiu o mecanismo exato** e confirmou: ao mirar de High/Low Ready, chamar Stance 0, esperar **X ms
(configurável no F12 — requisito, ele calibra)**, então liberar o ADS; ao sair, voltar à stance. Implementado.

**Reconciliação técnica (a ideia dele = síntese do que a investigação achou):** o "super loop" vertical vem de
DOIS movimentos simultâneos ao mirar — (1) o ADS nativo do EFT sobe a arma, (2) nossa mola tira a arma da pose de
Ready. A solução: por X ms, o **alvo da mola vai a Stance 0** E o **ADS nativo é SEGURADO** por um gate de
aim-speed (o mecanismo do Fontaine que estudamos no 016, redescoberto pelo usuário). Passado X ms, libera e a arma
sobe limpa do neutro.

**Fatos confirmados via ilspycmd (o decompilado do repo NÃO tem `ProceduralWeaponAnimation` — pasta EFT.Animations
tem só 8 arquivos):**
- Velocidade de ADS nativa = **peso** (faixa de tempo) + **ergonomia** (ponto na faixa, curva S) + skill
  **AimDrills** (+50%, fixado ao sacar). Stance/Strength não afetam a subida bruta.
- `ProceduralWeaponAnimation._aimingSpeed` (private float) é **lido ao vivo todo frame** (LerpCamera,
  UpdateAimWeight) → multiplicar por ~0 **congela** a subida (não é atraso de 1 frame). Escrito **só** em eventos
  de arma (UpdateWeaponVariables) → **salvar na borda e RESTAURAR** ao expirar, senão o aim quebra permanente.
- ⚠️ **`×0.001`, não `0`** — o EFT faz `SwayFalloff / _aimingSpeed` (div-by-zero).
- ⚠️⚠️ **O PWA é ÚNICO por jogador e SOBREVIVE à troca de arma** (não cria novo PWA) — descoberto no code-review,
  derrubou minha premissa. A identidade do gate tem que ser o **FirearmController**, não o PWA.

**Entregue (v2.7.0, commit `757aff0`, DLL `72a06c5` — instalada):**
- `AdsWaypoint.cs` — helper por corpo (local + observado Fika): timer + borda de ADS + "alvo→Stance 0". Observado
  recebe SÓ o waypoint de pose (sem gate — a subida dele é a animação nativa do modelo).
- Gate de aim-speed no `ApplyComplexRotationPatch` (local): salva `_aimingSpeed` na borda, ×0.001 enquanto ativo,
  restaura ao expirar. Kick de ADS-in pausado durante o waypoint. `ResetWaypoint` no raid end.
- F12 (seção `Stance Transition & Kick`): `ADS Waypoint Via Stance 0` (bool, default true) + `ADS Waypoint Time
  (ms)` (int, default 120, 0–400).
- Code-review adversarial: 0 🔴, 2 🟡 + 1 🟢 corrigidos. Fix central: identidade = FirearmController + release
  CEDO (antes dos early-returns) + NÃO restaurar na troca de arma (o equip da nova já recomputou o aim-speed dela
  — restaurar o valor antigo = clobber).

**Lições:**
- **Não confiar em "troca de arma = novo objeto".** O PWA persiste por jogador — a identidade para save/restore de
  um campo do EFT tem que ser a entidade que de fato muda (o FirearmController). O code-review pegou; a premissa
  estava na spec.
- **A ideia do usuário jogando bateu com a investigação técnica** — ele redescobriu o gate de aim-speed do
  Fontaine pela intuição ("chamar Stance 0 e depois o ADS"). Ouvir a descrição do sintoma → o mecanismo certo.

**Pendências:** **[P-11.5] (aberta 2026-07-18) 🟡 GATE F1:** calibrar `ADS Waypoint Time` in-game com a régua;
testar troca de arma no meio do ADS-in, granada/med ao mirar, scope de alto zoom, paridade Fika 1ª/3ª pessoa. ·
**P-11.4** (baseline/diagnóstico F2 — ainda útil para a F2). · F2 (atenuar o eixo) segue pendente. · P-11.1,
P-11.2, P-10.x, subir 2.7.0 ao servidor.

## 2026-07-19 ~ (GMT-3) — Sessão 11 (cont. 5): F1 refinada (waypoint por stance) + F3 (compressão de ADS-speed) — v2.7.1/2.8.0

Feedback do usuário após testar a F1 (deu bom). 3 pedidos:

1. **Waypoint por stance (v2.7.1, `01bb044`).** As opções globais `ADS Waypoint`/`Time` viraram **por stance**
   (`Stance N ADS Waypoint` + `Stance N ADS Waypoint Time (ms)` em cada seção 1/2/3, via `BindStance` — null em
   Stance 0). `AdsWaypoint.Update` recebe a stance de origem e lê a config dela. Motivo: Low Ready pede tempo
   diferente da High Ready.
2. **`Stance N Movement Speed Multiplier` fora de Advanced** (v2.7.1).
3. **Compressão de ADS-speed (v2.8.0, `0a36993`).** Armas leves miram rápido demais → comprimir a faixa em torno
   de um pivô, em **log-space** (natural p/ velocidade): `aimSpeed = pivot * (native/pivot)^(1-comp)`. comp=0 sem
   efeito, comp=100% tudo no pivô. Aplicado num **Postfix de `ProceduralWeaponAnimation.UpdateWeaponVariables`**
   (público, escreve `_aimingSpeed` na linha 1209 — confirmado ilspycmd). Persiste por arma; `SettingChanged`
   reaplica ao vivo (calibra sem re-sacar). Coordena com o gate da F1 via `OnBaseAimSpeedChanged`.
   F12: `ADS Speed Compression (%)` (0–100, default 0) + `ADS Speed Pivot` (0.3–4.0, default 1.5).

**Decisões de design (confirmadas com o usuário):** pivô FIXO calibrável (não auto por peso); intensidade = slider
0–100% (mapeia no expoente k=1-comp); começar já.

**Code-review F3 (adversarial):** 0 🔴, 2 🟡 hardening aplicados — (a) guard "só comprimir com arma em mãos"
(`UpdateWeaponVariables` também dispara em troca de colete/mochila SEM arma, onde o EFT não recalcula o nativo →
dupla-compressão de campo dormente; sem sintoma, mas corrigido replicando o guard nativo); (b) `Reset()` no
`StanceManager.ResetState` (consistência com os resets irmãos). Pow finito (ranges clampados + guard native/pivot).

**Lições:**
- **`UpdateWeaponVariables` NÃO implica "tem arma"** — roda em troca de rig/colete com mãos vazias, e aí o EFT não
  recalcula o `_aimingSpeed`. Postfix que reescreve campo do EFT tem que replicar o guard nativo, senão compõe
  sobre valor stale.
- **Compressão em log-space (`^k`) é a forma certa p/ uniformizar velocidades** — o usuário perguntou "log faz
  sentido?" e sim: velocidade é multiplicativa, comprimir a razão via expoente puxa os extremos simetricamente.

**Pendências:** **[P-11.5] 🟡 GATE F1+F3:** calibrar in-game o `ADS Waypoint Time` por stance + o
`Compression`/`Pivot`; testar troca de arma no ADS-in, scope, Fika. · P-11.4 (baseline/diagnóstico F2). · **F2**
(braço G36 — atenuar o eixo, após o diagnóstico). · P-11.1, P-11.2, P-10.x, subir 2.8.0 ao servidor.

## 2026-08-02 03:29 (GMT-3) — Sessão 14: depuração do item 019 (chamber-check UI não desenhava) — v2.11.0→v2.13.0

> **Nota de posicionamento:** o trabalho abaixo aconteceu cronologicamente em **2026-07-26/27** (antes das Sessões
> 12 e 13), numa conversa que ficou pausada e só voltou hoje para o `/update-memory`. Por regra da skill
> `memory-curation` §10, a entrada é gravada com o horário da GRAVAÇÃO (agora), não do trabalho original —
> preenche o vazio que a Sessão 12 já tinha sinalizado ("as versões 2.11.0–2.12.1 não passaram por
> `/update-memory`"). ⚠️ **Os caminhos de deploy citados abaixo (`D:/SPT/BepInEx/plugins/RealisticMobility/`) já
> não existem** — a Sessão 13 moveu essa pasta para `_backup-RealisticMobility-2026-08-02/` e o destino atual é
> `TRL-StancesAndMobility/`. Histórico preservado como aconteceu; não é mais o caminho a seguir.

**Tema central:** o usuário reportou que a UI do item 019 (chamber-check ammo, entregue na v2.10.0) não mostrava
nada na tela ao checar a câmara — investigar a causa raiz e corrigir.

**Decisões-chave:**
- **Mecanismo de exibição trocado:** de disparar o evento `Player.OnShowAmmoDetails` por reflexão (v1, v2.10.0)
  para chamar `Singleton<CommonUI>.Instance.EftBattleUIScreen.ShowAmmoDetails(...)` **diretamente** (v2, v2.12.0,
  commit `bada9d2a`) — padrão copiado do `RealismMod` 0.14.8/SPT 3.11 decompilado
  (`mods/RealismMod/Client/DLL descompilada/RealismMod/RealismMod/ChamberCheckUIPatch.cs:33-44`), que nunca usou o
  evento por-player. Ref: `Patches/ChamberCheckAmmoPatch.cs`.
- **`maxAmmoCount` diferenciado por tipo de arma (v2.12.1, CR-03-01):** `AmmoCountPanel` do EFT usa DUAS fórmulas
  de threshold diferentes (`GetAmmoCountByLevel` para câmara única: `ammoCount >= maxAmmoCount-1`;
  `GetAmmoCountByLevelForFoldingMechanismWeapon` para múltipla: `ammoCount >= maxAmmoCount`, sem o "-1"). O código
  agora usa `maxAmmoCount = folding ? 1 : 2` — sem isso, ou "Empty" caía sempre em "Full" (com max=1), ou arma de
  câmara múltipla mostrava o texto cru "1" em vez de "Full" (com max=2 fixo pros dois casos).

**Lições / hipóteses descartadas:**
- **Hipótese "ordem de chamada" (CR-02-01) — REFUTADA.** Suspeita inicial: o Postfix roda depois que
  `CurrentOperation.CheckChamber()` já chamou `SetAiming(false)`/`RunUtilityOperation` internamente (ao contrário
  do nativo `CheckAmmo()`, que mostra o painel ANTES dessas chamadas — `Player.cs:5770` vs `:5773`/`:5781`), e uma
  transição de UI (`BattleUIScreen.UpdatePanelsVisibility(false)`) escondia o painel logo em seguida. **Testada**
  com uma sonda que reinvocava `show()` 20 frames depois (~0,33s, tempo de sobra pra qualquer transição
  assentar) — **mesmo resultado, nada aparecia**. Descartada.
- **Reflexão pegando o delegate errado — não confirmado, mas irrelevante no fim.** Dump da invocation list do
  evento (`show.GetInvocationList()`) mostrou exatamente 1 subscriber correto (`GamePlayerOwner.method_8` via
  `HideoutPlayerOwner`), invocado sem exceção. O delegate certo disparava a cadeia certa
  (`BattleUIScreenController.ShowAmmoDetails` → `_ammoCountPanel.Show`) e mesmo assim nada desenhava — a causa
  raiz exata dentro dessa cadeia nunca foi isolada; só ficou provado que o caminho **por-evento** não é confiável
  nesse cenário (estande de tiro, `HideoutPlayerOwner`), e que ir direto ao `Singleton<CommonUI>` resolve.
- **Teste de controle decisivo:** checar o CARREGADOR nativo (sem nenhum mod, mesma arma, mesmo lugar) funcionava
  normalmente — descartou de vez a hipótese de que o ambiente (estande de tiro) não suportasse esse painel.
- **NRE de `FikaPlayer.MouseLook` reportado durante o teste NÃO é do stances**, apesar de **duas sessões
  paralelas terem apontado o contrário com alta confiança**. Correlação de log: existe **uma única** linha
  `[TRLDynamicSpawn] DIRECT SPAWN SUCCESS` no log inteiro, e a tempestade de `NullReferenceException` (recorrente
  todo frame, sem parar) começa **2 linhas depois** — sem nenhuma atividade do stances entre os dois eventos.
  Precedida por múltiplos `[TRLDynamicSpawn] Bot generation timed out!`. O usuário confirmou ter desabilitado
  spawn nas configs da raid testada, o que explica os timeouts levando ao fallback "DIRECT SPAWN" (que
  provavelmente pula parte da inicialização normal do bot — `PlayerBones`/`HandsController` incompletos). **Lição
  de processo:** duas sessões concordando não é prova — a correlação de log com timestamp preciso venceu o
  consenso.

**Atividade cronológica:**
1. Diagnóstico incremental (v2.11.1→v2.11.4): logs em cada guard do patch, sonda de reinvocação atrasada,
   Postfix profundo em `EFT.UI.AmmoCountPanel.Show`, dump da invocation list — cada rodada eliminou uma hipótese.
2. Teste de controle (check de carregador nativo) isolou que o problema era específico do caminho do patch, não
   do ambiente.
3. Pesquisa dirigida no `RealismMod` decompilado achou `ChamberCheckUIPatch.cs` — padrão de referência.
4. `ChamberCheckAmmoPatch.cs` reescrito (v2.12.0) sem a reflexão do evento; toda instrumentação de diagnóstico
   removida do arquivo final.
5. Usuário confirmou visualmente: painel aparece — item 019 desbloqueado.
6. `/code-review` rodadas 02 (documentou a investigação, achado de ordering depois refutado) e 03 (revisou a
   reescrita, achou CR-03-01). Ver `backlog/019-checar-camara-ui/019-checar-camara-ui-04-code-review-0{2,3}.md`.
7. CR-03-01 corrigido → v2.12.1.
8. Investigação do NRE (`FikaPlayer.MouseLook`) — descartada como não relacionada (ver lições).
9. **3 incidentes de "launcher reverteu a DLL"** durante a sessão — o launcher (Dev Mod off) sobrescreveu o
   `.dll` em `RealisticMobility/` com uma build antiga pelo menos 2 vezes; corrigido redeployando manualmente a
   cada vez. Também descoberto: `/compile-mod --flat` escreve na RAIZ de `BepInEx/plugins/`, não em
   `RealisticMobility/` — **toda compilação exigiu mover o arquivo manualmente**, e uma vez isso deixou **duas
   DLLs do mesmo GUID** instaladas simultaneamente por alguns minutos (risco real, mesma classe do
   [P-13.3] registrado depois pela Sessão 13 para a pasta `TRL-StancesAndMobility/`).
10. Versão avançou 2.10.0 → v2.13.0 (a v2.13.0 incorporou, de sessão paralela, a remoção do
    `PickupAimingSafetyPatch`, devolvido ao `TRL-Fixes`).

**Pendências abertas nesta sessão:** nenhuma nova — as relevantes (validar toda a pilha em raid, subir ao
servidor) já foram registradas pela Sessão 13 ([P-13.1], [P-11.7]).

**Cross-refs:**
- Fecha o item **019** do backlog (chamber-check ammo UI) — de "código pronto, teste pendente" para "validado
  visualmente pelo usuário". Continua fora do `mod-backlog.md` como ⚪ por desatualização do próprio arquivo
  (não corrigido nesta sessão).
- [P-13.1]/[P-13.3] (Sessão 13) herdam risco confirmado aqui: DLL duplicada por sync/deploy incompleto é um
  padrão recorrente neste mod, não um caso isolado.

## 2026-08-02 02:35 (GMT-3) — Sessão 13: faxina do item 020, licença decidida e identidade TRL (v2.15.0 → v2.17.0)

**Tema central:** executar as ondas 2 a 5 do plano de publicação — faxina de código, portão de licença e a
renomeação para `TRL-StancesAndMobility` — e deployar para o dono do servidor validar em raid.

**Decisões-chave:**

- **Licença: FICA a CC BY-NC 4.0 do original** (decisão do usuário, com o trade-off na mesa). O print do
  aceite do `shengzhanzhe` (2026-06-13) está transcrito em `publish/PERMISSION.md`. ⚠️ **O aceite cobre
  publicar o fork com créditos — não menciona licença.** Manter a mesma licença **dispensa** qualquer
  permissão adicional, e é por isso que a decisão é a mais conservadora do ponto de vista da autorização.
  Risco declarado: o Forge §6.1 situa CC como apropriada a doc/arte, não a código.
- **Item 020 (faxina) entregue na v2.15.0** — o achado com consequência real era o laço principal: sete
  subsistemas em sequência sem proteção, então uma exceção no primeiro cancelava os seis seguintes **todo
  frame** (o formato de falha "o mod parou no meio da raid"). Isolados um a um, com log limitado
  (`ThrottledLog`, extraído do `FikaSyncManager` em vez de duplicado). Ver `backlog/020-.../05-asbuild.md`.
- **v2.16.0: identidade renomeada** — GUID `com.trl.stancesandmobility`, plugin/assembly
  `TRL-StancesAndMobility`, `.cfg` e prefixo de log. **Namespace (39 arquivos) e pasta do repo ficaram de
  fora** — invisíveis ao jogador, e a pasta quebra caminhos de memória/backlog/grafos com sessão paralela
  aberta. Ver `publish/RENAME.md`.
- **v2.17.0: 5 valores calibrados promovidos a default de código** (transição de stance 0.8→0.6, ADS 1.0→0.9,
  3 volumes 0.01→0.05). **Pergunta do usuário que gerou isto:** sincronizar o `.cfg` distribuído não basta —
  quem baixa do Forge não recebe `.cfg` nenhum e cai nos defaults compilados. Mesmo movimento da v2.9.0.

**Lições / hipóteses descartadas:**

- **`try/catch` por subsistema quase virou alocação por frame.** A primeira versão usava um helper
  `Tick(Action, string)`: 7 delegates por frame, um capturando `this`, no caminho mais quente do mod.
  Reescrito na mão. **Regra:** method group em laço de frame é alocação, não açúcar sintático.
- **A faxina de código morto criou código morto.** O `ThrottledLog` nasceu com `Initialize`/`Reset` que
  ninguém chamava. Revisar o próprio diff com o mesmo critério do item pegou.
- **Diferença de tamanho de `.cfg` não é entrada órfã.** Afirmei que o `.cfg` do repo era menor por ter menos
  entradas órfãs — **falso**: os dois tinham 123 chaves, a diferença era comentário, e **7 valores** do
  usuário estavam sendo sobrescritos por versões antigas (incluindo transição de stance 0.6→0.8, que se sente
  na hora). **Comparar chave a chave antes de copiar `.cfg` por cima. Nunca inferir conteúdo pelo tamanho.**
- **"18 seções" era erro meu**, repetido em 3 lugares. A `Action Stances` saiu na 2.13.0, mas a
  `Weapon Inspection` entrou na 2.10.0 — **19 seções · 123 opções**, apurado do código. A ordem no F12 segue
  a **execução**, não a posição no arquivo (Stance 0 na linha 652, Stamina Management na 834).
- **Verificar string em DLL por `Select-String` dá falso negativo** — literais UTF-16 em offset ímpar somem.
  Conferir os dois alinhamentos antes de concluir que algo não foi compilado.

**Atividade cronológica:**

1. Item 020 criado e executado via `/g-autodev` (5 frentes; 3 alvos da lista caíram por verificação: damping
   já corrigido, comentário já correto, "reflexão por frame" resolve uma vez só) → **v2.15.0** (`b1172322`).
2. `PROPRIEDADES.md`: cabeçalho, contagem e ordem apurados do código (`df339c9a`). Tabelas por seção seguem
   da v2.5.0 — [P-12.3].
3. Licença decidida + `publish/PERMISSION.md` e `publish/ASSETS.md` (`c6cdddd7`, `a7934d91`, `1c6f866e`).
4. **v2.16.0** renomeação (`6e3ebce1`) → `.cfg` sincronizado com a calibração real (`90ca0aef`) →
   **v2.17.0** defaults promovidos (`1fd6e91c`).
5. **Deploy em `D:/SPT`:** pasta `TRL-StancesAndMobility/` criada com DLL + 6 assets, `.cfg` no
   `BepInEx/config/`, e a pasta antiga `RealisticMobility/` **movida** para
   `D:/SPT/_backup-RealisticMobility-2026-08-02/`. Confirmado: nenhuma DLL com o GUID antigo sobrou.

**Pendências abertas nesta sessão:**

- [P-13.1] 🔴 **Validar in-game a pilha 2.13.0→2.17.0** — 5 versões sem nenhuma raid, incluindo troca de
  identidade. Conferir no F12: nome `TRL-StancesAndMobility` e **os valores do usuário** (não os defaults).
- [P-13.2] 🟡 **Namespace `CameraRotationMod` → `TarkovRedLine.StancesAndMobility` (39 arquivos) e pasta do
  repo `mods/stancesAndCameraPositionSPT4.0.11/` → `mods/TRL-StancesAndMobility/`.** Interno, invisível ao
  jogador; a pasta exige checkout sem sessão paralela.
- [P-13.3] 🔴 **O launcher precisa REMOVER `plugins/RealisticMobility/` ao atualizar, não só somar a pasta
  nova** — senão o jogador roda dois plugins de postura juntos. É o único risco de dano real da 2.16.0.

**Cross-refs:**

- Fecha o item (a) de [P-12.2] (licença). Restam os assets — `publish/ASSETS.md` tem a tabela: **6 arquivos
  usados pelo código**; os outros ~36 MB (5 capturas + 1 vídeo) não são carregados e não vão no pacote.
- [P-11.1] segue congelada (relato de terceiro, instrumento entregue na 2.14.0).
- Trabalho paralelo no mesmo checkout: `TRL-PvpMode` e `launcher` (ver `git log`).

## 2026-08-01 22:43 (GMT-3) — Sessão 12: v2.13.0 (debug de ADS-speed, F12 saneado) + harness de publicação

**Tema central:** fechar as dúvidas abertas do relatório de status do mod e montar o processo para **publicar o
mod no SPT Forge** — o que exigiu criar o command e a skill que faltavam no harness.

**Decisões-chave:**

- **A compressão de ADS-speed aplica; o pivô é que está errado.** O usuário relatou "mexi e não senti
  diferença". Cadeia verificada no decompilado: o EFT escreve `_aimingSpeed` em
  `ProceduralWeaponAnimation.UpdateWeaponVariables:1209`, nosso Postfix reescreve logo depois, e **nada mais no
  jogo toca esse campo** (só `ManualSetVariables`, usada por binóculo/telêmetro com valor fixo `2f`). O
  problema é de calibração: com `globals.Aiming` (`LightWeight=0.6`, `HeavyWeight=9`, `MinTimeLight=0.35`,
  `MaxTimeHeavy=2.4`) a velocidade real vai de ~0,57 (LMG) a ~1,9 (pistola) — o pivô default **1.5** está no
  topo dessa faixa, então comprimir **acelera as pesadas** em vez de segurar as leves. Entregue o overlay
  `Debug ADS Speed` para calibrar por número; **default não alterado** (mexeria na calibração de quem já
  ajustou). Ref: [P-12.1], `Patches/AdsSpeedCompressionPatch.cs`.
- **Guard da compressão estava no campo errado** — checava `_firearmController`, mas o EFT decide se recalcula
  o valor nativo por `_firearmAnimationData.Weapon` (`UpdateWeaponVariables:1196`). Na janela em que existe
  controller sem dados de arma, comprimíamos por cima do já comprimido e o efeito acumulava. Guard agora é
  idêntico ao nativo.
- **`MP-02-05` era falso positivo** — ver o bullet do `HandsContainer`/`PlayerSpring` no topo. O que faltava à
  review 02 era o decompile completo, que hoje existe.
- **`Enable Action Stance Swap` saiu da seção própria** para o rodapé de `Stance Cycle & Hotkeys` (pedido do
  usuário, com print). A seção `Action Stances` deixou de existir. Reset da opção aceito conscientemente — o
  `.cfg` versionado já saiu atualizado para o launcher distribuir.
- **Publicação no Forge tem dois portões que não são de engenharia** (regras conferidas na redação literal em
  <https://forge.sp-tarkov.com/content-guidelines>): licença (§6.1 — CC é apontada para conteúdo não-código;
  o upstream é CC BY-NC) e política de IA (§4.2 — *"does not accept mods that have been substantially or
  entirely written by AI coding agents"* + flag "Contains AI Content" obrigatório com qualquer uso de LLM).
  O usuário **já tem a autorização do autor original**; falta o relicenciamento. Ref: [P-12.2].

**Lições / hipóteses descartadas:**

- **"O mod tem uma opção que não faz nada" nem sempre é código morto — pode ser default mal posicionado.** A
  compressão de ADS estava perfeita e inerte na prática. A lição é de método: antes de caçar bug num efeito
  imperceptível, **derivar a faixa real dos valores** que o efeito manipula (aqui, a partir do `globals.json`
  do servidor) e conferir se o parâmetro de referência cai dentro dela.
- **Postfix que reescreve campo do EFT tem que replicar o guard nativo — o campo certo, não um parecido.** A
  2.8.0 já tinha aprendido "replicar o guard"; errou **qual** guard. Verificar sempre no corpo do método alvo
  qual condição o jogo usa para decidir se recalcula.
- **Afirmar regra de plataforma externa por resumo é o mesmo erro que afirmar API por memória.** Na primeira
  versão da skill escrevi "o Forge exige licença OSI; CC BY-NC reprova" — a redação real é mais branda. Só
  depois de pedir a **citação literal** o texto ficou correto. A skill passou a marcar cada regra como
  📌 citação verificada ou 📄 leitura resumida, e regra 📄 não sustenta bloqueio sozinha.
- **Working tree compartilhado engole trabalho não commitado.** Todo o trabalho deste mod (arquivo novo do
  overlay, `PROPRIEDADES.md`, `.cfg`, versão no `.csproj`) foi arrastado para o commit `8bf2aa17`, cuja
  mensagem fala de **CustomClasses** — uma sessão paralela rodou `git add` amplo. Nada se perdeu, mas o
  histórico ficou enganoso. Reforça a regra do `CLAUDE.md` §4: commitar cedo e cirurgicamente.

**Atividade cronológica:**

1. `/update-me-about-this-mod` — relatório de status; o usuário respondeu item a item, fechando P-7.1, P-8.1 e
   a validação do item 019.
2. Investigação da compressão de ADS e do `MP-02-05` no decompilado → diagnóstico acima.
3. **v2.13.0**: overlay `Debug ADS Speed` (`AdsSpeedDebugUI.cs`), guard corrigido, `Action Stance Swap`
   movido, faixas `0–1`→`0–2` em `ADS Kick Delay (In)` e `Tac Sprint Reset Delay` (`MP-02-06`), 7 títulos do
   `PROPRIEDADES.md` sem o sufixo fantasma (`MP-02-08`), `.cfg` do servidor migrado, changelog. Build limpa.
4. **Harness de publicação** (commit `dbe99c46`): command `/prepare-mod-for-publish` (5 fases, portão de
   elegibilidade primeiro), skill `trl-mod-publishing` (regras do Forge + padrão de identidade TRL) e template
   `publish-audit.md.tmpl`. Revisados por `/g-write-a-skill` e `/g-review-content`: 12 achados, todos
   aplicados — o mais grave era prometer que `/apply-code-review` consumiria a auditoria, o que é impossível
   (ele resolve `<ref>` para pasta de item de backlog e exige `05-asbuild.md`).
5. Plano de publicação em 8 ondas acordado com o usuário; onda 1 (higiene de registro) executada aqui.

**Pendências abertas nesta sessão:** [P-12.1] 🟡 calibrar o pivô · [P-12.2] 🔴 portão de publicação
(relicenciamento + política de IA + assets) · [P-12.3] 🟡 regenerar `PROPRIEDADES.md`.

**Cross-refs:**

- Resolve [P-7.1], [P-8.1] e [P-11.6] (validações confirmadas pelo usuário); reduz [P-8.2] e [P-10.2].
- [P-11.1] (velocidade presa) passou a **parcialmente resolvida**: instrumento entregue na v2.14.0 e a
  hipótese antiga refutada. Esclarecido nesta sessão que **o sintoma é relato de terceiro** — o dono do
  servidor nunca o sofreu, então não há como reproduzir sob demanda. Congelada até alguém reproduzir com o
  `Debug Speed Limits` ligado. **Deixa de ser bloqueio para publicar.**
- Trabalho paralelo no mesmo mod, por outra sessão: `PickupAimingSafetyPatch` devolvido ao `TRL-Fixes`
  (ver `mods/TRL-Fixes/docs/handoff-pickup-aiming-safety.md` e a seção "Escopo" da 2.13.0 no changelog).
- Harness criado nesta sessão não tem memória dedicada — ver `git log` do commit `dbe99c46`.

## 2026-07-25 01:49 (GMT-3) — Sessão 11 (cont. 6): v2.8.1→v2.10.0 (code-review 01, ADS waypoint no rodapé, defaults promovidos, chamber-check UI) — F2 encerrada

**Tema central:** encerrar a F2 (braço G36) e continuar o item 017 (régua no canônico) até fechar o item 019 (UI de chamber-check), com 4 releases (2.8.1→2.10.0).

**Decisões-chave:**
- **[P-11.2] braço deforma na G36 (High Ready) marcada ✅ resolvida** — o usuário reportou "já foi resolvido" sem detalhar a causa raiz (não investigada nesta sessão; só o sintoma parou de se manifestar). **F2 encerrada.** Ref: pendência aberta em 2026-07-15, ver bloco de topo (histórico).
- **v2.8.1 (`47d30935`):** code-review da 2.7.1 (pedido do usuário) achou colisão de `Order` na Stance 2 — o waypoint relativo (17/16) colidia com Forward/Backward e Up/Down. Ref: `backlog/017-.../017-...-04-code-review-v271.md`.
- **v2.8.2 (`316b6581`):** ADS Waypoint movido para o **rodapé** de cada seção de stance no F12 (`Order -1/-2`) — a pedido do usuário, para não competir com as opções principais.
- **v2.9.0 (`8c9ae609`):** as **30 configs calibradas do `.cfg` do servidor promovidas a default de código** (tupla `_stanceDefaults` ganhou campo `AdsWaypoint`). 2 exceções deliberadamente NÃO promovidas: `Debug Transition Metrics` (mantido `false`) e `Mouse Wheel Modifier` (mantido `LeftAlt`). ⚠️ Defaults só afetam install limpo ou chave ausente do `.cfg` — instalação existente mantém os valores salvos (não é reset).
- **Item 019 → v2.10.0 (`ed9cf500`):** chamber-check ammo UI — ao checar a câmara (item 010, Manual Chambering), mostra o painel nativo com a bala e o tipo, reutilizando `Player.OnShowAmmoDetails`. Gate GO validado via ilspycmd. Nova classe `Patches/ChamberCheckAmmoPatch.cs`. Code-review adversarial: 0 🔴. Docs: `backlog/019-checar-camara-ui/`.
- **Item 018 (backlog, não implementado):** "rastejar rápido / high-crawl" registrado só como ideia em `backlog/018-rastejar-rapido/` (mesmo commit `8c9ae609`).

**Lições / hipóteses descartadas:**
- Nenhuma lição nova de código nesta sessão — trabalho foi continuação direta do fluxo já validado do item 017/019 (a lição de log-space da F3 já está registrada na Sessão 11 cont. 5). A única "descoberta" foi de processo: o **decompile completo do EFT** (0 namespaces vazios, ver handoff) permitiu confirmar o gate GO do item 019 via `ilspycmd -t` sem os buracos que existiam antes — não gerou mudança de decisão, só reduziu incerteza.

**Atividade cronológica:**
1. Code-review da 2.7.1 → achado de colisão de `Order` → v2.8.1.
2. ADS Waypoint reposicionado no rodapé de cada stance → v2.8.2.
3. 30 configs calibradas promovidas a default (2 exceções mantidas) → v2.9.0.
4. Item 019 (chamber-check ammo UI) implementado, gate GO validado, code-review adversarial (0 🔴) → v2.10.0.
5. Item 018 registrado como ideia de backlog (não implementado).
6. Deploy do DLL em `D:/SPT/BepInEx/plugins/RealisticMobility/` (EFT estava fechado) — **servidor ainda não recebeu** (pendência abaixo).
7. Handoff gerado (`.handoffs/handoff-2026-07-25-eft-decompile-completo-e-stances-2.10.0.md`) antes de confirmar este `/update-memory` — sessão cortada por limite de contexto; retomada nesta entrada com o plano do handoff confirmado pelo usuário.

**Pendências abertas nesta sessão:**
- [P-11.6] (aberta 2026-07-25) 🔴 **Validar item 019 in-game.** Prioridade: o caso "Empty" com câmara vazia via Manual Chambering (item 010) — única suposição load-bearing não verificada (que `CheckChamber()` retorna `true` com câmara vazia). Requer bindar a tecla "Check Chamber" nos controles do EFT (não vem por default). Depois: câmara carregada, toggle F12, Fika.
- [P-11.7] (aberta 2026-07-25) 🟡 **Subir 2.8.0→2.10.0 ao servidor** via `config-server` do launcher (DLL + `.cfg`) — só foi deployado localmente em `D:/SPT`.

**Cross-refs:**
- Resolve [P-11.2] da sessão 2026-07-15 (Sessão 11).
- Fecha a F2 mencionada como pendência nas Sessões 11 (cont. 4) e (cont. 5).
- [P-11.5] (F1+F3, calibração in-game) e [P-11.1] (velocidade presa) seguem abertas — não tocadas nesta sessão.
- Trabalho paralelo no mesmo dia (fora deste mod): decompile completo do EFT cercado no harness (`.agents/tools/decompile-eft/`, skill `graph-code-navigation`) — não é um mod, sem `sessions.md` próprio (ver `.handoffs/handoff-2026-07-25-eft-decompile-completo-e-stances-2.10.0.md`).

---

## 2026-07-29 02:40 (GMT-3) — Sessão 12: Diretriz de Isolamento de Rede para Sincronização 3ª Pessoa (Canal 3 / TRLS)

**Tema central:** Especificação de diretriz de rede no `ROADMAP.md` para a sincronização visual da postura das mãos/arma para a 3ª pessoa (`ObservedPlayer`) entre parceiros de equipe coop.

**Decisões-chave:**
- **Diretriz de Rede (ROADMAP.md):** Especificada no [ROADMAP.md](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/stancesAndCameraPositionSPT4.0.11/ROADMAP.md) a transmissão dos eventos de mudança de postura e movimentação de mãos para outros jogadores no **Channel 3 Compartilhado TRL** (`Unreliable`) com a assinatura binária `TRLS` (`0x54 0x52 0x4C 0x53`).
- **Isolamento de Erros:** Garantido que qualquer variação de rede ou perda de pacote na troca de postura ocorra de forma isolada no Canal 3, mantendo o `Channel 0` do FIKA 100% livre de engasgos e livre de erros do tipo `ParseException: Undefined packet`.

**Lições / hipóteses descartadas:**
- A ideia inicial de criar um canal de rede exclusivo para este mod foi descartada em favor do **Channel 3 Compartilhado TRL**, onde múltiplos mods TRL de dados e postura compartilham o mesmo canal de rede utilizando assinaturas binárias (Magic Headers) de 4 bytes sem interferência mútua.

**Atividade cronológica:**
1. Análise do envio de postura de braços na visão em 3ª pessoa (`ObservedPlayer`).
2. Criação do `ROADMAP.md` formalizando a arquitetura do Canal 3 Compartilhado TRL com Magic Header `TRLS`.
