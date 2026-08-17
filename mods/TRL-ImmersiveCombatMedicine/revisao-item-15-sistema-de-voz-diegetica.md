# Relatório de Code Review — Item 15: Sistema de Voz Diegética e Expressões de Dor

> **Módulo:** `TRL-ImmersiveCombatMedicine` (Trauma 2.0)  
> **Workspace:** [`modded-testchannel`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel)  
> **Funcionalidade:** Item 15 · Sistema de Voz Diegética e Expressões de Dor  
> **Status:** 🟢 Aprovado com Validação Cruzada de Referências (0 Bloqueadores 🔴, 0 Importantes 🟠, 1 Menor 🟡, 2 Melhorias 🟢)  
> **Data:** 2026-08-15  

---

## 1. Escopo e Arquivos Analisados

- [`Patches/Trauma/TraumaVoice.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/TraumaVoice.cs) (140 linhas)
- [`Patches/Trauma/TraumaPainVoice.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Trauma/TraumaPainVoice.cs) (111 linhas)
- [`Helpers/VoiceAndHealthUtils.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Helpers/VoiceAndHealthUtils.cs) (47 linhas)

---

## 2. Visão Geral da Arquitetura & Fluxo

```mermaid
flowchart TD
    A[Transição no TraumaEngine ou Lockout de Mira] --> B[TraumaPainVoice / TraumaVoice]
    B --> C{Jogador está Desmaiado em BlackoutTimers?}
    C -- Sim --> D[Mordaça Dupla: Silêncio Absoluto]
    C -- Não --> E{Jogador é Bot IA?}
    E -- Sim --> F[Filtro Defensivo: IA Mantida em Silêncio]
    E -- Não --> G{Canal Anti-Spam (Player, Kind) Liberado?}
    G -- Sim --> H[Dispara Fala Tipada Nativa do EFT]
    H --> I[LegBroken / HandBroken / OnAgony / OnBeingHurt / OnBreath]
    I --> J[FIKA Transmite Automaticamente aos Peers via PhrasePacket]
    H --> K[Registra Cooldown de 2s no Canal Específico]
```

---

## 3. Validação Cruzada com as Referências Oficiais (EFT, FIKA e SPT)

### 3.1. Validação com `references/eft-decompiled` (EFT 0.16.9)
- **Eliminação de Strings Inválidas (Bug do Mod Antigo):**
  - Verificado em [`Assembly-CSharp/EFT/EPhraseTrigger.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/EPhraseTrigger.cs).
  - O mod antigo usava `Enum.Parse` com strings inexistentes (`OnLegBroken`, `OnHandBroken`, `OnPain`), que eram silenciadas em `catch {}` vazios.
  - O sistema novo utiliza os gatilhos fortemente tipados do EFT:
    - `EPhraseTrigger.LegBroken` (49) e `EPhraseTrigger.HandBroken` (48).
    - `EPhraseTrigger.OnAgony` (com `importance: 100` e flags `Combat | Dying`).
    - `EPhraseTrigger.OnBeingHurt` e `EPhraseTrigger.OnBreath`.
- **Filtro de Desmaio em Duas Camadas:**
  - `SilenceVoicePatch` intercepta chamadas públicas de `Player.Say`.
  - `TraumaVoice.Allowed` checa `BlackoutTimers` antes de invocar chamadas diretas em `Player.Speaker.Play`, garantindo que tiros em corpos desmaiados não façam o personagem gritar enquanto estiver inconsciente.

### 3.2. Validação com `references/fika-plugin` (Fika.Core 2.3.4)
- **Propagação Automática em Rede:**
  - O Fika intercepta `Player.Say` nativamente em `FikaPlayer.cs:1093-1103` e despacha o `PhrasePacket` para os demais companheiros de equipe na raid.
  - Com isso, todos os jogadores no coop ouvem os gritos de dor e fratura do médico e aliados de forma diegética e sincronizada no espaço 3D.

### 3.3. Validação com `references/fika-headless` e `references/spt-source`
- O guard `if (p.IsAI) return false;` impede que bots disparem falas por este sistema, evitando poluição sonora e respeitando o design de áudio do SPT.

---

## 4. Avaliação Detalhada por Critério

### 4.1. Corretude & Resiliência
- **Canais Independentes de Anti-Spam:** O enum `Kind` (`Strong`, `Light`, `Fracture`, `Zeroed`, `Effort`) cria canais segregados no dicionário `_nextAllowed`. Se o jogador sofrer uma fratura no braço e uma na perna simultaneamente, ambas as falas são emitidas sem que uma cancele a outra.
- **Readback de Aceite do Speaker:** Para `PlayZeroed` e `TryPlayStrong`, o cooldown de 2s só é consumido se o `Speaker.Play` retornar diferente de nulo (ou seja, se o clipe sonoro de fato foi aceito e começou a tocar).

### 4.2. Desempenho e Alocações de GC
- `TraumaVoice.Clear()` é chamado durante o reset de raid (`OnWorldGone`, `OnWorldSwap`), limpando completamente o dicionário `_nextAllowed` e eliminando vazamento de `ProfileId`s entre partidas.

---

## 5. Tabela de Achados e Recomendações

| ID | Severidade | Arquivo / Linha | Descrição | Sugestão / Solução |
| :--- | :--- | :--- | :--- | :--- |
| **CR15-01** | 🟡 Menor | `VoiceAndHealthUtils.cs:5` | Namespace `namespace TrueTrauma` no arquivo `VoiceAndHealthUtils.cs`. | Alinhar para `TRLImmersiveCombatMedicine.Helpers`. |
| **CR15-02** | 🟢 Sugestão | `TraumaVoice.cs:27` | Propriedade `ResidualCount` para auditoria pós-raid. | Monitora o tamanho do dicionário de anti-spam para certificar limpeza completa. |
| **CR15-03** | 🟢 Sugestão | `TraumaVoice.cs:109` | Filtro `p.IsAI` explícito. | Mantém o áudio do jogo limpo e focado nos operadores humanos. |

---

## 6. Veredito

- **Classificação:** 🟢 **APROVADO COM VALIDAÇÃO DE REFERÊNCIAS**
- **Bloqueadores:** 0 🔴
- **Problemas Importantes:** 0 🟠
- **Gaps ou Riscos de Vazamento de Memória:** Nenhum. A arquitetura de voz diegética elimina todos os bugs do mod legado, respeita os enums reais do EFT e sincroniza perfeitamente no FIKA.
