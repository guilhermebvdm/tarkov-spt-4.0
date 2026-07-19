# TRL-ImmersiveCombatMedicine — Propriedades (F12 / BepInEx)

> **Data:** 2026-07-12<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [docs/coop-heal-matrix.md](./docs/coop-heal-matrix.md)<br>

---

Fonte única de verdade das `ConfigEntry` expostas no menu F12 (regra do repo: toda entry nova atualiza este arquivo). Config gravada em `BepInEx/config/com.trl.immersivecombatmedicine.cfg`.

## Seção 1. Geral (Trauma)

| Nome (key) | Tipo | Padrão | Faixa | Tooltip |
|---|---|---|---|---|
| Ativar Mod | bool | `true` | — | Liga ou desliga todo o funcionamento do mod. |

## Seção 2. Mecanicas (Trauma)

| Nome (key) | Tipo | Padrão | Faixa | Tooltip |
|---|---|---|---|---|
| Sistema de Desmaio | bool | `true` | — | Ativa o desmaio ao receber muito dano massivo. |
| Sistema de Pernas | bool | `true` | — | (INERTE desde a v1.3.0 — substituído pelo Trauma 2.0 / Legs Effects. Remoção da key no item 010.) |
| Sistema de Braços | bool | `true` | — | Perder a mira ao perder os braços. |
| Sistema de Estomago | bool | `true` | — | Ficar sem ar ao tomar tiro no estômago. |

## Seção 3. Balanceamento (Trauma)

| Nome (key) | Tipo | Padrão | Faixa | Tooltip |
|---|---|---|---|---|
| Duracao do Desmaio | float | `20` | 5–120 | Quanto tempo (segundos) o jogador fica desmaiado. ALINHAR ENTRE TODOS OS PEERS. |

## Seção 4. Keybinds (Medic)

| Nome (key) | Tipo | Padrão | Faixa | Tooltip |
|---|---|---|---|---|
| Medic Interact Key | KeyboardShortcut | `F` | — | Tecla para FECHAR o modo medico (a abertura e pelo painel nativo de interacao, tecla F do jogo). |
| Medic Interact Mode | EBandAidPressMode | `Hold` | Press·Hold·DoubleTap | Modo de ativação: Press (aperta e solta), Hold (segura), DoubleTap (aperta 2x). |
| Emergency Drop Key | KeyboardShortcut | `F` | — | Tecla para drop emergencial do item durante animação de cura. |
| Emergency Drop Mode | EBandAidPressMode | `Press` | Press·Hold·DoubleTap | Modo de ativação do drop emergencial. |
| Medic Interact Distance | float | `5` | 1–15 | Distancia (m) do prompt E do acionamento do modo medico (mesma regra). Valor alto para testes; **reduzir no pacote final**. |

## Seção 5. Debug

| Nome (key) | Tipo | Padrão | Faixa | Tooltip |
|---|---|---|---|---|
| Invisivel para Bots | bool | `false` | — | DEBUG (host-only): bots deixam de mirar/atirar no jogador. Atirar num bot re-agroa por ≤2 s. Peers Fika continuam visíveis. |

## Seção 5. Trauma 2.0 (Motor)

Motor de estados do Trauma 2.0 (spec 002). Semântica: o motor publica sempre que `Ativar Mod` (master legado) **e** `Enable Trauma 2.0` estiverem on; estado neutro = rastrear e logar, zero efeito de gameplay. Keys em EN (migração dos textos antigos é o item 010). Entradas "Avançado" só aparecem com *Advanced settings* habilitado no F12.

| Nome (key) | Tipo | Padrão | Faixa | Avançado | Tooltip |
|---|---|---|---|---|---|
| Enable Trauma 2.0 | bool | `true` | — | — | Liga o motor de estados de trauma. Sem consumidores ligados não há NENHUM efeito de gameplay — só rastreamento e log. Desligar mid-raid publica a saída de todos os estados ativos. |
| Include Adrenaline As Painkiller | bool | `true` | — | — | Berserk/adrenalina conta como analgésico (paridade com o jogo — é o que o EFT considera em OnPainkillers). |
| One-Shot Cooldown Seconds | float | `4` | 3–5 | — | Anti-thrash (decisão 19): o mesmo one-shot involuntário (agachar/cair) não re-dispara nesse intervalo, por jogador e por tipo. Ciclos internos dos consumidores são isentos. |
| Reconciliation Polling Hz | float | `2` | 1–4 | Sim | Frequência do polling de reconciliação (cobre só caminhos sem evento: cirurgia FullRestore, revive do Fika, transit heal). Teto 4 Hz (D19). |
| Verbose Engine Log | bool | `false` | — | Sim | Loga detalhes de avaliação/polling. Transições de estado e supressões são SEMPRE logadas, independente desta opção. |

