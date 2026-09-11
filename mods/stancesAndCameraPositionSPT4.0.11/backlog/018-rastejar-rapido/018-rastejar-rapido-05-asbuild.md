# 018 — Correr agachado e rastejar rápido (crouch-run + high-crawl) · As-Built

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional:** [018-rastejar-rapido-01-spec.md](018-rastejar-rapido-01-spec.md)
**Spec técnica:** [018-rastejar-rapido-02-spec-tech.md](018-rastejar-rapido-02-spec-tech.md)
**Última review técnica:** [018-rastejar-rapido-03-spec-tech-review-02.md](018-rastejar-rapido-03-spec-tech-review-02.md)
**Build inicial:** 2026-09-09

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

## ⚠️ Correção de pasta (2026-09-09)

O build inicial foi implementado em `modded/`, mas o fork realmente ativo para este mod é `modded-testchannel/` (2.19.16 → 2.20.0 — tem `HandsStateGuard`, detecção de escada e outras features que `modded/` não tem). As mudanças em `modded/` foram **revertidas** (`git restore` + exclusão dos 2 arquivos novos) e reaplicadas em `modded-testchannel/`. A tabela abaixo já reflete o path correto.

## Arquivos alterados (build inicial, path corrigido)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/CrouchRunPatch.cs` | `CrouchRunEnableSprintPatch` (impede o stand-up nativo ao sprintar agachado) + `CrouchRunMaxSpeedPatch` (boost gradual de `MaxSpeed`, animação de sprint forçada, piso de postura, sobretaxa de stamina). |
| CRIADO | `mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/ProneRunPatch.cs` | `ProneRunObservePatch` (observa a tecla de sprint em prone) + `ProneRunMaxSpeedPatch` (boost gradual de `MaxSpeed` + sobretaxa de stamina). |
| MODIFICADO | `mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Plugin.cs` | 10 novos `ConfigEntry` (seção F12 "Crouch & Prone Sprint"), registro dos 4 novos patches via `SafeEnable`, versão `2.19.16 → 2.20.0` (bump conjunto com a correção do item 021). |
| MODIFICADO | `mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/StanceManager.cs` | `ResetState()` agora chama os 4 `ResetState()` dos patches novos (limpa flags de tecla, rampa, animação, piso de postura e sobretaxa de stamina entre raids). Inserido no mesmo bloco de reset já existente — sem interferir no `HandsStateGuard`/detecção de escada já presentes neste arquivo. |
| MODIFICADO | `mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/CameraRotationMod.csproj` | Versão `2.19.16 → 2.20.0`. |
| MODIFICADO | `mods/stancesAndCameraPositionSPT4.0.11/PROPRIEDADES.md` | Nova seção "Crouch & Prone Sprint" (10 props) + nota sobre as 2 lacunas conhecidas. |
| MODIFICADO | `mods/stancesAndCameraPositionSPT4.0.11/backlog/mod-backlog.md` | Status do item 018: ⚪ → 🟡 (não 🟢 — ver "Lacunas conhecidas" abaixo). |

## PA-NN-MM resolvidos durante o build

> Pontos das reviews técnicas que foram **aplicados como parte da implementação** (não como /apply-code-review posterior).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | AP-02 · 🔴 | Guarda `IsYourPlayer` implementada nos 4 patches, via `Traverse.Create(movementContext).Field<Player>("_player").Value` (`MovementContext._player`, confirmado protected field, `MovementContext.cs:157`). |
| PA-01-02 | AP-01 · 🔴 | `ResetState()` implementado nas 4 classes e conectado em `StanceManager.ResetState()`. |
| PA-01-03 | C · 🔴 | Condição `wantsBoost` recalculada a cada frame (nunca só a flag de tecla) em ambos os `MaxSpeedPatch` — cobre o cenário de "segurar sprint parado" relatado pelo usuário. |
| PA-02-01 | C · 🟠 | Piso de postura usa `force: false` (transição suave via `POSE_CHANGING_SPEED`/item 005), não `force: true` (que teleportaria a pose). |
| PA-02-02 | D · 🟡 | Documentado (não refatorado) — side-effects dentro do Postfix de `MaxSpeed` continuam como estavam, com nota de arquitetura na spec técnica para o `/code-review` reavaliar se necessário. |

## ⚠️ Fix pós-validação in-game (2026-09-09)

O usuário testou a build e reportou: crouch-run funcionou; **prone-run não mudava a velocidade** mesmo com `Prone Run Speed Multiplier = 2.0`. Também pediu pra reavaliar a sobretaxa de stamina e remover a rampa de velocidade.

**Causa raiz do bug do prone (confirmada no Assembly):** `ProneRunObservePatch` (removido) escutava `ProneMoveStateClass.EnableSprint`, mas esse método só existe enquanto o jogador já está **se movendo** em prone. Se a tecla de correr for pressionada com o jogador prone e **parado**, o estado ativo é `ProneIdleStateClass`, que **não sobrescreve `EnableSprint`** — a intenção nunca era capturada, e continuava perdida mesmo depois de o jogador começar a andar (não existe nenhum mecanismo nativo de "latch" para prone, ao contrário do que existe pra corrida em pé/agachada via `RunStateClass.Bool_1`).

**Fix:** substituído por `PlayerToggleSprintPatch`/`PlayerEnableSprintPatch` (novo `ProneRunPatch.cs`) — patcheiam `Player.ToggleSprint()` e `Player.EnableSprint(bool)`, os dois métodos **de nível Player** por onde TODO input de sprint do jogador passa, confirmado em `Class1728.cs:99-104` (`ECommand.ToggleSprinting`/`EndSprinting`), independente de qual sub-estado nativo esteja ativo no momento. Não dá pra usar `Physical.Sprinting` para saber o estado anterior (como o `ToggleSprint()` nativo faz) porque essa flag nunca fica `true` durante prone — por isso o fix mantém seu próprio flag (`ProneRunSprintIntent.Held`), alternado no mesmo evento.

**Descoberta sobre stamina (motivou a resposta às duas outras pedidas do usuário):** confirmado que `PlayerPhysicalClass.Sprint(bool)` (o método que ativa o dreno nativo de stamina de sprint) só é chamado via `MovementContext.EnableSprint()` — e o `ProneMoveStateClass.EnableSprint` **nunca** chama isso (só grava uma flag local). Ou seja: **o jogo não cobra stamina nativamente ao correr em prone**, mas **cobra normalmente ao correr agachado** (porque o Prefix do crouch-run já chama `movementContext.EnableSprint()`, que aciona o dreno nativo). Por isso:
- `_CrouchRunStaminaSurcharge` foi **removida** (dreno nativo já se aplica sozinho, confirmado pelo usuário como "1.0 já é suficiente e muito bem aplicado").
- `_ProneRunStaminaSurcharge` foi **mantida** (sem ela, o prone-run ficaria de graça).

**Rampa removida:** `_CrouchRunRampUpSeconds`/`_ProneRunRampUpSeconds` e toda a lógica de `_rampProgress`/`Mathf.Lerp` foram removidas dos dois `MaxSpeedPatch` — o multiplicador de velocidade agora aplica/remove instantaneamente junto com `wantsBoost`, a pedido do usuário ("não fez sentido nenhum ele no mod").

