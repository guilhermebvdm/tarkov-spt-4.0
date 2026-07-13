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
| Sistema de Pernas | bool | `true` | — | Cair no chão ao perder as pernas. |
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

## Removidas

| Nome (key) | Removida em | Motivo |
|---|---|---|
| Shoulder Tap Key / Shoulder Tap Mode | 2026-07-12 (CR-01-15) | O toque no ombro virou ação do painel nativo de interação — keybind própria ficou morta. Valores salvos no .cfg dos usuários ficam órfãos (inofensivo). |

## Renomeadas (migração automática)

| Key | Mudança | Migração |
|---|---|---|
| Sistema de Braços | 2026-07-12 (CR-02-04): a key gravada tinha bytes de encoding quebrado (`Sistema de BraÃ§os`) e foi corrigida — identidade mudou | `MigrateOrphanedConfigKeys()` no Awake copia o valor órfão 1× e REMOVE a key antiga do .cfg (CR-03-01: sem o remove, a migração re-rodava todo boot e clobberava mudanças do usuário) |

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-12 | Guilherme | Criação (CR-01-07) — 12 entries em 5 seções; registro das ShoulderTap removidas. |
| 2026-07-12 | Guilherme | CR-03: seção Renomeadas (migração da key Sistema de Braços). |
| 2026-07-13 | Guilherme | CR-04 (rodada 04): faixa 5–120 e tooltip novo em Duracao do Desmaio; micro-textos da seção 4 sincronizados literalmente com os Config.Bind (fecha o resíduo do CR-03-16). |
