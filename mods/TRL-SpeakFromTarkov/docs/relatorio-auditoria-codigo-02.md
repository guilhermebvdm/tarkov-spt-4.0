---
title: "Relatório de Auditoria Técnica de Código — TRL-SpeakFromTarkov (Review 02)"
date: 2026-08-27
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — TRL-SpeakFromTarkov (Review 02)

Segunda rodada de auditoria técnica estática profunda e minuciosa realizada na versão `1.5.2` (`modded-V3-audit`) do mod **TRL-SpeakFromTarkov**, cobrindo todas as 6 dimensões críticas com exceção do canal de menu (diferido para etapa isolada futura).

A auditoria re-avaliou todos os 15 arquivos C# do mod após as correções da versão 1.5.2 (canal de espectador 2D puro, escuta dupla, AGC inteligente em 2D, lookup $O(1)$ de speakers e mitigação de patches).

---

## 1. Resumo Executivo da Auditoria

| Severidade | Quantidade | Descrição |
|---|---|---|
| 🔴 **Crítico** | 0 | Falhas que causam crash instantâneo, corrupção de save ou desync total de rede |
| 🟠 **Alto** | 0 | Leaks entre raids, patches quebrados contra EFT 0.16.9 ou quebra de coop FIKA |
| 🟡 **Médio** | 2 | Instanciação/destruição de Texture2D e GUIStyle em OnGUI; retry de mic indevido no menu |
| 🔵 **Baixo** | 0 | Pequenos débitos de tipagem ou comentários desatualizados |
| 💡 **Otimização** | 1 | Varredura periódica de `FindObjectsOfType<MonoBehaviour>` no InRaidVoipHUD |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|---|---|---|---|
| `AUD-02-01` | 🟡 Médio | [`UI/InRaidVoipHUD.cs:L372`](../modded-V3-audit/UI/InRaidVoipHUD.cs#L372) / [`UI/VoipHUD.cs:L162`](../modded-V3-audit/UI/VoipHUD.cs#L162) | GC Pressure & GPU | Instanciação e destruição contínua de `Texture2D` e `new GUIStyle` a cada frame de repaint |
| `AUD-02-02` | 🟡 Médio | [`Core/VoipController.cs:L290-L300`](../modded-V3-audit/Core/VoipController.cs#L290-L300) | Ciclo de Vida | Loop de retry automático reabre o microfone no Menu Principal a cada 5s quando deveria permanecer fechado |
| `AUD-02-03` | 💡 Otimização | [`UI/InRaidVoipHUD.cs:L144`](../modded-V3-audit/UI/InRaidVoipHUD.cs#L144) | Performance & Unity | Varredura de `FindObjectsOfType<MonoBehaviour>()` a cada 2s até encontrar o `BattleStancePanel` |

---

## 3. Detalhamento dos Achados

### AUD-02-01 · ✅ Aplicado em 2026-08-27: Eliminação de GC Pressure e GPU Churn no OnGUI
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`UI/InRaidVoipHUD.cs:L372-L374`](../modded-V3-audit/UI/InRaidVoipHUD.cs#L372-L374) e [`UI/VoipHUD.cs:L69, L162-L165`](../modded-V3-audit/UI/VoipHUD.cs#L69)
- **Referência Cruzada:** [AP-03 / GC Pressure](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** Chamadas a `MakeTex(color)` / `Destroy()` no `OnGUI()` alocavam texturas na GPU e memória no GC a cada frame de repaint durante a transmissão.
- **Resolução:** Substituído por seleção direta das texturas 1x1 estáticas já pré-alocadas no `Initialize()` (`_greenTex`, `_yellowTex`, `_redTex`, `_grayTex`) em `InRaidVoipHUD.cs` e `VoipHUD.cs`. Adicionados campos de `GUIStyle` em cache e `OnDestroy()` defensivo.
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-02-02 · ✅ Aplicado em 2026-08-27: Guard in-raid no retry automático de microfone
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`Core/VoipController.cs:L290-L300`](../modded-V3-audit/Core/VoipController.cs#L290-L300)
- **Referência Cruzada:** [`Core/VoipController.cs:L166`](../modded-V3-audit/Core/VoipController.cs#L166)
- **Causa Raiz:** O retry de microfone disparava a cada 5s mesmo no Menu Principal onde o microfone deveria permanecer desligado.
- **Resolução:** Adicionada a checagem `if (!capturer.IsRecording && Singleton<EFT.GameWorld>.Instantiated)` para que a recuperação automática atue estritamente dentro da raid.
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-02-03 · ✅ Aplicado em 2026-08-27: Busca otimizada e condicional do BattleStancePanel
- **Severidade:** 💡 Otimização
- **Localização no Mod:** [`UI/InRaidVoipHUD.cs:L144-L157`](../modded-V3-audit/UI/InRaidVoipHUD.cs#L144-L157)
- **Causa Raiz:** Varredura indiscriminada de `FindObjectsOfType<MonoBehaviour>()` antes do spawn dos componentes de raid.
- **Resolução:** Condicionada a busca à instanciação do `GameWorld` e presença ativa do `MainPlayer`.
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## 4. Plano de Ação e Recomendações

1. Todas as correções da Review 02 foram implementadas e validadas com sucesso na versão `1.5.3`.
2. Projeto mantido com compilação 100% limpa (0 erros e 0 avisos).

---

## 5. Memória Consultada
- Memória consultada: `mods/TRL-SpeakFromTarkov/memory/sessions.md` (Sessões 1 a 15, v1.5.3).