**Arquivos tocados neste fix:** `Patches/CrouchRunPatch.cs` (removida rampa e sobretaxa), `Patches/ProneRunPatch.cs` (reescrito — nova fonte de sinal, removida rampa, sobretaxa mantida), `Plugin.cs` (removidos 3 `ConfigEntry`, `SafeEnable` atualizado), `StanceManager.cs` (`ResetState()` aponta pra `ProneRunSprintIntent` em vez de `ProneRunObservePatch`), `PROPRIEDADES.md`. Versão `2.20.0 → 2.20.1`. Compilado — 0 erros/0 warnings.

## ⚠️ Fix pós-validação in-game #2 (2026-09-09, build 2.20.1 → 2.20.2)

Usuário testou a 2.20.1 e reportou 3 bugs novos:

**1. "C" não levanta mais o personagem durante o crouch-run.** Causa raiz: `CrouchRunMaxSpeedPatch` restaurava `_poseLevelBeforeRun` (piso de postura) todo frame em que `wantsBoost` virasse `false` — inclusive quando o motivo de `wantsBoost` cair era o próprio jogador ter apertado C (`Player.method_7()` → `RunStateClass.ChangePose()` → `SetPoseLevel()` altera `PoseLevel` **na mesma hora**, síncrono — ref: `Assembly-CSharp/EFT/Player.cs:25785-25793`, `RunStateClass.cs:439-446`). O restore brigava com o comando manual e prendia o jogador agachado até soltar o movimento. **Fix:** o restore só roda se `PoseLevel < 0.9f` no momento (ou seja, só quando o jogador NÃO se levantou por conta própria nesse meio-tempo). `Patches/CrouchRunPatch.cs`.

**2. Prone-run: "teto" muda mas velocidade real não.** Investigação no Assembly mostrou que `ClampedSpeed` (o valor que efetivamente alimenta a animação/velocidade, via `RunStateClass.method_1`) é recomputado contra `StateSpeedLimit` (default `1f`, nunca sobe — é um sistema só de *slowdown*, ref: `MovementContext.cs:1801/1843-1845`), e `CharacterMovementSpeed` nasce igual a `MaxSpeed` na baseline (`Init()`, ref: `MovementContext.cs:1917`) — então simplesmente multiplicar `MaxSpeed` (o que `ProneRunMaxSpeedPatch` já fazia) não move a velocidade real, só o número que a UI lê como "teto". A prova de que quem realmente acelera o passo é o parâmetro do Animator (`PlayerAnimatorEnableSprint`, que não faz nenhuma checagem de pose — ref: `MovementContext.cs:3832-3835`) e não o número: o crouch-run já chama esse método (`CrouchRunMaxSpeedPatch`) e o usuário confirmou que o agachado corre mais rápido de verdade. **Fix:** `ProneRunMaxSpeedPatch` agora também chama `PlayerAnimatorEnableSprint(wantsBoost)` (mesmo padrão edge-triggered do crouch-run). `Patches/ProneRunPatch.cs`.