## Seção 6. Trauma 2.0 (Consumidores)

Toggle POR consumidor (comportamento 9 da spec funcional 002): cada consumidor se auto-gateia e, ao ser desligado mid-raid, desfaz os próprios efeitos. Todos nascem **OFF** até os itens 003+ entregarem.

| Nome (key) | Tipo | Padrão | Faixa | Avançado | Tooltip |
|---|---|---|---|---|---|
| Legs Effects | bool | `true` | — | — | Mancar N1/N2 + agachar involuntário (item 003). Governado pelo master Trauma 2.0; desligar mid-raid desfaz caps e cancela agachares pendentes. (Key renomeada na entrega do 003 — ver tabela Renomeadas.) |
| Fall Cycle | bool | `true` | — | — | Cair + ciclo de levantar (item 004). Governado pelo master Trauma 2.0; desligar mid-raid destrava o levantar na hora, cancela quedas pendentes e libera bots (o mancar interim do 003 NÃO volta). OFF com Legs Effects ON: o aviso (toast) da 1ª ocorrência da linha Cair ainda aparece — registry de consumidores é por região (PA-01-14). (Key renomeada na entrega do 004 — ver tabela Renomeadas.) |
| Arms Effects (item 005) | bool | `false` | — | — | Placeholder — tremor + cancela-ADS. Sem função até o item 005. |
| Stomach Effects (item 006) | bool | `false` | — | — | Placeholder — agachar involuntário do estômago. Sem função até o item 006. |
| Blackout 2.0 (item 007) | bool | `false` | — | — | Placeholder — desmaio percentual. Sem função até o item 007 (o desmaio ATUAL segue no toggle antigo "Sistema de Desmaio"). |
| Debug Test Consumer | bool | `false` | — | Sim | Consumidor de teste SEM efeito de gameplay: registra-se ATIVO para as TRÊS regiões (pernas/braços/estômago), destravando o toast/i18n para validação (AC5 da spec funcional). |

## Removidas

| Nome (key) | Removida em | Motivo |
|---|---|---|
| Shoulder Tap Key / Shoulder Tap Mode | 2026-07-12 (CR-01-15) | O toque no ombro virou ação do painel nativo de interação — keybind própria ficou morta. Valores salvos no .cfg dos usuários ficam órfãos (inofensivo). |

## Renomeadas (migração automática)

| Key | Mudança | Migração |
|---|---|---|
| Sistema de Braços | 2026-07-12 (CR-02-04): a key gravada tinha bytes de encoding quebrado (`Sistema de BraÃ§os`) e foi corrigida — identidade mudou | `MigrateOrphanedConfigKeys()` no Awake copia o valor órfão 1× e REMOVE a key antiga do .cfg (CR-03-01: sem o remove, a migração re-rodava todo boot e clobberava mudanças do usuário) |
| Legs Effects (item 003) → Legs Effects | 2026-07-19 (code-review 1 do 003): **rename-at-delivery** — a key nova nasce ON para todos; o `false` do placeholder das v1.2.x não era escolha do usuário | `MigrateOrphanedConfigKeys()` DELETA a entry órfã SEM copiar o valor + `Config.Save` (lição CR-03-01: sem o delete, o BepInEx re-persiste a key morta). **Padrão a repetir** nos placeholders `Arms Effects (item 005)` / `Stomach Effects (item 006)` / `Blackout 2.0 (item 007)` na entrega de cada item. |
| Fall Cycle (item 004) → Fall Cycle | 2026-07-19 (item 004, v1.5.0): **rename-at-delivery** — a key nova nasce ON para todos; o `false` do placeholder não era escolha do usuário | `MigrateOrphanedConfigKeys()` DELETA a entry órfã SEM copiar o valor + `Config.Save` (mesmo padrão do 003). |

## Seção 7. Trauma 2.0 (Pernas)

Consumidor de pernas (item 003). Alvos em % da velocidade **baseline composta** (Strength + classe/skill); se a penalidade vanilla for mais dura que o alvo, vale o vanilla (clamp logado — o mod nunca acelera). Efetivo do N2 = `min(N2, N1)` (warn 1× quando o clamp atua). **Interim do FallCycle removido no 004**: a linha Cair não recebe mais o cap N2 deste consumidor — o mancar da JANELA do ciclo é do 004 (causa própria, independente do toggle `Legs Effects`); com o `Fall Cycle` OFF, a linha Cair fica **sem efeito** do mod (o interim não volta).

