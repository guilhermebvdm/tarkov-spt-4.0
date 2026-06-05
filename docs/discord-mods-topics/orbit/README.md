# ORBIT — Análise de thread do Discord

Diretório de trabalho para a análise do thread **"ORBIT"** (mod de IA de bots para SPT 4.0, autor **Chazut**) no canal `#mods-development` do Discord da comunidade SPT.

**Fonte:** <https://discord.com/channels/875684761291599922/1509314495019745451> · período **27/05 → 04/06/2026** · **699 mensagens** · capturado em 2026-06-04.

## Conteúdo

| Arquivo | Descrição |
|---|---|
| [01-transcricao.md](./01-transcricao.md) | **Transcrição fiel** das 699 mensagens (idioma original, horários em GMT-3, anexos linkados). |
| [02-analise.md](./02-analise.md) | **Análise em PT** — responde quem é quem, etapa do mod, interação com SAIN, preset ideal, arquitetura, looting, bugs, roadmap, curiosidades e cheat-sheet. |
| [assets/](./assets/) | 30 imagens + 2 logs baixados do thread, mais os JSONs brutos da captura (`_capture.json`, `_attachments.json`, `_manifest.json`) e o gerador (`_gen-transcript.js`). |

## Como foi capturado

Navegação autenticada no Discord (extração do DOM via Chrome DevTools), com varredura completa topo→fundo e deduplicação por ID de mensagem. As imagens foram baixadas do CDN do Discord e analisadas visualmente; os 2 logs (`Player.log`) foram inspecionados para extrair a arquitetura interna do mod (load order, patches Harmony, erros).

## Resumo de 1 linha

ORBIT dá **objetivos, looting e extract** a squads de bots; o **SAIN** continua fazendo o combate. Lançado, removido por questão de permissões (uso de código do LootingBots), **reescrito do zero** e **relançado** em 04/06 como v1.0.0 — estável e em testes pesados.