**3. Crouch-run não aciona a animação de braços/arma baixa.** Investigado, **não corrigido nesta rodada** — evidência aponta para limitação do próprio Animator Controller (asset Mecanim, fora do alcance de patch C#/Harmony), não bug de código:
- O gate nativo que dispara essa animação (`RunStateClass.cs:252`) só chama `PlayerAnimatorEnableSprint(true)` quando `PoseLevel > 0.9f` (em pé).
- `CrouchRunMaxSpeedPatch` já **bypassa** esse gate de propósito, chamando `PlayerAnimatorEnableSprint(wantsBoost)` incondicionalmente (independente da pose) — e mesmo assim a animação visual não aparece agachado.
- Isso é evidência forte de que a própria árvore de transições do Animator Controller (não visível/editável via Assembly descompilado) tem sua própria condição de pose para a camada de sobreposição de braços/arma, separada da camada de locomoção (que aparentemente NÃO tem esse gate — por isso a velocidade em si funciona). Sem acesso ao asset do Animator Controller, não há patch de código que force essa camada a tocar fora do gate da própria FSM.
- Registrado como pendência de investigação (não de código) — ver Pendências.

**Arquivos tocados neste fix:** `Patches/CrouchRunPatch.cs` (guarda no restore do piso de postura), `Patches/ProneRunPatch.cs` (chamada de `PlayerAnimatorEnableSprint` adicionada), `Plugin.cs` + `CameraRotationMod.csproj` (versão `2.20.1 → 2.20.2`). Compilado — 0 erros/0 warnings. **Bug 3 não resolvido** (ver acima).

## ⚠️ Spy de diagnóstico (2026-09-09, build 2.20.2 → 2.20.3)

Usuário testou a 2.20.2 (fix do bug 2 via `PlayerAnimatorEnableSprint`) e reportou que o prone-run **continua sem efeito visual**. Como a teoria (baseada só em leitura de Assembly, sem teste ao vivo) já errou uma vez, em vez de tentar mais uma hipótese às cegas, foi adicionado um overlay de diagnóstico ao vivo — mesmo padrão já usado no mod pra esse tipo de bug (ver `SpeedLimitDebugUI`, P-11.1).

**Novo:** `ProneRunDebugUI.cs` (MonoBehaviour, toggle `Debug Prone Run` no F12, seção Debug) — mostra em tempo real:
- `featureOn` / `held` (`ProneRunSprintIntent.Held`) / `moving` / `wantsBoost` — confirma se a detecção da tecla está funcionando.
- `CurrentManagedState` (nome da classe) + `Bool_5` nativo (só existe se o estado ativo for `ProneMoveStateClass`) — confirma qual sub-estado nativo está ativo de fato.
- `PoseLevel`, `IsInPronePose`, `MovementDirection`.
- `MaxSpeed` (já com nosso boost aplicado), `StateSpeedLimit`, `CharacterMovementSpeed`, `ClampedSpeed`, `SmoothedCharacterMovementSpeed` — todos fatores normalizados, não m/s (mesma unidade documentada em `SpeedLimitDebugUI`).
- **O valor real do parâmetro `"Speed"` do Animator** (via `PlayerAnimator.GetCharacterMovementSpeed()`, que lê `Animator.GetFloat` diretamente) — este é o teste decisivo: se ele não muda quando `wantsBoost=true`, a causa não é código nosso, é a árvore do Animator Controller (ou algo não capturado aqui) não reagindo ao parâmetro.
- `IsSprintEnabled` (`Physical.Sprinting`).

**Arquivos tocados:** `ProneRunDebugUI.cs` (criado), `Plugin.cs` (`_DebugProneRun` ConfigEntry + `AddComponent<ProneRunDebugUI>()`), `CameraRotationMod.csproj` (versão `2.20.2 → 2.20.3`). Compilado — 0 erros/0 warnings. **Não é fix** — é instrumento. Próximo passo: usuário liga o toggle no F12, reproduz o bug e reporta os valores mostrados na tela (ou cola print/foto), pra sabermos exatamente qual valor não está acompanhando o boost.

## ⚠️ Fix real via spy (2026-09-09, build 2.20.3 → 2.20.4)

Usuário ativou o `ProneRunDebugUI` (toggle F12) e mandou 2 capturas de tela lado a lado, boost desligado vs. ligado. Isso resolveu a dúvida em aberto desde o fix da 2.20.2:

| Métrica | Boost OFF | Boost ON | Leitura |
|---|---|---|---|
| `MaxSpeed` (pós-multiplicador) | 0.565 | 1.131 | Multiplicador (2.0×) aplicando certinho. |
| `StateSpeedLimit` | 0.543 | 0.543 | Teto de slowdown já ativo nessa raid (causa fora do mod — Stance/peso/etc.), **não sobe com MaxSpeed**. |
| `CharacterMovementSpeed` | 0.565 | 0.593 | Quase não mexe — não está rampando de verdade. |
| `ClampedSpeed` | 0.543 | 0.543 | **Idêntico** — travado exatamente no `StateSpeedLimit`. |
| `SmoothedCharacterMovementSpeed` | 0.392 | 0.392 | **Idêntico.** |
| Parâmetro `"Speed"` do Animator | 0.392 | 0.392 | **Idêntico** — é literalmente o mesmo valor que `SmoothedCharacterMovementSpeed` (setter de um alimenta o outro, ref: `MovementContext.cs:794-808`). Prova que nada chegava no Animator. |
| `Bool_5` (nativo) | False | False | A rampa nativa (`ProneMoveStateClass.ManualAnimatorMoveUpdate`, `Bool_5`-gated) nunca liga — nosso fix da detecção de tecla (2.20.1) contorna esse campo de propósito, então ele nunca é setado. |

**Causa raiz definitiva:** `RunStateClass.method_1` (que roda incondicionalmente em `ManualAnimatorMoveUpdate`, mesmo em prone, porque `IsSprintEnabled` fica sempre `False` ali) alimenta `SmoothedCharacterMovementSpeed` a partir de `ClampedSpeed` (ref: `RunStateClass.cs:318-320`), e `ClampedSpeed = Clamp(CharacterMovementSpeed, 0, StateSpeedLimit)` (ref: `MovementContext.cs:1843-1845`) — um teto que **nunca sobe acima de `StateSpeedLimit`**, não importa o quanto `MaxSpeed`/`CharacterMovementSpeed` sejam multiplicados. `PlayerAnimatorEnableSprint` (fix da 2.20.2) liga só um bool do Animator, sem efeito nenhum sobre esse número.

**Fix real:** novo patch `ProneRunSpeedDriverPatch` (Postfix em `ProneMoveStateClass.ManualAnimatorMoveUpdate`) replica o padrão de `MovementContext.PreSprintAcceleration` (ref: `MovementContext.cs:2526-2542` — o canal que de fato acelera o sprint em pé/agachado) — escreve DIRETO em `SmoothedCharacterMovementSpeed`/`CharacterMovementSpeed` via `Mathf.MoveTowards` a cada frame (`player.Physical.PreSprintAcceleration` como taxa, mesma cadência do sprint nativo), alvo = multiplicador configurado (não o `1f` fixo que o nativo usa) — ignorando `ClampedSpeed`/`StateSpeedLimit` por completo, do mesmo jeito que o canal nativo de sprint já ignora. Ao soltar a tecla, o patch simplesmente não escreve mais nada nesse frame — `method_1` (que já rodou antes, dentro do `base.ManualAnimatorMoveUpdate`) recalcula o ritmo normal sozinho, dando o retorno imediato que o critério de aceite pede.

**Arquivos tocados:** `Patches/ProneRunPatch.cs` (novo `ProneRunSpeedDriverPatch`), `Plugin.cs` (`SafeEnable` do novo patch, versão `2.20.3 → 2.20.4`), `CameraRotationMod.csproj`. Compilado — 0 erros/0 warnings. **Ainda não validado in-game** — pendente próximo teste do usuário (com `Debug Prone Run` ligado pra confirmar `SmoothedCharacterMovementSpeed`/Animator Speed subindo de verdade desta vez).

## ⚠️ Fix rodada 2 via spy (2026-09-09, build 2.20.4 → 2.20.5)

A 2.20.4 provou (via nova captura do `ProneRunDebugUI`) que `SmoothedCharacterMovementSpeed`/parâmetro `"Speed"` do Animator SUBIRAM de verdade (0.392 → 1.279), mas o usuário confirmou que o rastejo continuou visualmente na mesma velocidade ("nada ainda"). Isso derruba a premissa de que esse parâmetro controla a velocidade real do deslocamento.

**Causa raiz (2ª camada):** confirmado em `MovementContext.cs:2025-2054` (`DirectApplyMotion`) e `:1384-1394` (`PlayerAnimatorDeltaPosition`) que a EFT usa **Root Motion puro** pra mover o personagem em prone — o deslocamento por frame é literalmente `CharacterController.Move(PlayerAnimator.DeltaPosition, deltaTime)`, sem nenhuma escala manual por `CharacterMovementSpeed`/`MaxSpeed` no caminho. O parâmetro `"Speed"` (que alimentamos) é só um EIXO de blend tree — ele escolhe qual sample da blend tree tocar, não escala a velocidade de reprodução do clipe. Por isso subir o número não mudava nada visualmente: a blend tree provavelmente satura no sample de maior threshold (crawl "cheio") e ignora qualquer valor acima disso.

**Novo lever:** `Animator.speed` (multiplicador GLOBAL de playback do Mecanim — escala Root Motion proporcionalmente, comportamento padrão da Unity) é DIFERENTE do parâmetro de blend, e a EFT já expõe um wrapper pronto, **nunca chamado por nenhum código nativo no Assembly** (zero callers encontrados): `MovementContext.PlayerAnimatorSetSprintToIdleSpeed(float)` (ref: `MovementContext.cs:3837-3840`, literalmente `PlayerAnimator_1.Animator.speed = value`).

**Fix:** `ProneRunSpeedDriverPatch` agora também chama `mc.PlayerAnimatorSetSprintToIdleSpeed(target)` enquanto `wantsBoost`, e restaura pra `1f` (edge-triggered, via novo campo `_speedOverrideActive` + `ResetState()`) assim que o boost desliga.

**⚠️ Risco conhecido, ainda não testado:** `Animator.speed` é um multiplicador **global** do Animator Controller inteiro — se ele afetar camadas além da locomoção (ex.: sway da arma, respiração, outras animações rodando no mesmo Animator), o boost pode acelerar coisas além do rastejo em si. A validação in-game precisa checar isso especificamente.

**Arquivos tocados:** `Patches/ProneRunPatch.cs` (`ProneRunSpeedDriverPatch` — novo lever + `ResetState()`), `StanceManager.cs` (`ResetState()` conectado), `ProneRunDebugUI.cs` (nova linha mostrando `Animator.speed` ao vivo), `Plugin.cs` + `CameraRotationMod.csproj` (versão `2.20.4 → 2.20.5`). Compilado — 0 erros/0 warnings. **Ainda não validado in-game.**

## ⚠️ Extensões pedidas pelo usuário após validar o fix (2026-09-09, build 2.20.5 → 2.21.0)

Usuário confirmou o fix da 2.20.5 funcionando ("Funcionou!") e pediu duas extensões, usando o mesmo lever recém-descoberto (`Animator.speed` via `PlayerAnimatorSetSprintToIdleSpeed`):

**1. "Barrinha" nativa de ajuste de velocidade (scroll wheel) funcional em prone, permitindo reduzir abaixo de 1x** (ex.: 0.5x, para reposicionamento cauteloso tipo atirador de precisão). O comando nativo (`Player.ChangeSpeed`, acionado por `ECommand.ScrollNext`/`ScrollPrevious`, ref: `Class1728.cs:69-74`) já mexia em `CharacterMovementSpeed` normalmente em prone (nenhum override bloqueia) — só nunca tinha efeito visível, pelo mesmo motivo do bug original (nada escrevia em `Animator.speed`). `ProneRunSpeedDriverPatch` foi generalizado: agora espelha `Animator.speed = CharacterMovementSpeed` **sempre** que `_EnableProneRun` está ligado e o jogador está rastejando em movimento (não só durante o boost do sprint) — isso destrava o scroll wheel nativo pra baixo (lento) E mantém o boost pra cima (rápido) no mesmo canal.

**2. Gap de stamina infinita no prone-run.** Como o prone-run não passa pelo dreno nativo de sprint (usa sobretaxa própria), também precisava da própria checagem de exaustão — nada nativo cortava isso. Adicionado `ProneRunSprintIntent.ExhaustedLatch`/`UpdateExhaustionGate(BasePhysicalClass)`: usa o mesmo critério que o próprio jogo já usa pra "vermelho" (`PlayerPhysicalClass.Exhausted`, `Stamina.Current < 15f`, ref: `PlayerPhysicalClass.cs:455-465`). Assim que a stamina esgota, o boost desliga (latch); só volta a ficar disponível quando a stamina sobe de novo pra ACIMA desse teto (não no instante em que sai de 0) — evita o boost "piscar" ligado/desligado no fundo do poço. Aplicado tanto em `ProneRunMaxSpeedPatch` (multiplicador de `MaxSpeed` + sobretaxa) quanto em `ProneRunSpeedDriverPatch` (o driver real).

**Arquivos tocados:** `Patches/ProneRunPatch.cs` (`ProneRunSprintIntent` ganhou o gate de exaustão; `ProneRunMaxSpeedPatch` e `ProneRunSpeedDriverPatch` passam a checar `staminaOk`; `ProneRunSpeedDriverPatch` generalizado pra espelhar `Animator.speed` sempre, não só durante boost), `Plugin.cs` + `CameraRotationMod.csproj` (versão `2.20.5 → 2.21.0`, minor — feature nova visível: velocidade de prone ajustável pra baixo). Compilado — 0 erros/0 warnings. **Ainda não validado in-game.**

## ⚠️ Gap adicional: movimento durante ação de mão em prone (2026-09-09, build 2.21.0 → 2.21.1)

Usuário reportou conseguir continuar rastejando (WASD) enquanto checa carregador/câmara em prone. Investigação: a recarga (reload) já bloqueia movimento nativamente (confirmado pelo usuário); checar carregador/câmara não — e o mod já tem uma família de patches (`ActionStanceCheckChamberPatch` etc., em `ActionStancePatches.cs`) mexendo nessa área por outro motivo (gestão de Stance), então não dava pra presumir se era vanilla ou introduzido pelo mod sem checar. Perguntado ao usuário o escopo do fix — resposta: **bloquear movimento WASD durante qualquer ação de mão, mas só em prone** (em pé/agachado fica como está).

**Implementação:** novo `ProneHandsBusyMovementBlockPatch.cs` — Prefix em `MovementState.ApplyMotion(ref Vector3 motion, float deltaTime)` (ref: `MovementState.cs:164-172`, já é `ref` nativamente — nem `RunStateClass` nem `ProneMoveStateClass` sobrescrevem esse método, então o Prefix cobre o caminho real). Zera `motion` quando `MovementContext.IsInPronePose` E `!player.HandsController.FirearmsAnimator.IsIdling()` — reaproveita o MESMO sinal de "mão ocupada" que a própria EFT já usa pra decidir se pode iniciar uma interação nova (ref: `MovementContext.cs:3171`, `SetInteractInHands`). Sem toggle novo no F12 — tratado como correção, não feature opcional (mesmo padrão dos outros bugs desta sessão).

**Arquivos tocados:** `Patches/ProneHandsBusyMovementBlockPatch.cs` (criado), `Plugin.cs` (`SafeEnable`, versão `2.21.0 → 2.21.1`), `CameraRotationMod.csproj`. Compilado — 0 erros/0 warnings. **Ainda não validado in-game.**

## ⚠️ Sync FIKA do Animator.speed do prone-run (2026-09-09, build 2.21.1 → 2.22.0)

Usuário pediu pra investigar se as mudanças de velocidade do prone-run precisavam de sync via FIKA (cenários: solo, host+convidado, headless+convidados). Pesquisa dedicada (agente Explore) achou:

- **Solo:** sem gap — não há observador.
- **Host+convidado / Headless+convidados:** gap real, mas puramente VISUAL, não de posição. `CharacterMovementSpeed`/`Position` já são replicados normalmente (nativo + Fika `PlayerStateData`). Mas `Animator.speed` (o lever que realmente acelera o rastejo, via `PlayerAnimatorSetSprintToIdleSpeed`) é **100% client-local** — nem o pacote nativo (`MovementInfoPacketStruct`) nem o do Fika (`PlayerStateData`, que ainda por cima empacota `SmoothedCharacterMovementSpeed` num `ushort` 0..1 via `PackFloatToUShort` — qualquer boost >1.0 já sai cortado) carregam esse valor. Sintoma: outro jogador veria a posição do jogador com prone-run avançar no ritmo boostado, mas as pernas animando no ritmo normal — descompasso tipo "patinação".
- Crouch-run **não** tem esse problema — o boost dele passa pela cadeia nativa já replicada (`ClampedSpeed`/`CharacterMovementSpeed`, sem exceder o teto vanilla) e usa `PlayerAnimatorEnableSprint`, já sincronizado nativamente como bool.

**Fix:** novo pacote de rede próprio `Networking/ProneRunSpeedSyncPacket.cs`, seguindo EXATAMENTE o padrão já estabelecido por `StanceSyncPacketV2`/`ChamberStateSyncPacket` (envelope `PutBytesWithLength`, `[ThreadStatic]` writer, try/catch contido no Deserialize). Registrado/enviado/recebido via `FikaSyncManager.cs` (`SendProneRunSpeed`/`OnProneRunSpeedSyncPacketReceived`). No recebimento, aplica direto em `observedPlayer.MovementContext.PlayerAnimatorSetSprintToIdleSpeed(...)` — confirmado que `ObservedMovementContext` (Fika) NÃO sobrescreve esse método, então a implementação base (`Animator_1.speed = value`) escala o playback do Animator PRÓPRIO do jogador observado, sem precisar de um MonoBehaviour dedicado tipo `ObservedStanceAnimator` (é só um valor que fica setado até o próximo pacote, sem decaimento por frame). Envio throttlado em `ProneRunSpeedDriverPatch` (não em `FikaSyncManager`, pra não duplicar lógica de throttle): só reenvia em mudança >0.03 ou keepalive de 1s — evita mandar pacote todo frame (60x/s) pra um valor que muda continuamente.

**⚠️ Lockstep:** como `StanceSyncPacketV2` já documenta, registrar um pacote NOVO é incompatibilidade de mão única — um peer numa versão anterior a 2.22.0 não tem handler pro hash desse pacote e um `ParseException` derruba o lote de rede daquele frame (inclusive pacotes de posição/movimento de outros mods). **Todos os peers da raid (host, convidados, headless) precisam estar em 2.22.0+ juntos.**

**Arquivos tocados:** `Networking/ProneRunSpeedSyncPacket.cs` (criado), `Networking/FikaSyncManager.cs` (registro + `SendProneRunSpeed` + `OnProneRunSpeedSyncPacketReceived`), `Patches/ProneRunPatch.cs` (`ProneRunSpeedDriverPatch` ganhou throttle + chamadas de sync nos dois pontos onde `Animator.speed` local muda), `Plugin.cs` + `CameraRotationMod.csproj` (versão `2.21.1 → 2.22.0`, minor — mesmo critério usado quando `StanceSyncPacketV2` foi introduzido). Compilado — 0 erros/0 warnings. **Ainda não validado em multiplayer real** (nenhum ambiente de teste com 2+ peers disponível nesta sessão).

## 🔴 Crash reportado pelo usuário — corrigido (2026-09-09, build 2.22.0 → 2.22.1)

Após instalar a 2.22.0, o usuário reportou crash impedindo a criação do player em QUALQUER cenário (`CreateLocalPlayer`/`InitPlayer` falhando, log com `NullReferenceException` em `PlayerPhysicalClass.get_Exhausted()` chamado a partir de `ProneRunSprintIntent.UpdateExhaustionGate`, dentro de `ProneRunMaxSpeedPatch.Postfix`).

**Causa raiz:** `MovementContext.Init()` chama o getter de `MaxSpeed` (que aciona `ProneRunMaxSpeedPatch.Postfix`) durante a CRIAÇÃO do player (`SetCharacterMovementSpeed(MaxSpeed, force: true)`, ref: `MovementContext.cs:1917`) — momento em que `Physical.Stamina`/`Physical.Oxygen` ainda não foram inicializados. `PlayerPhysicalClass.Exhausted` (adicionado na rodada anterior desta sessão, ver "Extensões pedidas pelo usuário") lê `Stamina.Current`/`Oxygen.Current` sem proteção, e `UpdateExhaustionGate` (novo, sem try/catch) não capturava a exceção — diferente de TODO o resto do padrão de patches deste mod, que sempre envolve chamadas de risco em try/catch. A exceção subia até `EFT.Player.Init()` e derrubava a criação do player por completo, em qualquer cenário (inclusive solo — não era um bug de rede, apesar de o usuário estar testando logo depois de mexer em código de sync).

**Fix:** `UpdateExhaustionGate` agora envolve a leitura de `physical.Exhausted` em try/catch — qualquer exceção (estado interno ainda não pronto) não altera o latch e tenta de novo no próximo frame, igual ao padrão usado em todo o resto do mod. Também adicionado try/catch de corpo inteiro em `ProneRunMaxSpeedPatch.Postfix` e `ProneRunSpeedDriverPatch.Postfix` (defesa em profundidade — nenhum dos dois tinha esse guard, apesar de ser item obrigatório no checklist de `csharp-mod-best-practices`), e trocado `player.Physical != null` por `player.Physical?.Stamina != null` antes de `AddConsumption` (mesma classe de risco).

**Arquivos tocados:** `Patches/ProneRunPatch.cs` (3 pontos: `UpdateExhaustionGate`, `ProneRunMaxSpeedPatch.Postfix`, `ProneRunSpeedDriverPatch.Postfix`), `Plugin.cs` + `CameraRotationMod.csproj` (versão `2.22.0 → 2.22.1`, patch — fix, não feature). Compilado — 0 erros/0 warnings. **Ainda não revalidado in-game pelo usuário** (crash reportado, fix aplicado nesta mesma rodada).

## ⚠️ Dois bugs reportados após validar o fix de multiplayer (2026-09-09, build 2.22.1 → 2.22.2)

**Bug 1 — reduzir velocidade em prone entra em câmera lenta absurda, ignorando input (levantar, mirar).** Causa raiz: `Animator.speed` é o multiplicador GLOBAL de playback do Animator inteiro (ref: `MovementContext.cs:3837-3840`), não só da locomoção — e o scroll wheel nativo (`Player.ChangeSpeed`) não tem piso prático, deixando `CharacterMovementSpeed` cair perto de 0 (ref: `Class1728.cs:69-74`). `ProneRunSpeedDriverPatch` espelhava isso direto em `Animator.speed` sem piso, derrubando TODAS as animações do personagem (levantar, mirar, etc.) em câmera lenta — o usuário já tinha alertado sobre esse risco quando o lever foi descoberto. **Fix:** novo `ConfigEntry` `Prone Run Min Speed Floor` (padrão `0.4`, faixa `0.1–1.0`) — `animatorSpeed = Max(floor, CharacterMovementSpeed)` no branch sem boost, em vez do piso fixo `0.05` anterior.

**Bug 2 — depois de crouch-run → levantar pra sprint em pé → voltar a caminhar, apertar C tenta correr em pé; só "reseta" parando o movimento por completo.** Causa raiz é AP-03 (despacho virtual): `SprintStateClass : RunStateClass` **sobrescreve** `EnableSprint` com implementação própria que **não chama `base.EnableSprint`** (ref: `SprintStateClass.cs:97-100`). `CrouchRunEnableSprintPatch` só intercepta o MethodInfo de `RunStateClass.EnableSprint` — quando o jogador levanta segurando sprint, `CurrentManagedState` vira `SprintStateClass` (estado dedicado de sprint em pé), e TODA chamada de `EnableSprint` a partir daí (inclusive soltar a tecla) passa batido pelo patch. `SprintKeyHeld` ficava preso em `true`; ao tentar agachar de novo (voltando pro `RunStateClass` "puro"), `CrouchRunMaxSpeedPatch` via essa flag obsoleta e reengajava o boost no meio da tentativa de agachar. **Fix:** novo `SprintStateEnableSprintSyncPatch` (Postfix dedicado em `SprintStateClass.EnableSprint`) — apenas re-sincroniza `SprintKeyHeld` com `IsSprintEnabled` depois que o método próprio já rodou (não precisa interceptar/pular nada, já que `SprintStateClass` não tem pose pra forçar).

**Arquivos tocados:** `Patches/ProneRunPatch.cs` (piso configurável), `Patches/CrouchRunPatch.cs` (`SprintStateEnableSprintSyncPatch`, novo), `Plugin.cs` (`_ProneRunMinSpeedFloor` + bind + `SafeEnable` do novo patch, versão `2.22.1 → 2.22.2`), `PROPRIEDADES.md` (nova prop documentada). Compilado — 0 erros/0 warnings. **Ainda não revalidado in-game.**

## ⚠️ Câmera lenta "vazava" pra fora do prone (2026-09-09, build 2.22.2 → 2.22.3)

Usuário confirmou (com print do `ProneRunDebugUI`) que o piso da 2.22.2 funcionava, mas a câmera lenta persistia mesmo DEPOIS de levantar e andar em pé — print mostrando `CurrentManagedState=IdleStateClass`, `IsInPronePose=False`, `PoseLevel=1.000`, e ainda assim `Animator.speed=0.400` (preso exatamente no piso configurado).

**Causa raiz:** o reset de `Animator.speed` em `ProneRunSpeedDriverPatch` só roda dentro do PRÓPRIO Postfix de `ProneMoveStateClass.ManualAnimatorMoveUpdate` — método que só é chamado enquanto `CurrentManagedState` AINDA é `ProneMoveStateClass`. Ao sair do prone de vez (levantar), esse método para de rodar e o `Animator.speed` reduzido fica preso pra sempre — nada mais no jogo toca nesse valor (`PlayerAnimatorSetSprintToIdleSpeed` não tem nenhum outro caller nativo, confirmado nas investigações anteriores desta sessão).

**Fix:** novo `ProneMoveExitResetPatch` — Postfix em `ProneMoveStateClass.Exit(bool)`, que dispara sempre que se sai do estado de prone EM MOVIMENTO, pra qualquer estado seguinte (parado-prone, levantando, etc.). Reset incondicional pra `Animator.speed=1f` (não só quando havia override ativo) — mais barato e seguro que mapear todo caminho de saída possível. `ProneRunSpeedDriverPatch` ganhou um método `ForceReset` interno reutilizado pelos dois pontos.

**Arquivos tocados:** `Patches/ProneRunPatch.cs` (`ForceReset` + `ProneMoveExitResetPatch`, novo), `Plugin.cs` (`SafeEnable`, versão `2.22.2 → 2.22.3`). Compilado — 0 erros/0 warnings. **Ainda não revalidado in-game.**

## ⚠️ Dois pedidos do usuário — segurança ao sair do prone + remoção do piso de postura (2026-09-09, build 2.22.3 → 2.23.0)

**1. Sair do prone pelo comando de agachar (C) levava direto pro EM PÉ, causando morte real do usuário.** Investigado no Assembly: `Player.method_7()` (handler de `ECommand.ToggleDuck`) delega pra `CurrentManagedState.ChangePose(...)`, mas `ProneMoveStateClass`/`ProneIdleStateClass` IGNORAM o delta calculado e chamam `Prone()` incondicionalmente (ref: `ProneMoveStateClass.cs:21-25`), que chama `MovementContext.UpdatePoseAfterProne()` = `SetPoseLevel(_player.PoseMemo)` (ref: `MovementContext.cs:2134-2137`). `PoseMemo` guarda a pose de ANTES de deitar (ref: `Player.cs:25654/25769) — como a maioria entra em prone estando EM PÉ, `PoseMemo` normalmente vale `1f`, então "agachar" pra sair do prone restaura direto pro em pé, expondo o jogador.

**Fix:** novo `ProneToggleDuckStandUpPatch` — Prefix em `Player.method_7()` (resolvido por string, sem entrada na tabela de deofuscação — risco de quebra silenciosa já assumido por outros membros ofuscados deste mod) que injeta `PoseMemo=0f` (agachado/duck, mesmo `PoseLevel` numérico do prone, só sem o flag `IsInPronePose`) IMEDIATAMENTE ANTES do método original rodar, só quando `IsInPronePose=true`. Preserva 100% dos efeitos colaterais nativos (collider, POM, Animator) — só troca o ALVO da restauração. `PoseMemo` se autocorrige na próxima chamada real de `Player.ChangePose`, sem precisar desfazer manualmente. **Espaço (Jump) e o botão de Prone continuam levando direto pro em pé — comportamento intocado, só o comando de agachar muda**, conforme pedido explícito do usuário.

**2. Piso de postura do crouch-run removido.** Usuário identificou que o vanilla já dá uma "levantadinha" sozinho ao iniciar a corrida agachado (transição nativa, fora do nosso controle) — o piso de postura que este mod adicionava por cima (`_CrouchRunPoseFloorThreshold`/`Target`, erguia a postura se estivesse muito baixa) empilhava com isso, deixando o personagem quase em pé. Removido por completo: campos `_poseFloorEngaged`/`_poseLevelBeforeRun` e a lógica associada em `CrouchRunMaxSpeedPatch.Postfix`, e os dois `ConfigEntry` (`Crouch Run Pose Floor Threshold`/`Target`) — deletados, não deprecados (nada os referenciava fora desta feature). O boost de velocidade do crouch-run continua intocado.

**Arquivos tocados:** `Patches/ProneToggleDuckStandUpPatch.cs` (criado), `Patches/CrouchRunPatch.cs` (piso de postura removido), `Plugin.cs` (2 `ConfigEntry` removidos, `SafeEnable` do novo patch, versão `2.22.3 → 2.23.0`, minor — feature nova visível), `PROPRIEDADES.md` (2 linhas removidas). Compilado — 0 erros/0 warnings. **Ainda não revalidado in-game.**

## ⚠️ Crouch-run bloqueava troca de Stance (2026-09-09, build 2.23.0 → 2.23.1)

Usuário reportou: sprint agachado não deixa trocar de Stance (do próprio mod, item 001/002). Causa raiz em `StanceManager.HandleStanceHotkeys()`: um guard pré-existente (`if (gw.MainPlayer.IsSprintEnabled) return false;`, pensado pra bloquear troca de Stance durante sprint EM PÉ — cenário onde não faz sentido mudar de postura) passou a bloquear também o crouch-run, porque `CrouchRunEnableSprintPatch` deliberadamente liga a mesma flag nativa (`Physical.Sprinting`/`IsSprintEnabled`) enquanto AGACHADO — é o mecanismo usado pra acionar o dreno nativo de stamina (ver histórico desta sessão). O guard não existia quando essa combinação (sprint + ainda agachado) passou a ser possível.

**Fix:** guard agora só bloqueia se `IsSprintEnabled && PoseLevel >= 1f` (realmente em pé) — crouch-run continua agachado (`PoseLevel < 1`), então trocar de Stance nele passa a ser permitido, sem afetar o bloqueio original pro sprint em pé.

**Nota:** esse guard só existe em `HandleStanceHotkeys()` (hotkeys dedicadas Stance0-3); a tecla de toggle principal (V) e o scroll do mouse nunca tiveram esse bloqueio.

**Arquivos tocados:** `StanceManager.cs` (1 condição ajustada), `Plugin.cs` + `CameraRotationMod.csproj` (versão `2.23.0 → 2.23.1`). Compilado — 0 erros/0 warnings.

## ⚠️ Investigação: prone→agachado ainda passa visualmente por "em pé" (2026-09-09, sem novo fix)

Usuário testou a 2.23.0 e reportou que o efeito "levanta em pé e depois agacha" continua, mesmo com `ProneToggleDuckStandUpPatch` aplicado. Investigação adicional no Assembly:

Ao sair do prone, o jogo entra num estado de TRANSIÇÃO dedicado, `Prone2StandStateClass : IdleStateClass` (ref: `Prone2StandStateClass.cs`), cujo `Enter()` chama `MovementContext.AdjustCharacterController(prone: false)` — o collider do personagem só distingue **prone vs. não-prone** (binário), não agachado vs. em pé (isso é controlado por outro sistema, contínuo via `PoseLevel`/`SmoothedPoseLevel`). Ou seja: **durante toda a animação de "levantar", o collider já é o de TAMANHO PADRÃO (em pé)**, independente de qual pose final (`PoseMemo`) o `ProneToggleDuckStandUpPatch` mirar. A própria animação de "levantar do chão" também é um clipe único (não existem clipes alternativos "levantar até agachado" vs. "levantar até em pé") — fisicamente ela mostra o personagem se erguendo antes de qualquer blend pra pose final.

**Conclusão preliminar:** o fix atual (`PoseMemo=0`) provavelmente continua correto quanto ao RESULTADO FINAL (o personagem acaba agachado, não em pé, depois que a transição termina) — mas a JANELA DE EXPOSIÇÃO perigosa (collider de tamanho em pé) parece ser inerente à própria transição `Prone2StandStateClass`, não ao alvo de pose numérico. Corrigir isso exigiria mexer no timing do collider dentro dessa transição — uma mudança bem mais profunda e arriscada, sem garantia de sucesso, ainda não tentada. **Decisão pendente do usuário:** manter o fix atual (resultado final agachado, mesmo que a transição passe visualmente por em pé) vs. reverter `ProneToggleDuckStandUpPatch` por não resolver o problema de exposição que motivou o pedido.

## ⚠️ Bloqueio de sprint removido por completo (2026-09-09, build 2.23.1 → 2.23.2)

Usuário pediu pra remover de vez o guard `IsSprintEnabled` em `HandleStanceHotkeys()` (não só ajustar pra excluir crouch-run, como na 2.23.1) — não fazia mais sentido mantê-lo. Trocar de Stance agora funciona normalmente mesmo durante sprint em pé, além do crouch-run já liberado na rodada anterior.

**Arquivos tocados:** `StanceManager.cs` (condição removida), `Plugin.cs` + `CameraRotationMod.csproj` (versão `2.23.1 → 2.23.2`). Compilado — 0 erros/0 warnings.

## ⚠️ ProneToggleDuckStandUpPatch revertido (2026-09-09, build 2.23.2 → 2.23.3)

O fix não resolvia o problema real que motivou o pedido (ver "Investigação: prone→agachado ainda passa
visualmente por 'em pé'" acima) — a exposição de collider durante `Prone2StandStateClass` acontece
independente do alvo de pose numérico. Usuário decidiu reverter em vez de manter um fix que muda o
resultado final sem resolver a exposição. **Removido:** `Patches/ProneToggleDuckStandUpPatch.cs`
(deletado) e seu `SafeEnable` em `Plugin.cs`. Comportamento de "agachar" enquanto prone volta a ser
100% vanilla (restaura `PoseMemo`, geralmente vai pro em pé).

A investigação sobre o timing do collider foi promovida a item de backlog dedicado — ver
[022-collider-transicao-prone/](../022-collider-transicao-prone/) — sem urgência, pra revisitar quando
fizer sentido.

**Arquivos tocados:** `Patches/ProneToggleDuckStandUpPatch.cs` (deletado), `Plugin.cs` (`SafeEnable` removido, versão `2.23.2 → 2.23.3`), `CameraRotationMod.csproj`. Compilado — 0 erros/0 warnings.

## ⚠️ Rastejo BASE (sem sprint, sem scroll) mais lento que o vanilla (2026-09-09, build 2.23.3 → 2.23.4)

Usuário comparou vanilla vs. mod (sem ativar sprint nem mexer no scroll) e o prone do mod estava visivelmente mais lento.

**Causa raiz (bug nº2):** o branch "sem boost" de `ProneRunSpeedDriverPatch` usava `mc.CharacterMovementSpeed` BRUTO (valor absoluto) como `Animator.speed`. Mas a baseline nativa de `CharacterMovementSpeed` (= `MaxSpeed` desde o `Init()`) não é `1.0` — é o que `Evaluate(WalkSpeed, Strength/60)` resultar pra aquele perfil (numa captura real do spy, ~0.565). Vanilla nunca toca em `Animator.speed` pra prone, então o rastejo normal sempre toca a `1.0` (nativo) — não a "CharacterMovementSpeed crua". Resultado: mesmo sem nenhuma alteração do jogador, o mod tocava o rastejo ~43% mais devagar que o vanilla.

**Fix:** normalizar pela própria baseline — `ratio = CharacterMovementSpeed / MaxSpeed`. Dá exatamente `1.0` quando o jogador não mexeu no scroll (`CharacterMovementSpeed == MaxSpeed` por padrão), e escala pra baixo corretamente quando ele reduz.

**Bug nº3 (achado ao revisar o nº2):** ao soltar o boost, `CharacterMovementSpeed` ficava preso no valor absoluto que a rampa alcançou (podia passar de `MaxSpeed`, ex. ~2.0) — nada trazia de volta. Combinado com o `ratio` acima, isso faria o cálculo disparar bem acima de 1 logo após soltar o boost, antes de qualquer scroll. **Fix:** reset explícito de `CharacterMovementSpeed`/`SmoothedCharacterMovementSpeed` pra `MaxSpeed` (baseline) na borda de descida do boost, uma vez só.

**Arquivos tocados:** `Patches/ProneRunPatch.cs` (`ProneRunSpeedDriverPatch` — normalização por ratio + reset na borda de descida), `Plugin.cs` + `CameraRotationMod.csproj` (versão `2.23.3 → 2.23.4`). Compilado — 0 erros/0 warnings. **Ainda não revalidado in-game.**

## ⚠️ Flag de sprint do prone-run sobrevivia à saída do prone (2026-09-09, build 2.23.4 → 2.23.5)

Usuário reportou: usar prone-run, levantar (sprint ainda "ligado" fisicamente — segurado ou alternado sem soltar), e voltar pro prone reativava o boost sozinho, com um blip de animação parecendo "acabou de soltar sprint" bem na hora que reentrava no prone.

**Causa raiz:** `ProneRunSprintIntent.Held` (a flag state-independent que rastreia a tecla de sprint, ref: `PlayerToggleSprintPatch`/`PlayerEnableSprintPatch`) só é limpa quando a tecla física é solta/alternada de novo — nunca ao SAIR do prone. Se o jogador levanta com o sprint ainda fisicamente "ligado", `Held` atravessa a mudança de postura inteira sem ser tocado; a próxima entrada em prone via `wantsBoost=true` direto no primeiro frame, sem uma tecla nova ter sido pressionada nessa sessão de prone. `_wasBoosting` (usado pelo fix de reset da 2.23.4) tinha o mesmo problema — não era limpo por `ForceReset`.

**Fix:** `ProneMoveExitResetPatch` (Postfix em `ProneMoveStateClass.Exit`) agora também zera `ProneRunSprintIntent.Held` explicitamente, e `ProneRunSpeedDriverPatch.ForceReset` também zera `_wasBoosting`. Sair do prone por qualquer caminho força uma tecla NOVA (aperto ou alternar) antes do boost poder reengajar — não herda mais o estado físico da tecla através da transição de postura.

**Arquivos tocados:** `Patches/ProneRunPatch.cs` (`ForceReset` + `ProneMoveExitResetPatch.Postfix`), `Plugin.cs` + `CameraRotationMod.csproj` (versão `2.23.4 → 2.23.5`). Compilado — 0 erros/0 warnings. **Ainda não revalidado in-game.**

## ⚠️ Segundo bloqueio de Stance durante sprint (2026-09-09, build 2.23.5 → 2.23.6)

Usuário reportou que trocar de Stance durante crouch-run ainda não funcionava, mesmo depois da 2.23.2 (que removeu o guard dentro de `HandleStanceHotkeys()`). Investigação achou um SEGUNDO guard, mais alto no loop principal do `StanceManager` (antes até de chegar em `HandleStanceHotkeys()`): `if (isSprinting) { ...; return; }` — onde `isSprinting = gameWorld.MainPlayer.IsSprintEnabled`, a MESMA flag nativa que o crouch-run liga enquanto agachado. Esse `return` bloqueava TODAS as vias de trocar Stance (hotkeys, tecla de toggle, scroll do mouse) de uma vez, reintroduzindo por outra porta o mesmo bloqueio que a 2.23.2 já tinha removido de um lugar só.

**Fix:** `return` removido — trocar de Stance agora funciona durante qualquer sprint (em pé ou crouch-run), consistente com a decisão já tomada na 2.23.2. O `EndActionStance(forceCancel: true)` dentro do mesmo bloco foi mantido (encerrar uma Action Stance ativa ao começar a correr continua fazendo sentido, independente da hotkey de Stance estar liberada ou não).

**Arquivos tocados:** `StanceManager.cs` (`return` removido), `Plugin.cs` + `CameraRotationMod.csproj` (versão `2.23.5 → 2.23.6`). Compilado — 0 erros/0 warnings.

## ⚠️ Sobretaxa de stamina do crouch-run re-adicionada (2026-09-09, build 2.23.6 → 2.23.7)

`Crouch Run Stamina Surcharge` tinha sido removida em 2026-09-09 mais cedo (nesta mesma sessão), quando o usuário confirmou que o dreno nativo de sprint (já acionado pelo Prefix do crouch-run) sozinho bastava. Usuário pediu de volta a opção — não como padrão novo, mas como config OPCIONAL (`0.0` = desligada, comportamento atual preservado por padrão).

**Implementação:** reaproveitado 100% o padrão já usado no prone-run (`ProneRunMaxSpeedPatch`) — `PlayerPhysicalClass.GClass773`/`EConsumptionType.Sprint`/`AddConsumption`, engatado/desengatado junto com `wantsBoost` dentro do mesmo bloco 1x-por-frame que já existia em `CrouchRunMaxSpeedPatch`.

**Arquivos tocados:** `Patches/CrouchRunPatch.cs` (`_removeStaminaSurcharge` + bloco de consumo), `Plugin.cs` (`_CrouchRunStaminaSurcharge` + bind, versão `2.23.6 → 2.23.7`), `PROPRIEDADES.md`. Compilado — 0 erros/0 warnings.

## Lacunas conhecidas (não implementadas nesta rodada)

Deliberadamente fora do escopo desta implementação — motivo: escopo de pesquisa não coberto nas rodadas de investigação desta sessão (marcado como TODO na própria spec técnica antes do código, não descoberto tarde):

1. **Clamp do teto de velocidade contra `MovementContext.SprintSpeed`.** O critério de aceite da spec funcional ("a velocidade nunca ultrapassa o sprint em pé") hoje só é respeitado pela faixa do slider do F12 (`1.0–2.0×`), não por um clamp em runtime. Se o usuário configurar os multiplicadores de forma incomum (`Sprint Speed Multiplier` alto + `Crouch/Prone Run Speed Multiplier` no teto), o crouch/prone-run pode teoricamente ultrapassar o sprint em pé.
2. **Guarda de ADS/mount.** Nenhum dos 4 patches verifica `IsAiming`/`IsMountedState` — hoje é possível ativar o crouch-run/prone-run enquanto mirando ou com a arma montada, contrariando o corner case da spec funcional que pede consistência com as outras transições de postura do mod.

Ambas as lacunas estão documentadas no checklist §8 da spec técnica. Se a validação in-game abaixo confirmar que são perceptíveis/problemáticas, o caminho é um `018-rastejar-rapido-06-fix-01.md`.

## Validação pendente (obrigatória antes de fechar o item — ver checklist §8 da spec técnica)

- [x] Crouch-run isolado — **validado in-game pelo usuário (2026-09-09): funcionou.**
- [x] Prone-run isolado — **reportado quebrado (2026-09-09), corrigido nesta rodada. Falta re-validar a correção in-game.**
- [ ] Re-validar prone-run depois do fix: (a) segurar sprint já em movimento; (b) segurar sprint PARADO e só depois começar a andar (o cenário que estava quebrado).
- [ ] Cenário que motivou a review 01: segurar sprint PARADO mirando (simulando "prender a respiração") — sem boost, sem dreno extra.
- [ ] Fluxo PA-01-04: segurar sprint agachado parado, depois apertar W (mesma classe de bug do prone — ainda não corrigida para o crouch, ver nota na spec técnica).
- [ ] Interação com Stance customizada ativa (1/2/3).
- [ ] Dreno de stamina do prone-run com skill alta vs. baixa (confirma que a sobretaxa escala junto com o dreno nativo). Crouch-run não precisa desse teste — usa o dreno nativo direto, já validado.
- [ ] Fika com 2 jogadores (velocidade replicada corretamente para o peer observado).
- [ ] Teto baixo (crouch-run deve continuar funcionando na postura mais baixa, sem forçar o piso de postura).

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-09 | Build concluído via `/code-mod` em `modded/`. Compilado com sucesso (`dotnet build`, 0 erros/0 warnings) — **não instalado no jogo**. Versão `2.18.0 → 2.19.0`. |
| 2026-09-09 | **Correção de pasta.** O usuário apontou que o fork correto é `modded-testchannel/`, não `modded/`. Revertidas as mudanças em `modded/` (`git restore` + exclusão dos arquivos novos) e reaplicadas em `modded-testchannel/` — mesma lógica, mesmos nomes de classe, só o path muda. `StanceManager.cs` do testchannel tem `HandsStateGuard`/detecção de escada que `modded` não tinha; o reset dos 4 patches novos foi inserido no mesmo bloco de reset existente, sem tocar nessas features. Compilado via `dotnet build` contra `modded-testchannel/CameraRotationMod.csproj` — 0 erros/0 warnings — DLL em `mods/stancesAndCameraPositionSPT4.0.11/builds/TRL-StancesAndMobility.dll` (compilado a partir do `.csproj` de `modded-testchannel/`, saída no `builds/` padrão — gitignored). Versão do testchannel `2.19.16 → 2.20.0`. Validação in-game e as 2 lacunas conhecidas seguem pendentes. |
| 2026-09-09 | **Fix pós-validação.** Usuário testou: crouch-run OK, prone-run sem efeito na velocidade. Causa raiz encontrada e corrigida (ver seção acima) — nova fonte de sinal de sprint (`Player.ToggleSprint`/`Player.EnableSprint`, não mais `ProneMoveStateClass.EnableSprint`). Removida sobretaxa de stamina do crouch (vanilla já cobra) e a rampa de velocidade das duas posturas (a pedido do usuário). Versão `2.20.0 → 2.20.1`. Compilado — 0 erros/0 warnings. Falta re-validar prone-run in-game. |
