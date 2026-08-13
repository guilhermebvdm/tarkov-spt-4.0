# Backlog — TRL-SpeakFromTarkov

> Índice de itens de backlog. Cada linha aponta para uma pasta `NNN-<slug>/` com a spec funcional, técnica e revisões.

| # | Título | Resumo | Pasta | Status |
|---|---|---|---|---|
| 001 | portabilidade-spt-4 | Transição do mod do SPT 3.11 para a API do SPT 4.0.x e EFT 0.16.9, adaptando BepInEx, Fika hooks e referências da Unity. | [001-portabilidade-spt-4/](./001-portabilidade-spt-4/) | 🟢 |
| 002 | canais-comunicacao-spectator | Regras de transmissão por estado (Lobby P2P, Raid-Vivo, Espectador com Reverb Espectral e Mute nos Vivos). | [002-canais-comunicacao-spectator/](./002-canais-comunicacao-spectator/) | ⚪ |
| 003 | validacao-calibracao-volume | Alcance dinâmico por volume (Sussurro 10m, Normal 30m, Grito 60m) com telemetria e validação no Profiler F9. | [003-validacao-calibracao-volume/](./003-validacao-calibracao-volume/) | ⚪ |
| 004 | imersao-equipamento-mascaras-capacete | Abafamento de voz para máscaras (LPF), eco interno da fala (Self-Reverb 1ms-5ms), isolamento de capacete e dor grave. | [004-imersao-equipamento-mascaras-capacete/](./004-imersao-equipamento-mascaras-capacete/) | ⚪ |
| 005 | interacao-ia-bots-sain | Percepção 3D da voz pelos bots via EPhraseTrigger.OnMutter (0% humano, 100% IA) e resposta verbal dos bots. | [005-interacao-ia-bots-sain/](./005-interacao-ia-bots-sain/) | ⚪ |
| 006 | walkie-talkie-radio-hideout | Walkie-Talkie equipável no inventário, efeitos de estática/squelch e rádio no menu via Intelligence Center do Hideout. | [006-walkie-talkie-radio-hideout/](./006-walkie-talkie-radio-hideout/) | ⚪ |
| 007 | otimizacoes-hud-multithread | Decodificação Opus paralela em ThreadPool, HUD minimalista discreto de gameplay e volume individual por jogador no F12. | [007-otimizacoes-hud-multithread/](./007-otimizacoes-hud-multithread/) | ⚪ |
| 008 | calibrador-voz-assistente | Assistente de calibração interativo em 3 fases (Sussurro, Voz Normal, Falar Alto) com frases temáticas, medição de RMS e cálculo de limiares dinâmicos por jogador. | [008-calibrador-voz-assistente/](./008-calibrador-voz-assistente/) | 🟡 |

## Legenda

- ⚪ Backlog · 🟡 Em progresso · 🟢 Entregue · 🔴 Cancelado

## Fluxo

1. `/add-backlog-item <mod> <descrição>` → cria entrada + invoca `/create-spec`
2. `/create-spec <ref>` → spec funcional (critérios de aceite + corner cases)
3. `/review-spec <ref>` → editor crítico da spec funcional
4. `/create-technical-spec <ref>` → pré-código com refs ao Assembly
5. `/review-technical-spec <ref>` → cria review-NN.md (incremental); resolver até zerar
6. `/code-mod <ref>` → implementa em `modded/`
