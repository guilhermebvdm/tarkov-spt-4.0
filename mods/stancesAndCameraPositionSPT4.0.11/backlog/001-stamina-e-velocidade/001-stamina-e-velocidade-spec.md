# 001 — Stamina e Velocidade por Postura

**Mod:** stancesAndCameraPositionSPT4.0.11
**Status:** Backlog
**Criado:** 2026-05-07

## Visão geral

Adicionar controle de stamina e velocidade por **stance** ao mod. Cada stance (0 = posição vanilla com arma à frente; 1, 2, 3 = posições de descanso) recebe três blocos de configuração novos no F12: efeito sobre stamina (Drain ou Recovery, mutuamente exclusivos), redutor de velocidade (50–100%), e toggle de aplicação em prone. Hoje o mod não toca em nenhum desses sistemas — esta feature é totalmente nova.

## Comportamento atual

- O mod só altera **offsets visuais** de mãos/arma e ângulos de câmera por stance ([modded/Plugin.cs](../../modded/Plugin.cs), [modded/StanceManager.cs](../../modded/StanceManager.cs), [modded/Patches/SpringGetPatch.cs](../../modded/Patches/SpringGetPatch.cs)). Não há manipulação de stamina nem de velocidade.
- O EFT vanilla drena `HandsStamina` apenas durante ADS (taxa base `AimDrainRate = 3/s`); em hipfire (incluindo qualquer stance atual do mod), regenera a `HandsRestoration = 5/s`. O peso da arma, `EPose` (Stand/Sit/Prone), skill Strength e bipod já modulam essa drenagem nativamente.
- Velocidade de movimentação é controlada por `MovementContext.StateSpeedLimit`, que pega o **mínimo** entre limites ativos. O mod não registra nenhum limite hoje.
- Conferido: `mods/<mod>/original/` não menciona `stamina`, `MovementSpeed` ou `EPose` — o único uso de `MovementContext` lê `CovertEquipmentNoise` para áudio.

## Comportamento desejado

### Stamina (modos mutuamente exclusivos por stance)

**Drain e Recovery são simetricamente *fora-de-ADS*:**

- **ADS** (mirando): comportamento vanilla preservado. Drain do EFT (`method_10`) roda normal. Stances do mod **não interferem** em stamina nesse estado, em nenhum modo.
- **Hipfire** (não-ADS): aqui as stances atuam. `Drain` consome stamina extra; `Recovery` acelera a regen base.

Comportamento por modo:

Identificação das stances (com base nos offsets em [modded/Plugin.cs](../../modded/Plugin.cs)):

- **Stance 0** — posição vanilla, arma à frente em "pronto de tiro".
- **Stance 1** — pronto baixo (cano apontando para baixo, arma puxada para trás). Posição mais relaxada.
- **Stance 2** — coringa / mira inclinada (yaw -30°, sem deslocamento). Uso a definir pelo jogador.
- **Stance 3** — pronto alto (cano apontando para cima, arma levemente à frente). Patrulha urbana.

Configuração típica:

- **Stance 0 (pronto de tiro):** `Drain` leve — manter arma erguida cansa, mas não excessivamente.
- **Stances 1 e 3 (prontos baixo/alto):** `Recovery` — descansar regenera `HandsStamina` mais rápido que o vanilla. Pronto baixo recupera mais que pronto alto (mais relaxado).
- **Stance 2 (coringa):** a definir.
- **Drain:** em **hipfire**, a stance ativa drena `HandsStamina` a uma taxa proporcional a `Intensity` (referência base ≈ `AimDrainRate = 3/s` do EFT, multiplicada por `Intensity`). Implementado via tick manual no `StanceManager` chamando `HandsStamina.Consume(...)` enquanto a stance está ativa e o jogador não está em ADS. **Não aplica durante ADS** — o drain vanilla do EFT toma conta nesse estado.
- **Recovery:** em **hipfire**, multiplica `HandsRestoration` pela `Intensity`. **Não aplica durante ADS** — regen do EFT já é zero ali.
- **None:** stance não interfere em stamina. **Comportamento idêntico ao vanilla EFT** (e à versão atual do mod, que não toca em stamina). Esse é o modo a usar para "desligar" o efeito de uma stance específica.