| Nome (key) | Tipo | Padrão | Faixa | Avançado | Tooltip |
|---|---|---|---|---|---|
| N1 Target Total Speed Percent | float | `80` | 50–95 | — | Velocidade TOTAL experienciada no Mancar N1, em % do baseline (composto com classe/skill). Se a penalidade vanilla for mais dura que o alvo, vale o vanilla (clamp logado — nunca acelera o jogador). |
| N2 Target Total Speed Percent | float | `55` | 30–90 | — | Velocidade TOTAL experienciada no Mancar N2, em % do baseline. Mesma regra de clamp do N1. Se configurado ACIMA do N1, vale o efetivo min(N2, N1) — N2 nunca é mais leve que N1 (warn no log, 1x). |
| Block Sprint On N2 | bool | `true` | — | — | Em Mancar N2 o sprint fica bloqueado, inclusive sob analgésico (o vanilla libera sprint com analgésico; este toggle mantém o bloqueio do mod). N1 segue a regra vanilla. |
| Bot Crouch Dip Seconds | float | `0.7` | 0.3–1.5 | Sim | Duração do dip de agachar de bot FORA de combate antes de devolver a pose (em combate o SAIN restaura sozinho). |

## Seção 8. Trauma 2.0 (Queda)

Consumidor do ciclo de queda (item 004, spec 004 §3). Timers lidos por `.Value` no **início de cada fase** (deadline absoluto): mudanças no F12 valem a partir da PRÓXIMA fase iniciada — contagem em andamento nunca é re-baseada. O sprint da JANELA é sempre bloqueado (contrato do cap N2 do ciclo — **não** respeita `Block Sprint On N2` da seção 7). Pisos > 0 intencionais (documentados nos tooltips).

| Nome (key) | Tipo | Padrão | Faixa | Avançado | Tooltip |
|---|---|---|---|---|---|
| Fall Window Seconds | float | `3` | 1–10 | — | JANELA: tempo DE PÉ antes de cair de novo com as duas pernas quebradas (linha Cair). Conta do fim do levantar; mudanças valem a partir da PRÓXIMA janela iniciada. Piso 1s intencional (0 degeneraria em prone permanente). |
| Fall Block Seconds | float | `15` | 5–60 | — | BLOQUEIO: tempo no chão sem poder levantar após cada queda (tentar dá som de dor e nada acontece; rastejar é livre). Mudanças valem a partir do PRÓXIMO bloqueio iniciado. Piso 5s intencional (0 anularia o ciclo, conflitando com o anti-thrash do motor). |
| Bot Fall Hold Seconds | float | `15` | 5–120 | — | Tempo MÍNIMO que um bot com linha Cair fica no chão SEM combater antes de a IA poder levantar (ao levantar, é re-derrubado enquanto a condição durar). Separado dos timers humanos. |

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-12 | Guilherme | Criação (CR-01-07) — 12 entries em 5 seções; registro das ShoulderTap removidas. |
| 2026-07-12 | Guilherme | CR-03: seção Renomeadas (migração da key Sistema de Braços). |
| 2026-07-13 | Guilherme | CR-04 (rodada 04): faixa 5–120 e tooltip novo em Duracao do Desmaio; micro-textos da seção 4 sincronizados literalmente com os Config.Bind (fecha o resíduo do CR-03-16). |
| 2026-07-18 | Guilherme | Item 002 (motor Trauma 2.0, v1.2.0): seções novas `5. Trauma 2.0 (Motor)` (5 entries) e `6. Trauma 2.0 (Consumidores)` (6 entries, todas OFF). Nota: a key de seção `5. Trauma 2.0 (Motor)` coexiste com `5. Debug` (strings distintas no .cfg; ordenação do F12 intercala). |
| 2026-07-19 | Guilherme | Item 003 (pernas Trauma 2.0, v1.3.0): seção nova `7. Trauma 2.0 (Pernas)` (4 entries); `Legs Effects (item 003)` passa a default ON com tooltip real; `Sistema de Pernas` (seção 2) marcado INERTE — legado de pernas aposentado (D10), remoção da key no item 010. |
| 2026-07-19 | Guilherme | Code-review 1 do 003 (v1.3.1): RENAME `Legs Effects (item 003)` → `Legs Effects` (default ON efetivo p/ todos; órfã deletada sem copiar valor) + padrão rename-at-delivery registrado p/ os placeholders 004/005/006/007. |
| 2026-07-19 | Guilherme | Item 004 (ciclo de queda, v1.5.0): seção nova `8. Trauma 2.0 (Queda)` (3 entries); RENAME `Fall Cycle (item 004)` → `Fall Cycle` (default ON; órfã deletada sem copiar valor — tabela Renomeadas); nota do interim do 003 removido na seção 7 (linha Cair sem cap N2 do 003; `Fall Cycle` OFF = linha Cair sem efeito do mod). |