> Defaults pré-configurados por stance estão na seção [Defaults recomendados](#defaults-recomendados).

### Velocidade

- Cada stance pode opcionalmente registrar um redutor de velocidade entre 50% e 100% via `MovementContext.AddStateSpeedLimit`.
- 100% = sem redução. Valores > 100% não fazem sentido (`min` ignoraria) — slider limitado ao topo em 100.
- Ao trocar de stance, o limite anterior é removido e o novo aplicado.

### Comportamento em prone

- Toggle `Apply When Prone` por stance (default `false`). Quando `false` e jogador está em `EPose.Prone`, todos os efeitos do mod (offsets, drain/recovery e speed limit) ficam **suspensos** até voltar a Stand/Sit. A stance permanece "armada", apenas pausada.
- Quando `true`: stance aplica normalmente em prone (modo experimental — pode conflitar com animações nativas).

### Cleanup e ciclo de vida

- **Início de raid:** o estado das stances é re-resolvido a partir da config (defaults ou últimos valores que o jogador setou). Estado em cache (`StanceStaminaState`) é zerado antes de aplicar novos valores.
- **Fim de raid (qualquer caminho — extract, morte, MIA):** speed limit registrado é removido explicitamente; estado estático em cache é resetado. Saídas precisam funcionar nos 3 caminhos (`Left`, `Killed`, `MissingInAction`) — saída por morte ou MIA não pode deixar resíduo na próxima raid.
- **Hideout / menu:** feature **inerte** — sem drain, sem recovery, sem speed limit. Os offsets visuais das stances 1/2/3 (que já existem hoje no mod) continuam funcionando como antes. <!-- review: confirmar — esta feature (drain/recovery/velocidade) deve ficar inerte no hideout, certo? Os offsets visuais já são pre-existentes e mantemos como estão. -->
- **Falha isolada:** se um dos patches lançar exceção, ele deve logar via `BepInEx` e seguir como no-op naquela invocação — sem derrubar o mod inteiro nem outros patches do mod.

### Aplicação só ao jogador local

A feature aplica **somente ao `MainPlayer` (jogador local)**. Bots, `NetworkPlayer` e demais entidades em `gameWorld.AllPlayers` não recebem drain/recovery/speed limit por stance — eles não usam o sistema de stances do mod, então a feature não tem semântica para eles.

### Defaults recomendados

Valores que o mod deve trazer pré-configurados ao instalar — calibrados para dar uma curva de gameplay coerente sem o jogador precisar mexer no F12:

| Stance | Posição | `Stamina Mode` | `Stamina Intensity` | `Modifies Movement Speed` | `Movement Speed Multiplier` | `Apply When Prone` |
|---|---|---|---|---|---|---|
| 0 | Pronto de tiro (vanilla) | `Drain` | `0.50` | `true` | `90` | `false` |
| 1 | Pronto baixo | `Recovery` | `2.00` | `true` | `100` | `false` |
| 2 | Coringa | `None` | `1.00` | `false` | `100` | `false` |
| 3 | Pronto alto | `Recovery` | `1.50` | `true` | `95` | `false` |

Racional:

- **Stance 0** drena devagar (intensity 0.50) — consequência leve por manter arma erguida o tempo todo. Reduz velocidade a 90% para encorajar transição para uma postura de descanso quando não há ameaça imediata.
- **Stance 1** é a "estação de recuperação" — Recovery na intensidade máxima (2.0), velocidade plena (100%). Prêmio para o jogador que escolhe relaxar.
- **Stance 2** começa neutra (Mode None, sem redutor) — o jogador define o que ela faz conforme uso.
- **Stance 3** é o meio-termo — Recovery moderada (1.5), leve redução de velocidade (95%) por manter cano levantado.

> Estes são os valores **iniciais** persistidos no `BepInEx.cfg` ao primeiro carregamento. O jogador pode ajustar livremente via F12.

## Critérios de aceite

- [ ] F12 expõe 5 propriedades novas para cada uma das 4 stances (0, 1, 2, 3) — total **20 entradas novas** organizadas em seções `Stance 0/1/2/3`.
- [ ] Stance 0 ganha seção dedicada no F12 contendo apenas as 5 propriedades deste backlog (sem offsets de mãos, pois a posição da Stance 0 é a vanilla).
- [ ] Numa instalação limpa do mod (sem `BepInEx.cfg` prévio), os defaults persistidos batem **exatamente** com a tabela de [Defaults recomendados](#defaults-recomendados): Stance 0 = Drain/0.50/90%, Stance 1 = Recovery/2.00/100%, Stance 2 = None/1.00/sem redutor, Stance 3 = Recovery/1.50/95%, todas com `Apply When Prone = false`.
- [ ] Com `Stance N Stamina Mode = None` em todas as stances, o comportamento de stamina das mãos é **idêntico ao vanilla** (regressão observável: drain em ADS, regen em hipfire, mesmas taxas de antes do mod).
- [ ] Com `Stance 0 Stamina Mode = Drain` em **hipfire** (não-ADS) em pé com arma de peso médio: `Intensity = 1.0` produz drain cronometrável em valor próximo de `AimDrainRate = 3/s` do EFT; `Intensity = 2.0` na mesma situação esgota `HandsStamina` em **metade do tempo** observado com `Intensity = 1.0`.
- [ ] Em **ADS**, com qualquer `Stance Stamina Mode` (Drain, Recovery ou None), o drain de stamina das mãos é **idêntico ao vanilla** — o mod não interfere em ADS, em nenhum modo.
- [ ] Com `Stance 1 Stamina Mode = Recovery` e `Intensity = 1.5` em **hipfire**, a regen de mãos é ~1.5× a vanilla (cronometrar tempo até cheia partindo de 50%); em ADS, regen segue inalterada (zero, como vanilla).
- [ ] Após trocar de stance, o efeito da stance anterior cessa **antes do próximo tick visível de stamina** (a barra para de drenar/regenerar com a taxa antiga e adota a nova; sem stamina sendo consumida para a stance errada).
- [ ] Com `Stance N Modifies Movement Speed = true` e `Multiplier = 75`, a velocidade base de movimentação fica em 75% (medível percorrendo distância fixa em tempo cronometrado, comparado com a mesma stance + `Multiplier = 100`).
- [ ] Com `Apply When Prone = false`, ao ir para prone enquanto numa stance ativa, drain/recovery cessam (barra de stamina volta a se comportar como vanilla) e speed limit é removido (velocidade volta ao baseline daquela posição) até sair do prone.
- [ ] Ao terminar uma raid e iniciar outra, **a velocidade base não fica reduzida sem stance ativa** (verificar correndo em linha reta no início da nova raid; tempo deve ser igual ao vanilla).
- [ ] Mudar `Mode` ou `Intensity` no F12 com a stance já ativa **atualiza o efeito sem precisar reiniciar a raid** (efeito novo aplica em < 1 segundo).
- [ ] **Cleanup em todas as saídas de raid:** após sair via extract (`Left`), morte (`Killed`) ou MIA (`MissingInAction`), uma raid seguinte começa **sem speed limit residual** e **sem drain/recovery aplicado** até o jogador trocar de stance (cada caminho testado pelo menos uma vez).
- [ ] **Hideout:** entrar no hideout (com ou sem stance ativa setada antes) **não dispara drain, recovery ou speed limit**. A barra de stamina e velocidade no hideout permanecem idênticas ao vanilla.
- [ ] **Falha isolada:** se um patch crashar (simulado via injeção de exceção em ambiente de teste), o mod continua carregado, outros patches seguem funcionando, e há entrada de `LogError` no console do BepInEx com stack trace.
- [ ] **Tick em hot path é alocação-zero:** o tick de Drain manual (`StanceManager.Update`) e os postfixes Harmony **não alocam** por frame (sem LINQ, sem `string.Format`, sem `new List`/`Dictionary` em hot path) — verificável via Unity Profiler em raid de 60s mostrando GC.Alloc estável.
- [ ] **Apenas o `MainPlayer` afetado:** num cenário com bots em volta, observação dos bots não mostra alteração de comportamento atribuível a este backlog (controle: bots se movimentam à velocidade vanilla independente das stances do jogador).

## Corner cases

- [ ] **Stamina zero em modo Drain:** a stance permanece ativa (não força saída); efeitos vanilla de exhausto (sway, arma tremendo) acontecem normalmente.
- [ ] **Troca rápida de stance (ex: scroll do mouse com cycle habilitado) durante ADS:** drain antigo cessa imediatamente, novo entra sem janela de estado inconsistente.
- [ ] **Mod externo já registrou um speed limit** com a mesma cause que escolhermos: nosso limit pode sobrescrever ou ser sobrescrito. Documentar incompatibilidade no README.
- [ ] **Mudança de `Mode` no F12 com a stance já ativa:** o efeito antigo cessa e o novo aplica (ou suspende, se virou `None`) sem reiniciar a raid.
- [ ] **Jogador entra em prone exatamente no frame da troca de stance** com `Apply When Prone = false`: ordem das checagens deve garantir que ou a stance fica suspensa, ou aplica e é suspensa logo após — sem janela de "speed limit aplicado mas drain pausado".
- [ ] **Stance ativa em raid + jogador morre/extrai:** speed limit é limpo junto com o `MovementContext` (verificar; se não, fazer cleanup explícito).
- [ ] **`Intensity = 0`:** equivalente a `Mode = None` em runtime — não deve introduzir patch sem efeito, ou deve fazer no-op cedo.
- [ ] **Composição com bipod/mounting:** quando jogador está com bipod (`BipodAimDrainRateMultiplier = 0.2`) e Stance 0 com Drain `Intensity = 1.5`, o resultado deve ser multiplicação dos dois fatores (0.2 × 1.5 = 0.3 do drain base) — não substituição.
- [ ] **Composição com skill Strength (até −20% em Elite):** nosso multiplicador é aplicado **depois** do desconto da skill (já é o caso se postfix em `method_10`), preservando recompensa de skill.
- [ ] **Estado `HandsExhausted` ativo no momento da troca de stance:** se jogador troca para uma Recovery stance enquanto exhausted, regen acelerada deve ajudá-lo a sair de exhausted mais rápido (sem hack — só regen 1.5×).
- [ ] **Recovery atingindo cap de `HandsCapacity` (150):** regen multiplicada não deve "transbordar" — EFT já capa, mas confirmar que não há warn de overflow.
- [ ] **Mudança de `Apply When Prone` no F12 enquanto jogador está em prone:** se jogador está em prone com stance ativa e flip o toggle de `false` para `true`, efeitos devem ativar; flip de `true` para `false` deve suspender. Sem precisar sair do prone.
- [ ] **Cycle de stances:** o cycle nativo do mod (`_EnableStance1/2/3 in Cycle` em [modded/Plugin.cs](../../modded/Plugin.cs)) é **independente** deste backlog — continua passando por todas as stances habilitadas no cycle, mesmo as sem efeito de stamina/velocidade. Stance 0 não entra no cycle (é o estado default, não-stance, ao qual o cycle volta).
- [ ] **Reload da config (F12 "Reset to default" ou recarregamento de arquivo):** estado em cache no `StanceStaminaState` precisa ser invalidado e recalculado a partir do novo valor da config.
- [ ] **Transição ADS↔hipfire com Stance Drain ativa:** ao soltar ADS, drain do EFT cessa (vanilla) e nosso tick assume — sem janela em que ambos drenam, sem janela em que nenhum drena.
- [ ] **Troca de personagem/profile entre raids:** se o jogador volta ao menu, troca de PMC/Scav e entra em outra raid, o estado de stance é re-inicializado a partir da config (sem leak de cache do personagem anterior).
- [ ] **Volta ao menu mid-raid (encerramento abrupto):** se o jogo é encerrado abruptamente e o jogador volta ao menu sem passar pelos hooks de fim de raid, a próxima raid inicia limpa (estado estático é defensivamente zerado no início de cada raid, não só no fim).
- [ ] **Mod recarregado mid-raid (BepInEx reload):** improvável em uso normal, mas se acontecer, ao recarregar plugins o estado é reconstruído a partir da config sem requisitar saída/entrada do jogador. Se incompatível com o estado atual da raid, ficar inerte até a próxima troca de stance.
- [ ] **`gameWorld.MainPlayer` ainda nulo no início da raid:** o tick de Drain e os postfixes precisam de `null-check` no `MainPlayer` antes de acessar — a janela de inicialização da raid pode ter frames em que o `Player` ainda não está pronto. Não pode haver `NullReferenceException` nesse intervalo.
- [ ] **Múltiplas instâncias de `Player` no `gameWorld` (bots, network):** os patches em `PlayerPhysicalClass.GetHandsRestorationFunc` rodam para qualquer `Player`. Postfix precisa filtrar para aplicar Recovery **somente quando `__instance` corresponde ao `MainPlayer`** (ou equivalente — verificar via referência ao `Player_0` interno).

## Fora de escopo

- Aumento de velocidade (> 100%). Sistema de `min` do EFT impede, e patches no animator são considerados invasivos demais para esta versão.
- Modificar drain/regen base do EFT (`AimDrainRate`, `HandsRestoration`). Esta feature só multiplica o cálculo final.
- Mexer em `EPose.Prone` (forçar entrada/saída, alterar animação). Apenas detectamos para suspender efeitos.
- Stamina de pernas (`Stamina` em vez de `HandsStamina`). Sprint/jump não são afetados.
- Aplicar a bots, `NetworkPlayer` ou outras entidades além do `MainPlayer`. Stances são conceito do jogador local.
- Compatibilidade explícita com Fika (multiplayer): este backlog não testa nem ajusta para cenário multiplayer; comportamento em Fika é "best effort" sem garantia.
- Persistência de stance ativa entre raids. Cada raid começa em Padrão (não-stance), independente do que estava ativo na raid anterior.

## Referências

- Spec técnica: [001-stamina-e-velocidade-technical-spec.md](001-stamina-e-velocidade-technical-spec.md)
- Propriedades atuais do F12: [PROPRIEDADES.md](../../PROPRIEDADES.md)
- Mod externo de referência (drain ajustável por stance): [SPT-BetterArmStamina](https://github.com/goatonabicycle/SPT-BetterArmStamina)

## Histórico

| Data | Evento |
|---|---|
| 2026-05-07 | Item criado |
| 2026-05-07 | Adicionada Stance 0; Drain/Recovery mutuamente exclusivos via enum; toggle Apply When Prone; faixa de velocidade fixada em 50–100% |
| 2026-05-07 | Sincronizado para o template canônico de `/create-spec` |
| 2026-05-07 | Revisão `/review-spec` — 4 critérios reescritos para verificabilidade, 2 critérios novos (regressão vanilla + reload de config), 6 corner cases adicionados, 3 trechos com `<!-- review: -->` para decisão humana |
| 2026-05-07 | Decisões aplicadas: Drain implementado via tick manual em hipfire (Opção B) — em ADS o EFT vanilla mantém o drain, mod não interfere; cycle de stances independe deste backlog; AC#3 reescrito para hipfire com referência ao `AimDrainRate=3/s`. Markers `<!-- review: -->` resolvidos. |
| 2026-05-07 | Identificadas as stances (1=pronto baixo, 2=coringa, 3=pronto alto) e adicionada tabela de **Defaults recomendados** com valores de Drain/Recovery/Velocidade pré-configurados. Stance 2 fica neutra (`None` + sem redutor) até o jogador definir uso. Tabela de defaults também replicada na spec técnica. |
| 2026-05-07 | Revisão `/review-spec` (com skills `spt-mod-best-practices` + `csharp-mod-best-practices`) — adicionados: seção "Cleanup e ciclo de vida" (raid start/end nos 3 paths, hideout inerte, falha isolada); restrição "apenas MainPlayer"; 6 ACs novos (cleanup nos 3 paths, hideout, falha isolada, alocação-zero, MainPlayer-only) + 5 corner cases (profile switch, encerramento abrupto, BepInEx reload, MainPlayer null no início, postfix filtrado por MainPlayer); 3 itens novos em "Fora de escopo" (bots/network, Fika, persistência entre raids); 1 marker `<!-- review: -->` para confirmar comportamento em hideout. |
